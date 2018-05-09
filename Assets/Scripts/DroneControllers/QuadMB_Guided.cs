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

        public override void OnLateUpdate()
        {

            Vector3 attitude = controller.AttitudeEuler();
            Vector3 angularVelocity = controller.AngularRatesBody();
            Vector3 localVelocity = controller.VelocityLocal();
            Vector3 localPosition = controller.PositionLocal();

            Vector3 targetPosition;
            AttitudeControl attCtrl = controller.attCtrl;
            PositionControl posCtrl = controller.posCtrl;
            // float yawCmd = controller.guidedCommand.w;
            /*
            targetPosition.x = controller.guidedCommand.x;
            targetPosition.y = controller.guidedCommand.y;
            targetPosition.z = controller.guidedCommand.z;
            */
            targetPosition = controller.GetPositionTarget();
            // Debug.Log(string.Format("local postiion {0}, target position {1}", localPosition, targetPosition));

            float yawCmd = posCtrl.YawLoop(controller.attitudeTarget.z, attitude.z);
            float yawMoment = attCtrl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector3 targetVelocity = posCtrl.PositionLoop(targetPosition, localPosition);

            Vector2 targetRollPitch = posCtrl.VelocityLoop(targetVelocity, localVelocity, attitude.z);

            Vector2 targetRate = attCtrl.RollPitchLoop(targetRollPitch, attitude);
            Vector2 rollPitchMoment = attCtrl.RollPitchRateLoop(targetRate, angularVelocity);

            float thrust = controller.attCtrl.VerticalVelocityLoop(-targetVelocity.z, attitude, -localVelocity.z,Time.deltaTime,-1.0f*controller.rb.mass*Physics.gravity[1]);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawMoment);
            controller.CommandTorque(totalMoment);
            controller.CommandThrust(thrust);
        }

        

    }
}