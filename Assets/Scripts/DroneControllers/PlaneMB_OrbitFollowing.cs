using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Orbit Following")]
    public class PlaneMB_OrbitFollowing : PlaneMovementBehavior
    {
        float throttle = 0.0f;
        float maxRoll = 60.0f * Mathf.PI / 180.0f;
        float maxSideslip = 10.0f * Mathf.PI / 180.0f;
        float throttleStep = 30.0f / 5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;
        float altSwitch = 25f;

        float nominalSpeed = 40.0f;//61.0f;
        float nominalThrottle = 0.66f;//0.75f;
        float altCommand;

        float yawCommand;
        public float yawIncr = 1.0f * Mathf.PI / 180.0f;

        PlaneControl PlaneControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);

            PlaneControl.altInt = 0.0f;
            PlaneControl.speedInt = 0.0f;
            PlaneControl.sideslipInt = 0f;
            PlaneControl.yawInt = 0f;
            yawCommand = controller.ControlAttitude.z;

        }

        public override void OnLateUpdate()
        {
            float speedCommand;
            float sideslipCommand = 0.0f;
            Vector3 positionCommand;
            if (controller.Guided())
            {
                speedCommand = controller.VelocityTarget.magnitude;
                altCommand = -controller.PositionTarget.z;
                maxRoll = 45.0f * Mathf.PI / 180.0f;
                positionCommand = controller.PositionTarget;
            }
            else
            {
                Debug.Log("Orbit Following Not Available as Manual Mode");
                return;
            }
            bool clockwise = controller.BodyRateTarget.z >= 0;
            float yawCommand = PlaneControl.OrbitLoop(controller.PositionTarget, Mathf.Abs(controller.VelocityTarget.x / controller.BodyRateTarget.z), controller.ControlPosition, controller.ControlAttitude.z,clockwise);
            Vector3 attitudeTarget = controller.AttitudeTarget;

            attitudeTarget.z = yawCommand;
            float roll_ff = Mathf.Atan(speedCommand * speedCommand / (9.81f * controller.VelocityTarget.x / controller.BodyRateTarget.z));
            float rollCommand = PlaneControl.YawLoop(yawCommand, controller.ControlAttitude.z, Time.fixedDeltaTime, roll_ff);

            
            float aileron = PlaneControl.RollLoop(rollCommand, controller.ControlAttitude.x, controller.ControlBodyRate.x);
            float rudder = PlaneControl.SideslipLoop(sideslipCommand, controller.ControlWindData.z);

            float elevator;
            altSwitch = PlaneControl.altitudeSwitch;
            if ((-controller.ControlPosition.z - altCommand) > altSwitch)
            {
                throttle = 0.1f;
                float pitchCommand = PlaneControl.AirspeedLoop2(speedCommand, controller.ControlWindData.x);
                attitudeTarget.y = pitchCommand;
                elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);
            }
            else if ((-controller.ControlPosition.z - altCommand) < -altSwitch)
            {
                throttle = 1.0f;
                float pitchCommand = PlaneControl.AirspeedLoop2(speedCommand, controller.ControlWindData.x);
                attitudeTarget.y = pitchCommand;
                elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);
            }
            else
            {
                throttle = PlaneControl.AirspeedLoop(speedCommand, controller.ControlWindData.x) + nominalThrottle;
                float pitchCommand = PlaneControl.AltitudeLoop(altCommand, -controller.ControlPosition.z);
                attitudeTarget.y = pitchCommand;
                elevator = PlaneControl.PitchLoop(pitchCommand, controller.ControlAttitude.y, controller.ControlBodyRate.y);
            }

            controller.CommandControls(aileron, elevator, rudder, throttle);
            controller.AttitudeTarget = attitudeTarget;
            
            
        }
    }
}