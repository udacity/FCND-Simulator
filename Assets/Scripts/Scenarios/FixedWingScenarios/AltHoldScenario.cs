using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class AltHoldScenario : Scenario
{
    public float currentAltitude = 0.0f;
    private float lastAltitudeTime = 0.0f;
    private float altitudeThreshold = 1.0f;

    public float currentAirspeed = 0.0f;
    private float lastAirspeedTime = 0.0f;
    private float airspeedThreshold = 0.1f;

    private float timeInterval = 5.0f;
    private float finalTime = 15;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float targetAirspeed = 41.0f;
    public float targetAltitude = 450.0f;

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);

        initTime = Time.time;
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Altitude Hold Scenario Successful!";
        return true;

    }

	protected override bool OnCheckFailure ()
	{
        drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);
        currTime = Time.time - initTime;
        currentAltitude = -drone.CoordsLocal().z;
        if (currTime > finalTime - timeInterval && currTime <= finalTime)
        {
            if (Mathf.Abs(currentAltitude - targetAltitude) > altitudeThreshold)
            {
                data.failText = "Altitude scenario not successful:\n" +
                    "Target Altitude = " + targetAltitude + "\n" +
                    "Altitude = " + currentAltitude + " at t = " + currTime;
                return true;
                
            }

        }

        return false;
    }

	protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}