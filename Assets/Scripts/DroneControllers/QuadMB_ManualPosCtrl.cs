using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Manual Pos Ctrl")]
    public class QuadMB_ManualPosCtrl : QuadMovementBehavior
    {
        float prevTime = 0.0f;
        public override void OnLateUpdate()
        {


            //var QuadControl = controller.QuadControl;
            //var QuadControl = controller.QuadControl;
            QuadControl QuadControl = (QuadControl)controller.control;

            Vector3 attitude = controller.ControlAttitude;// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.ControlBodyRate;// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.ControlVelocity;// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.ControlPosition;// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            float cosYaw = Mathf.Cos(attitude.z);
            float sinYaw = Mathf.Sin(attitude.z);


            Vector3 deltaPosition = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 posCmd = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), -Input.GetAxis("Thrust"));
            float yawCmd = Input.GetAxis("Yaw");

            float posCmdNorm = Mathf.Sqrt(posCmd.x * posCmd.x + posCmd.y * posCmd.y);
            float maxDistance = Mathf.Min(10.0f * Mathf.Sqrt(localVelocity.x * localVelocity.x + localVelocity.y * localVelocity.y) + 2.0f, 5.0f);
            Vector3 posHoldLocal = controller.PositionTarget;
            if (posCmdNorm > QuadControl.posctl_band)
            {
                deltaPosition.x = posCmd.x / posCmdNorm * maxDistance;
                deltaPosition.y = posCmd.y / posCmdNorm * maxDistance;
                posHoldLocal.x = localPosition.x + deltaPosition.x * cosYaw - deltaPosition.y * sinYaw;
                posHoldLocal.y = localPosition.y + deltaPosition.x * sinYaw + deltaPosition.y * cosYaw;
            }

            float maxDeltaAltitude = 5.0f;
            if (Mathf.Abs(posCmd.z) > QuadControl.posctl_band)
            {
                deltaPosition.z = posCmd.z * maxDeltaAltitude;
                posHoldLocal.z = localPosition.z + deltaPosition.z;
            }

            controller.PositionTarget = posHoldLocal;


            float yawMoment = QuadControl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector3 targetVelocity = QuadControl.PositionLoop(controller.PositionTarget, localPosition);
            controller.VelocityTarget = targetVelocity;

            Vector2 targetRollPitch = QuadControl.VelocityLoop(targetVelocity, localVelocity, attitude.z);
            Vector3 attitudeTarget = controller.AttitudeTarget;
            attitudeTarget.x = targetRollPitch.x;
            attitudeTarget.y = targetRollPitch.y;
            controller.AttitudeTarget = attitudeTarget;


            Vector2 targetRate = QuadControl.RollPitchLoop(targetRollPitch, attitude);
            Vector3 bodyRateTarget = controller.BodyRateTarget;
            bodyRateTarget.x = targetRate.x;
            bodyRateTarget.y = targetRate.y;
            bodyRateTarget.z = yawCmd;
            controller.BodyRateTarget = bodyRateTarget;
            Vector2 rollPitchMoment = QuadControl.RollPitchRateLoop(targetRate, angularVelocity);

            float dt = Time.fixedDeltaTime;

            float thrust = QuadControl.VerticalVelocityLoop(-targetVelocity.z, attitude, -localVelocity.z, dt, 0.5f);
            

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawMoment);
            controller.CommandMoment(totalMoment, thrust);
        }

    }
}