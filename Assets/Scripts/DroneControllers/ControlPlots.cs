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

 

        void Start()
        {
            ctrl = GetComponent<SimpleQuadController> ();
            nav = GetComponent<QuadController>();
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
                //				Plotting.AddSample ( "Altitude", (float) System.Math.Sin ( GetTime () * d2r ) * 3, GetTime () );
                
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
