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
        Vector3 lastVelocityErrorBody;
        float hDotInt;

        public override void OnLateUpdate()
        {
            controller.moveSpeed = 15.0f;
            controller.turnSpeed = 2.0f;
            controller.maxTilt = 0.5f;

            Vector3 pitchYawRoll = controller.controller.eulerAngles * Mathf.Deg2Rad;
            Vector3 qrp = controller.controller.AngularVelocityBody;

            Vector3 prqRate = controller.controller.AngularAccelerationBody;
            Vector3 localPosition;
            localPosition.z = controller.controller.GetLocalNorth();
            localPosition.y = (float)controller.controller.GetAltitude();
            localPosition.x = controller.controller.GetLocalEast();
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

            Vector3 velCmdBody = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Thrust"), Input.GetAxis("Vertical"));
            float yawCmd = Input.GetAxis("Yaw");
            //Outer control loop for from a position/velocity command to a hdot, yaw rate, pitch, roll command

            //If no control input provided (or in guided mode), use position hold
            /*			if ( velCmdBody.sqrMagnitude < controller.posctl_band )
            //			if ( Mathf.Sqrt ( Mathf.Pow ( velCmdBody.x, 2.0f ) + Mathf.Pow ( velCmdBody.y, 2.0f ) + Mathf.Pow ( velCmdBody.z, 2.0f ) ) < controller.posctl_band )
                        {
                            //Set position
                            if ( !controller.pos_set )
                            {
                                controller.posHoldLocal = localPosition;                            
                                controller.pos_set = true;
            //					Debug.Log ( controller.posHoldLocal );
                            }

                            Vector3 posErrorLocal = controller.posHoldLocal - localPosition;
                            Vector3 velCmdLocal;
                            // print("Position Hold: " + posHoldLocal);
                            // print("Local Position: " + localPosition);

                            //Deadband around the position hold
                            if ( Mathf.Sqrt ( Mathf.Pow ( posErrorLocal.x, 2.0f ) + Mathf.Pow ( posErrorLocal.z, 2.0f ) ) < controller.posHoldDeadband )
                            {
                                velCmdLocal.x = 0.0f;
                                velCmdLocal.z = 0.0f;
                            } else
                            {
                                velCmdLocal.x = controller.Kp_pos * posErrorLocal.x;
                                velCmdLocal.z = controller.Kp_pos * posErrorLocal.z;
                            }

                            velCmdLocal.y = controller.Kp_alt * posErrorLocal.y;


                            //Rotate into the local heading frame
                            float cosYaw = Mathf.Cos ( pitchYawRoll.y );
                            float sinYaw = Mathf.Sin ( pitchYawRoll.y );
                            velCmdBody.x = cosYaw * velCmdLocal.x - sinYaw * velCmdLocal.z;
                            velCmdBody.z = sinYaw * velCmdLocal.x + cosYaw * velCmdLocal.z;

                            velCmdBody.y = velCmdLocal.y;
                        } else
                        {
                            controller.pos_set = false;
                        }*/

            //Heading hold if in guided mode or no input
            /*			if ( Mathf.Abs ( yawCmd ) <= 0.0f )
                        {
                            if ( !controller.yawSet )
                            {
                                controller.yawHold = pitchYawRoll.y;
                                controller.yawSet = true;
                            }

                            float yawError = controller.yawHold - pitchYawRoll.y;
                            if ( yawError > Mathf.PI )
                            {
                                yawError = yawError - 2.0f * Mathf.PI;
                            } else
                            if ( yawError < -1.0f * Mathf.PI )
                            {
                                yawError = yawError + 2.0f * Mathf.PI;
                            }
                            yawCmd = controller.Kp_yaw * yawError;
                        } else
                        {
                            controller.yawSet = false;
                        }*/

            /*
            //Control loop from a body velocity command to a Hdot, yaw rate, pitch, and roll command
            float yawError = 0.0f - pitchYawRoll.y;
            if (yawError > Mathf.PI)
            {
                yawError = yawError - 2.0f * Mathf.PI;
            }
            else
            if (yawError < -1.0f * Mathf.PI)
            {
                yawError = yawError + 2.0f * Mathf.PI;
            }

            yawCmd = controller.Kp_yaw * (0.0f - pitchYawRoll.y);
			*/

            Vector3 velocityErrorBody = Vector3.zero;
            Vector3 velocityErrorBodyD = Vector3.zero;
            velocityErrorBody.x = controller.moveSpeed * velCmdBody.x - linearVelocity.x;// (bodyVelocity.x);
            velocityErrorBody.z = controller.moveSpeed * velCmdBody.z - linearVelocity.z;// (bodyVelocity.z);
            velocityErrorBodyD = (velocityErrorBody - lastVelocityErrorBody) / Time.deltaTime;
            lastVelocityErrorBody = velocityErrorBody;

            angle_input[2] = controller.Kp_vel * velocityErrorBody.z + controller.Kd_vel * velocityErrorBodyD.z;
            angle_input[3] = -controller.Kp_vel * velocityErrorBody.x + -controller.Kd_vel * velocityErrorBodyD.x;

            float angle_magnitude = Mathf.Sqrt(Mathf.Pow(angle_input[2], 2.0f) + Mathf.Pow(angle_input[3], 2.0f));
            if (angle_magnitude > controller.maxTilt)
            {
                angle_input[2] = controller.maxTilt * angle_input[2] / angle_magnitude;
                angle_input[3] = controller.maxTilt * angle_input[3] / angle_magnitude;
            }


            angle_input[0] = velCmdBody.y;
            angle_input[1] = yawCmd;

            //Constrain the angle inputs between -1 and 1 (tilt, turning speed, and vert speed taken into account later)
            angle_input[1] = Mathf.Clamp(angle_input[1], -1f, 1f);

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

            controller.controller.ApplyMotorForce(thrust);
            controller.controller.ApplyMotorTorque(total_moment);
        }
    }
}