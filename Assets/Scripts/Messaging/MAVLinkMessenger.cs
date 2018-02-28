using System.Collections.Generic;
using System;
using Drones;
using DroneInterface;
using MavLink;
using UnityEngine;
using UdacityNetworking;
using FlightUtils;

// TODO: Is an Interface a reference or value type?
namespace Messaging
{
    public class MAVLinkMessenger
    {
        private double prev_time = 0.0;
        private double total_commands = 0.0;

        DateTime startTime;

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


        /// <summary>
        /// enum for the set of masks that are used for setting the location position target
        /// </summary>
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

        public enum SET_ATTITUDE_MASK : byte
        {
            IGNORE_ATTITUDE = (byte)128,
            IGNORE_RATES = (byte)7,
        }

        public Mavlink mav { get; private set; }
        private IDrone drone;

        public MAVLinkMessenger()
        {
            startTime = DateTime.Now;
            mav = new Mavlink();
            drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
            // setup event listeners
            mav.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);
            mav.PacketFailedCRC += new PacketCRCFailEventHandler(OnPacketFailure);
        }

        public void ParseMessageInfo(MessageInfo msgInfo)
        {
            mav.ParseBytesV2(msgInfo.message);
        }

        /// <summary>
        /// </summary>
        uint TimeSinceSystemStart()
        {
            var now = DateTime.Now;
            var span = now - startTime;
            return (uint)span.TotalMilliseconds;
        }

        ///
        /// Senders
        ///

