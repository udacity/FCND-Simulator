using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

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

        float nominalSpeed = 40.0f;//61.0f;
        float nominalThrottle = 0.66f;//0.75f;
        float altCommand;

        float yawCommand;
        public float yawIncr = 1.0f * Mathf.PI / 180.0f;

        public override void OnSelect(PlaneAutopilot _controller)
        {
            base.OnSelect(_controller);
            controller.planeControl.altInt = 0.0f;
            controller.planeControl.speedInt = 0.0f;
            yawCommand = controller.AttitudeEuler().z;
            controller.planeControl.yawInt = 0.0f;
            controller.planeControl.sideslipInt = 0.0f;

            if (!_controller.planeVehicle.MotorsArmed())
                throttle = controller.GetThrustTarget();
                

        }

        public override void OnLateUpdate()
        {
            float courseCommand, speedCommand;
            float sideslipCommand = 0.0f;
            Vector3 positionCommand;
            if (controller.guided)
            {
                courseCommand = Mathf.Atan2(controller.velocityTarget.y, controller.velocityTarget.x);
                speedCommand = controller.velocityTarget.magnitude;
                altCommand = -controller.positionTarget.z;
                //maxRoll = 45.0f * Mathf.PI / 180.0f;
                positionCommand = controller.positionTarget;
                //elevator = controller.momentThrustTarget.y;
                //throttle = controller.momentThrustTarget.w;
            }
            else
            {
                Debug.Log("Line Following Not Available as Manual Mode");
                return;
            }

            float yawCommand = controller.planeControl.CrossTrackLoop(positionCommand, courseCommand, controller.PositionLocal());
            controller.attitudeTarget.z = yawCommand;
            float rollCommand = controller.planeControl.YawLoop(yawCommand, controller.AttitudeEuler().z, Time.fixedDeltaTime, 0f);
            //if (Mathf.Abs(rollCommand) > maxRoll)
            //    rollCommand = Mathf.Sign(rollCommand) * maxRoll;

            float aileron = controller.planeControl.RollLoop(rollCommand, controller.AttitudeEuler().x, controller.AngularRatesBody().x);
            float rudder = controller.planeControl.SideslipLoop(sideslipCommand, controller.Sideslip());

            throttle = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed()) + nominalThrottle;
            //controller.attitudeTarget.z = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed());
            //controller.positionTarget.z = altCommand;
            float pitchCommand = controller.planeControl.AltitudeLoop(altCommand, -controller.PositionLocal().z);
            //pitchCommand = Mathf.Clamp(pitchCommand, -20.0f * Mathf.PI / 180.0f, 20.0f * Mathf.PI / 180.0f);
            controller.attitudeTarget.y = pitchCommand;
            float elevator = controller.planeControl.PitchLoop(pitchCommand, controller.AttitudeEuler().y, controller.AngularRatesBody().y);

            controller.CommandControls(aileron, elevator, rudder, throttle);
            //Debug.Log("Sideslip Command: " + sideslipCommand);
            
            
        }
    }
}