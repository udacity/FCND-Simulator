using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;

[System.Serializable]
public class AirspeedHoldScenario : Scenario
{
    

    public float currentAltitude = 0.0f;
    private float lastAltitudeTime = 0.0f;
    private float altitudeThreshold = 0.5f;

    public float currentAirspeed = 0.0f;
    private float lastAirspeedTime = 0.0f;
    public float airspeedThreshold = 0.5f;

    private float timeInterval = 5.0f;
    private float finalTime = 15.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float targetAirspeed = 41.0f;
    private float targetAltitude = 150.0f;

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
//        planeControl.SetDefaultLongitudinalGains();
//        planeControl.SetDefaultLateralGains();
//        planeControl.SetStudentLongitudinalGains();
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        currTime = drone.FlightTime();
        initTime = drone.FlightTime();
        
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Scenario Success!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
        currTime = drone.FlightTime() - initTime;
        currentAirspeed = drone.VelocityLocal().magnitude;
        if (currTime > finalTime - timeInterval && currTime <= finalTime)
        {
            currentAirspeed = drone.VelocityLocal().magnitude;
            if (Mathf.Abs(currentAirspeed - targetAirspeed) > airspeedThreshold)
            {
                data.failText = "Scenario Failed:\n" +
                    "Airspeed = " + currentAirspeed + " at t = " + currTime;
                return true;
            }
        }
        return false;
    }

    protected override void OnEnd()
    {
        drone.SetGuided(false);
        Debug.Log("Released control of the Drone");
        base.OnEnd();
        
    }

    protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}