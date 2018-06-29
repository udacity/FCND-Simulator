using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

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
            controller.planeControl.sideslipInt = 0f;
            controller.planeControl.yawInt = 0f;
            yawCommand = controller.AttitudeEuler().z;
            if (!_controller.planeVehicle.MotorsArmed())
                throttle = controller.GetThrustTarget();

        }

        public override void OnLateUpdate()
        {
            float speedCommand;
            float sideslipCommand = 0.0f;
            Vector3 positionCommand;
            if (controller.guided)
            {
                //courseCommand = Mathf.Atan2(controller.velocityTarget.y, controller.velocityTarget.x);
                speedCommand = controller.velocityTarget.magnitude;
                altCommand = -controller.positionTarget.z;
                maxRoll = 45.0f * Mathf.PI / 180.0f;
                positionCommand = controller.positionTarget;
                //elevator = controller.momentThrustTarget.y;
                //throttle = controller.momentThrustTarget.w;
            }
            else
            {
                Debug.Log("Orbit Following Not Available as Manual Mode");
                return;
            }
            bool clockwise = controller.bodyRateTarget.z >= 0;
            float yawCommand = controller.planeControl.OrbitLoop(controller.positionTarget, Mathf.Abs(controller.velocityTarget.x / controller.bodyRateTarget.z), controller.PositionLocal(), controller.AttitudeEuler().z,clockwise);
            controller.attitudeTarget.z = yawCommand;
            float roll_ff = Mathf.Atan(speedCommand * speedCommand / (9.81f * controller.velocityTarget.x / controller.bodyRateTarget.z));
            float rollCommand = controller.planeControl.YawLoop(yawCommand, controller.AttitudeEuler().z, Time.fixedDeltaTime, roll_ff);


            /*
            rollCommand = rollCommand + roll_ff;
            if (Mathf.Abs(rollCommand) > maxRoll)
                rollCommand = Mathf.Sign(rollCommand) * maxRoll;
            */
            Debug.Log("Roll Cmd " + rollCommand);
            float aileron = controller.planeControl.RollLoop(rollCommand, controller.AttitudeEuler().x, controller.AngularRatesBody().x);
            float rudder = controller.planeControl.SideslipLoop(sideslipCommand, controller.Sideslip());

            throttle = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed()) + nominalThrottle;
            //controller.attitudeTarget.z = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed());
            //controller.positionTarget.z = altCommand;
            float pitchCommand = controller.planeControl.AltitudeLoop(altCommand, -controller.PositionLocal().z);
            pitchCommand = Mathf.Clamp(pitchCommand, -20.0f * Mathf.PI / 180.0f, 20.0f * Mathf.PI / 180.0f);
            controller.attitudeTarget.y = pitchCommand;
            float elevator = controller.planeControl.PitchLoop(pitchCommand, controller.AttitudeEuler().y, controller.AngularRatesBody().y);

            controller.CommandControls(aileron, elevator, rudder, throttle);
            //Debug.Log("Sideslip Command: " + sideslipCommand);
            
            
        }
    }
}