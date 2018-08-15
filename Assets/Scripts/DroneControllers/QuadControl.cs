using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;

[System.Serializable]
public class QuadControl : IControlLaw {

    public float Kp_r;//20.0f;
    public float maxYawRate = 0.1f; //radians/sec

    public float Kp_hdot;//5.0f;
    public float Ki_hdot;//0.5f;

    public float Kp_p;//10.0f;
    public float Kp_roll;//6.5f;

    public float Kp_q;//10.0f;
    public float Kp_pitch;//6.5f;

    public float maxTilt;
    public float maxAscentRate;
    public float maxDescentRate;

    private float hDotInt;
    private float maxHDotInt = 0.1f;

    public float Kp_pos;
    public float Kp_pos2;
    public float Kp_alt;
    public float posHoldDeadband;
    public float maxSpeed;
    public float Kp_vel;
    public float Kp_yaw;

    public float posctl_band;

    public QuadControl()
    {
        hDotInt = 0.0f;
    }

    public void SetScenarioParameters(string[] names)
    {

    }

    /// <summary>
    /// Closes a loop on the position error.
    /// </summary>
    /// <param name="targetPosition">The desired local position</param>
    /// <param name="localPosition">The current local position</param>
    /// <returns>The desired local velocity</returns>
    public Vector3 PositionLoop(Vector3 targetPosition, Vector3 localPosition)
    {
        Vector3 positionError = targetPosition - localPosition;
        Vector3 velocityCmd = Vector3.zero;

        if (Mathf.Sqrt(positionError.x * positionError.x + positionError.y * positionError.y) >= posHoldDeadband)
        {
            velocityCmd.x = Kp_pos * positionError.x;
            velocityCmd.y = Kp_pos * positionError.y;
        }
        else
        {
            velocityCmd.x = Kp_pos2 * positionError.x;
            velocityCmd.y = Kp_pos2 * positionError.y;
        }

        velocityCmd.z = Kp_alt * positionError.z;
        return velocityCmd;
    }

    public Vector3 PositionVelocityLoop(Vector3 targetPosition, Vector3 targetVelocity, Vector3 localPosition, Vector3 localVelocity, float yaw)
    {
        Vector3 positionError = targetPosition - localPosition;
        Vector3 velocityError = targetVelocity - localVelocity;

        Vector3 output;

        output.y = -((Kp_pos * positionError.x + Kp_vel * velocityError.x)*Mathf.Cos(yaw) + (Kp_pos * positionError.y + Kp_vel * velocityError.y)*Mathf.Sin(yaw));
        output.x = -(Kp_pos * positionError.x + Kp_vel * velocityError.x) * Mathf.Sin(yaw) + (Kp_pos * positionError.y + Kp_vel * velocityError.y) * Mathf.Cos(yaw);

        output.z = Kp_alt * positionError.z + Kp_hdot*velocityError.z;

        return output;
    }

    /// <summary>
    /// Closes the loop on the local velocity to command a roll and pitch angle
    /// </summary>
    /// <param name="targetVelocity">Target local velocity</param>
    /// <param name="localVelocity">Current local velocity</param>
    /// <param name="yaw">vehicle heading (in radians)</param>
    /// <returns>A commanded roll and pitch angle</returns>
    public Vector2 VelocityLoop(Vector3 targetVelocity, Vector3 localVelocity, float yaw)
    {
        float targetSpeed = Mathf.Sqrt(targetVelocity.x * targetVelocity.x + targetVelocity.y * targetVelocity.y);
        if (targetSpeed > maxSpeed)
        {
            targetVelocity.x = maxSpeed * targetVelocity.x / targetSpeed;
            targetVelocity.y = maxSpeed * targetVelocity.y / targetSpeed;
        }

        float cosYaw = Mathf.Cos(yaw);
        float sinYaw = Mathf.Sin(yaw);

        Vector3 velError = targetVelocity - localVelocity;
        Vector3 velErrorBody = Vector3.zero;
        velErrorBody.x = cosYaw * velError.x + sinYaw * velError.y;
        velErrorBody.y = -sinYaw * velError.x + cosYaw * velError.y;

        float pitchCmd = -Kp_vel * velErrorBody.x;
        float rollCmd = Kp_vel * velErrorBody.y;

        return new Vector2(rollCmd, pitchCmd);
    }

