using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Yaw Hold")]
    public class PlaneMB_YawHold : PlaneMovementBehavior
    {
        float throttle = 0.0f;
        float maxRoll = 60.0f * Mathf.PI / 180.0f;
        float maxSideslip = 10.0f * Mathf.PI / 180.0f;
        float throttleStep = 30.0f / 5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        float nominalSpeed = 40.0f;//61.0f;
        float nominalThrottle = 0.66f;//0.75f;
        float altCommand;

        float yawCommand;
        public float yawIncr = 1.0f * Mathf.PI / 180.0f;
        PlaneControl PlaneControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            PlaneControl = (PlaneControl)controller.control;
            PlaneControl.altInt = 0.0f;
            PlaneControl.speedInt = 0.0f;
            PlaneControl.sideslipInt = 0f;
            PlaneControl.yawInt = 0f;
            yawCommand = controller.ControlAttitude.z;


        }

        public override void OnLateUpdate()
        {
            float rollCommand, sideslipCommand,  speedCommand;
            Vector3 attitudeTarget, velocityTarget, positionTarget;
            attitudeTarget = controller.AttitudeTarget;
            velocityTarget = controller.VelocityTarget;
            positionTarget = controller.PositionTarget;
            if (controller.Guided())
            {
                sideslipCommand = controller.VelocityTarget.y;
                yawCommand = controller.AttitudeTarget.z;
                speedCommand = controller.VelocityTarget.x;
                altCommand = controller.PositionTarget.z;
                maxRoll = 45.0f * Mathf.PI / 180.0f;

            }
            else
            {
                maxRoll = 45.0f * Mathf.PI / 180.0f;
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");

                yawCommand = yawCommand + yawIncr * Input.GetAxis("Horizontal");

                attitudeTarget.z = yawCommand;
                sideslipCommand = maxSideslip * Input.GetAxis("Yaw");
                velocityTarget.y = sideslipCommand;
                speedCommand = nominalSpeed + 11.0f * Input.GetAxis("Thrust");
                if (Input.GetAxis("Vertical") != 0.0f)
                    altCommand = -1.0f * controller.ControlPosition.z - 10.0f * Input.GetAxis("Vertical");            
            }
            rollCommand = PlaneControl.YawLoop(yawCommand, controller.ControlAttitude.z, Time.fixedDeltaTime);
            if (Mathf.Abs(rollCommand) > maxRoll)
                rollCommand = Mathf.Sign(rollCommand) * maxRoll;

            float aileron = PlaneControl.RollLoop(rollCommand, controller.ControlAttitude.x, controller.ControlBodyRate.x);
            float rudder = PlaneControl.SideslipLoop(sideslipCommand, controller.ControlWindData.z);

            throttle = PlaneControl.AirspeedLoop(speedCommand, controller.ControlWindData.x) + nominalThrottle;

            positionTarget.z = altCommand;
            float pitchCommand = PlaneControl.AltitudeLoop(altCommand, -controller.ControlPosition.z);
            pitchCommand = Mathf.Clamp(pitchCommand, -20.0f * Mathf.PI / 180.0f, 20.0f * Mathf.PI / 180.0f);
            attitudeTarget.y = pitchCommand;
            float elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);

            controller.AttitudeTarget = attitudeTarget;
            controller.PositionTarget = positionTarget;
            controller.VelocityTarget = velocityTarget;
            controller.CommandControls(aileron, elevator, rudder, throttle);

            
            
        }
    }
}