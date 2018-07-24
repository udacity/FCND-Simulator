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
        drone.Status = 6;
        //drone.CommandAttitude(new Vector3(targetRoll, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
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
        data.successText = "Stabilize Roll Scenario Successful!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //droneObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        if (!drone.MotorsArmed())
            drone.CommandAttitude(new Vector3(targetRoll, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
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
        onParameter1Update((targetRoll-currentRoll)*180/Mathf.PI, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
    }

}