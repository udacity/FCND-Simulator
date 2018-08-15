using DroneInterface;
using DroneControllers;
using DroneVehicles;
using DroneSensors;
using UnityEngine;
using System;

namespace Drones
{
    /// <summary>
    /// Drone based off a Quadrotor.
    /// </summary>
    class QuadDrone : MonoBehaviour, IDrone
    {

        public QuadVehicle Vehicle;
        public QuadAutopilot Autopilot;
        public QuadSensors Sensors;

        public IControlLaw control { get { return Autopilot.control; } }
        public Vector3 AttitudeTarget { get { return Autopilot.AttitudeTarget; } set { Autopilot.AttitudeTarget = value; } } //roll, pitch, yaw target in radians
        public Vector3 PositionTarget { get { return Autopilot.PositionTarget; } set { Autopilot.PositionTarget = value; } }//north, east, down target in meters
        public Vector3 BodyRateTarget { get { return Autopilot.BodyRateTarget; } set { Autopilot.BodyRateTarget = value; } } //p, q, r target in radians/second
        public Vector3 VelocityTarget { get { return Autopilot.VelocityTarget; } set { Autopilot.VelocityTarget = value; } } //north, east, down, velocity targets in meters/second
        public Vector3 AccelerationTarget { get { return Autopilot.AccelerationTarget; } set { Autopilot.AccelerationTarget = value; } } //north, east, down acceleration targets in meters/second^2
        public Vector4 MomentThrustTarget { get { return Autopilot.MomentThrustTarget; } set { Autopilot.MomentThrustTarget = value; } }
        public Vector4 ControlTarget { get { return Autopilot.ControlTarget; } set { Autopilot.ControlTarget = value; } }

        public Vector3 ControlAttitude { get { return Autopilot.ControlAttitude; } }
        public Vector3 ControlPosition { get { return Autopilot.ControlPosition; } }
        public Vector3 ControlBodyRate { get { return Autopilot.ControlBodyRate; } }
        public Vector3 ControlVelocity { get { return Autopilot.ControlVelocity; } }
        public Vector3 ControlAcceleration { get { return Autopilot.ControlAcceleration; } }
        public Vector3 ControlWindData { get { return Autopilot.ControlWindData; } } // Airspeed, AoA, sideslip
        public float ControlMass { get { return Autopilot.ControlMass; } } 

        public int Status
        {
            get { return 0; }
            set {  }
        }

        public float FlightTime()
        {
            return Vehicle.FlightTime();
        }
		public bool Frozen
		{
			get { return Vehicle.Frozen; }
			set { Vehicle.Frozen = value; }
		}

        void Awake()
        {
            Vehicle = GetComponent<QuadVehicle>();
            Autopilot = GetComponent<QuadAutopilot>();
            Sensors = GetComponent<QuadSensors>();
            Simulation.ActiveDrone = this;
        }

        //IDroneVehicle Methods

        public Vector3 CoordsUnity()
        {
            return Vehicle.CoordsUnity();
        }

        /// <summary>
        /// The local coordinates relative to the Unity map in a NED frame
        /// </summary>
        public Vector3 CoordsLocal()
        {
            return Vehicle.CoordsLocal();
        }

        /// <summary>
        /// Vehicle attitude (roll, pitch, yaw) in radians (RH 3-2-1 transform from world to body)
        /// </summary>
        /// <returns></returns>
        public Vector3 AttitudeEuler()
        {
            return Vehicle.AttitudeEuler();
        }

        /// <summary>
        /// Vehicle attitude in quaternions (RH from world to body)
        /// </summary>
        /// <returns></returns>
        public Vector4 AttitudeQuaternion()
        {
            return Vehicle.AttitudeQuaternion();
        }

        /// <summary>
        /// The vehicle NED linear velocity in m/s
        /// </summary>
        public Vector3 VelocityLocal()
        {
            return Vehicle.VelocityLocal();
        }

        /// <summary>
        /// The linear velocity in the vehicle frame (front, right, down) in m/s
        /// </summary>
        /// <returns></returns>
        public Vector3 VelocityBody()
        {
            return Vehicle.VelocityBody();
        }

		/// <summary>
		/// Linear velocity in unity coords
		/// </summary>
		public Vector3 VelocityUnity()
		{
			return Vehicle.VelocityUnity ();
		}

        /// <summary>
        /// The vehicle NED linear acceleration in m/s^2
        /// </summary>
        public Vector3 AccelerationLocal()
        {
            return Vehicle.AccelerationLocal();
        }

        /// <summary>
        /// The linear acceleration in the vehicle frame (front, right, down) in m/s^2
        /// </summary>
        public Vector3 AccelerationBody()
        {
            return Vehicle.AccelerationBody();
        }

