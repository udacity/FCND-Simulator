using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class LongitudinalChallenge : Scenario
{
    Vector2 gate1 = new Vector2(200.0f, 0.0f);
    Vector2 gate2 = new Vector2(1100.0f, 100.0f);
    Vector2 gate3 = new Vector2(1400.0f, 80.0f);//new Vector2(1600.0f, -50.0f);
    Vector2 gate4 = new Vector2(2200.0f, 0.0f);
    Vector2 targetGate;

    Transform gate;
    //Transform line;
    //Material lineMat;
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
    public float vertThreshold = 2.0f;

    public float targetAltitude;
    public float altitudeSwitch = 25.0f;
    int mode;
    protected override void OnInit ()
	{
        gate = GameObject.Find("Gate").GetComponent<Transform>();
        //line = GameObject.Find("Line").GetComponent<Transform>();
        //lineMat = GameObject.Find("Line").GetComponent<MeshRenderer>().material;
        //lineMat.color = Color.red;
        
        base.OnInit ();
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(true);
        drone.Status = 5;
		//drone.CommandAttitude(new Vector3(0.0f, data.vehiclePosition.y, 0.0f), targetAirspeed);
        drone.SetHomePosition();
        targetGate = gate1;
        UpdateGatePosition();
    }

    protected override void OnBegin()
    {
        base.OnBegin();
        drone.CommandAttitude(new Vector3(0.0f, data.vehiclePosition.y, 0.0f), targetAirspeed);
        initTime = drone.FlightTime();
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
        droneObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        position2D.x = Mathf.Sqrt(Mathf.Pow(drone.CoordsUnity().x - data.vehiclePosition.x, 2.0f) + Mathf.Pow(drone.CoordsUnity().z - data.vehiclePosition.z, 2.0f));
        position2D.y = drone.CoordsUnity().y - data.vehiclePosition.y;
        if (!drone.MotorsArmed())
        {
            
            targetAltitude = targetGate.y + data.vehiclePosition.y;

            if (Mathf.Abs(-drone.CoordsLocal().z - targetAltitude) < altitudeSwitch)
            {
                drone.SetControlMode(4);
                drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);
                mode = 0;
            }
            else if (-drone.CoordsLocal().z > targetAltitude)
            {
                drone.SetControlMode(5);
                drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 0.1f);
                mode = -1;
            }
            else
            {
                drone.SetControlMode(5);
                drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 1.0f);
                mode = 1;
            }
        }

        currTime = drone.FlightTime() - initTime;
        currentAirspeed = drone.VelocityLocal().magnitude;
        currentClimbRate = -drone.VelocityLocal().z;
        float gateError = Mathf.Abs(position2D.y - targetGate.y);
        

        if (Mathf.Abs(position2D.y - targetGate.y) > vertThreshold) 
        {
            if ((position2D.x - targetGate.x) > -horizThreshold)
            {
                Debug.Log("Missed Gate");
                data.failText = "Longitudinal Challenge Unsuccessful:\n" +
                    "Gate Missed by " + Mathf.Abs(position2D.y - targetGate.y) + " meters " +
                    "(threshold = " + vertThreshold + " meters)";
                return true;
            }
            //lineMat.color = Color.red;
            ApplyLineColor(Color.red);
        }
        else
        {
            //lineMat.color = Color.green;
            ApplyLineColor(Color.green);
        }


        if (position2D.x <= gate1.x)
            targetGate = gate1;
        else if (position2D.x <= gate2.x)
            targetGate = gate2;
        else if (position2D.x <= gate3.x)
            targetGate = gate3;
        else
            targetGate = gate4;

        if (position2D.x > gate4.x)
            success = true;
        UpdateGatePosition();
        UpdateVizParameters();
        return false;
    }

    void UpdateGatePosition()
    {
        float heading = drone.AttitudeEuler().z;
        Vector3 position;
        float positionDiff = (targetGate.x-position2D.x);
        
        position.x = drone.CoordsUnity().x + positionDiff * Mathf.Sin(heading);
        position.z = drone.CoordsUnity().z + positionDiff * Mathf.Cos(heading);
        position.y = targetGate.y + data.vehiclePosition.y; ;
        if(positionDiff > 0.0f)
            gate.position = position;
        gate.localScale = new Vector3(4.0f, 1.0f, 4.0f);
        Vector3 linePosition = position;
        //linePosition.y = position.y;
        //line.position = linePosition;
        //Vector3 scale = line.localScale;
        //scale.x = 2000.0f;
        //line.localScale = new Vector3(2000.0f, 0.1f, 0.1f);
        //line.eulerAngles = new Vector3(0, 0, 0);

    }

    protected override void OnEnd()
    {
        drone.SetGuided(false);
        //line.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        gate.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        base.OnEnd();        
    }


    protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}

	public override void OnApplyTunableValues ()
	{
        base.OnApplyTunableValues();
		altitudeSwitch = TunableManager.GetParameter ( "altitudeSwitch" ).value;
	}

    void UpdateVizParameters()
    {
        onParameter1Update(position2D.y - targetGate.y, 1);
        //float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0) * 0.5f - 0.25f;
        //onParameter2Update(0.5f + noise, 2);
        onParameter2Update(mode, 0);
    }
}