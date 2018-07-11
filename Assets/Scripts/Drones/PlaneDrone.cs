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
        public void CommandVector(Vector3 localPosition, Vector3 localVelocity)
        {
            planeAutopilot.CommandVector(localPosition, localVelocity);

        }
        public float FlightTime()
        {
            return planeVehicle.FlightTime();
        }

		public bool Frozen
		{
			get { return planeVehicle.Frozen; }
			set { planeVehicle.Frozen = value; }
		}

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

		void Update ()
		{
			// shove some fake values into the ui
			Simulation.FixedWingUI.throttle.SetValue ( Mathf.Sin ( Time.time ) * 0.5f + 0.5f );
			Simulation.FixedWingUI.elevator.SetValue ( Mathf.Cos ( Time.time ) * 0.5f + 0.5f );
			Simulation.FixedWingUI.rudder.SetValue ( Mathf.Sin ( Time.time ) );
			Simulation.FixedWingUI.aileron.SetValue ( Mathf.Cos ( Time.time ) );
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
		/// Linear velocity in unity coords
		/// </summary>
		public Vector3 VelocityUnity()
		{
			return planeVehicle.VelocityUnity ();
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
		/// Angular velocity in unity coords
		/// </summary>
		public Vector3 AngularRatesUnity()
		{
			return planeVehicle.AngularRatesUnity ();
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

        public Vector3 AttitudeTarget()
        {
            return planeAutopilot.AttitudeTarget();
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



        public void FreezeDrone(bool freeze)
        {
            planeVehicle.FreezeDrone(freeze);
        }

        public bool IsFrozen()
        {
            return planeVehicle.IsFrozen();
        }
    }

}