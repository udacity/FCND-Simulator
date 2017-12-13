using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Guided")]
    public class QuadMB_Guided : QuadMovementBehavior
    {
        Vector3 lastVelocityErrorBody;
        float hDotInt;

        public override void RemoteUpdate(float thrust, float pitchRate, float yawRate, float rollRate)
        {
            var pitchYawRoll = new Vector3(controller.controller.GetPitch(), controller.controller.GetYaw(), controller.controller.GetRoll());
            Vector3 qrp = controller.controller.AngularVelocityBody;

            // Debug.Log(string.Format("{0} {1} {2} {3}", thrust, pitchRate, yawRate, rollRate));
            // Debug.Log(pitchYawRoll);

            // // Direct Control of the moments
            // var thrustV = Vector3.zero;

            // // Inner control loop: angle commands to forces
            // float thrust_nom = -controller.rb.mass * Physics.gravity[1];
            // float hDotError = 0.0f;
            // if (thrust > 0.0f)
            // {
            //     hDotError = (controller.maxAscentRate * thrust - 1.0f * controller.controller.LinearVelocity.y);
            // }
            // else
            // {
            //     hDotError = (controller.maxDescentRate * thrust - 1.0f * controller.controller.LinearVelocity.y);
            // }
            // hDotInt = hDotInt + hDotError * Time.deltaTime;

            // // hdot to thrust
            // thrustV[1] = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(pitchYawRoll.x) * Mathf.Cos(pitchYawRoll.z));

            // angle to angular rate command (for pitch and roll)
            float pitchError = pitchRate - pitchYawRoll.x;
            float rollError = rollRate - pitchYawRoll.z;
            float pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
            float rollRateError = controller.Kp_roll * rollError - qrp.z;

            // angular rate to moment (pitch and roll)
            var pitchMoment = controller.Kp_q * pitchRateError;
            var rollMoment = controller.Kp_p * rollRateError;
            // yaw rate to yaw moment
            var yawMoment = controller.Kp_r * (controller.turnSpeed * yawRate - qrp.y);

            // var pitchMoment = pitchRate;
            // var yawMoment = yawRate;
            // var rollMoment = rollRate;
            var thrustV = new Vector3(0, thrust, 0);

            var totalMoment = new Vector3(pitchMoment, yawMoment, rollMoment);

            Debug.Log(string.Format("thrust vector {0}, moments vector {1}", thrustV, totalMoment));

            controller.controller.ApplyMotorForce(thrustV);
            controller.controller.ApplyMotorTorque(totalMoment);
        }

        public override void OnLateUpdate()
        {
            controller.moveSpeed = 15.0f;
            controller.turnSpeed = 2.0f;
            controller.maxTilt = 0.5f;

            var pitch = controller.controller.GetPitch();
            var yaw = controller.controller.GetYaw();
            var roll = controller.controller.GetRoll();

            var pitchYawRoll = new Vector3(pitch, yaw, roll);
            Vector3 qrp = controller.controller.AngularVelocityBody;

            Vector3 prqRate = controller.controller.AngularAccelerationBody;
            Vector3 localPosition;
            localPosition.z = controller.controller.GetLocalNorth();
            localPosition.y = (float)controller.controller.GetAltitude();
            localPosition.x = controller.controller.GetLocalEast();
            Vector3 bodyVelocity = controller.controller.BodyVelocity;

            //Direct Control of the moments
            Vector3 thrustV = Vector3.zero;
            Vector3 yawMoment = Vector3.zero;
            Vector3 pitchMoment = Vector3.zero;
            Vector3 rollMoment = Vector3.zero;
            Vector4 angle_input = Vector4.zero;

            //Outer control loop for from a position/velocity command to a hdot, yaw rate, pitch, roll command
            //If no control input provided (or in guided mode), use position hold
            Vector3 velCmdBody = Vector3.zero;
            float yawCmd = 0;

            //Set position
            if (!controller.pos_set)
            {
                controller.posHoldLocal = localPosition;
                controller.pos_set = true;
                //				Debug.Log ( controller.posHoldLocal );
            }

            Vector3 posErrorLocal = controller.posHoldLocal - localPosition;
            Vector3 velCmdLocal;
            // print("Position Hold: " + posHoldLocal);
            // print("Local Position: " + localPosition);

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

            velCmdLocal.y = controller.Kp_alt * posErrorLocal.y;
            // Debug.Log(controller.Kp_alt);

            //Rotate into the local heading frame
            float cosYaw = Mathf.Cos(pitchYawRoll.y);
            float sinYaw = Mathf.Sin(pitchYawRoll.y);
            velCmdBody.x = cosYaw * velCmdLocal.x - sinYaw * velCmdLocal.z;
            velCmdBody.z = sinYaw * velCmdLocal.x + cosYaw * velCmdLocal.z;

            velCmdBody.y = velCmdLocal.y;

            //Heading hold if in guided mode or no input
            if (!controller.yawSet)
            {
                controller.yawHold = pitchYawRoll.y;
                controller.yawSet = true;
            }

            float yawError = controller.yawHold - pitchYawRoll.y;
            if (yawError > Mathf.PI)
            {
                yawError = yawError - 2.0f * Mathf.PI;
            }
            else if (yawError < -1.0f * Mathf.PI)
            {
                yawError = yawError + 2.0f * Mathf.PI;
            }
            yawCmd = controller.Kp_yaw * yawError;
            // yawCmd = 0f;

            //Control loop from a body velocity command to a Hdot, yaw rate, pitch, and roll command
            Vector3 velocityErrorBody = Vector3.zero;
            Vector3 velocityErrorBodyD = Vector3.zero;
            velocityErrorBody.x = controller.moveSpeed * velCmdBody.x - bodyVelocity.x;
            velocityErrorBody.z = controller.moveSpeed * velCmdBody.z - bodyVelocity.z;
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
            angle_input[2] = Mathf.Clamp(angle_input[2], -1f, 1f);
            angle_input[3] = Mathf.Clamp(angle_input[3], -1f, 1f);

            //Inner control loop: angle commands to forces
            if (controller.stabilized)
            {
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
                thrustV[1] = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(pitchYawRoll.x) * Mathf.Cos(pitchYawRoll.z));

                //yaw rate to yaw moment
                yawMoment[1] = controller.Kp_r * (controller.turnSpeed * angle_input[1] - qrp.y);


                //angle to angular rate command (for pitch and roll)
                float pitchError = angle_input[2] - pitchYawRoll.x;
                float rollError = angle_input[3] - pitchYawRoll.z;
                float pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
                float rollRateError = controller.Kp_roll * rollError - qrp.z;

                //angular rate to moment (pitch and roll)
                pitchMoment[0] = controller.Kp_q * pitchRateError;
                rollMoment[2] = controller.Kp_p * rollRateError;
            }
            else //User controls forces directly (not updated, do not use)
            {
                thrustV = controller.thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
                yawMoment = controller.thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
                pitchMoment = controller.thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
                rollMoment = controller.thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
            }

            Vector3 total_moment = yawMoment + pitchMoment + rollMoment;

            controller.controller.ApplyMotorForce(thrustV);
            controller.controller.ApplyMotorTorque(total_moment);
        }

    }
}