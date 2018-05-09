using DroneInterface;
using DroneControllers;
using DroneVehicles;
using DroneSensors;
using UnityEngine;

namespace Drones
{
    /// <summary>
    /// Drone based off a Quadrotor.
    /// </summary>
    class PlaneDrone : MonoBehaviour, IDrone
    {
        public PlaneVehicle planeVehicle;
        public PlaneAutopilot planeAutopilot;
        public PlaneSensors planeSensors;

        void Awake()
        {
            planeVehicle = GetComponent<PlaneVehicle>();
            planeAutopilot = GetComponent<PlaneAutopilot>();
            planeSensors = GetComponent<PlaneSensors>();
            Simulation.ActiveDrone = this;
        }

        //IDroneVehicle Methods

        public Vector3 CoordsUnity()
        {
            return planeVehicle.CoordsUnity();
        }

        /// <summary>
        /// The local coordinates relative to the Unity map in a NED frame
        /// </summary>
        public Vector3 CoordsLocal()
        {
            return planeVehicle.CoordsLocal();
        }

        /// <summary>
        /// Vehicle attitude (roll, pitch, yaw) in radians (RH 3-2-1 transform from world to body)
        /// </summary>
        /// <returns></returns>
        public Vector3 AttitudeEuler()
        {
            return planeVehicle.AttitudeEuler();
        }

        /// <summary>
        /// Vehicle attitude in quaternions (RH from world to body)
        /// </summary>
        /// <returns></returns>
        public Vector4 AttitudeQuaternion()
        {
            return planeVehicle.AttitudeQuaternion();
        }

        /// <summary>
        /// The vehicle NED linear velocity in m/s
        /// </summary>
        public Vector3 VelocityLocal()
        {
            return planeVehicle.VelocityLocal();
        }

        /// <summary>
        /// The linear velocity in the vehicle frame (front, right, down) in m/s
        /// </summary>
        /// <returns></returns>
        public Vector3 VelocityBody()
        {
            return planeVehicle.VelocityBody();
        }

        /// <summary>
        /// The vehicle NED linear acceleration in m/s^2
        /// </summary>
        public Vector3 AccelerationLocal()
        {
            return planeVehicle.AccelerationLocal();
        }

        /// <summary>
        /// The linear acceleration in the vehicle frame (front, right, down) in m/s^2
        /// </summary>
        public Vector3 AccelerationBody()
        {
            return planeVehicle.AccelerationBody();
        }

        /// <summary>
        /// The angular velocity around the vehicle frame axes (front, right, down) in rad/s
        /// </summary>
        /// <returns></returns>
        public Vector3 AngularRatesBody()
        {
            return planeVehicle.AngularRatesBody();
        }

        /// <summary>
        /// The current body frame control moments being applied to the vehicle in kg*m^2/s^2
        /// </summary>
        public Vector3 MomentBody()
        {
            return planeVehicle.MomentBody();
        }

        /// <summary>
        /// The current body frame control forces being applied to the vehicle in kg*m/s^2
        /// </summary>
        /// <returns></returns>
        public Vector3 ForceBody()
        {
            return planeVehicle.ForceBody();
        }

        /// <summary>
        /// The state of the motors
        /// </summary>
        /// <returns></returns>
        public bool MotorsArmed()
        {
            return planeVehicle.MotorsArmed();
        }

        /// <summary>
        /// Arms/disarms the vehicle motors
        /// </summary>
        /// <param name="arm">true=arm, false=disarm</param>
        public void ArmDisarm(bool arm)
        {
            planeAutopilot.ArmDisarm(arm);
        }

