using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using DroneSensors;
using DroneInterface;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Guided")]
    public class QuadMB_Guided : QuadMovementBehavior
    {
        float prevTime = 0.0f;
        QuadControl QuadControl;
        public override void OnLateUpdate()
        {

            Vector3 attitude = controller.ControlAttitude;
            Vector3 angularVelocity = controller.ControlBodyRate;
            Vector3 localVelocity = controller.ControlVelocity;
            Vector3 localPosition = controller.ControlPosition;

            Vector3 targetPosition;
            QuadControl = (QuadControl)controller.control;

            // float yawCmd = controller.guidedCommand.w;
            /*
            targetPosition.x = controller.guidedCommand.x;
            targetPosition.y = controller.guidedCommand.y;
            targetPosition.z = controller.guidedCommand.z;
            */
            targetPosition = controller.PositionTarget;
            // Debug.Log(string.Format("local postiion {0}, target position {1}", localPosition, targetPosition));

            float yawCmd = QuadControl.YawLoop(controller.AttitudeTarget.z, attitude.z);
            float yawMoment = QuadControl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector3 targetVelocity = QuadControl.PositionLoop(targetPosition, localPosition);

            Vector2 targetRollPitch = QuadControl.VelocityLoop(targetVelocity, localVelocity, attitude.z);

            Vector2 targetRate = QuadControl.RollPitchLoop(targetRollPitch, attitude);
            Vector2 rollPitchMoment = QuadControl.RollPitchRateLoop(targetRate, angularVelocity);

            float dt = Time.fixedDeltaTime;
            /*
            if (prevTime != 0.0f)
                dt = controller.quadVehicle.FlightTime()-prevTime;
            prevTime = controller.quadVehicle.FlightTime();
            */
            float thrust = QuadControl.VerticalVelocityLoop(-targetVelocity.z, attitude, -localVelocity.z,dt,-1.0f*controller.ControlMass*Physics.gravity[1]);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawMoment);
            controller.CommandMoment(totalMoment,thrust);
        }

        

    }
}