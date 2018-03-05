using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdaciPlot;
namespace DroneControllers
{
    public class ControlPlots : MonoBehaviour
    {
        SimpleQuadController ctrl;
        QuadController nav;
        private bool alive;

        //Names of all the control variables to plot
        string pNorthPosition = "North Position (m)";
        string pEastPosition = "East Position (m)";
        string pDownPosition = "Down Position (m)";

        string pTargetNorthPosition = "Target North Position (m)";
        string pTargetEastPosition = "Target East Position (m)";
        string pTargetDownPosition = "Target Down Position (m)";

        string pNorthVelocity = "North Velocity (m/s)";
        string pEastVelocity = "East Velocity (m/s)";
        string pDownVelocity = "Down Velocity (m/s)";

        string pTargetNorthVelocity = "Target North Velocity (m/s)";
        string pTargetEastVelocity = "Target East Velocity (m/s)";
        string pTargetDownVelocity = "Target Down Velocity (m/s)";

        string pNorthAcceleration = "North Acceleration (m^2/s)";
        string pEastAcceleration = "East Acceleration (m^2/s)";
        string pDownAcceleration = "Down Acceleration (m^2/s)";

        string pTargetNorthAcceleration = "Target North Acceleration (m^2/s)";
        string pTargetEastAcceleration = "Target East Acceleration (m^2/s)";
        string pTargetDownAcceleration = "Target Down Acceleration (m^2/s)";

        string pRoll = "Roll (deg)";
        string pPitch = "Pitch (deg)";
        string pYaw = "Yaw (deg)";

        string pTargetRoll = "Target Roll (deg)";
        string pTargetPitch = "Target Pitch (deg)";
        string pTargetYaw = "Target Yaw (deg)";

        string pRollRate = "Roll Rate (deg/s)";
        string pPitchRate = "Pitch Rate (deg/s)";
        string pYawRate = "Yaw Rate (deg/s)";

        string pTargetRollRate = "Target Roll Rate (deg/s)";
        string pTargetPitchRate = "Target Pitch Rate (deg/s)";
        string pTargetYawRate = "Target Yaw Rate (deg/s)";

        string pTotalThrust = "Total Thrust (Newton)";
        string pTorqueMag = "Torque Magnitude (Newton*meter)";

 

        void Start()
        {

            ctrl = GetComponent<SimpleQuadController> ();
            nav = GetComponent<QuadController>();

            //Add the plots to the list
            Plotting.AddPlottable1D(pNorthPosition);
            Plotting.AddPlottable1D(pEastPosition);
            Plotting.AddPlottable1D(pDownPosition);
            Plotting.AddPlottable1D(pTargetNorthPosition);
            Plotting.AddPlottable1D(pTargetEastPosition);
            Plotting.AddPlottable1D(pTargetDownPosition);

            Plotting.AddPlottable1D(pNorthVelocity);
            Plotting.AddPlottable1D(pEastVelocity);
            Plotting.AddPlottable1D(pDownVelocity);
            Plotting.AddPlottable1D(pTargetNorthVelocity);
            Plotting.AddPlottable1D(pTargetEastVelocity);
            Plotting.AddPlottable1D(pTargetDownVelocity);

            Plotting.AddPlottable1D(pNorthAcceleration);
            Plotting.AddPlottable1D(pEastAcceleration);
            Plotting.AddPlottable1D(pDownAcceleration);
            Plotting.AddPlottable1D(pTargetNorthAcceleration);
            Plotting.AddPlottable1D(pTargetEastAcceleration);
            Plotting.AddPlottable1D(pTargetDownAcceleration);

            Plotting.AddPlottable1D(pRoll);
            Plotting.AddPlottable1D(pPitch);
            Plotting.AddPlottable1D(pYaw);
            Plotting.AddPlottable1D(pTargetRoll);
            Plotting.AddPlottable1D(pTargetPitch);
            Plotting.AddPlottable1D(pTargetYaw);

            Plotting.AddPlottable1D(pRollRate);
            Plotting.AddPlottable1D(pPitchRate);
            Plotting.AddPlottable1D(pYawRate);
            Plotting.AddPlottable1D(pTargetRollRate);
            Plotting.AddPlottable1D(pTargetPitchRate);
            Plotting.AddPlottable1D(pTargetYawRate);


            System.Threading.Tasks.Task.Run(() => Sample());
        }

