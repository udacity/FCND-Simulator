using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneControl {

//    [Tunable(0.0f,-1.0f, 1.0f)] // 0.2f
    public float Kp_speed;
//    public float Kp_speed_student;

//    [Tunable(0.0f,-1.0f, 1.0f)] // 0.1f
    public float Ki_speed;
//    public float Ki_speed_student;

    public float speedInt = 0.0f;
    public float minThrottle = 0.0f;
    public float maxThrottle = 1.0f;
    public float maxSpeedInt = 0.25f;
    public float nominalThrottle = 0.66f;


//    [Tunable(0.0f,-50.0f,50.0f)] //20.0f
    public float Kp_pitch = 8.0f;
//    public float Kp_pitch_student;

//    [Tunable(0.0f,-50.0f,50.0f)] //10.0f
    public float Kp_q = 5.0f;
//    public float Kp_q_student;

//    [Tunable(0.0f, -1.0f, 1.0f)] // 0.03f
    public float Kp_alt;
//    public float Kp_alt_student;

//    [Tunable(0.0f, -1.0f, 1.0f)] // 0.05f
    public float Ki_alt;
//    public float Ki_alt_student;

    public float altInt = 0.0f;
    public float minPitch = -30f * Mathf.PI / 180f;
    public float maxPitch = 30f * Mathf.PI / 180f;
    public float maxAltInt = 0.1f;

//    [Tunable(0.0f, -1.0f, 1.0f)] // 0.2f
    public float Kp_speed2;
//    public float Kp_speed2_student;

//    [Tunable(0.0f, -1.0f, 1.0f)] // 0.2f
    public float Ki_speed2 = 0.001f;
//    public float Ki_speed2_student;

    public float speedInt2 = 0.0f;
    public float minPitch2 = -45f * Mathf.PI / 180f;
    public float maxPitch2 = 45f * Mathf.PI / 180f;
    public float maxSpeedInt2 = 10.0f;

    public float altitudeSwitch = 25.0f;

    //Not used
    public float Kp_climb = 0.01f;
    public float Ki_climb = 0.001f;
    public float climbInt = 0.0f;
    public float maxClimbInt = 1.0f;

    public float Kp_roll = 8.0f;
    public float Kp_p = 5.0f;

    public float Kp_sideslip = 0.1f;
    public float Ki_sideslip = 0.1f;
    public float sideslipInt = 0.0f;
    public float maxSideslipInt = 10.0f;

    public float Kp_yaw = 1.0f;
    public float Ki_yaw = 0.1f;
    public float maxRoll = 60.0f * Mathf.PI / 180f;
    public float yawInt = 0.0f;
    public float maxYawInt = 1.0f;

    public float Kp_xTrack = 1.0f;
    public float approachYaw = Mathf.PI / 2.0f;

    public float Kp_orbit = 1.0f;

    private float hDotInt;
    private float maxHDotInt = 0.1f;


	// Use this for initialization
	public PlaneControl() {

        altInt = 0.0f;
        speedInt = 0.0f;
        sideslipInt = 0.0f;
        speedInt2 = 0.0f;
        maxSpeedInt2 = 50.0f;

        Kp_climb = 0.1f;
        Ki_climb = 0.1f;
        maxClimbInt = 10.0f;

        Kp_sideslip = 1.0f;
        Ki_sideslip = 1.0f;

        Kp_roll = 5*8.0f;
        Kp_p = 1.0f;

        Kp_yaw = 2.0f;
        Ki_yaw = 0.01f;
        maxYawInt = 100.0f;

        Kp_xTrack = 0.002f;

        Kp_orbit = 2.5f;


    }


    public float AirspeedLoop(float targetAirspeed, float airspeed, float dt = 0.0f)
    {
        float airspeedError = (targetAirspeed - airspeed);
        if (dt == 0.0)
            dt = Time.fixedDeltaTime;
        speedInt = speedInt + airspeedError * dt;

        /*if (speedInt > maxSpeedInt)
            speedInt = maxSpeedInt;
        else if (speedInt < -maxSpeedInt)
            speedInt = -maxSpeedInt;
        */
        float outputUnsat = Kp_speed * airspeedError + Ki_speed * speedInt + nominalThrottle;
        
        float output = Mathf.Clamp(outputUnsat, minThrottle, maxThrottle);
        if (Ki_speed != 0.0f)
            speedInt = speedInt + dt / Ki_speed * (output - outputUnsat);


        return output;
        
    }

    /// <summary>
    /// Closes the loop on pitch and pitch rate
    /// </summary>
    public float PitchLoop(float targetPitch, float pitch, float pitchrate)
    {
        float output = Kp_pitch * (targetPitch - pitch) - Kp_q * pitchrate;
        return output;
    }

    public float AltitudeLoop(float targetAltitude, float altitude, float dt = 0f)
    {
        if (dt == 0)
            dt = Time.fixedDeltaTime;

        float altError = targetAltitude-altitude;
        altInt = altInt + altError*dt;

        /*if (altInt > maxAltInt)
            altInt = maxAltInt;
        else if (altInt < -maxAltInt)
            altInt = -maxAltInt;
        */

        float outputUnsat = Kp_alt * altError + Ki_alt * altInt;
        float output = Mathf.Clamp(outputUnsat, minPitch, maxPitch);
        if (Ki_alt != 0f)
            altInt = altInt + dt / Ki_alt * (output - outputUnsat);
        return output;
    }

    

    public float AirspeedLoop2(float targetAirspeed, float airspeed, float dt = 0.0f)
    {
        if (dt == 0.0)
            dt = Time.fixedDeltaTime;
        float speedError = (targetAirspeed - airspeed);
        speedInt2 = speedInt2 + speedError* dt;
        /*
        if (speedInt2 > maxSpeedInt2)
            speedInt2 = maxSpeedInt2;
        else if (speedInt2 < -maxSpeedInt2)
            speedInt2 = -maxSpeedInt2;
        */
        float outputUnsat = Kp_speed2 * speedError + Ki_speed2 * speedInt2;
        float output = Mathf.Clamp(outputUnsat, minPitch2, maxPitch2);
        if (Ki_speed2 != 0f)
            speedInt2 = speedInt2 + dt / Ki_speed2 * (output - outputUnsat);
        return output;
    }

    public float ClimbRateLoop(float targetClimbRate, float climbRate, float dt = 0.0f)
    {
        if (dt == 0.0)
            dt = Time.fixedDeltaTime;
        climbInt = climbInt + (targetClimbRate - climbRate) * dt;
        if (climbInt > maxClimbInt)
            climbInt = maxClimbInt;
        else if (climbInt < -maxClimbInt)
            climbInt = -maxClimbInt;
        float output = Kp_climb * (targetClimbRate - climbRate) + Ki_climb * climbInt;
        return output;
    }

    public float RollLoop(float targetRoll, float roll, float rollrate)
    {
        float output = Kp_roll * (targetRoll - roll) - Kp_p * rollrate;
        return output;
    }

    public float SideslipLoop(float targetSideslip, float sideslip, float dt = 0f)
    {
        if (dt == 0)
            dt = Time.fixedDeltaTime;

        sideslipInt = sideslipInt + dt*(targetSideslip - sideslip);


        /*if (sideslipInt > maxSideslipInt)
            sideslipInt = maxSideslipInt;
        else if (sideslipInt < -maxSideslipInt)
            sideslipInt = -maxSideslipInt;
        */
        /*
        float outputUnsat = Mathf.Clamp(Kp_sideslip * (targetSideslip - sideslip), -1f, 1f);
        outputUnsat = outputUnsat + Ki_sideslip * sideslipInt;
        */
        float outputUnsat = Kp_sideslip * (targetSideslip - sideslip) + Ki_sideslip * sideslipInt;
        float output = Mathf.Clamp(outputUnsat, -1f, 1f);
        if (Ki_sideslip != 0)
            sideslipInt = sideslipInt + dt / Ki_sideslip * (output - outputUnsat);
        
        return output;
        
    }

    public float YawLoop(float targetYaw, float yaw, float dt, float rollFF = 0f)
    {
        float yawError = targetYaw - yaw;
        while (Mathf.Abs(yawError) >= Mathf.PI)
            yawError = yawError - Mathf.Sign(yawError) * Mathf.PI * 2f;

        yawInt = yawInt + dt*yawError;

        /*
        if (yawInt > maxYawInt)
            yawInt = maxYawInt;
        else if (yawInt < -maxYawInt)
            yawInt = -maxYawInt;
        */
        //float outputUnsat = Mathf.Clamp(Kp_yaw * yawError + rollFF,-maxRoll, maxRoll);
        float outputUnsat = Mathf.Clamp(Kp_yaw*yawError + rollFF,-maxRoll, maxRoll) + Ki_yaw * yawInt;
        float output = Mathf.Clamp(outputUnsat, -maxRoll, maxRoll);

        if (Ki_yaw != 0f)
            yawInt = yawInt + dt / Ki_yaw * (output - outputUnsat);

        //Debug.Log("Roll Command: " + output + " Int: " + (Ki_yaw * yawInt) + " Yaw Error: " + yawError);

        return output;
    }

    public float CrossTrackLoop(Vector3 targetPosition, float targetCourse, Vector3 position)
    {
        float xTrackError = Mathf.Cos(targetCourse) * (position.y - targetPosition.y) + Mathf.Sin(-targetCourse) * (position.x - targetPosition.x);
        float output = -Mathf.PI / 2.0f * Mathf.Atan(Kp_xTrack * xTrackError) + targetCourse;

        return output;
    }

    public float OrbitLoop(Vector3 orbitCenter, float targetRadius, Vector3 position, float yaw, bool clockwise=true)
    {
        float radius = Mathf.Sqrt(Mathf.Pow(orbitCenter.x - position.x, 2.0f) + Mathf.Pow(orbitCenter.y - position.y, 2.0f));
        float output = Mathf.PI / 2.0f + Mathf.Atan(Kp_orbit * (radius - targetRadius) / targetRadius);
        if (!clockwise)
            output = -output;

        float addon = Mathf.Atan2(position.y - orbitCenter.y, position.x - orbitCenter.x);
        if (addon-yaw < -Mathf.PI)
            while (addon-yaw < -Mathf.PI)
                addon = addon + Mathf.PI * 2f;
        else if (addon - yaw > Mathf.PI)
            while (addon - yaw > Mathf.PI)
                addon = addon - Mathf.PI * 2f;
        output = output + addon;
        return output;
    }

	public void SetScenarioParameters (string[] names)
	{
		TunableParameter p = TunableManager.GetParameter ( "Kp_speed" );
		if ( names.Contains ( "Kp_speed" ) )
			Kp_speed = p.value;
		else
			Kp_speed = p.fixedValue;

		p = TunableManager.GetParameter ( "Ki_speed" );
		if ( names.Contains ( "Ki_speed" ) )
			Ki_speed = p.value;
		else
			Ki_speed = p.fixedValue;

		p = TunableManager.GetParameter ( "Kp_pitch" );
		if ( names.Contains ( "Kp_pitch" ) )
			Kp_pitch = p.value;
		else
			Kp_pitch = p.fixedValue;

		p = TunableManager.GetParameter ( "Kp_q" );
		if ( names.Contains ( "Kp_q" ) )
			Kp_q = p.value;
		else
			Kp_q = p.fixedValue;

		p = TunableManager.GetParameter ( "Kp_alt" );
		if ( names.Contains ( "Kp_alt" ) )
			Kp_alt = p.value;
		else
			Kp_alt = p.fixedValue;

		p = TunableManager.GetParameter ( "Ki_alt" );
		if ( names.Contains ( "Ki_alt" ) )
			Ki_alt = p.value;
		else
			Ki_alt = p.fixedValue;

		p = TunableManager.GetParameter ( "Kp_speed2" );
		if ( names.Contains ( "Kp_speed2" ) )
			Kp_speed2 = p.value;
		else
			Kp_speed2 = p.fixedValue;

		p = TunableManager.GetParameter ( "Ki_speed2" );
		if ( names.Contains ( "Ki_speed2" ) )
			Ki_speed2 = p.value;
		else
			Ki_speed2 = p.fixedValue;

        p = TunableManager.GetParameter("Kp_roll");
        if (names.Contains("Kp_roll"))
            Kp_roll = p.value;
        else
            Kp_roll = p.fixedValue;

        p = TunableManager.GetParameter("Kp_p");
        if (names.Contains("Kp_p"))
            Kp_p = p.value;
        else
            Kp_p = p.fixedValue;

        p = TunableManager.GetParameter("Kp_yaw");
        if (names.Contains("Kp_yaw"))
            Kp_yaw = p.value;
        else
            Kp_yaw = p.fixedValue;

        p = TunableManager.GetParameter("Ki_yaw");
        if (names.Contains("Ki_yaw"))
            Ki_yaw = p.value;
        else
            Ki_yaw = p.fixedValue;

        p = TunableManager.GetParameter("Kp_sideslip");
        if (names.Contains("Kp_sideslip"))
            Kp_sideslip = p.value;
        else
            Kp_sideslip = p.fixedValue;

        p = TunableManager.GetParameter("Ki_sideslip");
        if (names.Contains("Ki_sideslip"))
            Ki_sideslip = p.value;
        else
            Ki_sideslip = p.fixedValue;

        p = TunableManager.GetParameter("Kp_xTrack");
        if (names.Contains("Kp_xTrack"))
            Kp_xTrack = p.value;
        else
            Kp_xTrack = p.fixedValue;

        p = TunableManager.GetParameter("Kp_orbit");
        if (names.Contains("Kp_orbit"))
            Kp_orbit = p.value;
        else
            Kp_orbit = p.fixedValue;

        p = TunableManager.GetParameter("altitudeSwitch");
        if (names.Contains("altitudeSwitch"))
            altitudeSwitch = p.value;
        else
            altitudeSwitch = p.fixedValue;
    }

/*    public void SetDefaultLongitudinalGains()
    {
        Kp_speed_student = Kp_speed;
        Kp_speed = 0.2f;

        Ki_speed_student = Ki_speed;
        Ki_speed = 0.2f;

        Kp_pitch_student = Kp_pitch;
        Kp_pitch = 8.0f;

        Kp_q_student = Kp_q;
        Kp_q = 5.0f;

        Kp_alt_student = Kp_alt;
        Kp_alt = 0.03f;

        Ki_alt_student = Ki_alt;
        Ki_alt = 0.02f;

        Kp_speed2_student = Kp_speed2;
        Kp_speed2 = 0.2f;

        Ki_speed2_student = Ki_speed2;
        Ki_speed2 = 0.2f;
    }

    public void SetStudentLongitudinalGains()
    {
        Kp_speed = Kp_speed_student;

        Ki_speed = Ki_speed_student;

        Kp_pitch = Kp_pitch_student;

        Kp_q = Kp_q_student;

        Kp_alt = Kp_alt_student;

        Ki_alt = Ki_alt_student;

        Kp_speed2= Kp_speed2_student;

        Ki_speed2 = Ki_speed2_student;

    }


    public void SetDefaultLateralGains()
    {

    }

    public void SetStudentLateralGains()
    {

    }*/

   
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
