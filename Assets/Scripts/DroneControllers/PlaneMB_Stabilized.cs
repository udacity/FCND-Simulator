using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Stabilized")]
    public class PlaneMB_Stabilized : PlaneMovementBehavior
    {
        float throttle = 0.0f;
        float maxRoll = 60.0f * Mathf.PI / 180.0f;
        float maxSideslip = 10.0f * Mathf.PI / 180.0f;
        float throttleStep = 30.0f / 5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        float nominalSpeed = 40.0f;//61.0f;
        float altCommand;
        PlaneControl PlaneControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            PlaneControl = (PlaneControl)controller.control;

            altCommand = -controller.ControlPosition.z;
            PlaneControl.altInt = 0.0f;
            PlaneControl.speedInt = 0.0f;
            PlaneControl.sideslipInt = 0f;

        }

        public override void OnLateUpdate()
        {
            float rollCommand, sideslipCommand,  speedCommand;
            if (controller.Guided())
            {
                rollCommand = controller.AttitudeTarget.x;
                sideslipCommand = controller.AttitudeTarget.z;
                speedCommand = controller.VelocityTarget.x;
                altCommand = controller.PositionTarget.z;
            }
            else
            {
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                rollCommand = maxRoll*Input.GetAxis("Horizontal");
                sideslipCommand = maxSideslip * Input.GetAxis("Yaw");
                speedCommand = nominalSpeed + 11.0f * Input.GetAxis("Thrust");
                if (Input.GetAxis("Vertical") != 0.0f)
                    altCommand = -1.0f * controller.ControlPosition.z - 10.0f * Input.GetAxis("Vertical");               
            }

            float aileron = PlaneControl.RollLoop(rollCommand, controller.ControlAttitude.x, controller.ControlBodyRate.x);
            float rudder = PlaneControl.SideslipLoop(sideslipCommand, controller.ControlWindData.z);

            throttle = PlaneControl.AirspeedLoop(speedCommand, controller.ControlWindData.x);
            Vector3 positionTarget = controller.PositionTarget;
            positionTarget.z = altCommand;
            controller.PositionTarget = positionTarget;

            float pitchCommand = PlaneControl.AltitudeLoop(altCommand, -controller.ControlPosition.z);
            pitchCommand = Mathf.Clamp(pitchCommand, -20.0f * Mathf.PI / 180.0f, 20.0f * Mathf.PI / 180.0f);

            Vector3 attitudeTarget = controller.AttitudeTarget;
            attitudeTarget.y = pitchCommand;
            controller.AttitudeTarget = attitudeTarget;

            float elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);

            controller.CommandControls(aileron, elevator, rudder, throttle);

            
            
        }
    }
}