        /// <summary>
		/// Place the drone at a specific world position
		/// </summary>
		public void Place(Vector3 location)
        {
            planeVehicle.Place(location);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">Coordinate in Unity frame</param>
        /// <param name="velocity">Velocity in Unity frame</param>
        /// <param name="euler">rotation in Unity (LH) frame</param>
        public void InitializeVehicle(Vector3 location, Vector3 velocity, Vector3 euler)
        {
            planeVehicle.InitializeVehicle(location, velocity, euler);
        }


        /// IDroneController Methods

        /// <summary>
        /// Returns true if the vehicle is being controlled from outside the simulator
        /// </summary>
        public bool Guided()
        {
            return planeAutopilot.Guided();
        }

        /// <summary>
        /// Enables/disables offboard control
        /// </summary>
        /// <param name="offboard">true=enable offboard, false=disable offboard</param>
        public void SetGuided(bool offboard)
        {
            planeAutopilot.SetGuided(offboard);
        }

        /// <summary>
        /// Command the vehicle to hover at the current position and altitude
        /// </summary>
        public void CommandHover()
        {
            planeAutopilot.CommandHover();
        }

        /// <summary>
        /// Command the vehicle to the altitude, the position/attitude target does nto change
        /// </summary>
        /// <param name="altitude">Altitude in m</param>
        public void CommandAltitude(float altitude)
        {
            planeAutopilot.CommandAltitude(altitude);
        }

        /// <summary>
        /// Command the vehicle position. If in Offboard, changes the vehicle control to PositionControl
        /// </summary>
        /// <param name="localPosition">Target local NED position</param>
        public void CommandPosition(Vector3 localPosition)
        {
            planeAutopilot.CommandPosition(localPosition);
        }

        /// <summary>
        /// If in PositionControl or VelocityControl mode, command the vehicle heading to the specified
        /// </summary>
        /// <param name="heading">Target vehicle heading in radians</param>
        public void CommandHeading(float heading)
        {
            planeAutopilot.CommandHeading(heading);
        }

        /// <summary>
        /// Command the vehicle local velocity. If in Offboard, changes the vehicle control VelocityControl
        /// </summary>
        /// <param name="localVelocity">Target local NED velocity in m/s</param>
        public void CommandVelocity(Vector3 localVelocity)
        {
            planeAutopilot.CommandVelocity(localVelocity);
        }

        /// <summary>
        /// Command the vehicle's attitude and thrust
        /// </summary>
        /// <param name="attitude">Euler angles (roll, pitch, yaw) in radians (RH 3-2-1 from world to body)</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        public void CommandAttitude(Vector3 attitude, float thrust)
        {
            planeAutopilot.CommandAttitude(attitude, thrust);
        }

        /// <summary>
        /// Command the vehicle's body rates and thrust
        /// </summary>
        /// <param name="bodyrates">Body frame angular rates (p, q, r) in radians/s</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        public void CommandAttitudeRate(Vector3 bodyrates, float thrust)
        {
            planeAutopilot.CommandAttitude(bodyrates, thrust);
        }


        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        /// <param name="bodyMoment">Body frame moments in kg*m^2/s^2</param>
        /// <param name="thrust"></param>
        public void CommandMoment(Vector3 bodyMoment, float thrust)
        {
            planeAutopilot.CommandMoment(bodyMoment, thrust);
        }

        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        public void CommandControls(float controlX, float controlY, float controlZ, float controlW)
        {
            planeAutopilot.CommandControls(controlX, controlY, controlZ, controlW);
        }

        /// <summary>
        /// Sets the value of the position target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalPositionTarget(Vector3 v)
        {
            planeAutopilot.LocalPositionTarget(v);
        }

        /// <summary>
        /// Sets the value of the velocity target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalVelocityTarget(Vector3 v)
        {
            planeAutopilot.LocalVelocityTarget(v);
        }

        /// <summary>
        /// Sets the value of the acceleration target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalAccelerationTarget(Vector3 v)
        {
            planeAutopilot.LocalAccelerationTarget(v);
        }

        /// <summary>
        /// Sets the value of the attitude target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void AttitudeTarget(Vector3 v)
        {
            planeAutopilot.AttitudeTarget(v);
        }

        /// <summary>
        /// Sets the value of the body rate target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void BodyRateTarget(Vector3 v)
        {
            planeAutopilot.BodyRateTarget(v);
        }


        /// IDroneSensors Methods

        /// <summary>
        /// The body angular rate measurements from the gyro in radians/s
        /// </summary>
        /// <returns></returns>
        public Vector3 GyroRates()
        {
            return planeSensors.GyroRates();
        }

        /// <summary>
        /// The linear acceleration measurements from the IMU in m/s^2
        /// </summary>
        public Vector3 IMUAcceleration()
        {
            return planeSensors.IMUAcceleration();
        }

        /// <summary>
        /// The compass heading in radians
        /// </summary>
        /// <returns></returns>
        public float CompassHeading()
        {
            return planeSensors.CompassHeading();
        }

        /// <summary>
        /// The body 3-axis magnetometer measurement in Gauss.
        /// </summary>
        /// <returns></returns>
        public Vector3 CompassMagnetometer()
        {
            return planeSensors.CompassMagnetometer();
        }

        /// <summary>
        /// The barometeric pressure altitude in m (positive up)
        /// </summary>
        /// <returns></returns>
        public float BarometerAltitude()
        {
            return planeSensors.BarometerAltitude();
        }

        /// <summary>
        /// The vehicle's attitude estimated from the compass, IMU and gyro
        /// </summary>
        /// <returns></returns>
        public Vector3 AttitudeEstimate()
        {
            return planeSensors.AttitudeEstimate();
        }

        /// <summary>
        /// The vehicle latitude in degrees
        /// </summary>
        /// <returns></returns>
        public double GPSLatitude()
        {
            return planeSensors.GPSLatitude();
        }

        /// <summary>
        /// The vehicle longitude in degrees
        /// </summary>
        /// <returns></returns>
        public double GPSLongitude()
        {
            return planeSensors.GPSLongitude();
        }

        /// <summary>
        /// The vehicle altitude in m, relative to sea level (positive up)
        /// </summary>
        /// <returns></returns>
        public double GPSAltitude()
        {
            return planeSensors.GPSAltitude();
        }

        /// <summary>
        /// The home latitude in degrees, used to calculate a local position
        /// </summary>
        /// <returns></returns>
        public double HomeLatitude()
        {
            return planeSensors.HomeLatitude();
        }

        /// <summary>
        /// The home longitude in degrees, used to calculate a local position
        /// </summary>
        /// <returns></returns>
        public double HomeLongitude()
        {
            return planeSensors.HomeLongitude();
        }

        /// <summary>
        /// The home altitude in m, from sea level  (positive up)
        /// </summary>
        /// <returns></returns>
        public double HomeAltitude()
        {
            return planeSensors.HomeAltitude();
        }

        /// <summary>
        /// Local NED position in m, relative to the home position
        /// </summary>
        /// <returns></returns>
        public Vector3 LocalPosition()
        {
            return planeSensors.LocalPosition();
        }

        /// <summary>
        /// Local NED velocity in m/s
        /// </summary>
        public Vector3 GPSVelocity()
        {
            return planeSensors.GPSVelocity();
        }

        /// <summary>
        /// Sets the home position to the current GPS position
        /// </summary>
        public void SetHomePosition()
        {
            planeSensors.SetHomePosition();
        }

        /// <summary>
        /// Sets the home position used in the local position calculation
        /// </summary>
        /// <param name="longitude">longitude in degrees</param>
        /// <param name="latitude">latitude</param>
        /// <param name="altitude">altitude in m, relative to seal level</param>
        public void SetHomePosition(double longitude, double latitude, double altitude)
        {
            planeSensors.SetHomePosition(longitude, latitude, altitude);
        }
        
        public void SetControlMode(int controlMode)
        {
            planeAutopilot.SetControlMode(controlMode);
        }

        /// <summary>
        /// Returns an integer corresponding to the mode of control
        /// </summary>
        /// <returns></returns>
        public int ControlMode()
        {
            return planeAutopilot.ControlMode();
        }
        /*
        public QuadController quadCtrl;
        public SimpleQuadController simpleQuadCtrl;

        void Awake()
        {
            quadCtrl = GetComponent<QuadController>();
            simpleQuadCtrl = GetComponent<SimpleQuadController>();
			Simulation.ActiveDrone = this;
        }

        public Vector3 Forward { get { return quadCtrl.Forward; } }

        /// <summary>
        /// Returns coordinates in EUN frame
        /// </summary>
        public Vector3 UnityCoords()
        {
			return quadCtrl.Position;
//            return this.transform.position;
        }

        /// <summary>
        /// Returns coordinates in NED frame
        /// </summary>
        public Vector3 LocalCoords()
        {
            return new Vector3(quadCtrl.GetLocalNorth(), quadCtrl.GetLocalEast(), -1.0f * (float)quadCtrl.GetAltitude());
        }

        public double Altitude()
        {
            return quadCtrl.GetAltitude();
        }

        public void Arm(bool arm)
        {
            if (arm == true)
            {
                quadCtrl.TriggerReset();
                simpleQuadCtrl.ArmVehicle();
            }
            else
            {
                simpleQuadCtrl.DisarmVehicle();
            }
        }

        public bool Armed()
        {
            return simpleQuadCtrl.armed;
        }

        public double EastVelocity()
        {
            return quadCtrl.GetEastVelocity();
        }

        public bool ExecutingCommand()
        {
            throw new System.NotImplementedException();
        }

        public void Goto(double north, double east, double altitude)
        {
            simpleQuadCtrl.CommandLocal((float)north, (float)east, (float)-altitude);
        }

        public void SetHome(double longitude, double latitude, double altitude)
        {
            quadCtrl.SetHomePosition(longitude, latitude, altitude);
        }

        public bool Guided()
        {
            return simpleQuadCtrl.guided;
        }

        public void Hover(double altitude)
        {
            var lat = this.Latitude();
            var lon = this.Longitude();
            this.Goto(lat, lon, altitude);
        }

        public double Latitude()
        {
            return quadCtrl.GetLatitude();
        }

        public double Longitude()
        {
            return quadCtrl.GetLongitude();
        }

        public double HomeLatitude()
        {
            return quadCtrl.GetHomeLatitude();
        }

        public double HomeLongitude()
        {
            return quadCtrl.GetHomeLongitude();
        }


        public double NorthVelocity()
        {
            return quadCtrl.GetNorthVelocity();
        }

        public double Pitch()
        {
            return quadCtrl.GetPitch();
        }

        public double Roll()
        {
            return quadCtrl.GetRoll();
        }

        public void SetHeading(double heading)
        {
            simpleQuadCtrl.CommandHeading((float)heading);
        }

        public void SetAttitude(double pitch, double yawrate, double roll, double thrust)
        {
            simpleQuadCtrl.CommandAttitude((float)roll, (float)pitch, (float)yawrate, (float)thrust);
            //simpleQuadCtrl.CommandHeading((float)yaw);
        }

        public void SetAttitudeRate(double pitchRate, double yawRate, double rollRate, double thrust)
        {
            //simpleQuadCtrl.currentMovementBehavior.RemoteUpdate((float)thrust, (float)pitchRate, (float)yawRate, (float)rollRate);
            throw new System.NotImplementedException();
        }

        public void SetMotors(float throttle, float pitchMoment, float yawMoment, float rollMoment)
        {
            simpleQuadCtrl.CommandMotors(rollMoment, pitchMoment, yawMoment, throttle);
        }

        public void SetVelocity(double vx, double vy, double vz, double heading)
        {
            throw new System.NotImplementedException();
        }

        public void TakeControl(bool guided)
        {
            simpleQuadCtrl.SetGuidedMode(guided);
        }

        public double DownVelocity()
        {
            return quadCtrl.GetDownVelocity();
        }

        public Vector3 LocalVelocity()
        {
            return new Vector3(quadCtrl.GetNorthVelocity(), quadCtrl.GetEastVelocity(), quadCtrl.GetDownVelocity());
        }

        public double VerticalVelocity()
        {
            return quadCtrl.GetVerticalVelocity();
        }

        public double Yaw()
        {
            return quadCtrl.GetYaw();
        }

        public Vector3 EulerAngles()
        {
            return new Vector3(quadCtrl.GetRoll(), quadCtrl.GetPitch(), quadCtrl.GetYaw());
        }

        public Vector4 QuaternionAttitude()
        {
            return quadCtrl.QuaternionAttitude();
        }

        public Vector3 AngularVelocity()
        {
            return new Vector3(quadCtrl.GetRollrate(),
                quadCtrl.GetPitchrate(),
                quadCtrl.GetYawrate());
        }

        public double Rollrate()
        {
            return quadCtrl.GetRollrate();
        }

        public double Pitchrate()
        {
            return quadCtrl.GetPitchrate();
        }

        public double Yawrate()
        {
            return quadCtrl.GetYawrate();
        }

        public Vector3 AngularAcceleration()
        {
            return quadCtrl.AngularAccelerationBody;
        }

        public Vector3 LinearAcceleration()
        {
            return new Vector3(this.quadCtrl.GetFrontAcceleration(),
                this.quadCtrl.GetRightAcceleration(),
                this.quadCtrl.GetBottomAcceleration());
        }

        public void ControlRemotely(bool remote)
        {
            this.simpleQuadCtrl.remote = remote;
        }

        public bool ControlledRemotely()
        {
            return this.simpleQuadCtrl.remote;
        }

		public void Place (Vector3 location)
		{
			quadCtrl.SetPositionAndOrientation ( location, Quaternion.identity );
		}

        public void LocalPositionTarget(Vector3 pos)
        {
            simpleQuadCtrl.positionTarget = pos;
        }

        public void LocalVelocityTarget(Vector3 vel)
        {
            simpleQuadCtrl.velocityTarget = vel;
        }

        public void LocalAccelerationTarget(Vector3 acc)
        {
            simpleQuadCtrl.accelerationTarget = acc;
        }

        public void AttitudeTarget(Vector3 att)
        {
            simpleQuadCtrl.attitudeTarget = att;
        }

        public void BodyRateTarget(Vector3 br)
        {
            simpleQuadCtrl.bodyRateTarget = br;
        }
        */
    }

}