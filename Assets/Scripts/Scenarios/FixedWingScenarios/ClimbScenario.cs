using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class ClimbScenario : Scenario
{
    IDrone drone;
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
        drone = GameObject.Find("Plane Drone").GetComponent<PlaneDrone>();
        drone.SetControlMode(5); //AscendDescend Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 0.7f);
        Vector3 startVelocity = new Vector3(0.0f, 0.0f, 41.0f);
        Vector3 startLocation = new Vector3(1500.0f, 450.0f, 0.0f);
        Vector3 startEuler = new Vector3(-1.5f, 0.0f, 0.0f);
        drone.InitializeVehicle(startLocation, startVelocity, startEuler);
        ((PlaneDrone)drone).FreezeDrone(true);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        Vector3 startVelocity = new Vector3(0.0f, 0.0f, 45.0f);
        Vector3 startLocation = new Vector3(1500.0f, 450.0f, 0.0f);
        Vector3 startEuler = new Vector3(-1.5f, 0.0f, 0.0f);
        drone.InitializeVehicle(startLocation, startVelocity, startEuler);
        ((PlaneDrone)drone).FreezeDrone(false);
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