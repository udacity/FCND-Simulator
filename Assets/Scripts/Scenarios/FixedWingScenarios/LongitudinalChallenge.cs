using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class LongitudinalChallenge : Scenario
{
    Vector2 gateStart = new Vector2(200.0f, 0.0f);
    Vector2 gateHigh = new Vector2(1100.0f, 80.0f);
    Vector2 gateLow = new Vector2(1400.0f, 80.0f);//new Vector2(1600.0f, -50.0f);
    Vector3 gateEnd = new Vector2(2200.0f, 0.0f);

    Transform gate1, gate2, gate3, gate4;
    public Vector2 position2D;


    public float currentAirspeed = 0.0f;
    private float lastAirspeedTime = 0.0f;
    private float airspeedThreshold = 1.0f;


    private float timeInterval = 2.0f;
    private float finalTime = 22.0f;
    private float initTime = 0.0f;
    public float currTime = 0.0f;
    private bool success = true;
    public float targetAirspeed = 41.0f;
    private float nominalThrust = 0.7f;
    public float currentClimbRate = 0.0f;

    float horizThreshold = 0.5f;
    float vertThreshold = 1.0f;

    public float targetAltitude;

    protected override void OnInit ()
	{
        gate1 = GameObject.Find("Gate1").GetComponent<Transform>();
        gate2 = GameObject.Find("Gate2").GetComponent<Transform>();
        gate3 = GameObject.Find("Gate3").GetComponent<Transform>();
        gate4 = GameObject.Find("Gate4").GetComponent<Transform>();

        base.OnInit ();
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
		drone.CommandAttitude(new Vector3(0.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        drone.CommandAttitude(new Vector3(0.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);

        initTime = Time.time;
        success = false;
    }

	protected override bool OnCheckSuccess ()
	{
        if (success)
        {
            data.successText = "Congratulations! You completed the Longitudinal Challenge!";
            return true;
        }
        return false;
    }

	protected override bool OnCheckFailure ()
	{
        float altitudeSwitch = 25.0f;

        position2D.x = Mathf.Sqrt(Mathf.Pow(drone.CoordsUnity().x - data.vehiclePosition.x, 2.0f) + Mathf.Pow(drone.CoordsUnity().z - data.vehiclePosition.z, 2.0f));
        position2D.y = drone.CoordsUnity().y - data.vehiclePosition.y;
        
        if (position2D.x <= gateStart.x)
        {
            targetAltitude = gateStart.y + data.vehiclePosition.y;
        }
        else if (position2D.x <= gateHigh.x)
        {
            targetAltitude = gateHigh.y + data.vehiclePosition.y;
        }
        else if (position2D.x <= gateLow.x)
        {
            targetAltitude = gateLow.y + data.vehiclePosition.y;
        }
        else if (position2D.x <= gateEnd.x)
        {
            targetAltitude = gateEnd.y + data.vehiclePosition.y;
        }
        else
        {
            targetAltitude = data.vehiclePosition.y;
        }

        if (Mathf.Abs(-drone.CoordsLocal().z - targetAltitude) < altitudeSwitch)
        {
            drone.SetControlMode(4);
            drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);
        }
        else if (-drone.CoordsLocal().z > targetAltitude)
        {
            drone.SetControlMode(5);
            drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 0.1f);
        }
        else
        {
            drone.SetControlMode(5);
            drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 1.0f);
        }

        currTime = Time.time - initTime;
        currentAirspeed = drone.VelocityLocal().magnitude;
        currentClimbRate = -drone.VelocityLocal().z;

        if (position2D.x <= gateStart.x)
        {
            if (Mathf.Abs(position2D.x - gateStart.x) < horizThreshold)
            {
                if (Mathf.Abs(position2D.y - gateStart.y) > vertThreshold)
                {
                    Debug.Log("Missed Gate Start");
                    data.failText = "Longitudinal Challenge Unsuccessful:\n" +
                        "Gate #1 Missed by " + Mathf.Abs(position2D.y - gateStart.y) + " meters " +
                        "(threshold = " + vertThreshold + " meters)";
                    return true;
                }
            }
        }
        else if (position2D.x <= gateHigh.x)
        {
            if (Mathf.Abs(position2D.x - gateHigh.x) < horizThreshold)
            {
                if (Mathf.Abs(position2D.y - gateHigh.y) > vertThreshold)
                {
                    Debug.Log("Missed Gate High");
                    data.failText = "Longitudinal Challenge Unsuccessful:\n" +
                        "Gate #2 Missed by " + Mathf.Abs(position2D.y - gateStart.y) + " meters " +
                        "(threshold = " + vertThreshold + " meters)";
                    return true;
                }
            }
        }
        else if (position2D.x <= gateLow.x)
        {
            if (Mathf.Abs(position2D.x - gateLow.x) < horizThreshold)
            {
                if (Mathf.Abs(position2D.y - gateLow.y) > vertThreshold)
                {
                    Debug.Log("Missed Gate Low");
                    data.failText = "Longitudinal Challenge Unsuccessful:\n" +
                        "Gate #3 Missed by " + Mathf.Abs(position2D.y - gateStart.y) + " meters " +
                        "(threshold = " + vertThreshold + " meters)";
                    return true;
                }
            }
        }
        else if (position2D.x <= gateEnd.x)
        {
            if (Mathf.Abs(position2D.x - gateEnd.x) < horizThreshold)
            {
                if (Mathf.Abs(position2D.y - gateEnd.y) > vertThreshold)
                {
                    Debug.Log("Missed Gate End");
                    data.failText = "Longitudinal Challenge Unsuccessful:\n" +
                        "Gate #4 Missed by " + Mathf.Abs(position2D.y - gateStart.y) + " meters " +
                        "(threshold = " + vertThreshold + " meters)";
                    return true;
                }
            }
        }
        else
        {
            success = true;
        }
        UpdateGatePosition();

        return false;
    }

    void UpdateGatePosition()
    {
        float heading = drone.AttitudeEuler().z;
        Vector3 position;
        float positionDiff = (gateStart.x-position2D.x);
        
        position.x = drone.CoordsUnity().x + positionDiff * Mathf.Sin(heading);
        position.z = drone.CoordsUnity().z + positionDiff * Mathf.Cos(heading);
        position.y = gate1.position.y;
        if(positionDiff > 0.0f)
            gate1.position = position;

        positionDiff = -(position2D.x - gateHigh.x);

        position.x = drone.CoordsUnity().x + positionDiff * Mathf.Sin(heading);
        position.z = drone.CoordsUnity().z + positionDiff * Mathf.Cos(heading);
        position.y = gate2.position.y;
        if(positionDiff > 0.0f)
            gate2.position = position;

        positionDiff = -(position2D.x - gateLow.x);

        position.x = drone.CoordsUnity().x + positionDiff * Mathf.Sin(heading);
        position.z = drone.CoordsUnity().z + positionDiff * Mathf.Cos(heading);
        position.y = gate3.position.y;
        if(positionDiff > 0.0f)
            gate3.position = position;

        positionDiff = -(position2D.x - gateEnd.x);

        position.x = drone.CoordsUnity().x + positionDiff * Mathf.Sin(heading);
        position.z = drone.CoordsUnity().z + positionDiff * Mathf.Cos(heading);
        position.y = gate4.position.y;
        if (positionDiff > 0.0f)
            gate4.position = position;
    }

    

	protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}