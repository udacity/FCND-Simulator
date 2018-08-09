using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Hybrid Quad Plane")]
    public class PlaneMB_HybridQuadPlane : PlaneMovementBehavior
    {
        float alpha;
        float trimV = 41;
        float throttleStep = 30.0f / 5000.0f;
        float airspeedStep;
        float targetAirspeed;
        QuadPlaneControl QuadPlaneControl;


        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            QuadPlaneControl = (QuadPlaneControl)controller.control;
            controller.VelocityTarget = new Vector3(0f, 0f, 0f);
            targetAirspeed = 0.0f;

        }

        public override void OnLateUpdate()
        {
            float aileron, elevator, rudder, throttle, thrust;
            Vector3 totalMoment;

            if (!controller.Guided())
            {
                alpha = 1 - (controller.ControlWindData.x - 0.2f * trimV) / (0.8f * trimV - 0.2f * trimV);
                alpha = Mathf.Clamp(alpha, 0f, 1f);
                // Command the large prop throttle using the Thrust axis (Space/c)
                targetAirspeed = targetAirspeed + Input.GetAxis("Thrust");
                targetAirspeed = Mathf.Clamp(targetAirspeed, 0, trimV);
                throttle = QuadPlaneControl.PlaneControl.AirspeedLoop(targetAirspeed, controller.ControlWindData.x, Time.fixedDeltaTime);
                if (targetAirspeed == 0)
                    throttle = 0;
                throttle = Mathf.Clamp(throttle, 0f, 1f);

                // Command the ascent/descent rate using the Vertical axis (Up/Down Arrow)
                float altCmd = Input.GetAxis("Vertical");
                if (altCmd > 0)
                    altCmd = altCmd * QuadPlaneControl.QuadControl.maxAscentRate;
                else
                    altCmd = altCmd * QuadPlaneControl.QuadControl.maxDescentRate;

                thrust = QuadPlaneControl.QuadControl.VerticalVelocityLoop(altCmd, controller.ControlAttitude, -controller.ControlVelocity.z, Time.fixedDeltaTime, 0.5f);

                // Control the roll/pitch angles with a combination of aileron/elevator and quad moment
                float rollCmd = Input.GetAxis("Horizontal");
                Vector2 targetRate = QuadPlaneControl.QuadControl.RollPitchLoop(new Vector2(rollCmd, 0f), controller.ControlAttitude);
                Vector2 rollPitchMoment = QuadPlaneControl.QuadControl.RollPitchRateLoop(targetRate, controller.ControlBodyRate);

                aileron = QuadPlaneControl.PlaneControl.RollLoop(rollCmd, controller.ControlAttitude.x, controller.ControlBodyRate.x);
                float pitchCmd = altCmd / controller.ControlWindData.x;

                float yawRateCmd = Input.GetAxis("Yaw");
                float yawMoment = QuadPlaneControl.QuadControl.YawRateLoop(yawRateCmd, controller.ControlBodyRate.z);

                float dt = Time.fixedDeltaTime;

                if (yawRateCmd == 0)
                    rudder = QuadPlaneControl.PlaneControl.SideslipLoop(0, controller.ControlWindData.z, dt);
                else
                    rudder = 0;

                elevator = QuadPlaneControl.PlaneControl.PitchLoop(pitchCmd, controller.ControlAttitude.y, controller.ControlBodyRate.y);




                totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawMoment);

                controller.CommandMoment(totalMoment, thrust);
                controller.CommandControls(aileron, elevator, rudder, throttle);
            }

        }
    }
}