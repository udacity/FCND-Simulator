using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Longitude")]
    public class PlaneMB_Longitude : PlaneMovementBehavior
    {
        float throttle = 0.0f;
        float maxRoll = 30.0f * Mathf.PI / 180.0f;
        float maxSideslip = 10.0f * Mathf.PI / 180.0f;
        float throttleStep = 30.0f / 5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        PlaneControl PlaneControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            PlaneControl = (PlaneControl)controller.control;
        }

        public override void OnLateUpdate()
        {
            float rollCommand, sideslipCommand, elevator;
            if (controller.Guided())
            {
                rollCommand = controller.AttitudeTarget.x;
                sideslipCommand = controller.AttitudeTarget.z;
                elevator = controller.MomentThrustTarget.y;
                throttle = controller.MomentThrustTarget.w;
            }
            else
            {
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                rollCommand = maxRoll*Input.GetAxis("Horizontal");
                sideslipCommand = maxSideslip * Input.GetAxis("Yaw");
                elevator = -1.0f * Input.GetAxis("Vertical") + elevatorTrim;
                throttle = controller.ControlTarget.w + throttleStep * Input.GetAxis("Thrust");
                
            }
            throttle = Mathf.Clamp01(throttle);

            float aileron = PlaneControl.RollLoop(rollCommand, controller.ControlAttitude.x, controller.ControlBodyRate.x);
            float rudder = PlaneControl.SideslipLoop(sideslipCommand, controller.ControlWindData.z);
            controller.CommandControls(aileron, elevator, rudder, throttle);
            
            
        }
    }
}