using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class TrimScenario : Scenario
{
    public float currentVelocity = 0.0f;
    private float lastVelocityTime = 0.0f;
    private float velocityThreshold = 0.5f;

    public float currentAirspeedRate = 0.0f;
    private float lastAirspeedRateTime = 0.0f;
    private float airspeedRateThreshold = 0.1f;

    private float timeInterval = 5.0f;
    private float lastTrimTime = 0.0f;
    private float currTime;

    public float elevatorTrim = 0.0f;
    public float throttleTrim = 0.0f;

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(2); //Longitude mode
        drone.SetGuided(true);
    }

    protected override void OnBegin()
    {
        drone.CommandControls(0.0f, 0.0f, 0.0f, 0.67f);

        currTime = drone.FlightTime();
        lastTrimTime = currTime;
        lastVelocityTime = currTime;
        lastAirspeedRateTime = currTime;
    }
	protected override bool OnCheckSuccess ()
	{
        currTime = drone.FlightTime();

        //elevatorTrim += Input.GetAxis("Trim")*0.001f;
        float trimInput = Input.GetAxis("Thrust") * 0.001f;
        if (trimInput != 0.0f)
        {
            throttleTrim += trimInput;
            lastTrimTime = currTime;
        }
        drone.CommandAttitude(new Vector3(0.0f, elevatorTrim, 0.0f), 0.7f + throttleTrim);

        currentVelocity = drone.VelocityLocal().z;// drone.VelocityLocal().z * 0.001f + currentVelocity * 0.999f;
        if (Mathf.Abs(currentVelocity) > velocityThreshold)
        {
            lastVelocityTime = currTime;
        }

        currentAirspeedRate = drone.AccelerationBody().x;// drone.AccelerationBody().x * 0.001f + currentAirspeedRate * 0.999f;
        if (Mathf.Abs(currentAirspeedRate) > airspeedRateThreshold)
        {
            lastAirspeedRateTime = currTime;
        }

        UpdateVizParameters();

        if (((currTime - lastVelocityTime) > timeInterval) &&
            ((currTime - lastAirspeedRateTime) > timeInterval) &&
            ((currTime - lastTrimTime) > timeInterval))
        {
            data.successText = "Scenario 0 Complete:\n" + 
                "Throttle = " + (0.7f+throttleTrim) + "\n" + 
                "Pitch: " + drone.AttitudeEuler().y*180.0f/Mathf.PI + "degrees\n" + 
                "Airspeed: " + drone.VelocityLocal().magnitude + "m/s";
            Debug.Log("Scenario 0 Complete: Throttle = " + throttleTrim + " Pitch: " + drone.AttitudeEuler().y + " Airspeed: " + drone.VelocityLocal().magnitude);
            return true;
        }
        else
        {
            return false;
        }

    }

	protected override bool OnCheckFailure ()
	{
		return base.OnCheckFailure ();
	}

	protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}

    void UpdateVizParameters()
    {
        onParameter1Update(-currentVelocity, 2);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        onParameter2Update(currentAirspeedRate, 2);
    }
}