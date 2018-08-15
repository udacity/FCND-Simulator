using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;
using DroneVehicles;

public class FlyingCarChallenge: Scenario
{
    CoraVehicle vehicle; 


    protected override void OnInit()
    {
        base.OnInit();
        drone.SetControlMode(10);
        drone.SetGuided(true);
        drone.Status = 13;

        base.OnInit();
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        vehicle = GameObject.Find("Plane Drone").GetComponent<CoraVehicle>();
        drone.SetHomePosition();
        vehicle.energy = 1.0f;
        Vector3 takeoffPosition = Vector3.zero;
        takeoffPosition.z = drone.CoordsLocal().z + 15f;
        drone.CommandPosition(takeoffPosition);

    }


    protected override bool OnCheckSuccess()
    {
        return false;
    }

    protected override bool OnCheckFailure()
    {
        UpdateVizParameters();

        return false;
    }


    protected override void OnEnd()
    {
        base.OnEnd();
    }


    protected override void OnCleanup()
    {
        base.OnCleanup();
    }



    void UpdateVizParameters()
    {
        onParameter1Update(vehicle.energy, 3);
        //onParameter1Update(xTrackError, 1);
        //onParameter2Update(-vertError, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
    }

}