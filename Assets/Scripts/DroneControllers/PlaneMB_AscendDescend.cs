using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane AscendDescend")]
    public class PlaneMB_AscendDescend : PlaneMovementBehavior
    {
        float throttle = 0.0f;
        float maxRoll = 30.0f * Mathf.PI / 180.0f;
        float maxSideslip = 10.0f * Mathf.PI / 180.0f;
        float throttleStep = 30.0f / 5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        float nominalSpeed = 40.0f;//61.0f;
        float nominalThrottle = 0.66f;//0.75f;
        float altCommand;
        PlaneControl planeControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            planeControl = (PlaneControl)controller.control;
            planeControl.speedInt2 = 0.0f;
            /*
            if (!_controller.planeVehicle.MotorsArmed())
                throttle = controller.MomentThrustTarget.w;
            */
        }

        public override void OnLateUpdate()
        {
            float rollCommand, sideslipCommand,  speedCommand, climbCommand;
            if (controller.Guided())
            {
                rollCommand = controller.AttitudeTarget.x;
                sideslipCommand = controller.AttitudeTarget.z;
                speedCommand = controller.VelocityTarget.x;
                throttle = controller.MomentThrustTarget.w;
            }
            else
            {
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                rollCommand = maxRoll*Input.GetAxis("Horizontal");
                sideslipCommand = maxSideslip * Input.GetAxis("Yaw");
                speedCommand = nominalSpeed + 11.0f * Input.GetAxis("Thrust");
                throttle = nominalThrottle + 0.5f * Input.GetAxis("Vertical");
              
            }
            float aileron = planeControl.RollLoop(rollCommand, controller.ControlAttitude.x, controller.ControlBodyRate.x);
            float rudder = planeControl.SideslipLoop(sideslipCommand, controller.ControlWindData.z);
            float pitchCommand = planeControl.AirspeedLoop2(speedCommand, controller.ControlWindData.y);

            controller.VelocityTarget = new Vector3(speedCommand, controller.ControlWindData.x, controller.VelocityTarget.z);
            Vector3 attitudeTarget = controller.AttitudeTarget;
            attitudeTarget.y = pitchCommand;
            controller.AttitudeTarget = attitudeTarget;
            float elevator = planeControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);

            controller.CommandControls(aileron, elevator, rudder, throttle);

            
            
        }
    }
}