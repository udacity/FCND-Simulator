using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Manual")]
    public class PlaneMB_Manual : PlaneMovementBehavior
    {
        float throttleStep = 30.0f/5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        public override void OnSelect(IDroneController _controller)
        {
            Debug.Log("Controller: " + _controller.GetType());
            base.OnSelect(_controller);
            elevatorTrim = 0.0f;
        }

        public override void OnLateUpdate()
        {
            
            
            if (!controller.Guided())
            {
                float elevator, aileron, rudder, throttle;
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                elevator = -1.0f * Input.GetAxis("Vertical") + elevatorTrim;
                aileron = Input.GetAxis("Horizontal");
                rudder = Input.GetAxis("Yaw");
                throttle = controller.MomentThrustTarget.w + throttleStep * Input.GetAxis("Thrust");
                controller.CommandControls(aileron, elevator, rudder, throttle);
                
            }

            
            
            
        }
    }
}