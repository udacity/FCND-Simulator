using DroneInterface;
using DroneControllers;
using UnityEngine;

namespace Drones
{
    // Drone based off a quadrotor/quadcopter.
    class QuadDrone : MonoBehaviour, IDrone
    {
        public QuadController quadCtrl;
        public SimpleQuadController simpleQuadCtrl;

        // TODO: Add the components here at runtime instead of in
        // the unity editor.
        void Awake()
        {
            // gameObject.AddComponent<QuadController>();
            // gameObject.AddComponent<SimpleQuadController>();
            quadCtrl = GetComponent<QuadController>();
            simpleQuadCtrl = GetComponent<SimpleQuadController>();
        }

        public Vector3 Forward { get { return quadCtrl.Forward; } }

        public Vector3 UnityCoords()
        {
            return this.transform.position;
        }

        /// <summary>
        /// NED coordinate frame
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
                simpleQuadCtrl.ArmVehicle();
            }
            else
            {
                simpleQuadCtrl.DisarmVehicle();
            }
        }

        public bool Armed()
        {
            return simpleQuadCtrl.rotors_armed;
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

        public void SetAttitude(double roll, double pitch, double yaw, double velocity)
        {
            simpleQuadCtrl.CommandHeading((float)yaw);
        }

        public void SetAttitudeRate(double rollRate, double pitchRate, double yawRate, double thrust)
        {
            throw new System.NotImplementedException();
        }

        public void SetMotors(float throttle, float pitchRate, float yawRate, float rollRate)
        {
            simpleQuadCtrl.currentMovementBehavior.OverrideUpdate(throttle, pitchRate, yawRate, rollRate);
        }

        public void SetVelocity(double northVelocity, double eastVelocity, double verticalVelocity, double heading)
        {
            throw new System.NotImplementedException();
        }

        public void TakeControl(bool guided)
        {
            simpleQuadCtrl.SetGuidedMode(guided);
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

        public Vector3 AngularAcceleration()
        {
            return quadCtrl.AngularAccelerationBody;
        }

        public Vector3 LinearAcceleration()
        {
            return quadCtrl.LinearAcceleration;
        }
    }

}