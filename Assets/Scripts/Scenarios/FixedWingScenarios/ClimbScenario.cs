using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class ClimbScenario : Scenario
{
    public float currentAirspeed = 0.0f;
    private float lastAirspeedTime = 0.0f;
    private float airspeedThreshold = 1.0f;


    private float timeInterval = 5.0f;
    private float finalTime = 15.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float targetAirspeed = 41.0f;
    private float nominalThrust = 0.7f;
    public float currentClimbRate = 0.0f;

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(5); //AscendDescend Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 0.7f);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 1.0f);

        initTime = Time.time;
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Steay Climb Scenario Successful!\n" +
            "Climb rate = " + currentClimbRate + " meters/sec";
        return true;

    }

	protected override bool OnCheckFailure ()
	{
        drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 1.0f);
        currTime = Time.time - initTime;
        currentAirspeed = drone.VelocityLocal().magnitude;
        currentClimbRate = -drone.VelocityLocal().z;
        if (currTime > finalTime - timeInterval && currTime <= finalTime)
        {
            if (Mathf.Abs(currentAirspeed - targetAirspeed) > airspeedThreshold)
            {
                data.failText = "Steady Climb Scenario Not Successful:\n" +
                    "Airspeed = " + currentAirspeed + " meters/sec at t = " + currTime;
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