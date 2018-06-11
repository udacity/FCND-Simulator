using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class StabilizeRollScenario : Scenario
{ 

    public float timeInterval = 3.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float currentRoll;
    public float targetRoll = 0.0f;
    public float rollThreshold = 5f;
    

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(4); //Yaw Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(targetRoll, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        currTime = drone.FlightTime();
        initTime = drone.FlightTime();
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Stabilize Roll Scenario Successful!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
        
        currTime = drone.FlightTime() - initTime;
        currentRoll = drone.AttitudeEuler().x;
        if (currTime > data.runtime - timeInterval && currTime <= data.runtime)
        {
            
            if (Mathf.Abs(currentRoll)*180.0f/Mathf.PI > rollThreshold)
            {
                data.failText = "Scenario Failed:\n" +
                    "Roll = " + (currentRoll*180.0f/Mathf.PI) + " deg at t = " + currTime;
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