    /// <summary>
    /// Closes the loop on the vehicle yaw
    /// </summary>
    /// <param name="targetYaw">Desired yaw angle</param>
    /// <param name="yaw">Current yaw angle</param>
    /// <returns>The commanded yaw rate</returns>
    public float YawLoop(float targetYaw, float yaw)
    {
        targetYaw = targetYaw % (2.0f * Mathf.PI);
        float yawError = targetYaw - yaw;
        if (yawError > Mathf.PI)
            yawError = yawError - 2.0f * Mathf.PI;
        else if (yawError < -Mathf.PI)
            yawError = yawError + 2.0f * Mathf.PI;
        // float yawError = targetYaw - yaw;
        return Kp_yaw * yawError;
    }

    /// <summary>
    /// Closes the loop on the vehicle yaw rate returning a desired yaw moment
    /// </summary>
    public float YawRateLoop(float targetYawrate, float yawrate)
    {
        if (Mathf.Abs(targetYawrate) > maxYawRate)
            targetYawrate = Mathf.Sign(targetYawrate) * maxYawRate;
        float yawMoment = Kp_r * (targetYawrate - yawrate);
        return yawMoment;
    }

    /// <summary>
    /// Closes the loop on vertical velocity using a PI controller to output a thrust command. Optionally a ff command can be passed in.
    /// </summary>   
    public float VerticalVelocityLoop(float targetVerticalVelocity, Vector3 attitude, float verticalVelocity, float dt, float thrustNom = 0.0f)
    {

        if (targetVerticalVelocity > maxAscentRate)
            targetVerticalVelocity = maxAscentRate;
        else if (targetVerticalVelocity < -maxDescentRate)
            targetVerticalVelocity = -maxDescentRate;

        float hDotError = targetVerticalVelocity - verticalVelocity;

        hDotInt += hDotError * dt;
        if (hDotInt > maxHDotInt)
            hDotInt = maxHDotInt;
        else if (hDotInt < -maxHDotInt)
            hDotInt = -maxHDotInt;

        float thrust = (Kp_hdot * hDotError + Ki_hdot * hDotInt + thrustNom) / (Mathf.Cos(attitude.x) * Mathf.Cos(attitude.y));
        return thrust;
    }

    /// <summary>
    /// Command a desired roll and pitch using a PD cascading controller. Returns a roll and pitch rate
    /// </summary>
    public Vector2 RollPitchLoop(Vector2 targetRollPitch, Vector3 attitude)
    {
        //Enfoce a maximum tilt angle
        float angleMagnitude = Mathf.Sqrt(targetRollPitch.x * targetRollPitch.x + targetRollPitch.y * targetRollPitch.y);
        if (angleMagnitude > maxTilt)
        {
            targetRollPitch = maxTilt * targetRollPitch / angleMagnitude;
        }

        //PD control on roll
        float roll = attitude.x;
        float targetRoll = targetRollPitch.x;
        targetRoll = targetRoll % (2.0f * Mathf.PI);

        float rollError = targetRoll - roll;
        if (rollError > Mathf.PI)
            rollError = rollError - 2.0f * Mathf.PI;
        else if (rollError < -Mathf.PI)
            rollError = rollError + 2.0f * Mathf.PI;

        float rollrateCmd = Kp_roll * rollError;

        //PD control on pitch
        float pitch = attitude.y;
        float targetPitch = targetRollPitch.y;
        targetPitch = targetPitch % (2.0f * Mathf.PI);

        float pitchError = targetPitch - pitch;
        if (pitchError > Mathf.PI)
            pitchError = pitchError - 2.0f * Mathf.PI;
        else if (pitchError < -Mathf.PI)
            pitchError = pitchError + 2.0f * Mathf.PI;

        float pitchrateCmd = Kp_pitch * pitchError;


        return new Vector2(rollrateCmd, pitchrateCmd);
    }

    /// <summary>
    /// Command a desired roll/pitch rate. Returns a desired roll/pitch moment
    /// </summary>
    public Vector2 RollPitchRateLoop(Vector2 targetRate, Vector3 angularRate)
    {
        float rollMoment = Kp_p * (targetRate.x - angularRate.x);
        float pitchMoment = Kp_q * (targetRate.y - angularRate.y);
        return new Vector2(rollMoment, pitchMoment);
    }

}
