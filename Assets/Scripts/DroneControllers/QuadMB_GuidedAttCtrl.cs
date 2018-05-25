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
            Vector3 attitude = controller.AttitudeEuler();// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.AngularRatesBody();// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.VelocityLocal();// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.PositionLocal();// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 attCmd = Vector3.zero;
            attCmd.x = controller.guidedCommand.x;
            attCmd.y = controller.guidedCommand.y;

            float yawCmd = controller.guidedCommand.w;
            float yawOutput = attCtrl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector2 targetRate = attCtrl.RollPitchLoop(new Vector2(attCmd.x,attCmd.y),attitude);
            Vector2 rollPitchMoment = attCtrl.RollPitchRateLoop(targetRate, angularVelocity); 
            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawOutput);

            float downCmd = controller.guidedCommand.z;
            float dt = 0.0f;
            if (prevTime != 0.0f)
                dt = controller.quadVehicle.FlightTime() - prevTime;
            prevTime = controller.quadVehicle.FlightTime();
            float altOutput = attCtrl.VerticalVelocityLoop(downCmd, attitude, -localVelocity.z,dt,-1.0f*Physics.gravity[1]*controller.rb.mass);            
            
            controller.CommandTorque(totalMoment);
            controller.CommandThrust(altOutput);

        }

        
    }
}