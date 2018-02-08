using DroneInterface;
using DroneControllers;
using UnityEngine;

namespace Drones
{
    /// <summary>
    /// Drone based off a Quadrotor.
    /// </summary>
    class QuadDrone : MonoBehaviour, IDrone
    {
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

        public double VerticalVelocity()
        {
            return quadCtrl.GetVerticalVelocity();
        }

        public double Yaw()
        {
            return quadCtrl.GetYaw();
        }

        public Vector3 AngularVelocity()
        {
            return quadCtrl.AngularVelocityBody;
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
            return quadCtrl.LinearAcceleration;
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
    }

}