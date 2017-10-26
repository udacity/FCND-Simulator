using System.Collections.Concurrent;
using UnityEngine;

// usings needed for TCP/IP
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using MavLink;
using FlightUtils;
using Drones;
using DroneInterface;

public class BackyardFlyer : MonoBehaviour
{
    private IDrone drone;
    private Mavlink mav;
    private bool running = true;
    // track all clients
    // private ConcurrentBag<MAVLinkClientConn> clients = new ConcurrentBag<MAVLinkClientConn>();
    private ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 10;

    public Int32 port = 5760;
    public string ip = "127.0.0.1";

    // Use this for initialization
    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        mav = new Mavlink();
        // setup event listeners
        mav.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);
        mav.PacketFailedCRC += new PacketCRCFailEventHandler(OnPacketFailure);
        var tcpTask = TcpListenAsync();
    }

    // TODO: Make the sure the velocities correspond to the correct axis.
    // Emits telemetry data:
    //      Latitude
    //      Longitude
    //      Altitude
    //      Relative Altitude
    //      North Velocity (vx)
    //      East Velocity (vy)
    //      Vertical Velocity (vz)
    async Task EmitTelemetry(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(telemetryIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            // TODO: Make these magic numbers part of a util function?
            var lat = drone.Latitude() * 1e7d;
            var lon = drone.Longitude() * 1e7d;
            var alt = drone.Altitude() * 1000;
            var vx = drone.NorthVelocity() * 100;
            var vy = drone.EastVelocity() * 100;
            var vz = drone.VerticalVelocity() * 100;
            var hdg = drone.Yaw() * 100;
            var msg = new Msg_global_position_int
            {
                lat = (int)lat,
                lon = (int)lon,
                alt = (int)alt,
                relative_alt = (int)alt,
                vx = (short)vx,
                vy = (short)vy,
                vz = (short)vz,
                hdg = (ushort)hdg
            };
            var serializedPacket = mav.SendV2(msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);
            await Task.Delay(waitFor);
        }
    }

    // Emits a heartbeat message.
    async Task EmitHearbeat(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(heartbeatIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            byte base_mode;
            var guided = drone.Guided();
            var armed = drone.Armed();
            if (guided && armed)
            {
                base_mode = (byte)MAV_MODE.MAV_MODE_GUIDED_ARMED;
            }
            else if (guided)
            {
                base_mode = (byte)MAV_MODE.MAV_MODE_GUIDED_DISARMED;
            }
            else if (armed)
            {
                base_mode = (byte)MAV_MODE.MAV_MODE_MANUAL_ARMED;
            }
            else
            {
                base_mode = (byte)MAV_MODE.MAV_MODE_MANUAL_DISARMED;
            }
            Msg_heartbeat msg = new Msg_heartbeat
            {
                type = 1,
                autopilot = 1,
                system_status = 1,
                base_mode = base_mode,
                custom_mode = 1,
                mavlink_version = 3
            };
            var serializedPacket = mav.SendV2(msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);
            await Task.Delay(waitFor);
        }
    }

    async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        var telemetryTask = EmitTelemetry(stream);
        var heartbeatTask = EmitHearbeat(stream);

        while (running && client.Connected && stream.CanRead)
        {
            print("Reading from stream ... ");
            var buf = new byte[1024];
            var bytesRead = await stream.ReadAsync(buf, 0, buf.Length);
            if (bytesRead > 0)
            {
                var dest = new byte[bytesRead];
                Array.Copy(buf, dest, bytesRead);
                mav.ParseBytesV2(dest);
            }
            else
            {
                break;
            }
        }
        stream.Close();
        client.Close();
        print("CLIENT DISCONNECTED !!!");
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
    }

    // Starts an HTTP server and listens for new client connections.
    async Task TcpListenAsync()
    {
        try
        {
            // Setup the TcpListener 
            var addr = IPAddress.Parse(ip);
            var listener = new TcpListener(addr, port);
            // Start listening for client requests.
            listener.Start();
            print("Starting TCP MAVLink server ...");

            while (running)
            {
                var client = await listener.AcceptTcpClientAsync();
                print("Accepted connection !!!");
                clients.Add(client);
                var clientTask = HandleClientAsync(client);
            }
        }
        catch (SocketException e)
        {
            print(string.Format("SocketException: {0}", e));
        }
        finally
        {
            // this is to ensure the sever stops once a disconnection happens, or when done with everything
        }
    }

    // called when this is destroyed
    private void OnDestroy()
    {
        running = false;
    }

    void OnPacketReceived(object sender, MavlinkPacket packet)
    {
        print(string.Format("Received packet, message type = {0}", packet.Message));
        var msgstr = packet.Message.ToString();
        switch (msgstr)
        {
            case "MavLink.Msg_heartbeat":
                MsgHeartbeat(packet);
                break;
            case "MavLink.Msg_command_int":
                MsgCommandInt(packet);
                break;
            default:
                Debug.Log("Unknown message type !!!");
                break;
        }
    }

    void OnPacketFailure(object sender, PacketCRCFailEventArgs args)
    {
        print("failed to receive a packet!!!");
    }

    // The methods below determine what to do with the drone.
    // TODO: Make this a separate file (DroneControllerMeta.cs?)
    void MsgCommandInt(MavlinkPacket pack)
    {
        var msg = (MavLink.Msg_command_int)pack.Message;
        var command = (MAV_CMD)msg.command;

        print(string.Format("Command = {0}", command));

        if (command == MAV_CMD.MAV_CMD_COMPONENT_ARM_DISARM)
        {
            var param1 = msg.param1;
            if (param1 == 1.0)
            {
                drone.Arm(true);
                print("ARMED VEHICLE !!!");
            }
            else
            {
                drone.Arm(false);
                print("DISARMED VEHICLE !!!");
            }
        }
        else if (command == MAV_CMD.MAV_CMD_NAV_GUIDED_ENABLE)
        {
            var param1 = msg.param1;
            if (param1 > 0.5)
            {
                drone.TakeControl(true);
                print("VEHICLE IS BEING GUIDED !!!");
            }
            else
            {
                drone.TakeControl(false);
                print("VEHICLE IS NOT BEING GUIDED !!!");
            }
        }
        else if (command == MAV_CMD.MAV_CMD_NAV_LOITER_UNLIM)
        {
            var lat = msg.x / 1e7d;
            var lon = msg.y / 1e7d;
            var alt = msg.z;
            print("Vehicle Command: " + msg.x + "," + msg.y + "," + msg.z);
            print("Vehicle Command: (" + lat + "," + lon + "," + alt + ")");
            drone.Goto(lat, lon, alt);
        }
        else if (command == MAV_CMD.MAV_CMD_NAV_TAKEOFF)
        {
            drone.Goto(drone.Latitude(), drone.Longitude(), msg.z);
            print(string.Format("TAKING OFF to {0} altitude", msg.z));
        }
        else if (command == MAV_CMD.MAV_CMD_NAV_LAND)
        {
            drone.Goto(drone.Latitude(), drone.Longitude(), msg.z);
            print("LANDING !!!");
        }
        else
        {
            print(string.Format("Unknown MAVLink Command: {0}", command));
        }
    }

    // TODO: keep track of when last heartbeat was received and
    // potentially do something.
    void MsgHeartbeat(MavlinkPacket pack)
    {
        // var msg = (MavLink.Msg_heartbeat) pack.Message;
    }
}