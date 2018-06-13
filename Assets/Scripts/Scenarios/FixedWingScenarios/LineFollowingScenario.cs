using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class LineFollowingScenario : Scenario
{ 

    public float timeInterval = 3.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float currentXTrack;
    public float xTrackThreshold = 3f;
    public float targetCourse;
    public Vector3 startWaypoint;
    public Vector3 endWaypoint;

    Transform line;
    Material lineMat;
    

    protected override void OnInit ()
	{
        base.OnInit ();
        drone.SetControlMode(7); //Line Following Mode
        drone.SetGuided(true);
        startWaypoint.x = data.vehiclePosition.z;
        startWaypoint.y = data.vehiclePosition.x + 20.0f;
        startWaypoint.z = -data.vehiclePosition.y;
        endWaypoint.x = startWaypoint.x + 2000.0f;
        endWaypoint.y = startWaypoint.y;
        endWaypoint.z = startWaypoint.z;
        drone.CommandVector(startWaypoint, 41.0f*(endWaypoint-startWaypoint)/((endWaypoint - startWaypoint).magnitude));

        line = GameObject.Find("Line").GetComponent<Transform>();
        lineMat = GameObject.Find("Line").GetComponent<MeshRenderer>().material;
        lineMat.color = Color.red;
        line.position = new Vector3((startWaypoint.y + endWaypoint.y) / 2f, -(startWaypoint.z+endWaypoint.z)/2f, (startWaypoint.x+endWaypoint.x)/2f);
        line.localScale = new Vector3(Mathf.Abs(startWaypoint.y - endWaypoint.y)+0.1f, Mathf.Abs(startWaypoint.z - endWaypoint.z) + 0.1f, Mathf.Abs(startWaypoint.x - endWaypoint.x) + 0.1f);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        currTime = drone.FlightTime();
        initTime = drone.FlightTime();
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Line Following Scenario Successful!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
        
        
        currTime = drone.FlightTime() - initTime;
        targetCourse = Mathf.Atan2((endWaypoint - startWaypoint).y , (endWaypoint - startWaypoint).x);
        currentXTrack = Mathf.Cos(targetCourse) * (drone.CoordsUnity().x - startWaypoint.y) + Mathf.Sin(-targetCourse) * (drone.CoordsUnity().z - startWaypoint.x);
        if (Mathf.Abs(currentXTrack) > xTrackThreshold)
        {
            lineMat.color = Color.red;
            if (currTime > data.runtime - timeInterval && currTime <= data.runtime)          
            
            {
                data.failText = "Scenario Failed:\n" +
                    "Crostrack Error = " + currentXTrack + " m at t = " + currTime;
                return true;
            }
        }
        else
        {
            lineMat.color = Color.green;
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