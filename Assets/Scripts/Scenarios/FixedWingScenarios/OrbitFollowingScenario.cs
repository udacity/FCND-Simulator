using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;

public class OrbitFollowingScenario : Scenario
{ 

    public float timeInterval = 3.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float currentRadius;
    public float targetRadius = 100.0f;
    public float radiusThreshold = 3f;

    public Vector3 orbitCenter;

    Transform line;
    Material lineMat;
    

    protected override void OnInit ()
	{
        var planeControl = GameObject.Find("Plane Drone").GetComponent<PlaneAutopilot>().planeControl;
//        planeControl.SetDefaultLongitudinalGains();
//        planeControl.SetStudentLateralGains();
        base.OnInit ();
        drone.SetControlMode(8); //Line Following Mode
        drone.SetGuided(true);
        orbitCenter.x = data.vehiclePosition.z;
        orbitCenter.y = data.vehiclePosition.x + targetRadius;
        orbitCenter.z = -data.vehiclePosition.y;

        drone.CommandVector(orbitCenter, new Vector3(41.0f, 0.0f, 41.0f / targetRadius));


        /*
        endWaypoint.x = startWaypoint.x + 2000.0f;
        endWaypoint.y = startWaypoint.y;
        endWaypoint.z = startWaypoint.z;
                
        line = GameObject.Find("Line").GetComponent<Transform>();
        lineMat = GameObject.Find("Line").GetComponent<MeshRenderer>().material;
        lineMat.color = Color.red;
        line.position = new Vector3((startWaypoint.y + endWaypoint.y) / 2f, -(startWaypoint.z+endWaypoint.z)/2f, (startWaypoint.x+endWaypoint.x)/2f);
        line.localScale = new Vector3(Mathf.Abs(startWaypoint.y - endWaypoint.y)+0.1f, Mathf.Abs(startWaypoint.z - endWaypoint.z) + 0.1f, Mathf.Abs(startWaypoint.x - endWaypoint.x) + 0.1f);
        */
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        
        currTime = drone.FlightTime();
        initTime = drone.FlightTime();
    }

	protected override bool OnCheckSuccess ()
	{
        data.successText = "Orbit Following Scenario Successful!";
        return true;
    }

	protected override bool OnCheckFailure ()
	{
        //drone.CommandAttitude(new Vector3(0.0f, 450.0f, 0.0f), targetAirspeed);
        
        
        currTime = drone.FlightTime() - initTime;

        currentRadius  = Mathf.Sqrt(Mathf.Pow(orbitCenter.x - drone.CoordsUnity().z, 2.0f) + Mathf.Pow(orbitCenter.y - drone.CoordsUnity().x, 2.0f));
        if (Mathf.Abs(currentRadius-targetRadius) > radiusThreshold)
        {
            //lineMat.color = Color.red;
            if (currTime > data.runtime - timeInterval && currTime <= data.runtime)          
            
            {
                data.failText = "Scenario Failed:\n" +
                    "Crostrack Error = " + (currentRadius-targetRadius) + " m at t = " + currTime;
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