using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Line Following")]
    public class PlaneMB_LineFollowing : PlaneMovementBehavior
    {
        float throttle = 0.0f;
        float maxRoll = 60.0f * Mathf.PI / 180.0f;
        float maxSideslip = 10.0f * Mathf.PI / 180.0f;
        float throttleStep = 30.0f / 5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;

        float altitudeSwitch = 25.0f;

        float nominalSpeed = 40.0f;//61.0f;
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
            yawCommand = controller.ControlAttitude.z;
            PlaneControl.yawInt = 0.0f;
            PlaneControl.sideslipInt = 0.0f;


        }

        public override void OnLateUpdate()
        {
            float courseCommand, speedCommand;
            float sideslipCommand = 0.0f;
            Vector3 positionCommand;
            if (controller.Guided())
            {
                courseCommand = Mathf.Atan2(controller.VelocityTarget.y, controller.VelocityTarget.x);
                speedCommand = controller.VelocityTarget.magnitude;
                altCommand = -controller.PositionTarget.z;
                positionCommand = controller.PositionTarget;

            }
            else
            {
                Debug.Log("Line Following Not Available as Manual Mode");
                return;
            }

            float yawCommand = PlaneControl.CrossTrackLoop(positionCommand, courseCommand, controller.ControlPosition);
            Vector3 attitudeTarget = controller.AttitudeTarget;
            attitudeTarget.z = yawCommand;

            float rollCommand = PlaneControl.YawLoop(yawCommand, controller.ControlAttitude.z, Time.fixedDeltaTime, 0f);

            float aileron = PlaneControl.RollLoop(rollCommand, controller.ControlAttitude.x, controller.ControlBodyRate.x);
            float rudder = PlaneControl.SideslipLoop(sideslipCommand, controller.ControlWindData.z);
            float elevator;
            altitudeSwitch = PlaneControl.altitudeSwitch;
            float pitchCommand;
            if ((-controller.ControlPosition.z - altCommand) > altitudeSwitch)
            {
                throttle = 0.1f;
                pitchCommand = PlaneControl.AirspeedLoop2(speedCommand, controller.ControlWindData.x);
                elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);
            }
            else if ((-controller.ControlPosition.z - altCommand) < -altitudeSwitch)
            {
                throttle = 1.0f;
                pitchCommand = PlaneControl.AirspeedLoop2(speedCommand, controller.ControlWindData.x);
                elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);
            }
            else
            {
                throttle = PlaneControl.AirspeedLoop(speedCommand, controller.ControlWindData.x);
                pitchCommand = PlaneControl.AltitudeLoop(altCommand, -controller.ControlPosition.z);
                elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);
            }

            attitudeTarget.y = pitchCommand;
            controller.AttitudeTarget = attitudeTarget;
            
            
            controller.CommandControls(aileron, elevator, rudder, throttle);
            //Debug.Log("Sideslip Command: " + sideslipCommand);
            
            
        }
    }
}