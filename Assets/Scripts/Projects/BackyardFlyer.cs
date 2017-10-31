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
    private ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 10;
    public int homePositionIntervalHz = 1;

    public Int32 port = 5760;
    public string ip = "127.0.0.1";

    // enum to define the mode options
    // this follows the PX4 mode option set
    public enum MAIN_MODE : uint
    {
        CUSTOM_MAIN_MODE_MANUAL = 1,
        // other PX4 modes not of interest at the moment
        CUSTOM_MAIN_MODE_OFFBOARD = 6,  // guided
    }


    // enum for the set of masks that are used for setting the location position target
    public enum SET_POSITION_MASK : UInt16
    {
        IGNORE_POSITION = 0x007,
        IGNORE_VELOCITY = 0x038,
        IGNORE_ACCELERATION = 0x1C0,
        IGNORE_YAW = 0x400,
        IGNORE_YAW_RATE = 0x800,

        IS_FORCE = (1 << 9),
        IS_TAKEOFF = 0x1000,
        IS_LAND = 0x2000,
        IS_LOITER = 0x3000,
    }


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
    //      
    //      local coordinate - N
    //      Local coordinate - E
    //      Local coordinate - D
    async Task EmitTelemetry(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(telemetryIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            // send the GPS message
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

            // send the local position message
            var north = drone.LocalCoords().x;
            var east = drone.LocalCoords().y;
            var down = drone.LocalCoords().z;
            var local_msg = new Msg_local_position_ned
            {
                x = north,
                y = east,
                z = down,
                vx = (float) drone.NorthVelocity(),
                vy = (float) drone.EastVelocity(),
                vz = (float) drone.VerticalVelocity()
            };
            serializedPacket = mav.SendV2(local_msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);


            // wait
            await Task.Delay(waitFor);
        }
    }

    // Emits a heartbeat message.
    async Task EmitHearbeat(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(heartbeatIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            var guided = drone.Guided();
            var armed = drone.Armed();

            // build the base mode
            byte base_mode = (byte) MAV_MODE_FLAG.MAV_MODE_FLAG_CUSTOM_MODE_ENABLED;
            if (armed)
            {
                base_mode |= (byte)MAV_MODE_FLAG.MAV_MODE_FLAG_SAFETY_ARMED;
            }

            // build the custom mode (this specifies the mode of operation, using PX4 mode set)
            UInt32 custom_mode = ((byte)MAIN_MODE.CUSTOM_MAIN_MODE_MANUAL << 16);
            if (guided)
            {
                custom_mode = ((byte)MAIN_MODE.CUSTOM_MAIN_MODE_OFFBOARD << 16);
            }
            
            Msg_heartbeat msg = new Msg_heartbeat
            {
                type = 1,
                autopilot = 1,
                system_status = 1,
                base_mode = base_mode,
                custom_mode = custom_mode,
                mavlink_version = 3
            };
            var serializedPacket = mav.SendV2(msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);
            await Task.Delay(waitFor);
        }
    }

    // Emits the home position message.
    async Task EmitHomePosition(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(homePositionIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            // TODO: figure out where these are saved for the drone
            var home_lat = 1.0 * 1e7d;
            var home_lon = 1.0 * 1e7d;
            var home_alt = 1.0 * 1000;
            
            // NOTE: needed to initialize all the data for this to send properly
            var msg = new Msg_home_position
            {
                latitude = (int) home_lat,
                longitude = (int) home_lon,
                altitude = (int) home_alt,
                x = 0,
                y = 0,
                z = 0,
                q = new float[] { 0, 0, 0, 0 },
                approach_x = 0,
                approach_y = 0,
                approach_z = 0
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
        var homePositionTask = EmitHomePosition(stream); // TODO: maybe only want to send this once

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
            case "MavLink.Msg_command_long":
                print("handling command long");
                MsgCommandLong(packet);
                break;
            case "MavLink.Msg_set_position_target_local_ned":
                MsgLocalPositionTarget(packet);
                break;
            // TODO: add attitude, etc
            default:
                Debug.Log("Unknown message type !!!");
                break;
        }
    }

    void OnPacketFailure(object sender, PacketCRCFailEventArgs args)
    {
        print("failed to receive a packet!!!");
    }

    // handle the COMMAND_LONG message
    // used for:
    //      - arming / disarming
    //      - setting the mode
    //      - setting the home position
    void MsgCommandLong(MavlinkPacket pack)
    {
        var msg = (MavLink.Msg_command_long) pack.Message;
        var command = (MAV_CMD) msg.command;

        // DEBUG
        print(string.Format("Command = {0}", command));

        // handle the command types of interest
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
        else if (command == MAV_CMD.MAV_CMD_DO_SET_MODE)
        {
            var mode = (byte) msg.param1;
            var custom_mode = (byte) msg.param2;
            if ((mode & (byte) MAV_MODE_FLAG.MAV_MODE_FLAG_CUSTOM_MODE_ENABLED) > 0)
            {
                if (custom_mode == (byte) MAIN_MODE.CUSTOM_MAIN_MODE_OFFBOARD)
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
        }
        else if (command == MAV_CMD.MAV_CMD_DO_SET_HOME)
        {
            drone.SetHome(msg.param6, msg.param5, msg.param7);
            print("HOME POSITION PARAMS: " + msg.param1 + ", " + msg.param2 + ", " + msg.param3 + ", " + msg.param4 + ", " + msg.param5 + ", " + msg.param6 + ", " + msg.param7);
            print("Vehicle Home Position: " + msg.param6 + "," + msg.param5 + "," + msg.param7);
        }
        else
        {
            print(string.Format("Unknown MAVLink Command: {0}", command));
        }
    }


    // handle the SET_POSITION_TARGET_LOCAL_NED message
    // used for:
    //      - takeoff (to a given altitude)
    //      - landing
    //      - position control (goto)
    //      - velocity control
    void MsgLocalPositionTarget(MavlinkPacket pack)
    {
        var msg = (MavLink.Msg_set_position_target_local_ned) pack.Message;
        var mask = (UInt16) msg.type_mask;

        // split by the mask

        // TAKEOFF
        if ((mask & (UInt16) SET_POSITION_MASK.IS_TAKEOFF) > 0)
        {
            // TODO: z is being sent as negative, check to see if a sign change needs to occur
            //drone.Goto(drone.Latitude(), drone.Longitude(), msg.z);
            drone.Goto(drone.LocalCoords().x, drone.LocalCoords().y, msg.z);
            print(string.Format("TAKING OFF to {0} altitude", msg.z));
        }
        // LAND
        else if ((mask & (UInt16) SET_POSITION_MASK.IS_LAND) > 0)
        {
            // TODO: z is being sent as 0 here, make sure that is ok
            drone.Goto(drone.LocalCoords().x, drone.LocalCoords().y, msg.z);
            print("LANDING !!!");
        }
        // NEED TO REVIEW MASK
        else
        {
            // POSITION COMMAND
            if ((mask & (UInt16) SET_POSITION_MASK.IGNORE_POSITION) == 0)
            {
                // TODO: convert from local coordinate to lat/lon/alt
                // TODO: or have a local goto function
                var north = msg.x;
                var east = msg.y;
                var alt = msg.z;
                print("Vehicle Command: " + msg.x + "," + msg.y + "," + msg.z);
                print("Vehicle Command: (" + north + "," + east + "," + alt + ")");
                drone.Goto(north, east, alt);
            }
            else if ((mask & (UInt16) SET_POSITION_MASK.IGNORE_VELOCITY) == 0)
            {
                // TODO: set velocity is not yet implemented
                print("vehicle velocity command: " + msg.vx + ", " + msg.vy + ", " + msg.vz);
                //drone.SetVelocity(msg.vx, msg.vy, msg.vz, msg.yaw);
            }
        }
    }
    

    // TODO: keep track of when last heartbeat was received and
    // potentially do something.
    void MsgHeartbeat(MavlinkPacket pack)
    {
        // var msg = (MavLink.Msg_heartbeat) pack.Message;
    }
}