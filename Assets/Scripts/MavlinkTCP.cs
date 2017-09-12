using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// usings needed for TCP/IP
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

// needed for mavlink
using MavLink;

// TODO: possibly use a class like this to keep track of 
// task/client pairs.
public class ClientState {
    public TcpClient client = null;

    public Task task =  null;
}


public class MavlinkTCP : MonoBehaviour {

	private SimpleQuadController _simpleController;
	private QuadController _quadController;
    private Mavlink _mavlink;

    // thread for the tcp connection
    private Thread _tcpListenerThread;


    public int _heartbeatInterval = 1;

    public int _telemetryInterval = 10;

    public Int32 _port = 5760;

    public string _ip = "127.0.0.1";


    // Use this for initialization
    void Start () {
		_simpleController = GameObject.Find("Quad Drone").GetComponent<SimpleQuadController>();
		_quadController = GameObject.Find("Quad Drone").GetComponent<QuadController>();
        _mavlink = new Mavlink();
        // setup event listeners
        _mavlink.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);
        _mavlink.PacketFailedCRC += new PacketCRCFailEventHandler(OnPacketFailure);
        // start the thread for the tcp connection
        _tcpListenerThread = new Thread(TcpThread);
        _tcpListenerThread.Start();
    }

    async void EmitTelemetry(NetworkStream stream) {
        var waitFor = (int) (1000f / _telemetryInterval);
        while (stream.CanRead && stream.CanWrite) {
            // print("Emitting telemetry data ...");
            var msg = new Msg_global_position_int {
                lat = (int) (_quadController.GPS.x*1e7),
                lon = (int) (-1.0*_quadController.GPS.z*1e7),
                alt = (int) (_quadController.GPS.y*1000),
                relative_alt = (int) (_quadController.GPS.y*1000),
                vx = 0,
                vy = 0,
                vz = 0,
                hdg = 0
            };
            var serializedPacket = _mavlink.SendV2(msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);
            await Task.Delay(waitFor);
        }
    }
    
    async void EmitHearbeat(NetworkStream stream) {
        var waitFor = (int) (1000f / _heartbeatInterval);
        while (stream.CanRead && stream.CanWrite) {
            // print("Emitting hearbeat ...");
            byte base_mode;
            if (_simpleController.guided) {
                base_mode = (byte) MAV_MODE.MAV_MODE_GUIDED_ARMED;
            } else {
                base_mode = (byte) MAV_MODE.MAV_MODE_GUIDED_DISARMED;
            }
            Msg_heartbeat msg = new Msg_heartbeat {
                type = 1,
                autopilot = 1,
                system_status = 1,
                base_mode = base_mode,
                custom_mode = 1,
                mavlink_version = 3
            };
            var serializedPacket = _mavlink.SendV2(msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);
            await Task.Delay(waitFor);
        }
    }

    async void HandleClientAsync(TcpClient client) {
        var stream = client.GetStream();
        EmitTelemetry(stream);
        EmitHearbeat(stream);

        while (client.Connected && stream.CanRead) {
            var buf = new byte[1024];
            // var bytesRead = stream.Read(buf, 0, buf.Length);
            var bytesRead = await stream.ReadAsync(buf, 0, buf.Length);
            if (bytesRead > 0) {
                var dest = new byte[bytesRead];
                Array.Copy(buf, dest, bytesRead);
                _mavlink.ParseBytesV2(dest);
            }
        }
    }

    void Update () {
    }

    void FixedUpdate() {
    }

    async void TcpListenAsync() {
    }

    void TcpThread() {
        try {
            // Setup the TcpListener 
            var addr = IPAddress.Parse(_ip);
            var listener = new TcpListener(addr, _port);
            // Start listening for client requests.
            listener.Start();
            print("Starting TCP MAVLink server ...");

            while (true) {
                var client = listener.AcceptTcpClient();
                print("Accepted connection !!!");
                // Task task = await HandleClientAsync(client);
                HandleClientAsync(client);
            }
        } catch (SocketException e) {
            print(string.Format("SocketException: {0}", e));
        } finally {
            // this is to ensure the sever stops once a disconnection happens, or when done with everything
        }
    }

    // called when this is destroyed
    private void OnDestroy() {
        _tcpListenerThread = null;
    }

    void OnPacketReceived(object sender, MavlinkPacket packet) {
        print(string.Format("Received packet, message type = {0}", packet.Message));
        var msgstr = packet.Message.ToString();
        switch (msgstr) {
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

    void OnPacketFailure(object sender, PacketCRCFailEventArgs args) {
        print("failed to receive a packet!!!");
    }

    // The methods below determine what to do with the drone.
    // TODO: Make this a separate file (DroneControllerMeta.cs?)
    void MsgCommandInt(MavlinkPacket pack) {
        var msg = (MavLink.Msg_command_int) pack.Message;
        var command = (MAV_CMD) msg.command;

        print(string.Format("Command = {0}", command));

        switch (command) {
            case MAV_CMD.MAV_CMD_COMPONENT_ARM_DISARM:
                var param1 = msg.param1;
                if (param1 == 1) {
                    _simpleController.ArmVehicle();
                    print("ARMED VEHICLE !!!");
                } else {
                    _simpleController.DisarmVehicle();
                    print("DISARMED VEHICLE !!!");
                }
                break;
            case MAV_CMD.MAV_CMD_NAV_GUIDED_ENABLE:
                _simpleController.guided = true;
                print("VEHICLE IS BEING GUIDED !!!");
                break;
            case MAV_CMD.MAV_CMD_NAV_LOITER_UNLIM:
                break;
            case MAV_CMD.MAV_CMD_NAV_TAKEOFF:
                var gpsLoc = new Vector3(msg.x, msg.y, msg.z);
                _simpleController.CommandGPS(gpsLoc);
                print("TAKING OFF !!!");
                break;
            case MAV_CMD.MAV_CMD_NAV_LAND:
                print("LANDING !!!");
                break;
            default:
                break;
        }
    }

    // TODO: keep track of when last heartbeat was received and
    // potentially do something.
    void MsgHeartbeat(MavlinkPacket pack) {
        // var msg = (MavLink.Msg_heartbeat) pack.Message;
    }
}