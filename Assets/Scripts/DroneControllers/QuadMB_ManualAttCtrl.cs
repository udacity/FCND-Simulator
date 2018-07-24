using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
	[CreateAssetMenu (menuName = "MovementBehaviors/Quad Manual Att Ctrl")]
	public class QuadMB_ManualAttCtrl : QuadMovementBehavior
	{
		Vector3 lastVelocityErrorBody;
		float hDotInt;
        float prevTime = 0.0f;

        public override void OnLateUpdate()
        {

            //var nav = controller.controller;
            QuadControl QuadControl = (QuadControl)controller.control;
            Vector3 attitude = controller.ControlAttitude;// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.ControlBodyRate;// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.ControlVelocity;// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.ControlPosition;// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 attCmd = Vector3.zero;
            attCmd.y = -Input.GetAxis("Vertical");
            attCmd.x = Input.GetAxis("Horizontal");

            float yawCmd = Input.GetAxis("Yaw");
            float altCmd = Input.GetAxis("Thrust");
            if (altCmd > 0.0f)
                altCmd = altCmd * QuadControl.maxAscentRate;
            else
                altCmd = altCmd * QuadControl.maxDescentRate;

            float yawOutput = QuadControl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector2 targetRate = QuadControl.RollPitchLoop(new Vector2(attCmd.x,attCmd.y),attitude);
            Vector2 rollPitchMoment = QuadControl.RollPitchRateLoop(targetRate, angularVelocity);
            float dt = Time.fixedDeltaTime;
            float altOutput = QuadControl.VerticalVelocityLoop(altCmd, attitude, -localVelocity.z,dt,-1.0f*controller.ControlMass*Physics.gravity[1]);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawOutput);
            controller.CommandMoment(totalMoment, altOutput);


        }
        

        
	}
}