        /// <summary>
        /// The angular velocity around the vehicle frame axes (front, right, down) in rad/s
        /// </summary>
        /// <returns></returns>
        public Vector3 AngularRatesBody()
        {
            return Vehicle.AngularRatesBody();
        }

		/// <summary>
		/// Angular velocity in unity coords
		/// </summary>
		public Vector3 AngularRatesUnity()
		{
			return Vehicle.AngularRatesUnity ();
		}

        /// <summary>
        /// The current body frame control moments being applied to the vehicle in kg*m^2/s^2
        /// </summary>
        public Vector3 MomentBody()
        {
            return Vehicle.MomentBody();
        }

        /// <summary>
        /// The current body frame control forces being applied to the vehicle in kg*m/s^2
        /// </summary>
        /// <returns></returns>
        public Vector3 ForceBody()
        {
            return Vehicle.ForceBody();
        }

        /// <summary>
        /// The state of the motors
        /// </summary>
        /// <returns></returns>
        public bool MotorsArmed()
        {
            return Vehicle.MotorsArmed();
        }

        /// <summary>
        /// Arms/disarms the vehicle motors
        /// </summary>
        /// <param name="arm">true=arm, false=disarm</param>
        public void ArmDisarm(bool arm)
        {
            Autopilot.ArmDisarm(arm);
        }

