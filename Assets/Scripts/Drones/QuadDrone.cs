using DroneInterface;
using DroneControllers;
using UnityEngine;

namespace Drones
{
    class QuadDrone : MonoBehaviour, IDrone
    {
        public QuadController quadCtrl;
        public SimpleQuadController simpleQuadCtrl;

        public Vector3 LocalCoords()
        {
            return this.transform.position;
        }

        public double Altitude()
        {
            return quadCtrl.GetAltitude();
        }

        public void Arm(bool arm)
        {
            // simpleQuadCtrl.motors_armed = arm;
            if (arm)
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
            return simpleQuadCtrl.motors_armed;
        }

        public double EastVelocity()
        {
            return quadCtrl.GetEastVelocity();
        }

        public bool ExecutingCommand()
        {
            throw new System.NotImplementedException();
        }

        public void Goto(double latitude, double longitude, double altitude)
        {
            simpleQuadCtrl.CommandGPS(latitude, longitude, altitude);
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
            throw new System.NotImplementedException();
        }

        public void SetAttitudeRate(double rollRate, double pitchRate, double yawRate, double thrust)
        {
            throw new System.NotImplementedException();
        }

        public void SetMotors(double a, double b, double c, double d)
        {
            throw new System.NotImplementedException();
        }

        public void SetVelocity(double northVelocity, double eastVelocity, double verticalVelocity, double heading)
        {
            throw new System.NotImplementedException();
        }

        public void TakeControl(bool guided)
        {
            // simpleQuadCtrl.guided = guided;
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
    }
}