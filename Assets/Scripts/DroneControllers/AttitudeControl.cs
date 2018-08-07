using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;

public class AttitudeControl : IControlLaw {


    public float Kp_r = 0.04f;//20.0f;

    public float Kp_hdot = 2.5f;//5.0f;
    public float Ki_hdot = 0.25f;//0.5f;

    public float Kp_p = 0.1f;//10.0f;
    public float Kp_roll = 8.0f;//6.5f;

    public float Kp_q = 0.1f;//10.0f;
    public float Kp_pitch = 8.0f;//6.5f;

    public float maxTilt = 0.5f;
    public float maxAscentRate = 5.0f;
    public float maxDescentRate = 2.0f;



    private float hDotInt;
    private float maxHDotInt = 0.1f;


    public void SetScenarioParameters(string[] names)
    {

    }

    // Use this for initialization
    public AttitudeControl () {
        hDotInt = 0.0f;
    }
	
    /// <summary>
    /// Closes the loop on the vehicle yaw rate returning a desired yaw moment
    /// </summary>
    public float YawRateLoop(float targetYawrate, float yawrate)
    {
        float yawMoment = Kp_r * (targetYawrate - yawrate);
        return yawMoment;
    }

    /// <summary>
    /// Closes the loop on vertical velocity using a PI controller to output a thrust command. Optionally a ff command can be passed in.
    /// </summary>   
    public float VerticalVelocityLoop(float targetVerticalVelocity, Vector3 attitude, float verticalVelocity, float dt, float thrustNom=0.0f)
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
    public Vector2 RollPitchRateLoop(Vector2 targetRate,Vector3 angularRate)
    {
        float rollMoment = Kp_p * (targetRate.x - angularRate.x);
        float pitchMoment = Kp_q * (targetRate.y - angularRate.y);
        return new Vector2(rollMoment, pitchMoment);   
    }

}
