using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;

public class FlyingCarChallenge: Scenario
{



    protected override void OnInit()
    {
        base.OnInit();
        drone.SetControlMode(10);
        drone.SetGuided(true);
        drone.Status = 13;
        drone.SetHomePosition();
        base.OnInit();
    }

    protected override void OnBegin()
    {
        base.OnBegin();

    }

    protected override bool OnCheckSuccess()
    {
        return false;
    }

    protected override bool OnCheckFailure()
    {

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
        //onParameter1Update(xTrackError, 1);
        //onParameter2Update(-vertError, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
    }

}