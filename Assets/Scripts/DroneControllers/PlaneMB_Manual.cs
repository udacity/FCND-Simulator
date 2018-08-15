using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Plane Manual")]
    public class PlaneMB_Manual : PlaneMovementBehavior
    {
        float throttleStep = 30.0f/5000.0f;
        float elevatorTrim = 0.0f;
        float trimStep = 0.001f;
        float throttle, thrust;
        float thrustStep = 0.01f;

        public override void OnSelect(IDroneController _controller)
        {
            Debug.Log("Controller: " + _controller.GetType());
            base.OnSelect(_controller);
            elevatorTrim = 0.0f;
            throttle = controller.ControlTarget.w;
            thrust = controller.MomentThrustTarget.w;
        }

        public override void OnLateUpdate()
        {            
            if (!controller.Guided())
            {
                float elevator, aileron, rudder;//, roll_moment, pitch_moment, yaw_moment;
                elevatorTrim = elevatorTrim + trimStep * Input.GetAxis("Trim");
                elevator = -1.0f * Input.GetAxis("Vertical") + elevatorTrim;
                aileron = Input.GetAxis("Horizontal");
                rudder = Input.GetAxis("Yaw");
                throttle = controller.ControlTarget.w + throttleStep * Input.GetAxis("Thrust");
                throttle = Mathf.Clamp01(throttle);

                // Tried to have the ability to control both manually at the same time, but too difficult and probably not realistic
                /*
                thrust = controller.MomentThrustTarget.w + thrustStep * Input.GetAxis("Thrust Force");
                thrust = Mathf.Clamp01(thrust);
                roll_moment = Input.GetAxis("Roll Moment");
                pitch_moment = Input.GetAxis("Pitch Moment");
                yaw_moment = Input.GetAxis("Yaw Moment");
                */
                controller.CommandControls(aileron, elevator, rudder, throttle);
                //controller.CommandMoment(new Vector3(roll_moment, pitch_moment, yaw_moment), thrust);
                
            }            
        }
    }
}