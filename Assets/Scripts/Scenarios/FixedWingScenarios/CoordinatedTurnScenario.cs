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
        drone.Status = 7;
        //drone.CommandAttitude(new Vector3(targetRoll*Mathf.PI/180.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
        //Transform line = GameObject.Find("Line").GetComponent<Transform>();
        //line.localScale = new Vector3(0, 0, 0);
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
        if(!drone.MotorsArmed())
            drone.CommandAttitude(new Vector3(targetRoll * Mathf.PI / 180.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
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
            //Debug.Log("Sideslip (deg): " + (Mathf.Abs(currentSideslip) * 180.0f / Mathf.PI));
        }

        UpdateVizParameters();
        return false;
    }

    protected override void OnEnd()
    {
        drone.SetGuided(false);
        Debug.Log("Released control of the Drone");
        base.OnEnd();
        
    }

    void UpdateVizParameters()
    {
        onParameter1Update(currentSideslip*180/Mathf.PI, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
    }

    protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}