        /// <summary>
		/// Place the drone at a specific world position
		/// </summary>
		public void Place(Vector3 location)
        {
            Vehicle.Place(location);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location">Coordinate in Unity frame</param>
        /// <param name="velocity">Velocity in Unity frame</param>
        /// <param name="euler">rotation in Unity (LH) frame</param>
        public void InitializeVehicle(Vector3 location, Vector3 velocity, Vector3 euler)
        {
            Vehicle.InitializeVehicle(location, velocity, euler);
        }


        /// IDroneController Methods

        /// <summary>
        /// Returns true if the vehicle is being controlled from outside the simulator
        /// </summary>
        public bool Guided()
        {
            return Autopilot.Guided();
        }

        /// <summary>
        /// Enables/disables offboard control
        /// </summary>
        /// <param name="offboard">true=enable offboard, false=disable offboard</param>
        public void SetGuided(bool offboard)
        {
            Autopilot.SetGuided(offboard);
        }

        /// <summary>
        /// Used to enable different modes of control (for example stabilized vs position control)
        /// </summary>
        /// <param name="controlMode"></param>
        public void SetControlMode(int controlMode)
        {
            Autopilot.SetControlMode(controlMode);
        }

        /// <summary>
        /// Returns an integer corresponding to the mode of control
        /// </summary>
        /// <returns></returns>
        public int ControlMode()
        {
            return Autopilot.ControlMode();
        }

        /// <summary>
        /// Command the vehicle to hover at the current position and altitude
        /// </summary>
        public void CommandHover()
        {
            Autopilot.CommandHover();
        }

        /// <summary>
        /// Command the vehicle to the altitude, the position/attitude target does nto change
        /// </summary>
        /// <param name="altitude">Altitude in m</param>
        public void CommandAltitude(float altitude)
        {
            Autopilot.CommandAltitude(altitude);
        }

        /// <summary>
        /// Command the vehicle position. If in Offboard, changes the vehicle control to PositionControl
        /// </summary>
        /// <param name="localPosition">Target local NED position</param>
        public void CommandPosition(Vector3 localPosition)
        {
            Autopilot.CommandPosition(localPosition);
        }

        /// <summary>
        /// If in PositionControl or VelocityControl mode, command the vehicle heading to the specified
        /// </summary>
        /// <param name="heading">Target vehicle heading in radians</param>
        public void CommandHeading(float heading)
        {
            Autopilot.CommandHeading(heading);
        }

        /// <summary>
        /// Command the vehicle local velocity. If in Offboard, changes the vehicle control VelocityControl
        /// </summary>
        /// <param name="localVelocity">Target local NED velocity in m/s</param>
        public void CommandVelocity(Vector3 localVelocity)
        {
            Autopilot.CommandVelocity(localVelocity);
        }

        /// <summary>
        /// Command the vehicle's attitude and thrust
        /// </summary>
        /// <param name="attitude">Euler angles (roll, pitch, yaw) in radians (RH 3-2-1 from world to body)</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        public void CommandAttitude(Vector3 attitude, float thrust)
        {
            Autopilot.CommandAttitude(attitude, thrust);
        }

        /// <summary>
        /// Command the vehicle's body rates and thrust
        /// </summary>
        /// <param name="bodyrates">Body frame angular rates (p, q, r) in radians/s</param>
        /// <param name="thrust">The total commanded thrust from all motors</param>
        public void CommandAttitudeRate(Vector3 bodyrates, float thrust)
        {
            Autopilot.CommandAttitude(bodyrates, thrust);
        }

        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        /// <param name="bodyMoment">Body frame moments in kg*m^2/s^2</param>
        /// <param name="thrust"></param>
        public void CommandMoment(Vector3 bodyMoment, float thrust)
        {
            Autopilot.CommandMoment(bodyMoment, thrust);
        }

        /// <summary>
        /// Command the vehicle's body moment and thrust
        /// </summary>
        public void CommandControls(float controlX, float controlY, float controlZ, float controlW)
        {
            return;
        }

        /// <summary>
        /// Command a vehicle along a vector defined the position and velocity vectors
        /// </summary>
        /// <param name="localPosition">reference local position NED in m</param>
        /// <param name="localVelocity">reference local velocity NED in m/s</param>
        public void CommandVector(Vector3 localPosition, Vector3 localVelocity)
        {
            return;
        }

        /// <summary>
        /// Sets the value of the position target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalPositionTarget(Vector3 v)
        {
            Autopilot.LocalPositionTarget(v);
        }

        /// <summary>
        /// Sets the value of the velocity target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalVelocityTarget(Vector3 v)
        {
            Autopilot.LocalVelocityTarget(v);
        }

        /// <summary>
        /// Sets the value of the acceleration target for visualization in m
        /// Note: Does not command the vehicle
        /// </summary>
        public void LocalAccelerationTarget(Vector3 v)
        {
            Autopilot.LocalAccelerationTarget(v);
        }



        /// IDroneSensors Methods

        /// <summary>
        /// The body angular rate measurements from the gyro in radians/s
        /// </summary>
        /// <returns></returns>
        public Vector3 GyroRates()
        {
            return Sensors.GyroRates();
        }

        /// <summary>
        /// The linear acceleration measurements from the IMU in m/s^2
        /// </summary>
        public Vector3 IMUAcceleration()
        {
            return Sensors.IMUAcceleration();
        }

        /// <summary>
        /// The compass heading in radians
        /// </summary>
        /// <returns></returns>
        public float CompassHeading()
        {
            return Sensors.CompassHeading();
        }

        /// <summary>
        /// The body 3-axis magnetometer measurement in Gauss.
        /// </summary>
        /// <returns></returns>
        public Vector3 CompassMagnetometer()
        {
            return Sensors.CompassMagnetometer();
        }

        /// <summary>
        /// The barometeric pressure altitude in m (positive up)
        /// </summary>
        /// <returns></returns>
        public float BarometerAltitude()
        {
            return Sensors.BarometerAltitude();
        }

        /// <summary>
        /// The vehicle's attitude estimated from the compass, IMU and gyro
        /// </summary>
        /// <returns></returns>
        public Vector3 AttitudeEstimate()
        {
            return Sensors.AttitudeEstimate();
        }

        /// <summary>
        /// The vehicle latitude in degrees
        /// </summary>
        /// <returns></returns>
        public double GPSLatitude()
        {
            return Sensors.GPSLatitude();
        }

        /// <summary>
        /// The vehicle longitude in degrees
        /// </summary>
        /// <returns></returns>
        public double GPSLongitude()
        {
            return Sensors.GPSLongitude();
        }

        /// <summary>
        /// The vehicle altitude in m, relative to sea level (positive up)
        /// </summary>
        /// <returns></returns>
        public double GPSAltitude()
        {
            return Sensors.GPSAltitude();
        }

        /// <summary>
        /// The home latitude in degrees, used to calculate a local position
        /// </summary>
        /// <returns></returns>
        public double HomeLatitude()
        {
            return Sensors.HomeLatitude();
        }

        /// <summary>
        /// The home longitude in degrees, used to calculate a local position
        /// </summary>
        /// <returns></returns>
        public double HomeLongitude()
        {
            return Sensors.HomeLongitude();
        }

        /// <summary>
        /// The home altitude in m, from sea level  (positive up)
        /// </summary>
        /// <returns></returns>
        public double HomeAltitude()
        {
            return Sensors.HomeAltitude();
        }

        /// <summary>
        /// Local NED position in m, relative to the home position
        /// </summary>
        /// <returns></returns>
        public Vector3 LocalPosition()
        {
            return Sensors.LocalPosition();
        }

        /// <summary>
        /// Local NED velocity in m/s
        /// </summary>
        public Vector3 GPSVelocity()
        {
            return Sensors.GPSVelocity();
        }

        /// <summary>
        /// Sets the home position to the current GPS position
        /// </summary>
        public void SetHomePosition()
        {
            Sensors.SetHomePosition();
        }

        /// <summary>
        /// Sets the home position used in the local position calculation
        /// </summary>
        /// <param name="longitude">longitude in degrees</param>
        /// <param name="latitude">latitude</param>
        /// <param name="altitude">altitude in m, relative to seal level</param>
        public void SetHomePosition(double longitude, double latitude, double altitude)
        {
            Sensors.SetHomePosition(longitude, latitude, altitude);
        }

        public void CommandControls(float[] controls)
        {

        }

    }

}