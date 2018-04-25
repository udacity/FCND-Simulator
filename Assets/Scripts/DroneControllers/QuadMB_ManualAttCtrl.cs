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

        public override void OnLateUpdate()
        {

            //var nav = controller.controller;
            var attCtrl = controller.attCtrl;
            Vector3 attitude = controller.AttitudeEuler();// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.AngularRatesBody();// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.VelocityLocal();// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.PositionLocal();// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 attCmd = Vector3.zero;
            attCmd.y = -Input.GetAxis("Vertical");
            attCmd.x = Input.GetAxis("Horizontal");

            float yawCmd = Input.GetAxis("Yaw");
            float altCmd = Input.GetAxis("Thrust");
            if (altCmd > 0.0f)
                altCmd = altCmd * attCtrl.maxAscentRate;
            else
                altCmd = altCmd * attCtrl.maxDescentRate;

            float yawOutput = attCtrl.YawRateLoop(yawCmd, angularVelocity.z);
            Vector2 targetRate = attCtrl.RollPitchLoop(new Vector2(attCmd.x,attCmd.y),attitude);
            Vector2 rollPitchMoment = attCtrl.RollPitchRateLoop(targetRate, angularVelocity);
            float altOutput = attCtrl.VerticalVelocityLoop(altCmd, attitude, -localVelocity.z,Time.deltaTime,-1.0f*controller.rb.mass*Physics.gravity[1]);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawOutput);
            controller.CommandTorque(totalMoment);
            controller.CommandThrust(altOutput);

        }
        

        
	}
}