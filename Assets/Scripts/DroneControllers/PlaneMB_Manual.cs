using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Manual")]
    public class PlaneMB_Manual : PlaneMovementBehavior
    {
        float throttleStep = 30.0f/5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        public override void OnSelect(PlaneAutopilot _controller)
        {
            base.OnSelect(_controller);
            elevatorTrim = 0.0f;
        }

        public override void OnLateUpdate()
        {
            
            if (!controller.guided)
            {
                float elevator, aileron, rudder, throttle;
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                elevator = -1.0f * Input.GetAxis("Vertical") + elevatorTrim;
                aileron = Input.GetAxis("Horizontal");
                rudder = Input.GetAxis("Yaw");
                throttle = controller.momentThrustTarget.w + throttleStep * Input.GetAxis("Thrust");
                controller.CommandControls(aileron, elevator, rudder, throttle);
            }

            
            
            
        }
    }
}