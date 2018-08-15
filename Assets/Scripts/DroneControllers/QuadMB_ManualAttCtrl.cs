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


        public override void OnLateUpdate()
        {

            //var nav = controller.controller;
            QuadControl QuadControl = (QuadControl)controller.control;
            Vector3 attitude = controller.ControlAttitude;// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.ControlBodyRate;// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.ControlVelocity;// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.ControlPosition;// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 attCmd = Vector3.zero;
            float altCmd, yawCmd;
            if (!controller.Guided())
            {
                attCmd.y = -Input.GetAxis("Vertical");
                attCmd.x = Input.GetAxis("Horizontal");

                yawCmd = Input.GetAxis("Yaw");
                altCmd = Input.GetAxis("Thrust");
            }
            else
            {
                attCmd.y = controller.AttitudeTarget.y;
                attCmd.x = controller.AttitudeTarget.x;

                altCmd = controller.VelocityTarget.z;
                yawCmd = controller.BodyRateTarget.z;
            }
            if (altCmd > 0.0f)
                altCmd = altCmd * QuadControl.maxAscentRate;
            else
                altCmd = altCmd * QuadControl.maxDescentRate;

            float yawOutput = QuadControl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector2 targetRate = QuadControl.RollPitchLoop(new Vector2(attCmd.x,attCmd.y),attitude);
            Vector3 bodyRateTarget = controller.BodyRateTarget;
            bodyRateTarget.x = targetRate.x;
            bodyRateTarget.y = targetRate.y;
            bodyRateTarget.z = yawCmd;
            controller.BodyRateTarget = bodyRateTarget;
            Vector2 rollPitchMoment = QuadControl.RollPitchRateLoop(targetRate, angularVelocity);
            float dt = Time.fixedDeltaTime;
            float altOutput = QuadControl.VerticalVelocityLoop(altCmd, attitude, -localVelocity.z,dt,0.38f);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawOutput);
            controller.CommandMoment(totalMoment, altOutput);


        }
        

        
	}
}