using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;

public class FixedWingChallenge: Scenario
{

    //Vector2 gate1 = new Vector2(1700, 500);
    Vector2 startPosition = new Vector2(1700, 500);

    /*
    Vector2 gate1 = new Vector2(1700, 1000);
    Vector3 gate2 = new Vector2(1300, 1400);
    Vector3 gate3 = new Vector2(1000, 1100);
    Vector2 gate4 = new Vector2(1000, 100);
    */

    public Vector3 waypoint1 = new Vector3(0, 2000, -400);
    public Vector3 waypoint2 = new Vector3(2000, 2000, -500);
    public Vector3 waypoint3 = new Vector3(2000, 0, -400);
    public Vector3 waypoint4 = new Vector3(0, 2000, -450);
    public Vector3 waypoint5 = new Vector3(0, 0, -450);
    //public Vector3 gate1 = new Vector3(1500, 2000, -350);
    //Vector3 gate2 = new Vector3(2000, 1000, -500);
    //Vector3 gate3 = new Vector3(1300, 700, 450);


    Vector3[] waypoints = new Vector3[10];
    //Vector3[] gates = new Vector3[10];

    Vector3 prevWaypoint;
    Vector3 currWaypoint;
    Vector3 nextWaypoint;
    public Vector3 targetGate;
    Vector3 targetPlane;
    float state;
    public int gateNum;
    public int numOfGates = 10;
    public float targetRadius;

    Transform gate;
    Transform line;
    Material lineMat;
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

    public float gateError;
    public float errorThreshold = 2.0f;

    public float targetAltitude;

    float xTrackError;
    float vertError;

    protected override void OnInit()
    {
        gate = GameObject.Find("Gate").GetComponent<Transform>();
        line = GameObject.Find("Line").GetComponent<Transform>();
        lineMat = GameObject.Find("Line").GetComponent<MeshRenderer>().material;
        lineMat.color = Color.red;

        base.OnInit();
        drone.SetControlMode(7);
        drone.SetGuided(true);
        drone.Status = 12;

        prevWaypoint = waypoint1;
        currWaypoint = waypoint2;
        nextWaypoint = waypoint3;

        //drone.CommandVector(prevWaypoint, 41*(currWaypoint-prevWaypoint).normalized);
        drone.SetHomePosition();

        gateNum = 1;
        state = 1;
        UpdateGatePosition();
        SetWaypointValues();
    }

    protected override void OnBegin()
    {
        base.OnBegin();


        initTime = drone.FlightTime();
        success = false;
    }

    protected override bool OnCheckSuccess()
    {
        if (success)
        {
            data.successText = "Congratulations! You completed the Fixed Wing Challenge!";
            return true;
        }
        return false;
    }

    protected override bool OnCheckFailure()
    {
        gateError = (targetGate - drone.CoordsLocal()).magnitude;
        
        //Debug.Log("Gate Error: " + gateError);
        Vector2 q0 = new Vector2(currWaypoint.x - prevWaypoint.x, currWaypoint.y - prevWaypoint.y).normalized;
        Vector2 q1 = new Vector2(nextWaypoint.x - currWaypoint.x, nextWaypoint.y - currWaypoint.y).normalized;
        float angle = Mathf.Acos(-Vector2.Dot(q0, q1));
        float flag;
        float radius = 500;

        if (state == 1)
        {

            Debug.Log("Straight Line Segment");
            flag = 1;
            Vector2 r = new Vector2(prevWaypoint.x, prevWaypoint.y);
            Vector2 q = q0;
            
            Vector2 z = new Vector2(currWaypoint.x, currWaypoint.y) - (radius/ Mathf.Tan(angle / 2)) * q0;
            Vector2 p = new Vector2(drone.CoordsLocal().x, drone.CoordsLocal().y);
            float heading = Mathf.Atan2(q.y, q.x);
            xTrackError = (z.y-p.y) * Mathf.Cos(heading) - (z.x - p.x) * Mathf.Sin(heading);
            vertError = prevWaypoint.z - drone.CoordsLocal().z;
            if (gateNum != numOfGates)
            {
                targetGate = new Vector3(z.x, z.y, prevWaypoint.z);
            }
            else
            {
                targetGate = new Vector3(currWaypoint.x, currWaypoint.y, prevWaypoint.z);
                z = new Vector2(nextWaypoint.x, nextWaypoint.y);
            }
            
            if (Vector2.Dot(p-z,q) > 0)
            {
                if (gateError > errorThreshold)
                {
                    data.failText = "Fixed Wing Challenge Unsuccessful:\n" +
                        "Gate Missed by " + gateError + " meters " +
                        "(threshold = " + errorThreshold + " meters)";
                    return true;
                }
                else if (gateNum == numOfGates)
                {
                    success = true;
                    return false;
                }
                state = 2;
            }
            else
            {
                if (!drone.MotorsArmed())
                {
                    drone.SetControlMode(7);
                    drone.CommandVector(prevWaypoint, 41 * (currWaypoint - prevWaypoint).normalized);
                }
            }
        }

        if (state == 2)
        {
            Debug.Log("Orbit Segment");
            flag = 2;
            Vector2 c = new Vector2(currWaypoint.x, currWaypoint.y) - (radius / Mathf.Sin(angle / 2)) * ((q0 - q1).normalized);
            float rho = radius;
            float lambda = Mathf.Sign(q0.x * q1.y - q0.y * q1.x);
            Vector2 z = new Vector2(currWaypoint.x, currWaypoint.y) + (radius / Mathf.Tan(angle / 2)) * q1;
            Vector2 p = new Vector2(drone.CoordsLocal().x, drone.CoordsLocal().y);
            float heading = Mathf.Atan2(q1.y, q1.x);
            xTrackError = (z.y - p.y) * Mathf.Cos(heading) - (z.x - p.x) * Mathf.Sin(heading);
            vertError = currWaypoint.z - drone.CoordsLocal().z;
            if (Vector2.Dot(p - z, q1) > 0)
            {
                state = 1;
                prevWaypoint = currWaypoint;
                currWaypoint = nextWaypoint;
                gateNum++;
                nextWaypoint = waypoints[gateNum+1];
                Debug.Log("Prev Waypoint: " + prevWaypoint + " CurrWaypoint: " + currWaypoint + "  nextWaypoint: " + nextWaypoint);
            }
            else
            {
                
                Vector3 orbitCenter = new Vector3(c.x, c.y, prevWaypoint.z);
                Vector3 velocityVec = new Vector3(41.0f, 0.0f, lambda*41.0f / radius);
                Debug.Log("Orbit Center: " + orbitCenter);
                if (!drone.MotorsArmed())
                {
                    drone.SetControlMode(8);
                    drone.CommandVector(orbitCenter, velocityVec);
                }
            }
        }

        UpdateGatePosition();
        UpdateVizParameters();
        return false;
    }

