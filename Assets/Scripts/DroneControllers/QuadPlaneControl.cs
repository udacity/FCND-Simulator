using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;

[System.Serializable]
public class QuadPlaneControl : IControlLaw {

    public QuadControl QuadControl;
    public PlaneControl PlaneControl;
    public float toPlaneAirspeed;
    public float toQuadAirspeed;

    public float toPlaneThrottle;
    public float toQuadThrottle;

    public QuadPlaneControl()
    {

    }
    // Use this for initialization
	public QuadPlaneControl(QuadControl quadControl, PlaneControl planeControl)
    {
        QuadControl = quadControl;
        PlaneControl = planeControl;
    }

    public void SetScenarioParameters(string[] names)
    {
        QuadControl.SetScenarioParameters(names);
        PlaneControl.SetScenarioParameters(names);
    }



}
