using System.Collections.Concurrent;
using System.Collections.Generic;
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
using UdacityNetworking;

public class Controls : MonoBehaviour
{
    private IDrone drone;
    private Mavlink mav;
    public NetworkController networkController;
    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 15;
    public int homePositionIntervalHz = 1;

    /// <summary>
    /// enum to define the mode options
    /// this follows the PX4 mode option set
    /// </summary>
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

    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        drone.ControlRemotely(true);
        mav = new Mavlink();
        // setup event listeners
        mav.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);
        mav.PacketFailedCRC += new PacketCRCFailEventHandler(OnPacketFailure);

        networkController.AddMessageHandler(OnMessageReceived);
        networkController.EnqueueRecurringMessage(GlobalPosition, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(LocalPositionNED, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(Heartbeat, Utils.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(HomePosition, Utils.HertzToMilliSeconds(homePositionIntervalHz));
        networkController.EnqueueRecurringMessage(AttitudeTarget, Utils.HertzToMilliSeconds(telemetryIntervalHz));
    }

    /// <summary>
    /// http://mavlink.org/messages/common#SCALED_PRESSURE
    /// </summary>
    List<byte[]> ScaledPressure()
    {
        var msg = new Msg_raw_imu
        {
        };
        var serializedPacket = mav.SendV2(msg);
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
    }

    /// <summary>
    /// http://mavlink.org/messages/common#RAW_IMU
    /// </summary>
    List<byte[]> RawIMU()
    {
        var msg = new Msg_raw_imu
        {
        };
        var serializedPacket = mav.SendV2(msg);
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
    }

    /// <summary>
    /// http://mavlink.org/messages/common#GLOBAL_POSITION_INT
    /// </summary>
    List<byte[]> GlobalPosition()
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
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
    }

    /// <summary>
    /// http://mavlink.org/messages/common#ATTITUDE_TARGET
    /// <summary>
    List<byte[]> AttitudeTarget()
    {
        var gyro = drone.AngularVelocity();
        var pitch = (float)drone.Pitch();
        var yaw = (float)drone.Yaw();
        var roll = (float)drone.Roll();
        var q = Quaternion.Euler(pitch, yaw, roll);
        var msg = new Msg_attitude_target
        {
            type_mask = 0x00,
            // ENU to NED frame
            q = new float[4] { q.w, q.z, q.x, q.y },
            body_pitch_rate = gyro.x,
            body_roll_rate = gyro.y,
            body_yaw_rate = gyro.z,
            // TODO: Get drone thrust
            thrust = 0,
        };
        var serializedPacket = mav.SendV2(msg);
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
    }

    /// <summary>
    ///  http://mavlink.org/messages/common#LOCAL_POSITION_NED
    /// <summary>
    List<byte[]> LocalPositionNED()
    {
        var north = drone.LocalCoords().x;
        var east = drone.LocalCoords().y;
        var down = drone.LocalCoords().z;
        var msg = new Msg_local_position_ned
        {
            x = north,
            y = east,
            z = down,
            vx = (float)drone.NorthVelocity(),
            vy = (float)drone.EastVelocity(),
            vz = (float)drone.VerticalVelocity()
        };

        var serializedPacket = mav.SendV2(msg);
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
    }

    /// <summary>
    ///  http://mavlink.org/messages/common#HEARTBEAT
    /// <summary>
    List<byte[]> Heartbeat()
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

        var msg = new Msg_heartbeat
        {
            type = 1,
            autopilot = 1,
            system_status = 1,
            base_mode = base_mode,
            custom_mode = custom_mode,
            mavlink_version = 3
        };

        var serializedPacket = mav.SendV2(msg);
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
    }

    /// <summary>
    ///  http://mavlink.org/messages/common#HOME_POSITION
    /// <summary>
    List<byte[]> HomePosition()
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
        var msgs = new List<byte[]>();
        msgs.Add(serializedPacket);
        return msgs;
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
            case "MavLink.Msg_set_attitude_target":
                MsgSetAttitudeTarget(packet);
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

    void MsgSetAttitudeTarget(MavlinkPacket pack)
    {
        var msg = (MavLink.Msg_set_attitude_target)pack.Message;
        var rollRate = msg.body_roll_rate;
        var pitchRate = msg.body_pitch_rate;
        var yawRate = msg.body_yaw_rate;
        var thrust = msg.thrust;

        Debug.Log(string.Format("thrust = {0}, pitch rate = {1}, yaw rate = {2}, roll rate = {3}", thrust, pitchRate, yawRate, rollRate));
        drone.SetAttitudeRate(pitchRate, yawRate, rollRate, thrust);
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


    // handle the SET_POSITION_TARGET_LOCAL_NED message
    // used for:
    //      - takeoff (to a given altitude)
    //      - landing
    //      - position control (goto)
    //      - velocity control
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


    /// <summary>
    /// TODO: Potentially keep track of when last heartbeat was received and
    /// do something.
    /// </summary>
    void MsgHeartbeat(MavlinkPacket pack)
    {
        // var msg = (MavLink.Msg_heartbeat) pack.Message;
    }
}