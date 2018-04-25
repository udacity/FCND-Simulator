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
        float throttleStep = 10.0f/5000.0f;
        float throttle = 0.0f;

        public override void OnSelect(PlaneAutopilot _controller)
        {
            base.OnSelect(_controller);
            if (!_controller.planeVehicle.MotorsArmed())
                throttle = 0.0f;
        }

        public override void OnLateUpdate()
        {
            float elevator = -1.0f * Input.GetAxis("Vertical");
            float aileron = Input.GetAxis("Horizontal");
            float rudder = Input.GetAxis("Yaw");
            throttle = throttle + throttleStep * Input.GetAxis("Thrust");
            if (throttle > 1.0f)
                throttle = 1.0f;
            else if (throttle < 0.0f)
                throttle = 0.0f;

            controller.CommandControls(throttle, elevator, aileron, rudder);
            
            
        }
    }
}