using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;

public class PositionControl : IControlLaw
{
    public float Kp_pos = 2.0f;
    public float Kp_pos2 = 0.2f;
    public float Kp_alt = 10.0f;
    public float posHoldDeadband = 0.5f;
    public float maxSpeed = 10.0f;
    public float Kp_vel = 0.2f;
    public float Kp_yaw = 2.50f;

    public PositionControl()
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


}
