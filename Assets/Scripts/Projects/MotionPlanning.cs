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
using UdacityNetworking;

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
    public float lidarLengthWidth = 0.10f;
    private Mavlink mav;
    //    private bool running = true;
    public NetworkController networkController;
    private string collidersFile = "colliders.csv";
    // track all clients
    //    private ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;
    public int sensorIntervalHz = 1;
    public int homePositionIntervalHz = 1;
    public float maxSensorRange = 30;
    //    public Int32 port = 5760;
    //    public string ip = "127.0.0.1";
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
        droneGO = GameObject.Find("Quad Drone");
        drone = droneGO.GetComponent<QuadDrone>();
        mav = new Mavlink();
        // setup event listeners
        mav.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);
        mav.PacketFailedCRC += new PacketCRCFailEventHandler(OnPacketFailure);

        SetupLidarRays();
        networkController.AddMessageHandler(OnMessageReceived);
        networkController.EnqueueRecurringMessage(GlobalPosition, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(LocalPosition, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(Heartbeat, Utils.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(HomePosition, Utils.HertzToMilliSeconds(homePositionIntervalHz));
        networkController.EnqueueRecurringMessage(SensorInfo, Utils.HertzToMilliSeconds(sensorIntervalHz));
    }

    byte[] SensorInfo()
    {
        var pos = drone.UnityCoords();
        RaycastHit hitInfo;
        // Send multiple messages for different orientations
        BinarySerializer b = new BinarySerializer();
        foreach (var r in mavRays)
        {
            var dir = Quaternion.Euler(r.rotation) * droneGO.transform.forward;
            if (Physics.Raycast(pos, dir, out hitInfo, maxSensorRange))
            {
                var dist = hitInfo.distance;
                print(string.Format("ray hit - drone location {0}, rotation {1}, distance (meters) {2}", pos, r.rotation, dist));
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
                    // TODO: add variance model, likely using FastNoise
                    covariance = 0,
                };
                var serializedPacket = mav.SendV2(msg);
                b.WriteBytes(serializedPacket);
            }
        }
        return b.GetBytes();
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
    byte[] GlobalPosition()
    {
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
        return serializedPacket;
    }
    byte[] LocalPosition()
    {
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
        var serializedPacket = mav.SendV2(local_msg);
        return serializedPacket;
    }

    byte[] Heartbeat()
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
        return serializedPacket;
    }

    byte[] HomePosition()
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
        return serializedPacket;
    }

    void OnMessageReceived(MessageInfo msgInfo)
    {
        mav.ParseBytesV2(msgInfo.message);
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

    void MsgLocalPositionTarget(MavlinkPacket pack)
    {
        var msg = (MavLink.Msg_set_position_target_local_ned)pack.Message;
        var mask = (UInt16)msg.type_mask;

        // split by the mask

        // TAKEOFF
        if ((mask & (UInt16)SET_POSITION_MASK.IS_TAKEOFF) > 0)
        {
            // TODO: z is being sent as negative, check to see if a sign change needs to occur
            //drone.Goto(drone.Latitude(), drone.Longitude(), msg.z);
            drone.Goto(drone.LocalCoords().x, drone.LocalCoords().y, msg.z);
            print(string.Format("TAKING OFF to {0} altitude", msg.z));
        }
        // LAND
        else if ((mask & (UInt16)SET_POSITION_MASK.IS_LAND) > 0)
        {
            // TODO: z is being sent as 0 here, make sure that is ok
            drone.Goto(drone.LocalCoords().x, drone.LocalCoords().y, msg.z);
            print("LANDING !!!");
        }
        // NEED TO REVIEW MASK
        else
        {
            // POSITION COMMAND
            if ((mask & (UInt16)SET_POSITION_MASK.IGNORE_POSITION) == 0)
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
            else if ((mask & (UInt16)SET_POSITION_MASK.IGNORE_VELOCITY) == 0)
            {
                // TODO: set velocity is not yet implemented
                print("vehicle velocity command: " + msg.vx + ", " + msg.vy + ", " + msg.vz);
                //drone.SetVelocity(msg.vx, msg.vy, msg.vz, msg.yaw);
            }
        }
    }

    // handle the COMMAND_LONG message
    // used for:
    //      - arming / disarming
    //      - setting the mode
    //      - setting the home position
    void MsgCommandLong(MavlinkPacket pack)
    {
        var msg = (MavLink.Msg_command_long)pack.Message;
        var command = (MAV_CMD)msg.command;

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
            var mode = (byte)msg.param1;
            var custom_mode = (byte)msg.param2;
            if ((mode & (byte)MAV_MODE_FLAG.MAV_MODE_FLAG_CUSTOM_MODE_ENABLED) > 0)
            {
                if (custom_mode == (byte)MAIN_MODE.CUSTOM_MAIN_MODE_OFFBOARD)
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
        var collidersGenerator = go.GetComponent<GenerateColliderList>();
        var colliders = collidersGenerator.colliders;

        SimpleFileBrowser.ShowSaveDialog(CreateFile, null, true, null, "Select Folder", "Save");
    }

    void CreateFile(string path)
    {
        var filepath = Path.Combine(path, collidersFile);
        Debug.Log(string.Format("Writing colliders to {0} ...", filepath));
        if (File.Exists(filepath))
        {
            Debug.Log("Overwriting previous file");
        }

        var colliders = GameObject.Find("ColliderGatherer").GetComponent<GenerateColliderList>().colliders;
        var header = "posX,posY,posZ,halfSizeX,halfSizeY,halfSizeZ\n";

        File.Create(filepath).Close();
        // for comparison
        File.AppendAllText(filepath, header);
        foreach (var c in colliders)
        {
            var pos = c.position;
            var hsize = c.halfSize;
            var row = string.Format("{0},{1},{2},{3},{4},{5}\n", pos.x, pos.y, pos.z, hsize.x, hsize.y, hsize.z);
            File.AppendAllText(filepath, row);
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
        mavRays.Add(new MavlinkRay(new Vector3(90, 0, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90));
        mavRays.Add(new MavlinkRay(new Vector3(270, 0, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270));
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
        // mavRays.Add(new MavlinkRay(new Vector3(90, 45, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_45));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 90, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_90));
        // mavRays.Add(new MavlinkRay(new Vector3(90, 135, 0), MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_135));
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
        // manually sense lidar rays (for debugging)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Sense();
        }

        // Save colliders file
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                CollidersToCSV();
            }
        }
    }

    // For debugging
    void Sense()
    {
        var lines = new List<LineRenderer>();
        var pos = drone.UnityCoords();
        print("Sensing ... from location " + pos);
        RaycastHit hit;
        foreach (var r in mavRays)
        {
            var dir = Quaternion.Euler(r.rotation) * droneGO.transform.forward;
            if (Physics.Raycast(pos, dir, out hit, maxSensorRange))
            {
                var dist = hit.distance;
                print(string.Format("ray hit - drone loc {0}, rotation {1}, distance (meters) {2}, collision loc {3}", pos, r.rotation, dist, hit.point));
                // draw line
                // var line = new LineRenderer();
                // var linePoints = new List<Vector3>();
                // linePoints.Add(pos);
                // linePoints.Add(hit.point);
                // line.SetPositions(linePoints.ToArray());
                // line.enabled = true;
                // lines.Add(line);
            }
        }
    }
}