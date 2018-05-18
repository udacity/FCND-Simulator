using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class AirspeedHoldScenario : Scenario
{
    IDrone drone;
    public float currentAltitude = 0.0f;
    private float lastAltitudeTime = 0.0f;
    private float altitudeThreshold = 0.5f;

    public float currentAirspeed = 0.0f;
    private float lastAirspeedTime = 0.0f;
    private float airspeedThreshold = 0.1f;

    private float timeInterval = 5.0f;
    private float finalTime = 15.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float targetAirspeed = 45.0f;
    private float targetAltitude = 150.0f;

    protected override void OnInit ()
	{
        base.OnInit ();
        drone = GameObject.Find("Plane Drone").GetComponent<PlaneDrone>();
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
        Vector3 startVelocity = new Vector3(0.0f, 0.0f, 41.0f);
        Vector3 startLocation = new Vector3(1500.0f, 450.0f, 0.0f);
        Vector3 startEuler = new Vector3(-1.5f, 0.0f, 0.0f);
        drone.InitializeVehicle(startLocation, startVelocity, startEuler);
        ((PlaneDrone)drone).FreezeDrone(true);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        ((PlaneDrone)drone).FreezeDrone(false);
        Vector3 startVelocity = new Vector3(0.0f, 0.0f, 41.0f);
        Vector3 startLocation = new Vector3(1500.0f, 450.0f, 0.0f);
        Vector3 startEuler = new Vector3(-1.5f, 0.0f, 0.0f);
        drone.InitializeVehicle(startLocation, startVelocity, startEuler);
        drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);

        currTime = Time.time;
        initTime = Time.time;

        

    }
	protected override bool OnCheckSuccess ()
	{
        data.successText = "Scenario Succes!";
        return true;

    }

	protected override bool OnCheckFailure ()
	{
        drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);

        currTime = Time.time - initTime;
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

	protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}