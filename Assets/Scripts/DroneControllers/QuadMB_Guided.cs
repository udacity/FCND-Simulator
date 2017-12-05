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

        public override void OverrideUpdate(float throttle, float pitchRate, float yawRate, float rollRate)
        {
            Vector3 pitchYawRoll = controller.controller.eulerAngles * Mathf.Deg2Rad;
            Vector3 qrp = controller.controller.AngularVelocityBody;

            // Direct Control of the moments
            Vector3 thrust = Vector3.zero;
            Vector3 yaw_moment = Vector3.zero;
            Vector3 pitch_moment = Vector3.zero;
            Vector3 roll_moment = Vector3.zero;
            Vector4 angle_input = Vector4.zero;

            angle_input[0] = throttle;
            angle_input[1] = yawRate;
            angle_input[2] = pitchRate;
            angle_input[3] = rollRate;

            // Inner control loop: angle commands to forces
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

                // hdot to thrust
                thrust[1] = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(pitchYawRoll.x) * Mathf.Cos(pitchYawRoll.z));

                // yaw rate to yaw moment
                yaw_moment[1] = controller.Kp_r * (controller.turnSpeed * angle_input[1] - qrp.y);

                // angle to angular rate command (for pitch and roll)
                float pitchError = angle_input[2] - pitchYawRoll.x;
                float rollError = angle_input[3] - pitchYawRoll.z;
                float pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
                float rollRateError = controller.Kp_roll * rollError - qrp.z;

                // angular rate to moment (pitch and roll)
                pitch_moment[0] = controller.Kp_q * pitchRateError;
                roll_moment[2] = controller.Kp_p * rollRateError;
            }
            else // User controls forces directly (not updated, do not use)
            {
                thrust = controller.thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
                yaw_moment = controller.thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
                pitch_moment = controller.thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
                roll_moment = controller.thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
            }

            Vector3 total_moment = yaw_moment + pitch_moment + roll_moment;

            controller.controller.ApplyRotorForce(thrust);
            controller.controller.ApplyRotorTorque(total_moment);
        }

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

            // Direct Control of the moments
            Vector3 thrust = Vector3.zero;
            Vector3 yaw_moment = Vector3.zero;
            Vector3 pitch_moment = Vector3.zero;
            Vector3 roll_moment = Vector3.zero;
            Vector4 angle_input = Vector4.zero;

            // Outer control loop for from a position/velocity command to a hdot, yaw rate, pitch, roll command
            // If no control input provided (or in guided mode), use position hold
            Vector3 velCmdBody = Vector3.zero;
            float yawCmd = 0;

            // Set position
            if (!controller.pos_set)
            {
                controller.posHoldLocal = localPosition;
                controller.pos_set = true;
                //				Debug.Log ( controller.posHoldLocal );
            }

            //
            // Adjust 3D Position based on error
            //

            Vector3 posErrorLocal = controller.posHoldLocal - localPosition;
            Vector3 velCmdLocal;
            // print("Position Hold: " + posHoldLocal);
            // print("Local Position: " + localPosition);

            // Deadband around the position hold
            if (Mathf.Sqrt(Mathf.Pow(posErrorLocal.x, 2.0f) + Mathf.Pow(posErrorLocal.z, 2.0f)) < controller.posHoldDeadband)
            {
                velCmdLocal.x = 0.0f;
                velCmdLocal.z = 0.0f;
            }
            else
            {
                // TODO: why do x and z share the same coefficient but y doesn't?
                velCmdLocal.x = controller.Kp_pos * posErrorLocal.x;
                velCmdLocal.z = controller.Kp_pos * posErrorLocal.z;
            }
            velCmdLocal.y = controller.Kp_alt * posErrorLocal.y;

            // Rotate into the local heading frame
            float cosYaw = Mathf.Cos(pitchYawRoll.y);
            float sinYaw = Mathf.Sin(pitchYawRoll.y);

            // TODO: explain difference between velCmdBody and velCmdLocal
            velCmdBody.x = cosYaw * velCmdLocal.x - sinYaw * velCmdLocal.z;
            velCmdBody.z = sinYaw * velCmdLocal.x + cosYaw * velCmdLocal.z;
            velCmdBody.y = velCmdLocal.y;

            // Heading hold if in guided mode or no input
            if (!controller.yawSet)
            {
                controller.yawHold = pitchYawRoll.y;
                controller.yawSet = true;
            }

            //
            // Adjust heading based on error
            //

            float yawError = controller.yawHold - pitchYawRoll.y;
            // yawError must be in the range [-pi, pi]
            if (yawError > Mathf.PI)
            {
                yawError = yawError - 2.0f * Mathf.PI;
            }
            else if (yawError < -1.0f * Mathf.PI)
            {
                yawError = yawError + 2.0f * Mathf.PI;
            }
            yawCmd = controller.Kp_yaw * yawError;

            //
            // Adjust velocity based on error
            //

            // Control loop from a body velocity command to a Hdot, yaw rate, pitch, and roll command
            Vector3 velocityErrorBody = Vector3.zero;
            Vector3 velocityErrorBodyD = Vector3.zero;
            velocityErrorBody.x = controller.moveSpeed * velCmdBody.x - bodyVelocity.x;
            velocityErrorBody.z = controller.moveSpeed * velCmdBody.z - bodyVelocity.z;
            velocityErrorBodyD = (velocityErrorBody - lastVelocityErrorBody) / Time.deltaTime;
            lastVelocityErrorBody = velocityErrorBody;

            angle_input[2] = controller.Kp_vel * velocityErrorBody.z + controller.Kd_vel * velocityErrorBodyD.z;
            // TODO: explain why is there a negative sign in front the coeffs here?
            angle_input[3] = -controller.Kp_vel * velocityErrorBody.x + -controller.Kd_vel * velocityErrorBodyD.x;

            float angle_magnitude = Mathf.Sqrt(Mathf.Pow(angle_input[2], 2.0f) + Mathf.Pow(angle_input[3], 2.0f));
            if (angle_magnitude > controller.maxTilt)
            {
                angle_input[2] = controller.maxTilt * angle_input[2] / angle_magnitude;
                angle_input[3] = controller.maxTilt * angle_input[3] / angle_magnitude;
            }

            angle_input[0] = velCmdBody.y;
            angle_input[1] = yawCmd;

            // Constrain the angle inputs between -1 and 1 (tilt, turning speed, and vert speed taken into account later)
            // [-1, 1] rad == [-57.29577, 57.29577] deg
            angle_input[1] = Mathf.Clamp(angle_input[1], -1f, 1f);

            // Inner control loop: angle commands to forces
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

                // hdot to thrust
                thrust[1] = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(pitchYawRoll.x) * Mathf.Cos(pitchYawRoll.z));

                // yaw rate to yaw moment
                yaw_moment[1] = controller.Kp_r * (controller.turnSpeed * angle_input[1] - qrp.y);

                // angle to angular rate command (for pitch and roll)
                float pitchError = angle_input[2] - pitchYawRoll.x;
                float rollError = angle_input[3] - pitchYawRoll.z;
                float pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
                float rollRateError = controller.Kp_roll * rollError - qrp.z;

                // angular rate to moment (pitch and roll)
                pitch_moment[0] = controller.Kp_q * pitchRateError;
                roll_moment[2] = controller.Kp_p * rollRateError;
            }
            else // User controls forces directly (not updated, do not use)
            {
                thrust = controller.thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
                yaw_moment = controller.thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
                pitch_moment = controller.thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
                roll_moment = controller.thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
            }

            Vector3 total_moment = yaw_moment + pitch_moment + roll_moment;

            controller.controller.ApplyRotorForce(thrust);
            controller.controller.ApplyRotorTorque(total_moment);
        }
    }
}