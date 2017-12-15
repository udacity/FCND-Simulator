using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
	[CreateAssetMenu (menuName = "MovementBehaviors/Quad Manual Pos Ctrl")]
	public class QuadMB_ManualPosCtrl : QuadMovementBehavior
	{
		Vector3 lastVelocityErrorBody;
		float hDotInt;

        public override void OnLateUpdate()
        {
            controller.moveSpeed = 15.0f;
            controller.turnSpeed = 2.0f;
            controller.maxTilt = 0.5f;
            var nav = controller.controller;
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
            float maxDistance = 0.5f*Mathf.Sqrt(localVelocity.x*localVelocity.x+localVelocity.y*localVelocity.y)+1.0f;
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

            Debug.Log("Target Position: " + targetPosition);
            float yawOutput = YawRateControl(yawCmd, angularVelocity.z);
            Vector3 posOutput = PositionControl(targetPosition, attitude, angularVelocity, localVelocity,localPosition);
            //posOutput = VelocityControl(controller.moveSpeed*posCmd, attitude, angularVelocity, localVelocity);
            Vector3 totalMoment = new Vector3(posOutput.y, posOutput.z, yawOutput);
            nav.CmdTorque(totalMoment);
            nav.CmdThrust(posOutput.x);
        }

        private Vector3 PositionControl(Vector3 targetPosition, Vector3 attitude, Vector3 angularVelocity, Vector3 localVelocity, Vector3 localPosition)
        {
            Vector3 positionError = targetPosition - localPosition;
            Vector3 velocityCmd = Vector3.zero;

            if(Mathf.Sqrt(positionError.x*positionError.x+positionError.y*positionError.y) >= controller.posHoldDeadband)
            {
                velocityCmd.x = controller.Kp_pos * positionError.x;
                velocityCmd.y = controller.Kp_pos * positionError.y;
            }

            velocityCmd.z = controller.Kp_alt * positionError.z;

            return VelocityControl(velocityCmd, attitude, angularVelocity, localVelocity);
        }

        private Vector3 VelocityControl(Vector3 targetVelocity,Vector3 attitude,Vector3 angularVelocity, Vector3 localVelocity)
        {
            float targetSpeed = Mathf.Sqrt(targetVelocity.x * targetVelocity.x + targetVelocity.y * targetVelocity.y);
            if(targetSpeed > controller.moveSpeed)
            {
                targetVelocity.x = controller.moveSpeed * targetVelocity.x / targetSpeed;
                targetVelocity.y = controller.moveSpeed * targetVelocity.y / targetSpeed;
            }

            float cosYaw = Mathf.Cos(attitude.z);
            float sinYaw = Mathf.Sin(attitude.z);

            Vector3 velError = targetVelocity - localVelocity;
            Vector3 velErrorBody = Vector3.zero;
            velErrorBody.x = cosYaw * velError.x + sinYaw * velError.y;
            velErrorBody.y = -sinYaw * velError.x + cosYaw * velError.y;

            float pitchCmd = -controller.Kp_vel * velErrorBody.x;
            float rollCmd = controller.Kp_vel * velErrorBody.y;
            Vector3 attitudeCmd = new Vector3(rollCmd, pitchCmd, 0.0f);
            Vector2 rollPitchMoment = RollPitchControl(attitudeCmd, attitude, angularVelocity);
            float thrust = VerticalVelocityControl(-targetVelocity.z, attitude, -localVelocity.z);
            return new Vector3(thrust, rollPitchMoment.x, rollPitchMoment.y);
        }

        private float YawControl(float targetYaw, float yaw, float yawrate)
        {
            targetYaw = targetYaw % (2.0f * Mathf.PI);
            float yawError = targetYaw - yaw;
            if (yawError > Mathf.PI)
                yawError = yawError - 2.0f * Mathf.PI;
            else if (yawError < -Mathf.PI)
                yawError = yawError + 2.0f * Mathf.PI;

            float yawrateCmd = controller.Kp_yaw * yawError;
            float yawMoment = YawRateControl(yawrateCmd, yawrate);
            return yawMoment;
        } 

        
        private float YawRateControl(float targetYawrate,float yawrate)
        {
            float yawMoment = controller.Kp_r * (targetYawrate - yawrate);
            return yawMoment;
        }

        private float VerticalVelocityControl(float targetVerticalVelocity, Vector3 attitude, float verticalVelocity)
        {
            float thrustNom = -1.0f * controller.rb.mass * Physics.gravity[1];
            float dt = Time.deltaTime;
            if (targetVerticalVelocity > controller.maxAscentRate)
                targetVerticalVelocity = controller.maxAscentRate;
            else if (targetVerticalVelocity < -controller.maxDescentRate)
                targetVerticalVelocity = -controller.maxDescentRate;

            float hDotError = targetVerticalVelocity - verticalVelocity;
            hDotInt += hDotError * dt;

            float thrust = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrustNom) / (Mathf.Cos(attitude.x) * Mathf.Cos(attitude.y));
            return thrust;
        }

        //Command a desired roll and pitch using a PD cascading controller. Returns a roll and pitch moment
        private Vector2 RollPitchControl(Vector3 targetAttitude, Vector3 attitude, Vector3 angularRate)
        {
            //Enfoce a maximum tilt angle
            float angleMagnitude = Mathf.Sqrt(targetAttitude.x * targetAttitude.x + targetAttitude.y * targetAttitude.y);
            if (angleMagnitude > controller.maxTilt)
            {
                targetAttitude = controller.maxTilt * targetAttitude / angleMagnitude;
            }

            //PD control on roll
            float roll = attitude.x;
            float rollrate = angularRate.x;
            float targetRoll = targetAttitude.x;
            targetRoll = targetRoll % (2.0f * Mathf.PI);

            float rollError = targetRoll - roll;
            if (rollError > Mathf.PI)
                rollError = rollError - 2.0f * Mathf.PI;
            else if (rollError < -Mathf.PI)
                rollError = rollError + 2.0f * Mathf.PI;

            float rollrateError = controller.Kp_roll * rollError - rollrate;
            float rollMoment = controller.Kp_p * rollrateError;

            //PD control on pitch
            float pitch = attitude.y;
            float pitchrate = angularRate.y;
            float targetPitch = targetAttitude.y;
            targetPitch = targetPitch % (2.0f * Mathf.PI);

            float pitchError = targetPitch - pitch;
            if (pitchError > Mathf.PI)
                pitchError = pitchError - 2.0f * Mathf.PI;
            else if (pitchError < -Mathf.PI)
                pitchError = pitchError + 2.0f * Mathf.PI;

            float pitchrateError = controller.Kp_pitch * pitchError - pitchrate;
            float pitchMoment = controller.Kp_q * pitchrateError;

            return new Vector2(rollMoment, pitchMoment);
        }

        /*
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

            Vector3 linearVelocity = controller.controller.LinearVelocity;
            linearVelocity.x = linearVelocity.x * Mathf.Cos(pitchYawRoll.y) - linearVelocity.z * Mathf.Sin(pitchYawRoll.y);
            linearVelocity.z = linearVelocity.x * Mathf.Sin(pitchYawRoll.y) + linearVelocity.z * Mathf.Cos(pitchYawRoll.y);

            //Direct Control of the moments
            Vector3 thrust = Vector3.zero;
			Vector3 yaw_moment = Vector3.zero;
			Vector3 pitch_moment = Vector3.zero;
			Vector3 roll_moment = Vector3.zero;
			Vector4 angle_input = Vector4.zero;

            Vector3 velCmdBody = new Vector3();
            Vector3 deltaPosition = new Vector3(0.0f,0.0f,0.0f);
            Vector3 posCmd = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Thrust"), Input.GetAxis("Vertical"));
			float yawCmd = Input.GetAxis ( "Yaw" );


            float cosYaw = Mathf.Cos(pitchYawRoll.y);
            float sinYaw = Mathf.Sin(pitchYawRoll.y);
            float posCmdNorm = Mathf.Sqrt(posCmd.x * posCmd.x + posCmd.z * posCmd.z);
            float maxDistance = 5.0f;
            if (posCmdNorm > controller.posctl_band)
            {
                
                deltaPosition.x = posCmd.x/posCmdNorm* maxDistance;
                deltaPosition.z = posCmd.z / posCmdNorm * maxDistance;
                controller.posHoldLocal.x = localPosition.x + deltaPosition.x * cosYaw + deltaPosition.z * sinYaw;
                controller.posHoldLocal.z = localPosition.z - deltaPosition.x * sinYaw + deltaPosition.z * cosYaw;
            }

            float maxDeltaAltitude = 5.0f;
            if(Mathf.Abs(posCmd.y) > controller.posctl_band)
            {
                deltaPosition.y = posCmd.y * maxDeltaAltitude;
                controller.posHoldLocal.y = localPosition.y + deltaPosition.y;
            }


            
            Vector3 posErrorLocal = controller.posHoldLocal - localPosition;
            Vector3 velCmdLocal;

            //Deadband around the position hold
            if (Mathf.Sqrt(Mathf.Pow(posErrorLocal.x, 2.0f) + Mathf.Pow(posErrorLocal.z, 2.0f)) < controller.posHoldDeadband)
            {
                velCmdLocal.x = 0.0f;
                velCmdLocal.z = 0.0f;
            }
            else
            {
                velCmdLocal.x = controller.Kp_pos * posErrorLocal.x;
                velCmdLocal.z = controller.Kp_pos * posErrorLocal.z;
            }
            //Deadband around the position hold
            if (Mathf.Sqrt(Mathf.Pow(posErrorLocal.x, 2.0f) + Mathf.Pow(posErrorLocal.z, 2.0f)) < controller.posHoldDeadband)
            {
                velCmdLocal.x = 0.0f;
                velCmdLocal.z = 0.0f;
            }

            velCmdLocal.y = controller.Kp_alt * posErrorLocal.y;

            //Rotate into the local heading frame
            velCmdBody.x = cosYaw * velCmdLocal.x - sinYaw * velCmdLocal.z;
            velCmdBody.z = sinYaw * velCmdLocal.x + cosYaw * velCmdLocal.z;
            velCmdBody.y = velCmdLocal.y;
            

            //Horizontal Controller
            Vector3 velocityErrorBody = Vector3.zero;
			Vector3 velocityErrorBodyD = Vector3.zero;
            velocityErrorBody.x = controller.moveSpeed * velCmdBody.x - linearVelocity.x;// (bodyVelocity.x);
            velocityErrorBody.z = controller.moveSpeed * velCmdBody.z - linearVelocity.z;// (bodyVelocity.z);
            velocityErrorBodyD = (velocityErrorBody - lastVelocityErrorBody) / Time.deltaTime;
			lastVelocityErrorBody = velocityErrorBody;

			angle_input[2] = controller.Kp_vel * velocityErrorBody.z + controller.Kd_vel * velocityErrorBodyD.z;
			angle_input[3] = -controller.Kp_vel * velocityErrorBody.x + -controller.Kd_vel * velocityErrorBodyD.x;

			float angle_magnitude = Mathf.Sqrt(Mathf.Pow(angle_input[2], 2.0f) + Mathf.Pow(angle_input[3], 2.0f));
			if ( angle_magnitude > controller.maxTilt )
			{
				angle_input [ 2 ] = controller.maxTilt * angle_input [ 2 ] / angle_magnitude;
				angle_input [ 3 ] = controller.maxTilt * angle_input [ 3 ] / angle_magnitude;
			}

            //Vertical controller
            angle_input[0] = velCmdBody.y;

            //Yaw controller
            angle_input[1] = yawCmd;

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
			float thrust_nom = -1.0f * controller.rb.mass * Physics.gravity[1];
			float hDotError = 0.0f;
			if (angle_input[0] > 0.0f)
			{
				hDotError = (controller.maxAscentRate * angle_input[0] - 1.0f * controller.controller.LinearVelocity.y);
			}
			else
			{
				hDotError = (controller.maxDescentRate * angle_input[0] - 1.0f * controller.controller.LinearVelocity.y);
			}
			hDotInt = hDotInt + hDotError * Time.deltaTime;

			//hdot to thrust
			thrust[1] = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(pitchYawRoll.x) * Mathf.Cos(pitchYawRoll.z));

			//yaw rate to yaw moment
			yaw_moment[1] = controller.Kp_r * (controller.turnSpeed * angle_input[1] - qrp.y);


			//angle to angular rate command (for pitch and roll)
			float pitchError = angle_input[2] - pitchYawRoll.x;
			float rollError = angle_input[3] - pitchYawRoll.z;
			float pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
			float rollRateError = controller.Kp_roll * rollError - qrp.z;

			//angular rate to moment (pitch and roll)
			pitch_moment[0] = controller.Kp_q * pitchRateError;
			roll_moment[2] = controller.Kp_p * rollRateError;

			Vector3 total_moment = yaw_moment + pitch_moment + roll_moment;
            
			controller.controller.ApplyMotorForce ( thrust );
			controller.controller.ApplyMotorTorque ( total_moment );
		}
        */
	}
}