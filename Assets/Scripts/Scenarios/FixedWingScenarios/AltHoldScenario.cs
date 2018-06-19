using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class AltHoldScenario : Scenario
{
    Transform line;
    Material lineMat;
    public float currentAltitude = 0.0f;
    private float lastAltitudeTime = 0.0f;
    public float altitudeThreshold = 2.0f;

    public float currentAirspeed = 0.0f;
    private float lastAirspeedTime = 0.0f;
    private float airspeedThreshold = 0.1f;

    private float timeInterval = 5.0f;
    private float finalTime = 15;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float targetAirspeed = 41.0f;
    public float targetAltitude = 450.0f;

    protected override void OnInit ()
	{
        base.OnInit ();
        line = GameObject.Find("Line").GetComponent<Transform>();
        lineMat = GameObject.Find("Line").GetComponent<MeshRenderer>().material;
        lineMat.color = Color.red;
        line.position = new Vector3(data.vehiclePosition.x, targetAltitude, data.vehiclePosition.z);
        line.localScale = new Vector3(0.1f, 0.1f, 2000.0f);
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);
        planeControl.SetDefaultLongitudinalGains();
        planeControl.SetDefaultLateralGains();
        planeControl.SetStudentLongitudinalGains();
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        

        initTime = drone.FlightTime();
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Altitude Hold Scenario Successful!";
        return true;

    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);
        currTime = drone.FlightTime() - initTime;
        currentAltitude = -drone.CoordsLocal().z;
        if (Mathf.Abs(currentAltitude - targetAltitude) > altitudeThreshold)           
        {
            if (currTime > finalTime - timeInterval && currTime <= finalTime)
            {
                data.failText = "Altitude scenario not successful:\n" +
                    "Target Altitude = " + targetAltitude + "\n" +
                    "Altitude = " + currentAltitude + " at t = " + currTime;
                return true;
                
            }
            lineMat.color = Color.red;
        }else
        {
            lineMat.color = Color.green;
        }

        return false;
    }

    protected override void OnEnd()
    {
        line.localScale = new Vector3(0f, 0f, 0f);
        base.OnEnd();
        drone.SetGuided(false);
    }

    protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}