        private void Awake()
        {
            alive = true;
        }

        void OnDestroy()
        {
            alive = false;
        }

        async System.Threading.Tasks.Task Sample()
        {
            //			System.Random rand = new System.Random ( (int) GetTime () );
            //			FastNoise fn = new FastNoise ( rand.Next () );
            //			double d2r = System.Math.PI / 180;
            while (alive)
            {
                
                Plotting.AddSample(pNorthPosition, nav.GetLocalNorth(), GetTime());
                Plotting.AddSample(pEastPosition, nav.GetLocalEast(), GetTime());
                Plotting.AddSample(pDownPosition, nav.GetLocalDown(), GetTime());
                

                Plotting.AddSample(pTargetNorthPosition, ctrl.positionTarget.x, GetTime());
                Plotting.AddSample(pTargetEastPosition, ctrl.positionTarget.y, GetTime());
                Plotting.AddSample(pTargetDownPosition, ctrl.positionTarget.z, GetTime());

                Plotting.AddSample(pNorthVelocity, nav.GetNorthVelocity(), GetTime());
                Plotting.AddSample(pEastVelocity, nav.GetEastVelocity(), GetTime());
                Plotting.AddSample(pDownVelocity, nav.GetDownVelocity(), GetTime());

                Plotting.AddSample(pTargetNorthVelocity, ctrl.velocityTarget.x, GetTime());
                Plotting.AddSample(pTargetEastVelocity, ctrl.velocityTarget.y, GetTime());
                Plotting.AddSample(pTargetDownVelocity, ctrl.velocityTarget.z, GetTime());

                Plotting.AddSample(pNorthAcceleration, nav.GetNorthAcceleration(), GetTime());
                Plotting.AddSample(pEastAcceleration, nav.GetEastAcceleration(), GetTime());
                Plotting.AddSample(pDownAcceleration, nav.GetDownAcceleration(), GetTime());

                Plotting.AddSample(pTargetNorthAcceleration, ctrl.accelerationTarget.x, GetTime());
                Plotting.AddSample(pTargetEastAcceleration, ctrl.accelerationTarget.y, GetTime());
                Plotting.AddSample(pTargetDownAcceleration, ctrl.accelerationTarget.z, GetTime());

                Plotting.AddSample(pRoll, nav.GetRoll() * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pPitch, nav.GetPitch() * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pYaw, nav.GetYaw() * 180.0f / Mathf.PI, GetTime());

                Plotting.AddSample(pTargetRoll, ctrl.attitudeTarget.x * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pTargetPitch, ctrl.attitudeTarget.y * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pTargetYaw, ctrl.attitudeTarget.z * 180.0f / Mathf.PI, GetTime());

                Plotting.AddSample(pRollRate, nav.GetRollrate() * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pPitchRate, nav.GetPitchrate() * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pYawRate, nav.GetYawrate() * 180.0f / Mathf.PI, GetTime());

                Plotting.AddSample(pTargetRollRate, ctrl.bodyRateTarget.x * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pTargetPitchRate, ctrl.bodyRateTarget.y * 180.0f / Mathf.PI, GetTime());
                Plotting.AddSample(pTargetYawRate, ctrl.bodyRateTarget.z * 180.0f / Mathf.PI, GetTime());

                Plotting.AddSample(pTotalThrust, nav.thrustOut, GetTime());
                Plotting.AddSample(pTorqueMag, nav.TorqueOut, GetTime());

                await System.Threading.Tasks.Task.Delay(100);
            }
        }

        double GetTime()
        {
            var now = System.DateTime.UtcNow;
            var origin = new System.DateTime(1970, 1, 1, 0, 0, 0);
            return (now - origin).TotalSeconds;
        }



    }
}
