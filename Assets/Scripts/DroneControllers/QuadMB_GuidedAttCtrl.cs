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

        public override void RemoteUpdate(float thrust, float pitchRate, float yawRate, float rollRate)
        {
            var pitchYawRoll = new Vector3(controller.controller.GetPitch(), controller.controller.GetYaw(), controller.controller.GetRoll());
            Vector3 qrp = controller.controller.AngularVelocityBody;

            Debug.Log(string.Format("{0} {1} {2} {3}", thrust, pitchRate, yawRate, rollRate));

            // Inner control loop: angle commands to forces
            float thrust_nom = -controller.rb.mass * Physics.gravity[1];
            float hDotError = 0.0f;
            if (thrust > 0.0f)
            {
                hDotError = (controller.maxAscentRate * thrust - 1.0f * controller.controller.LinearVelocity.y);
            }
            else
            {
                hDotError = (controller.maxDescentRate * thrust - 1.0f * controller.controller.LinearVelocity.y);
            }
            hDotInt = hDotInt + hDotError * Time.deltaTime;

            // hdot to thrust
            thrust = (controller.Kp_hdot * hDotError + controller.Ki_hdot * hDotInt + thrust_nom) / (Mathf.Cos(pitchYawRoll.x) * Mathf.Cos(pitchYawRoll.z));

            // angle to angular rate command (for pitch and roll)
            var pitchError = pitchRate - pitchYawRoll.x;
            var pitchRateError = controller.Kp_pitch * pitchError - qrp.x;
            var rollError = rollRate - pitchYawRoll.z;
            var rollRateError = controller.Kp_roll * rollError - qrp.z;

            // angular rates to moments
            var pitchMoment = controller.Kp_q * pitchRateError;
            var rollMoment = controller.Kp_p * rollRateError;
            var yawMoment = controller.Kp_r * (controller.turnSpeed * yawRate - qrp.y);

            var thrustV = new Vector3(0, thrust, 0);
            var totalMoment = new Vector3(pitchMoment, yawMoment, rollMoment);
            Debug.Log(string.Format("thrust vector {0}, moments vector {1}", thrustV, totalMoment));
            controller.controller.ApplyMotorForce(thrustV);
            controller.controller.ApplyMotorTorque(totalMoment);
        }

        public override void OnLateUpdate()
        {

            controller.turnSpeed = 2.0f;
            controller.maxTilt = 0.5f;
            var nav = controller.controller;
            Vector3 attitude = new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 attCmd = Vector3.zero;
            attCmd.x = controller.guidedCommand.x;
            attCmd.y = controller.guidedCommand.y;

            float yawCmd = controller.guidedCommand.w;
            float yawOutput = YawRateControl(yawCmd, angularVelocity.z);
            Vector2 attOutput = RollPitchControl(attCmd, attitude, angularVelocity);
            Vector3 totalMoment = new Vector3(attOutput.x, attOutput.y, yawOutput);

            float altCmd = controller.guidedCommand.z;
            if (altCmd > 0.0f)
                altCmd = altCmd * controller.maxAscentRate;
            else
                altCmd = altCmd * controller.maxDescentRate;
            float altOutput = VerticalVelocityControl(altCmd, attitude, -localVelocity.z);
            
            
            nav.CmdTorque(totalMoment);
            nav.CmdThrust(altOutput);

        }


        private Vector3 PositionControl(Vector3 targetPosition, Vector3 attitude, Vector3 angularVelocity, Vector3 localVelocity, Vector3 localPosition)
        {
            Vector3 positionError = targetPosition - localPosition;
            Vector3 velocityCmd = Vector3.zero;

            if (Mathf.Sqrt(positionError.x * positionError.x + positionError.y * positionError.y) >= controller.posHoldDeadband)
            {
                velocityCmd.x = controller.Kp_pos * positionError.x;
                velocityCmd.y = controller.Kp_pos * positionError.y;
            }

            velocityCmd.z = controller.Kp_alt * positionError.z;

            return VelocityControl(velocityCmd, attitude, angularVelocity, localVelocity);
        }

        private Vector3 VelocityControl(Vector3 targetVelocity, Vector3 attitude, Vector3 angularVelocity, Vector3 localVelocity)
        {
            float targetSpeed = Mathf.Sqrt(targetVelocity.x * targetVelocity.x + targetVelocity.y * targetVelocity.y);
            if (targetSpeed > controller.moveSpeed)
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


        private float YawRateControl(float targetYawrate, float yawrate)
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

    }
}