        /// <summary>
        /// http://mavlink.org/messages/common#SCALED_PRESSURE
        /// </summary>
        public List<byte[]> ScaledPressure()
        {
            var msg = new Msg_scaled_pressure
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
        public List<byte[]> RawIMU()
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
        public List<byte[]> GlobalPosition()
        {
            var lat = drone.Latitude() * 1e7d;
            var lon = drone.Longitude() * 1e7d;
            var alt = drone.Altitude() * 1000;
            var vx = drone.NorthVelocity() * 100;
            var vy = drone.EastVelocity() * 100;
            var vz = drone.DownVelocity() * 100;
            var hdg = (drone.Yaw() * Mathf.Rad2Deg) * 100;
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
        /// http://mavlink.org/messages/common#ATTITUDE_QUATERNION
        /// <summary>
        public List<byte[]> AttitudeQuaternion()
        {

            var rollrate = (float)drone.Rollrate();
            var pitchrate = (float)drone.Pitchrate();
            var yawrate = (float)drone.Yawrate();
            var pitch = (float)drone.Pitch();
            var yaw = (float)drone.Yaw();
            var roll = (float)drone.Roll();
            var q = new Vector3(roll, pitch, yaw).ToRHQuaternion();
            var msg = new Msg_attitude_quaternion
            {
                q1 = q.w,
                q2 = q.x,
                q3 = q.y,
                q4 = q.z,
                rollspeed = rollrate,
                pitchspeed = pitchrate,
                yawspeed = yawrate,
                time_boot_ms = TimeSinceSystemStart(),
            };
            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        /// <summary>
        ///  http://mavlink.org/messages/common#LOCAL_POSITION_NED
        /// <summary>
        public List<byte[]> LocalPositionNED()
        {
            var north = drone.LocalCoords().x;
            var east = drone.LocalCoords().y;
            var down = drone.LocalCoords().z;
            // Debug.Log(drone.LocalCoords());
            var msg = new Msg_local_position_ned
            {
                x = north,
                y = east,
                z = down,
                vx = (float)drone.NorthVelocity(),
                vy = (float)drone.EastVelocity(),
                vz = (float)drone.DownVelocity(),
                time_boot_ms = TimeSinceSystemStart()
            };

            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        /// <summary>
        ///  http://mavlink.org/messages/common#HEARTBEAT
        /// <summary>
        public List<byte[]> Heartbeat()
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
                mavlink_version = 3,

            };

            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        /// <summary>
        ///  http://mavlink.org/messages/common#HOME_POSITION
        /// <summary>
        public List<byte[]> HomePosition()
        {
            // TODO: figure out where these are saved for the drone
            var home_lat = drone.HomeLatitude() * 1e7;
            var home_lon = drone.HomeLongitude() * 1e7;
            var home_alt = 0 * 1000;
            // Debug.Log(string.Format("Home pos - lat {0}, lon {1}", drone.HomeLatitude(), drone.HomeLongitude()));

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
                approach_z = 0,
                time_usec = TimeSinceSystemStart()
            };
            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        /// <summary>
        /// http://mavlink.org/messages/common/#HIL_SENSOR
        /// </summary>
        public List<byte[]> HILSensor()
        {
            var gyro = drone.AngularVelocity();
            var pitch = (float)drone.Pitch();
            var yaw = (float)drone.Yaw();
            var roll = (float)drone.Roll();
            var q = Quaternion.Euler(pitch, yaw, roll);
            var msg = new Msg_hil_sensor
            {
                xacc = (float)0,
                yacc = (float)0,
                zacc = (float)0,
                xgyro = (float)0,
                ygyro = (float)0,
                zgyro = (float)0,
                xmag = (float)0,
                ymag = (float)0,
                zmag = (float)0,
                abs_pressure = (float)0,
                diff_pressure = (float)0,
                pressure_alt = (float)0,
                temperature = (float)0,
                fields_updated = (UInt32)0,
            };
            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        /// <summary>
        /// http://mavlink.org/messages/common/#HIL_GPS
        /// </summary>
        public List<byte[]> HILGPS()
        {
            var gyro = drone.AngularVelocity();
            var pitch = (float)drone.Pitch();
            var yaw = (float)drone.Yaw();
            var roll = (float)drone.Roll();
            var q = Quaternion.Euler(pitch, yaw, roll);

            var vx = drone.NorthVelocity();
            var vy = drone.EastVelocity();
            var vz = drone.VerticalVelocity();

            var acc = drone.LinearAcceleration().EUNToNED();
            // Latitude and longitude defined in WGS84
            // https://en.wikipedia.org/wiki/World_Geodetic_System
            var lat = drone.Latitude() * 1e7d;
            var lon = drone.Longitude() * 1e7d;
            // NOTE: Altitude needs to be AMSL (above mean sea level, positive)
            var alt = drone.Altitude() * 1000;
            if (alt < 0)
            {
                alt = -alt;
            }

            var msg = new Msg_hil_gps
            {
                fix_type = (byte)0,
                lat = (Int32)lat,
                lon = (Int32)lon,
                alt = (Int32)alt,
                eph = (ushort)0,
                epv = (ushort)0,
                vel = (ushort)0,
                vn = (short)drone.NorthVelocity(),
                ve = (short)drone.EastVelocity(),
                vd = (short)drone.VerticalVelocity(),
                cog = (ushort)0,
                // Set to 255 if unknown
                satellites_visible = (byte)255,
            };
            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        /// <summary>
        /// http://mavlink.org/messages/common/#HIL_STATE_QUATERNION
        /// </summary>
        public List<byte[]> HILStateQuaternion()
        {
            var gyro = drone.AngularVelocity();
            var pitch = (float)drone.Pitch();
            var yaw = (float)drone.Yaw();
            var roll = (float)drone.Roll();
            var q = Quaternion.Euler(pitch, yaw, roll);

            var vx = drone.NorthVelocity();
            var vy = drone.EastVelocity();
            var vz = drone.VerticalVelocity();

            var acc = drone.LinearAcceleration().EUNToNED();
            var lat = drone.Latitude() * 1e7d;
            var lon = drone.Longitude() * 1e7d;
            var alt = drone.Altitude() * 1000;

            var msg = new Msg_hil_state_quaternion
            {
                // EUN to NED frame
                attitude_quaternion = new float[4] { q.w, q.z, q.x, q.y },

                rollspeed = (float)roll,
                pitchspeed = (float)pitch,
                yawspeed = (float)yaw,

                lat = (Int32)lat,
                lon = (Int32)lon,
                alt = (Int32)alt,

                // TODO: make these cm/s
                vx = (short)vx,
                vy = (short)vy,
                vz = (short)vz,

                // TODO: make these cm/s
                ind_airspeed = (ushort)0,
                true_airspeed = (ushort)0,

                xacc = (short)acc.x,
                yacc = (short)acc.y,
                zacc = (short)acc.z,
            };
            var serializedPacket = mav.SendV2(msg);
            var msgs = new List<byte[]>();
            msgs.Add(serializedPacket);
            return msgs;
        }

        ///
        /// Receiver methods
        ///

        void OnPacketReceived(object sender, MavlinkPacket packet)
        {
            //Debug.Log(string.Format("Received packet, message type = {0}", packet.Message));
            var msgstr = packet.Message.ToString();
            switch (msgstr)
            {
                case "MavLink.Msg_heartbeat":
                    MsgHeartbeat((Msg_heartbeat)packet.Message);
                    break;
                case "MavLink.Msg_command_long":
                    MsgCommandLong((Msg_command_long)packet.Message);
                    break;
                case "MavLink.Msg_set_position_target_local_ned":
                    MsgSetLocalPositionTarget((Msg_set_position_target_local_ned)packet.Message);
                    break;
                case "MavLink.Msg_set_attitude_target":
                    MsgSetAttitudeTarget((Msg_set_attitude_target)packet.Message);
                    break;
                case "MavLink.Msg_position_target_local_ned":
                    MsgLocalPositionTarget((Msg_position_target_local_ned)packet.Message);
                    break;
                case "MavLink.Msg_attitude_target":
                    MsgAttitudeTarget((Msg_attitude_target)packet.Message);
                    break;
                default:
                    Debug.Log("Unknown message type !!!: " + msgstr);
                    break;
            }
        }

        void OnPacketFailure(object sender, PacketCRCFailEventArgs args)
        {
            Debug.Log("failed to receive a packet!!!");
        }

        /// <summary>
        /// </summary>
        void MsgSetAttitudeTarget(Msg_set_attitude_target msg)
        {
            double curr_time = TimeSinceSystemStart() / 1000.0f;
            total_commands = total_commands + 1.0f;
            if (curr_time - prev_time > 2.0)
            {
                Debug.Log("Message Freq: " + total_commands / (curr_time - prev_time));
                prev_time = curr_time;
                total_commands = 0.0f;
            }
            bool attitudeCmd;
            if ((msg.type_mask & (byte) SET_ATTITUDE_MASK.IGNORE_ATTITUDE) == 0)
            {
                attitudeCmd = true;
            }
            else
            {
                attitudeCmd = false;
            }

            // Debug.Log(string.Format("ATTIDUE CMD {0}", attitudeCmd));
            if (attitudeCmd)
            {
                var yawrate = msg.body_yaw_rate;
                var thrust = msg.thrust;
                Vector4 attitudeQ;
                attitudeQ.w = msg.q[0];
                attitudeQ.x = msg.q[1];
                attitudeQ.y = msg.q[2];
                attitudeQ.z = msg.q[3];
                Vector3 attitudeEuler = attitudeQ.ToRHEuler();
                //drone.SetAttitudeRate(pitchRate, yawRate, rollRate, thrust);
                Debug.Log("Thrust Set:" + thrust);
                drone.SetAttitude(attitudeEuler.y, yawrate, attitudeEuler.x, thrust);
            }
            else
            {
                // Debug.Log(string.Format("thrust {0}, pitch rate {1}, yaw rate {2}, roll rate {3}", msg.thrust, msg.body_pitch_rate, msg.body_yaw_rate, msg.body_roll_rate));
                drone.SetMotors(msg.thrust, msg.body_pitch_rate, msg.body_yaw_rate, msg.body_roll_rate);
            }
        }

        /// <summary>
        /// handle the COMMAND_LONG message
        /// used for:
        ///      - arming / disarming
        ///      - setting the mode
        ///      - setting the home position
        /// </summary>
        void MsgCommandLong(Msg_command_long msg)
        {
            var command = (MAV_CMD)msg.command;

            // DEBUG
            Debug.Log(string.Format("Command = {0}", command));

            // handle the command types of interest
            if (command == MAV_CMD.MAV_CMD_COMPONENT_ARM_DISARM)
            {
                var param1 = msg.param1;
                if (param1 == 1.0)
                {
                    drone.Arm(true);
                    Debug.Log("ARMED VEHICLE !!!");
                }
                else
                {
                    drone.Arm(false);
                    Debug.Log("DISARMED VEHICLE !!!");
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
                        Debug.Log("VEHICLE IS BEING GUIDED !!!");
                    }
                    else
                    {
                        drone.TakeControl(false);
                        Debug.Log("VEHICLE IS NOT BEING GUIDED !!!");
                    }
                }
            }
            else if (command == MAV_CMD.MAV_CMD_DO_SET_HOME)
            {
                drone.SetHome(msg.param6, msg.param5, msg.param7);
                Debug.Log("HOME POSITION PARAMS: " + msg.param1 + ", " + msg.param2 + ", " + msg.param3 + ", " + msg.param4 + ", " + msg.param5 + ", " + msg.param6 + ", " + msg.param7);
                Debug.Log("Vehicle Home Position: " + msg.param6 + ", " + msg.param5 + ", " + msg.param7);
            }
            else
            {
                Debug.Log(string.Format("Unknown MAVLink Command: {0}", command));
            }
        }


        /// <summary>
        /// handle the SET_POSITION_TARGET_LOCAL_NED message
        /// used for:
        ///      - takeoff (to a given altitude)
        ///      - landing
        ///      - position control (goto)
        ///      - velocity control
        /// </summary>
        void MsgSetLocalPositionTarget(Msg_set_position_target_local_ned msg)
        {
            var mask = (UInt16)msg.type_mask;

            // split by the mask

            // TAKEOFF
            if ((mask & (UInt16)SET_POSITION_MASK.IS_TAKEOFF) > 0)
            {
                // TODO: z is being sent as negative, check to see if a sign change needs to occur
                //drone.Goto(drone.Latitude(), drone.Longitude(), msg.z);
                drone.Goto(drone.LocalCoords().x, drone.LocalCoords().y, msg.z);
                Debug.Log(string.Format("TAKING OFF to {0} altitude", msg.z));
            }
            // LAND
            else if ((mask & (UInt16)SET_POSITION_MASK.IS_LAND) > 0)
            {
                // TODO: z is being sent as 0 here, make sure that is ok
                drone.Goto(drone.LocalCoords().x, drone.LocalCoords().y, msg.z);
                Debug.Log("LANDING !!!");
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
                    Debug.Log("Vehicle Command: (" + north + "," + east + "," + alt + ")");
                    drone.Goto(north, east, alt);
                }
                else if ((mask & (UInt16)SET_POSITION_MASK.IGNORE_VELOCITY) == 0)
                {
                    // TODO: set velocity is not yet implemented
                    Debug.Log("vehicle velocity command: " + msg.vx + ", " + msg.vy + ", " + msg.vz);
                    //drone.SetVelocity(msg.vx, msg.vy, msg.vz, msg.yaw);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void MsgLocalPositionTarget(Msg_position_target_local_ned msg)
        {
            var mask = (UInt16)msg.type_mask;

            // split by the mask
            // POSITION COMMAND
            if ((mask & (UInt16)SET_POSITION_MASK.IGNORE_POSITION) == 0)
            {
                drone.LocalPositionTarget(new Vector3(msg.x, msg.y, msg.z));
                // TODO: Set the target variables
                //Debug.Log("Position target (t= " + msg.time_boot_ms + "): " + msg.x + ", " + msg.y + ", " + msg.z);

            }
            else if ((mask & (UInt16)SET_POSITION_MASK.IGNORE_VELOCITY) == 0)
            {
                drone.LocalVelocityTarget(new Vector3(msg.vx, msg.vy, msg.vz));
                // TODO: set the target variable
                //Debug.Log("Velocity target (t= " + msg.time_boot_ms + "): " + msg.vx + ", " + msg.vy + ", " + msg.vz);
            }
            else if ((mask & (UInt16)SET_POSITION_MASK.IGNORE_ACCELERATION) == 0)
            {
                drone.LocalAccelerationTarget(new Vector3(msg.afx, msg.afy, msg.afz));
                //Debug.Log("Acceleration target (t= " + msg.time_boot_ms + "): " + msg.afx + ", " + msg.afy + ", " + msg.afz);
            }
            
        }

        void MsgAttitudeTarget(Msg_attitude_target msg)
        {
            var mask = (byte)msg.type_mask;

            // split by the mask

            if ((mask & (byte)SET_ATTITUDE_MASK.IGNORE_ATTITUDE) == 0)
            {
                // TODO: Set the target variables
                Vector4 attitudeQ;
                attitudeQ.w = msg.q[0];
                attitudeQ.x = msg.q[1];
                attitudeQ.y = msg.q[2];
                attitudeQ.z = msg.q[3];
                Vector3 attitudeEuler = attitudeQ.ToRHEuler();
                drone.AttitudeTarget(attitudeEuler);
                //Debug.Log("Attitude target (t= " + msg.time_boot_ms + "): " + attitudeEuler.x + ", " + attitudeEuler.y + ", " + attitudeEuler.z);
            }
            else if ((mask & (byte)SET_ATTITUDE_MASK.IGNORE_RATES) == 0)
            {
                // TODO: set the target variable
                //Debug.Log("Body Rate target (t= " + msg.time_boot_ms + "): " + msg.body_roll_rate + ", " + msg.body_pitch_rate + ", " + msg.body_yaw_rate);
                drone.BodyRateTarget(new Vector3(msg.body_roll_rate, msg.body_pitch_rate, msg.body_yaw_rate));
            }
            else
            {
                Debug.Log("Mask: " + mask);
            }

        }

        /// <summary>
        /// TODO: Potentially keep track of when last heartbeat was received and
        /// do something.
        /// </summary>
        void MsgHeartbeat(Msg_heartbeat msg)
        {
        }


    }
}