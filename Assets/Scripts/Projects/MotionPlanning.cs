using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

// usings needed for TCP/IP
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

using MavLink;
using FlightUtils;
using Drones;
using DroneInterface;

struct MavlinkRay
{
    public Vector3 rotation { get; }
    public MAV_SENSOR_ORIENTATION mavlinkOrientation { get; }

    public MavlinkRay(Vector3 r, MAV_SENSOR_ORIENTATION mo)
    {
        rotation = r;
        mavlinkOrientation = mo;
    }
}


public class MotionPlanning : MonoBehaviour
{
    private IDrone drone;
    private GameObject droneGO;
    private GameObject lidarLayer;
    public float lidarLengthWidth = 0.10f;
    private Mavlink mav;
    private bool running = true;
    // track all clients
    private ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;
    public int sensorIntervalHz = 1;
    public int homePositionIntervalHz = 1;
    public float maxSensorRange = 30;
    public Int32 port = 5760;
    public string ip = "127.0.0.1";
    private List<MavlinkRay> mavRays;

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
        lidarLayer = GameObject.Find("LidarLayer");
        droneGO = GameObject.Find("Quad Drone");
        drone = droneGO.GetComponent<QuadDrone>();
        mav = new Mavlink();
        // setup event listeners
        mav.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);
        mav.PacketFailedCRC += new PacketCRCFailEventHandler(OnPacketFailure);
        var tcpTask = TcpListenAsync();
        SetupLidarRays();
    }

    async Task EmitSensorInfo(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(telemetryIntervalHz);
        var collidersGenerator = GameObject.Find("ColliderGatherer").GetComponent<GenerateColliderList>();
        while (running && stream.CanRead && stream.CanWrite)
        {
            var pos = drone.UnityCoords();
            RaycastHit hitInfo;
            // Send multiple messages for different orientations
            foreach (var r in mavRays)
            {
                var dir = Quaternion.Euler(r.rotation) * droneGO.transform.forward;
                var hit = Physics.Raycast(pos, dir, out hitInfo, maxSensorRange);
                if (hit && hitInfo.distance > 1)
                {
                    var dist = hitInfo.distance;
                    print(string.Format("ray hit - rotation {0}, distance {1}", r.rotation, dist));
                    var msg = new Msg_distance_sensor
                    {
                        // A unity unit is 1m and the distance unit
                        // required by this message is centimeters.
                        min_distance = 0,
                        max_distance = (UInt16)(maxSensorRange * 100),
                        current_distance = (UInt16)(dist * 100),
                        type = (byte)MAV_DISTANCE_SENSOR.MAV_DISTANCE_SENSOR_LASER,
                        id = 0,
                        orientation = (byte)r.mavlinkOrientation,
                        // TODO: whatever this is
                        covariance = 0,
                    };
                    var serializedPacket = mav.SendV2(msg);
                    stream.Write(serializedPacket, 0, serializedPacket.Length);
                }
            }
            await Task.Delay(waitFor);
        }
    }

    /// <summary>
    /// TODO: Make the sure the velocities correspond to the correct axis.
    /// Emits telemetry data:
    ///      Latitude
    ///      Longitude
    ///      Altitude
    ///      Relative Altitude
    ///      North Velocity (vx)
    ///      East Velocity (vy)
    ///      Vertical Velocity (vz)
    ///      
    ///      local coordinate - N
    ///      Local coordinate - E
    ///      Local coordinate - D
    /// </summary>

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
                vx = (float)drone.NorthVelocity(),
                vy = (float)drone.EastVelocity(),
                vz = (float)drone.VerticalVelocity()
            };
            serializedPacket = mav.SendV2(local_msg);
            stream.Write(serializedPacket, 0, serializedPacket.Length);


            // wait
            await Task.Delay(waitFor);
        }
    }

    /// <summary>
    /// Emits a heartbeat message.
    /// </summary>

    async Task EmitHearbeat(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(heartbeatIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            var guided = drone.Guided();
            var armed = drone.Armed();

            // build the base mode
            byte base_mode = (byte)MAV_MODE_FLAG.MAV_MODE_FLAG_CUSTOM_MODE_ENABLED;
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

    // TODO: send this only when explicitly queried
    // Emits the home position message.
    async Task EmitHomePosition(NetworkStream stream)
    {
        var waitFor = Utils.HertzToMilliSeconds(homePositionIntervalHz);
        while (running && stream.CanRead && stream.CanWrite)
        {
            // TODO: figure out where these are saved for the drone
            var home_lat = drone.HomeLatitude() * 1e7;
            var home_lon = drone.HomeLongitude() * 1e7;
            var home_alt = 0.0 * 1000;

            // NOTE: needed to initialize all the data for this to send properly
            var msg = new Msg_home_position
            {
                latitude = (int)home_lat,
                longitude = (int)home_lon,
                altitude = (int)home_alt,
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
        // var sensorTask = EmitSensorInfo(stream);

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

    /// <summary>
    /// Starts an HTTP server and listens for new client connections.
    /// </summary>
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

    void CollidersToCSV()
    {
        var go = GameObject.Find("ColliderGatherer");
        if (go == null)
        {
            Debug.Log("ColliderGatherer GameObject not found in scene ...");
            return;
        }
        SimpleFileBrowser.ShowSaveDialog(CreateFile, null, false, null, "Select Folder", "Save");
    }

    void CreateFile(string path)
    {
        var filename = "map.csv";
        var fullPath = Path.Combine(path, filename);
        var colliders = GameObject.Find("ColliderGatherer").GetComponent<GenerateColliderList>().colliders;

        Debug.Log("Overwriting previous file");
        // var fs = File.Create(fullPath);
        // if (File.Exists(fullPath)) {
        // }

        // Write headers
        // TODO: overwrite file
        File.AppendAllText(Path.Combine(filename), "posX,posY,posZ,halfSizeX,halfSizeY,halfSizeZ\n");
        foreach (var c in colliders)
        {
            var pos = c.position;
            var hsize = c.halfSize;
            var row = string.Format("{0},{1},{2},{3},{4},{5}\n", pos.x, pos.y, pos.z, hsize.x, hsize.y, hsize.z);
            File.AppendAllText(Path.Combine(filename), row);
        }
    }

    void SetupLidarRays()
    {
        // roll -> x-axis, yaw -> y-axis, pitch -> z-axis
        mavRays = new List<MavlinkRay>();

        // rays for forward, backward, left, right, up, down
        mavRays.Add(new MavlinkRay(new Vector3(0, 0, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_NONE));
        mavRays.Add(new MavlinkRay(new Vector3(0, 90, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_90));
        mavRays.Add(new MavlinkRay(new Vector3(0, 180, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_180));
        mavRays.Add(new MavlinkRay(new Vector3(0, 270, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_270));
        mavRays.Add(new MavlinkRay(new Vector3(0, 0, 90), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_90));
        mavRays.Add(new MavlinkRay(new Vector3(0, 0, 270), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_270));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 45, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_45));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 135, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_135));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 225, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_225));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 315, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_315));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 0, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 45, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_45));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 90, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_90));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 135, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_135));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 0, 180), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_180));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 225, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_225));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 270, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_270));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 315, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_315));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 0, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 45, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_45));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 90, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_90));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 135, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_135));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 0, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 45, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_YAW_45));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 90, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_YAW_90));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 135, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_YAW_135));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 90, 180), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_180_YAW_90));
        // mavRays.Add(new MavlinkRay(new Vector3(0, 270, 180), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_180_YAW_270));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 0, 90), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_90));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 0, 90), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_PITCH_90));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 0, 90), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_PITCH_90));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 0, 180), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_180));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 0, 180), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_PITCH_180));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 0, 270), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_270));
        // mavRays.Add(new MavlinkRay(new Vector3(180, 0, 270), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_PITCH_270));
        // mavRays.Add(new MavlinkRay(new Vector3(270, 0, 270), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_PITCH_270));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 90, 180), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_180_YAW_90));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 270, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_270));
        // mavRays.Add(new MavlinkRay(new Vector3(315, 315, 315), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_315_PITCH_315_YAW_315));
    }

    void LateUpdate()
    {
        Sense();
    }

    void Sense()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var lines = new List<LineRenderer>();
            var pos = drone.UnityCoords();
            print("Sensing ... from location " + pos);
            RaycastHit hit;
            foreach (var r in mavRays)
            {
                var dir = Quaternion.Euler(r.rotation) * droneGO.transform.forward;
                Physics.Raycast(pos, dir, out hit, maxSensorRange);
                if (hit.collider && hit.distance > 3)
                {
                    var dist = hit.distance;
                    print(string.Format("ray hit - drone loc {0}, rotation {1}, distance (meters) {2}, collision loc {3}", pos, r.rotation, dist, hit.point));
                    // draw line
                    var line = new LineRenderer();
                    var linePoints = new List<Vector3>();
                    linePoints.Add(pos);
                    linePoints.Add(hit.point);
                    line.SetPositions(linePoints.ToArray());
                    line.enabled = true;
                    lines.Add(line);
                }
            }
        }
    }
}