    void UpdateGatePosition()
    {
        if (state == 1)
        {
            line.position = new Vector3((currWaypoint.y + prevWaypoint.y) / 2, -prevWaypoint.z, (currWaypoint.x + prevWaypoint.x) / 2);
            float legLength = Mathf.Sqrt(Mathf.Pow(currWaypoint.x - prevWaypoint.x, 2) + Mathf.Pow(currWaypoint.y - prevWaypoint.y, 2));
            line.localScale = new Vector3(1f, 1f, legLength);
            float heading = Mathf.Atan2(currWaypoint.y - prevWaypoint.y, currWaypoint.x - prevWaypoint.x);
            line.eulerAngles = new Vector3(0f, heading * 180f / Mathf.PI, -90);
            Debug.Log("Rotation: " + line.rotation + " heading: " + heading);
            gate.position = new Vector3(targetGate.y, -targetGate.z, targetGate.x);
            gate.localScale = new Vector3(5f, 5f, 5f);
            gate.eulerAngles = new Vector3(0f, heading * 180f / Mathf.PI, -90f);
        }
        else
        {
            line.position = new Vector3((nextWaypoint.y + currWaypoint.y) / 2, -currWaypoint.z, (nextWaypoint.x + currWaypoint.x) / 2);
            float legLength = Mathf.Sqrt(Mathf.Pow(nextWaypoint.x - currWaypoint.x, 2) + Mathf.Pow(nextWaypoint.y - currWaypoint.y, 2));
            line.localScale = new Vector3(1f, 1f, legLength);
            float heading = Mathf.Atan2(nextWaypoint.y - currWaypoint.y, nextWaypoint.x - currWaypoint.x);
            line.eulerAngles = new Vector3(0f, heading * 180f / Mathf.PI, 0);
            Debug.Log("Rotation: " + line.rotation + " heading: " + heading);
            gate.position = new Vector3(targetGate.y, -targetGate.z, targetGate.x);
            gate.localScale = new Vector3(0f, 0f, 0f);
            gate.eulerAngles = new Vector3(0f, heading * 180f / Mathf.PI, -90f);
        }
    }

    protected override void OnEnd()
    {
        gate.localScale = new Vector3(0f, 0f, 0f);
        line.localScale = new Vector3(0f, 0f, 0f);
        drone.SetGuided(false);
        base.OnEnd();
    }


    protected override void OnCleanup()
    {
        base.OnCleanup();
    }

    void SetWaypointValues()
    {

        waypoints[0] = waypoint1;
        waypoints[1] = waypoint2;
        waypoints[2] = waypoint3;
        waypoints[3] = waypoint4;
        waypoints[4] = waypoint5;
    }

    void UpdateVizParameters()
    {
        onParameter1Update(xTrackError, 1);
        onParameter2Update(-vertError, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
    }

}