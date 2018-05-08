using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

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

        public override void OnSelect(PlaneAutopilot _controller)
        {
            base.OnSelect(_controller);

            if (!_controller.planeVehicle.MotorsArmed())
                throttle = controller.GetThrustTarget();

        }

        public override void OnLateUpdate()
        {
            float rollCommand, sideslipCommand, elevator;
            if (controller.guided)
            {
                rollCommand = controller.attitudeTarget.x;
                sideslipCommand = controller.attitudeTarget.z;
                elevator = controller.momentThrustTarget.y;
                throttle = controller.momentThrustTarget.w;
            }
            else
            {
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                rollCommand = maxRoll*Input.GetAxis("Horizontal");
                sideslipCommand = maxSideslip * Input.GetAxis("Yaw");
                elevator = -1.0f * Input.GetAxis("Vertical") + elevatorTrim;
                throttle = throttle = controller.momentThrustTarget.w + throttleStep * Input.GetAxis("Thrust");
                
            }

            float aileron = controller.planeControl.RollLoop(rollCommand, controller.AttitudeEuler().x, controller.AngularRatesBody().x);
            float rudder = controller.planeControl.SideslipLoop(sideslipCommand, controller.Sideslip());
            /*
            //float elevator = -1.0f * Input.GetAxis("Vertical");
            float aileron = Input.GetAxis("Horizontal");
            float rudder = Input.GetAxis("Yaw");
            float speedCommand = nominalSpeed + 11.0f * Input.GetAxis("Thrust");
            
            controller.attitudeTarget.x = controller.Airspeed();
            throttle = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed()) + nominalThrottle;
            //throttle = throttle + throttleStep * Input.GetAxis("Thrust");
            controller.attitudeTarget.z = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed());
            altCommand = -1.0f*Input.GetAxis("Vertical") + altCommand;
            controller.positionTarget.z = altCommand;
            float pitchCommand = controller.planeControl.AltitudeLoop(altCommand, -controller.PositionLocal().z);
            pitchCommand = Mathf.Clamp(pitchCommand, -20.0f * Mathf.PI / 180.0f, 20.0f * Mathf.PI / 180.0f);
            controller.attitudeTarget.y = pitchCommand;
            float elevator = controller.planeControl.PitchLoop(pitchCommand, controller.AttitudeEuler().y, controller.AngularRatesBody().y);
            
            if (throttle > 1.0f)
                throttle = 1.0f;
            else if (throttle < 0.0f)
                throttle = 0.0f;

            */
            controller.CommandControls(throttle, elevator, aileron, rudder);
            
            
        }
    }
}