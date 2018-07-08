using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class YawHoldScenario : Scenario
{ 

    public float timeInterval = 3.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float currentYaw;
    public float yawThreshold = 5f;
    

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(6); //Stabilized Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(0.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
        Transform line = GameObject.Find("Line").GetComponent<Transform>();
        line.localScale = new Vector3(0, 0, 0);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        currTime = drone.FlightTime();
        initTime = drone.FlightTime();
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Constant Yaw Scenario Successful!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
        
        currTime = drone.FlightTime() - initTime;
        currentYaw = drone.AttitudeEuler().z;
        if (currTime > data.runtime - timeInterval && currTime <= data.runtime)
        {
            
            if (Mathf.Abs(currentYaw)*180.0f/Mathf.PI > yawThreshold)
            {
                data.failText = "Scenario Failed:\n" +
                    "Yaw = " + (currentYaw*180.0f/Mathf.PI) + " deg at t = " + currTime;
                return true;
            }
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

    protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}

    void UpdateVizParameters()
    {
        onParameter1Update((0-currentYaw)*180/Mathf.PI, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
    }
}