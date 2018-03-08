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
        public override void OnLateUpdate()
        {


            var nav = controller.controller;
            var attCtrl = controller.attCtrl;
            var posCtrl = controller.posCtrl;

            Vector3 attitude = new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            float cosYaw = Mathf.Cos(attitude.z);
            float sinYaw = Mathf.Sin(attitude.z);


            Vector3 deltaPosition = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 posCmd = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), -Input.GetAxis("Thrust"));
            float yawCmd = Input.GetAxis("Yaw");

            float posCmdNorm = Mathf.Sqrt(posCmd.x * posCmd.x + posCmd.y * posCmd.y);
            float maxDistance = Mathf.Min(10.0f * Mathf.Sqrt(localVelocity.x * localVelocity.x + localVelocity.y * localVelocity.y) + 2.0f, 5.0f);
            if (posCmdNorm > controller.posctl_band)
            {
                deltaPosition.x = posCmd.x / posCmdNorm * maxDistance;
                deltaPosition.y = posCmd.y / posCmdNorm * maxDistance;
                controller.posHoldLocal.x = localPosition.x + deltaPosition.x * cosYaw - deltaPosition.y * sinYaw;
                controller.posHoldLocal.y = localPosition.y + deltaPosition.x * sinYaw + deltaPosition.y * cosYaw;
            }

            float maxDeltaAltitude = 5.0f;
            if (Mathf.Abs(posCmd.z) > controller.posctl_band)
            {
                deltaPosition.z = posCmd.z * maxDeltaAltitude;
                controller.posHoldLocal.z = localPosition.z + deltaPosition.z;
            }

            Vector3 targetPosition = controller.posHoldLocal;
            controller.positionTarget = targetPosition;


            float yawMoment = attCtrl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector3 targetVelocity = posCtrl.PositionLoop(targetPosition, localPosition);
            controller.velocityTarget = targetVelocity;

            Vector2 targetRollPitch = posCtrl.VelocityLoop(targetVelocity, localVelocity, attitude.z);
            controller.attitudeTarget.x = targetRollPitch.x;
            controller.attitudeTarget.y = targetRollPitch.y;


            Vector2 targetRate = attCtrl.RollPitchLoop(targetRollPitch, attitude);
            controller.bodyRateTarget.x = targetRate.x;
            controller.bodyRateTarget.y = targetRate.y;
            controller.bodyRateTarget.z = yawCmd;
            Vector2 rollPitchMoment = attCtrl.RollPitchRateLoop(targetRate, angularVelocity);

            float thrust = controller.attCtrl.VerticalVelocityLoop(-targetVelocity.z, attitude, -localVelocity.z, Time.deltaTime, -1.0f * controller.rb.mass * Physics.gravity[1]);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawMoment);
            nav.CmdTorque(totalMoment);
            nav.CmdThrust(thrust);

        }

    }
}