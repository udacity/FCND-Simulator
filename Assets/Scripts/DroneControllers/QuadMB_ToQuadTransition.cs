using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/To Quad Transition")]
    public class QuadMB_ToQuadTransition : QuadMovementBehavior
    {

        float altCommand;
        float rollCommand = 0f;
        float pitchCommand = 0f;
        float yawRateCommand = 0f;
        

        PlaneControl PlaneControl;
        QuadPlaneControl QuadPlaneControl;
        QuadControl QuadControl;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            QuadPlaneControl = (QuadPlaneControl)controller.control;
            PlaneControl = QuadPlaneControl.PlaneControl;
            QuadControl = QuadPlaneControl.QuadControl;

            PlaneControl.altInt = 0.0f;
            PlaneControl.speedInt = 0.0f;
            PlaneControl.sideslipInt = 0f;
            altCommand = -controller.ControlPosition.z;

        }

        public override void OnLateUpdate()
        {
            float aileron, elevator, rudder, throttle, thrust;
            Vector3 totalMoment;

            float airspeed = controller.ControlWindData.x;


            throttle = QuadPlaneControl.toQuadThrottle;
            aileron = 0f;
            elevator = 0f;
            rudder = 0;

            float yawOutput = QuadControl.YawRateLoop(yawRateCommand, controller.ControlBodyRate.z);
            Vector3 targetVelocity = QuadControl.PositionLoop(new Vector3(0f,0f,-altCommand), controller.ControlPosition);
            Vector2 targetRate = QuadControl.RollPitchLoop(new Vector2(pitchCommand, rollCommand), controller.ControlAttitude);
            Vector2 rollPitchMoment = QuadControl.RollPitchRateLoop(targetRate, controller.ControlBodyRate);
            float dt = Time.fixedDeltaTime;
            thrust = QuadControl.VerticalVelocityLoop(-targetVelocity.z, controller.ControlAttitude, -controller.ControlVelocity.z, dt, 0.5f);

            totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawOutput);
            controller.CommandMoment(totalMoment, thrust);
            controller.CommandControls(aileron, elevator, rudder, throttle);




        }
    }
}