using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

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
        PlaneControl PlaneControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);

            altCommand = -controller.ControlPosition.z;

            /*
            if (!_controller.planeVehicle.MotorsArmed())
                throttle = controller.GetThrustTarget();
            */
            PlaneControl = (PlaneControl)controller.control;
            nominalThrottle = 0.66f;
            nominalSpeed = 40.0f;

        }

        public override void OnLateUpdate()
        {
            float aileron, rudder, speedCommand;
            if (controller.Guided())
            {
                aileron = controller.MomentThrustTarget.x;
                rudder = controller.MomentThrustTarget.z;
                speedCommand = controller.VelocityTarget.x;
                altCommand = controller.PositionTarget.z;
            }
            else
            {
                aileron = Input.GetAxis("Horizontal");
                rudder = Input.GetAxis("Yaw");
                speedCommand = nominalSpeed + 11.0f * Input.GetAxis("Thrust");
                if(Input.GetAxis("Vertical") != 0.0f)
                    altCommand = -1.0f*controller.ControlPosition.z-10.0f * Input.GetAxis("Vertical");
            }

            //controller.attitudeTarget.x = controller.Airspeed();
            throttle = PlaneControl.AirspeedLoop(speedCommand, controller.ControlWindData.x) + nominalThrottle;
            //controller.attitudeTarget.z = controller.planeControl.AirspeedLoop(speedCommand, controller.Airspeed());

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