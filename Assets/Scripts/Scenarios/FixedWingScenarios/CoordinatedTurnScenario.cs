using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class CoordinatedTurnScenario : Scenario
{ 

    public float timeInterval = 5.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float currentSideslip;
    public float sideslipThreshold = 0.5f;
    public float targetRoll = 45.0f;
    

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(targetRoll*Mathf.PI/180.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        currTime = drone.FlightTime();
        initTime = drone.FlightTime();
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Coordinated Turn Scenario Successful!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
        
        currTime = drone.FlightTime() - initTime;
        currentSideslip = Mathf.Asin(drone.VelocityBody().y / drone.VelocityBody().magnitude);
        if (currTime > data.runtime - timeInterval && currTime <= data.runtime)
        {
            
            if (Mathf.Abs(currentSideslip)*180.0f/Mathf.PI > sideslipThreshold)
            {
                data.failText = "Scenario Failed:\n" +
                    "Sideslip = " + (currentSideslip*180.0f/Mathf.PI) + " deg at t = " + currTime;
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