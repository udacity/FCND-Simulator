using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
	[CreateAssetMenu (menuName = "MovementBehaviors/Quad Manual")]
	public class QuadMB_Manual : QuadMovementBehavior
	{
		Vector4 angle_input;
		Vector3 lastVelocityErrorBody;
		float hDotInt;

		public override void OnSelect (SimpleQuadController _controller)
		{
			base.OnSelect ( _controller );
			angle_input = Vector4.zero;
		}

		public override void OnLateUpdate ()
		{
			controller.moveSpeed = 15.0f;
			controller.turnSpeed = 2.0f;
			controller.maxTilt = 0.5f;

			Vector3 pitchYawRoll = controller.controller.eulerAngles * Mathf.PI / 180.0f;            
			Vector3 qrp = controller.controller.AngularVelocityBody;

			Vector3 prqRate = controller.controller.AngularAccelerationBody;
			Vector3 localPosition;
			localPosition.z = controller.controller.GetLocalNorth ();
			localPosition.y = (float) controller.controller.GetAltitude ();
			localPosition.x = controller.controller.GetLocalEast ();
			Vector3 bodyVelocity = controller.controller.BodyVelocity;

			//Direct Control of the moments
			Vector3 thrust = Vector3.zero;
			Vector3 yaw_moment = Vector3.zero;
			Vector3 pitch_moment = Vector3.zero;
			Vector3 roll_moment = Vector3.zero;
//			Vector4 angle_input = Vector4.zero;

			//Outer control loop for from a position/velocity command to a hdot, yaw rate, pitch, roll command
			controller.pos_set = false;
			controller.yawSet = false;
			//Pilot Input: Hdot, Yawrate, pitch, roll
			angle_input += new Vector4 ( Input.GetAxis ( "Thrust" ), Input.GetAxis ( "Yaw" ), Input.GetAxis ( "Vertical" ) * controller.maxTilt, -Input.GetAxis ( "Horizontal" ) * controller.maxTilt ) * Time.deltaTime;

			//Constrain the angle inputs between -1 and 1 (tilt, turning speed, and vert speed taken into account later)
			angle_input [ 1 ] = Mathf.Clamp ( angle_input [ 1 ], -1f, 1f );
//			for (int i = 1; i < 2; i++)
//			{
//				if (angle_input[i] > 1.0f)
//					angle_input[i] = 1.0f;
//				else if (angle_input[i] < -1.0f)
//					angle_input[i] = -1.0f;
//			}

			//Inner control loop: angle commands to forces
			if ( controller.stabilized )
			{
				float thrust_nom = -1.0f * controller.rb.mass * Physics.gravity [ 1 ];
				float hDotError = 0.0f;
				if ( angle_input [ 0 ] > 0.0f )
				{
					hDotError = ( controller.maxAscentRate * angle_input [ 0 ] - 1.0f * controller.controller.LinearVelocity.y );
				} else
				{
					hDotError = ( controller.maxDescentRate * angle_input [ 0 ] - 1.0f * controller.controller.LinearVelocity.y );
				}
				hDotInt = hDotInt + hDotError * Time.deltaTime;

				//hdot to thrust
				thrust [ 1 ] = ( controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom ) / ( Mathf.Cos ( pitchYawRoll.x ) * Mathf.Cos ( pitchYawRoll.z ) );

				//yaw rate to yaw moment
				yaw_moment [ 1 ] = controller.Kp_r * ( controller.turnSpeed * angle_input [ 1 ] - qrp.y );


				//angle to angular rate command (for pitch and roll)
				float pitchError = angle_input [ 2 ] - pitchYawRoll.x;
				float rollError = angle_input [ 3 ] - pitchYawRoll.z;
				float pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
				float rollRateError = controller.Kp_roll * rollError - qrp.z;

				//angular rate to moment (pitch and roll)
				pitch_moment [ 0 ] = controller.Kp_q * pitchRateError;
				roll_moment [ 2 ] = controller.Kp_p * rollRateError;
			}
			else //User controls forces directly (not updated, do not use)
			{
				thrust = controller.thrustForce * ( new Vector3 ( 0.0f, angle_input [ 0 ], 0.0f ) );
				yaw_moment = controller.thrustMoment * ( new Vector3 ( 0.0f, angle_input [ 1 ], 0.0f ) );
				pitch_moment = controller.thrustMoment * ( new Vector3 ( angle_input [ 2 ] * Mathf.Sqrt ( 2.0f ) / 2.0f, 0.0f, angle_input [ 2 ] * Mathf.Sqrt ( 2.0f ) / 2.0f ) );
				roll_moment = controller.thrustMoment * ( new Vector3 ( angle_input [ 3 ] * Mathf.Sqrt ( 2.0f ) / 2.0f, 0.0f, -1.0f * angle_input [ 3 ] * Mathf.Sqrt ( 2.0f ) / 2.0f ) );
			}

			Vector3 total_moment = yaw_moment + pitch_moment + roll_moment;

			controller.controller.ApplyRotorForce ( thrust );
			controller.controller.ApplyRotorTorque ( total_moment );
		}
	}
}