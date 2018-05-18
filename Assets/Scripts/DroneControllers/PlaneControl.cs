﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneControl {

    public float Kp_pitch = 8.0f;
    public float Kp_q = 5.0f;

    public float altInt = 0.0f;
    public float Kp_alt = 0.05f;
    public float Ki_alt = 0.01f;
    public float maxAltInt = 0.1f;

    public float Kp_speed = 0.01f;
    public float Ki_speed = 0.001f;
    public float speedInt = 0.0f;
    public float maxSpeedInt = 0.25f;

    public float Kp_speed2 = 0.01f;
    public float Ki_speed2 = 0.001f;
    public float speedInt2 = 0.0f;
    public float maxSpeedInt2 = 10.0f;

    public float Kp_climb = 0.01f;
    public float Ki_climb = 0.001f;
    public float climbInt = 0.0f;
    public float maxClimbInt = 1.0f;

    public float Kp_roll = 8.0f;
    public float Kp_p = 5.0f;

    public float Kp_sideslip = 0.1f;
    public float Ki_sideslip = 0.1f;
    public float sideslipInt = 0.0f;
    public float maxSideslipInt = 0.1f;

    /*
    public float Kp_r = 0.04f;//20.0f;

    public float Kp_hdot = 2.5f;//5.0f;
    public float Ki_hdot = 0.25f;//0.5f;

    public float Kp_p = 0.1f;//10.0f;
    public float Kp_roll = 8.0f;//6.5f;



    public float maxTilt = 0.5f;
    public float maxAscentRate = 5.0f;
    public float maxDescentRate = 2.0f;

    */

    private float hDotInt;
    private float maxHDotInt = 0.1f;


	// Use this for initialization
	public PlaneControl() {
        //hDotInt = 0.0f;
        altInt = 0.0f;
        speedInt = 0.0f;
        sideslipInt = 0.0f;
        Kp_speed = 0.2f;
        Ki_speed = 0.1f;

        Kp_alt = 0.03f;
        Ki_alt = 0.05f;
        Kp_pitch = 20.0f;
        Kp_q = 10.0f;

        Kp_speed2 = 0.1f;
        Ki_speed2 = 0.02f;
        speedInt2 = 0.0f;
        maxSpeedInt2 = 50.0f;

        Kp_climb = 0.1f;
        Ki_climb = 0.1f;
        maxClimbInt = 10.0f;


}
	
    /// <summary>
    /// Closes the loop on pitch and pitch rate
    /// </summary>
    public float PitchLoop(float targetPitch, float pitch, float pitchrate)
    {
        float output = Kp_pitch * (targetPitch - pitch) - Kp_q * pitchrate;
        return output;
    }

    public float AltitudeLoop(float targetAltitude, float altitude)
    {
        altInt = altInt + targetAltitude - altitude;
        if (altInt > maxAltInt)
            altInt = maxAltInt;
        else if (altInt < -maxAltInt)
            altInt = -maxAltInt;
        float output = Kp_alt * (targetAltitude - altitude) + Ki_alt * altInt;
        return output;
    }

    public float AirspeedLoop(float targetAirspeed, float airspeed)
    {
        speedInt = speedInt + (targetAirspeed - airspeed)*Time.deltaTime;
        if (speedInt > maxSpeedInt)
            speedInt = maxSpeedInt;
        else if (speedInt < -maxSpeedInt)
            speedInt = -maxSpeedInt;
        float output = Kp_speed * (targetAirspeed - airspeed) + Ki_speed*speedInt;
        return output;
    }

    public float AirspeedLoop2(float targetAirspeed, float airspeed)
    {
        speedInt2 = speedInt2 + (targetAirspeed - airspeed) * Time.deltaTime;
        if (speedInt2 > maxSpeedInt2)
            speedInt2 = maxSpeedInt2;
        else if (speedInt2 < -maxSpeedInt2)
            speedInt2 = -maxSpeedInt2;
        float output = Kp_speed2 * (targetAirspeed - airspeed) + Ki_speed2 * speedInt2;
        return output;
    }

    public float ClimbRateLoop(float targetClimbRate, float climbRate)
    {
        climbInt = climbInt + (targetClimbRate - climbRate) * Time.deltaTime;
        if (climbInt > maxClimbInt)
            climbInt = maxClimbInt;
        else if (climbInt < -maxClimbInt)
            climbInt = -maxClimbInt;
        float output = Kp_climb * (targetClimbRate - climbRate) + Ki_climb * climbInt;
        return output;
    }

    public float RollLoop(float targetRoll, float roll, float rollrate)
    {
        float output = Kp_roll * (targetRoll - roll) - Kp_pitch * rollrate;
        return output;
    }

    public float SideslipLoop(float targetSideslip, float sideslip)
    {
        sideslipInt = sideslipInt + (targetSideslip - sideslip);
        if (sideslipInt > maxSideslipInt)
            sideslipInt = maxSideslipInt;
        else if (sideslipInt < -maxSideslipInt)
            sideslipInt = -maxSideslipInt;

        float output = -1.0f*(Kp_sideslip * (targetSideslip - sideslip) + Ki_sideslip * sideslipInt);
        return output;
        
    }

   
    /*
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
    */

}
