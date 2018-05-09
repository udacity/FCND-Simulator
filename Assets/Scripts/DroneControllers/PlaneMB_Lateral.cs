using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Lateral")]
    public class PlaneMB_Lateral : PlaneMovementBehavior
    {
        float throttleStep = 30.0f/5000.0f;
        float throttle = 0.0f;
        float nominalSpeed = 40.0f;//61.0f;
        float nominalThrottle = 0.66f;//0.75f;
        float altCommand;

        public override void OnSelect(PlaneAutopilot _controller)
        {
            base.OnSelect(_controller);

            altCommand = -controller.PositionLocal().z;
            if (!_controller.planeVehicle.MotorsArmed())
                throttle = controller.GetThrustTarget();

            nominalThrottle = 0.66f;
            nominalSpeed = 40.0f;

        }

        public override void OnLateUpdate()
        {
            float aileron, rudder, speedCommand;
            if (controller.guided)
            {
                aileron = controller.momentThrustTarget.x;
                rudder = controller.momentThrustTarget.z;
                speedCommand = controller.velocityTarget.x;
                altCommand = controller.positionTarget.z;
            }
            else
            {
                aileron = Input.GetAxis("Horizontal");
                rudder = Input.GetAxis("Yaw");
                speedCommand = nominalSpeed + 11.0f * Input.GetAxis("Thrust");
                altCommand = -1.0f * Input.GetAxis("Vertical") + altCommand;
            }

            controller.attitudeTarget.x = controller.Airspeed();
            throttle = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed()) + nominalThrottle;
            controller.attitudeTarget.z = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed());
            
            controller.positionTarget.z = altCommand;
            float pitchCommand = controller.planeControl.AltitudeLoop(altCommand, -controller.PositionLocal().z);
            pitchCommand = Mathf.Clamp(pitchCommand, -20.0f * Mathf.PI / 180.0f, 20.0f * Mathf.PI / 180.0f);
            controller.attitudeTarget.y = pitchCommand;
            float elevator = controller.planeControl.PitchLoop(pitchCommand, controller.AttitudeEuler().y, controller.AngularRatesBody().y);

            controller.CommandControls(aileron, elevator, rudder, throttle);
            
            
        }
    }
}