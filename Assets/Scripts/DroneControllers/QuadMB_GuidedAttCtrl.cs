using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Guided Att Ctrl")]
    public class QuadMB_GuidedAttCtrl : QuadMovementBehavior
    {
        Vector3 lastVelocityErrorBody;
        float hDotInt;
        float prevTime = 0.0f;
        AttitudeControl attCtrl = new AttitudeControl();

        public override void OnLateUpdate()
        {

            //var nav = controller.controller;
            Vector3 attitude = controller.ControlAttitude;// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.ControlBodyRate;// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.ControlVelocity;// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.ControlPosition;// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 attCmd = Vector3.zero;
            attCmd.x = controller.AttitudeTarget.x;
            attCmd.y = controller.AttitudeTarget.y;

            float yawCmd = controller.AttitudeTarget.z;
            float yawOutput = attCtrl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector2 targetRate = attCtrl.RollPitchLoop(new Vector2(attCmd.x,attCmd.y),attitude);
            Vector2 rollPitchMoment = attCtrl.RollPitchRateLoop(targetRate, angularVelocity); 
            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawOutput);

            float downCmd = controller.VelocityTarget.z;
            float dt = Time.fixedDeltaTime;

            float altOutput = attCtrl.VerticalVelocityLoop(downCmd, attitude, -localVelocity.z,dt,-1.0f*Physics.gravity[1]*controller.ControlMass);            
            
            controller.CommandMoment(totalMoment, altOutput);

        }

        
    }
}