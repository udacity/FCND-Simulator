﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;

public class LateralChallenge: Scenario
{

    //Vector2 gate1 = new Vector2(1700, 500);
    Vector2 startPosition = new Vector2(1700, 500);
    Vector2 gate1 = new Vector2(1700, 1000);
    Vector3 gate2 = new Vector2(1300, 1400);
    Vector3 gate3 = new Vector2(1000, 1100);
    Vector2 gate4 = new Vector2(1000, 100);
    Vector2 targetGate;
    public int gateNum;
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

    

    protected override void OnInit()
    {
        
        
        gate = GameObject.Find("Gate").GetComponent<Transform>();
        line = GameObject.Find("Line").GetComponent<Transform>();
        lineMat = GameObject.Find("Line").GetComponent<MeshRenderer>().material;
        lineMat.color = Color.red;
        //gate2 = GameObject.Find("Gate2").GetComponent<Transform>();
        //gate3 = GameObject.Find("Gate3").GetComponent<Transform>();
        //gate4 = GameObject.Find("Gate4").GetComponent<Transform>();

        base.OnInit();
        drone.SetControlMode(7); //Stabilized Mode
        drone.SetGuided(true);
        Vector3 startWaypoint = new Vector3(startPosition.y, startPosition.x, -data.vehiclePosition.y);
        Vector3 goalWaypoint = new Vector3(gate1.y, gate1.x, -data.vehiclePosition.y);
        Vector3 velocityVec = goalWaypoint - startWaypoint;
        velocityVec = 41.0f * velocityVec.normalized;
        drone.CommandVector(startWaypoint, velocityVec);
        drone.SetHomePosition();
        targetGate = gate1;
        gateNum = 1;
        UpdateGatePosition();
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        //drone.CommandAttitude(new Vector3(0.0f, data.vehiclePosition.y, 0.0f), data.vehicleVelocity.magnitude);
        //Set the gains appropriate for the scenario
//        planeControl.SetDefaultLongitudinalGains();
//        planeControl.SetStudentLateralGains();

        initTime = drone.FlightTime();
        success = false;
    }

    protected override bool OnCheckSuccess()
    {
        if (success)
        {
            data.successText = "Congratulations! You completed the Lateral Challenge!";
            return true;
        }
        return false;
    }

    protected override bool OnCheckFailure()
    {

        gateError = Mathf.Sqrt(Mathf.Pow(drone.CoordsUnity().z - targetGate.y, 2f) + Mathf.Pow(drone.CoordsUnity().x - targetGate.x, 2f));
        if (gateNum == 1)
        {
            line.localPosition = new Vector3((startPosition.x + gate1.x) / 2, data.vehiclePosition.y, (startPosition.y + gate1.y) / 2);
            line.localScale = new Vector3(Mathf.Abs(startPosition.x - gate1.x) + 1f, 1f, Mathf.Abs(startPosition.y - gate1.y) + 1f);
            if (drone.CoordsUnity().z >= gate1.y)
            {
                if (gateError > errorThreshold)
                {
                    data.failText = "Lateral Challenged Not Completed.\n" +
                        "Gate was missed by " + gateError + " meters " +
                        " (Threshold = " + errorThreshold + " meters)";
                    return true;
                }

                
                Vector3 orbitCenter = new Vector3(1000f, 1300f, -data.vehiclePosition.y);
                targetRadius = 400f;
                Vector3 velocityVec = new Vector3(41.0f, 0.0f, -41.0f / targetRadius);
                if (drone.ControlMode() != 3)
                {
                    drone.SetControlMode(8);
                    drone.CommandVector(orbitCenter, velocityVec);
                }
                gateNum++;
                Debug.Log("Gate #1 Error = " + gateError);
                targetGate = gate2;
            }
        } else if (gateNum == 2)
        {
            line.localScale = new Vector3(0f,0f,0f);
            if (drone.CoordsUnity().x <= gate2.x)
            {
                if (gateError > errorThreshold)
                {
                    data.failText = "Lateral Challenged Not Completed.\n" +
                        "Gate was missed by " + gateError + " meters " +
                        " (Threshold = " + errorThreshold + " meters)";
                    return true;
                }
                
                Vector3 orbitCenter = new Vector3(1100f, 1300f, -data.vehiclePosition.y);
                targetRadius = 300f;
                Vector3 velocityVec = new Vector3(41.0f, 0.0f, -41.0f / targetRadius);
                if (drone.ControlMode() != 3)
                {
                    drone.SetControlMode(8);
                    drone.CommandVector(orbitCenter, velocityVec);
                }
                gateNum++;
                Debug.Log("Gate #2 Error = " + gateError);
                targetGate = gate3;
            }
        } else if (gateNum == 3)
        {

            if(drone.CoordsUnity().z <= gate3.y)
            {
                if (gateError > errorThreshold)
                {
                    data.failText = "Lateral Challenged Not Completed.\n" +
                        "Gate was missed by " + gateError + " meters " +
                        " (Threshold = " + errorThreshold + " meters)";
                    return true;
                }
                
                Vector3 startWaypoint = new Vector3(gate3.y, gate3.x, -data.vehiclePosition.y);
                Vector3 endWaypoint = new Vector3(gate4.y, gate4.x, -data.vehiclePosition.y);
                Vector3 velocityVec = 41f * (endWaypoint - startWaypoint).normalized;
                if (drone.ControlMode() != 3)
                {
                    drone.SetControlMode(7);
                    drone.CommandVector(startWaypoint, velocityVec);
                }
                gateNum++;
                Debug.Log("Gate #3 Error = " + gateError);
                targetGate = gate4;
            }
        }else if (gateNum  == 4)
        {
            line.localPosition = new Vector3((gate4.x + gate3.x) / 2, data.vehiclePosition.y, (gate4.y + gate3.y) / 2);
            line.localScale = new Vector3(Mathf.Abs(gate4.x - gate3.x) + 1f, 1f, Mathf.Abs(gate4.y - gate3.y) + 1f);
            if (drone.CoordsUnity().z <= gate4.y)
            {
                if (gateError > errorThreshold)
                {
                    data.failText = "Lateral Challenged Not Completed.\n" +
                        "Gate was missed by " + gateError + " meters " +
                        " (Threshold = " + errorThreshold + " meters)";
                    return true;
                }
                else
                {
                    Debug.Log("Gate #4 Error = " + gateError);
                    success = true;
                }

            }
            

        }


        UpdateGatePosition();

        return false;
    }

    void UpdateGatePosition()
    {
        float heading = drone.AttitudeEuler().z;
        gate.position = new Vector3(targetGate.x, data.vehiclePosition.y, targetGate.y);
        gate.localScale = new Vector3(4f, 1f, 4f);
        gate.rotation.SetEulerAngles(0f, heading, 0f);
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
}