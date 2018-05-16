using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;
using DroneVehicles;
using DroneSensors;

public class Scenario3 : MonoBehaviour
{
    private IDrone drone;

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
    
    /*
    public float elevatorTrim = 0.0f;
    public float throttleTrim = 0.0f;
    */
    /*private MAVLinkMessenger messenger;
    public NetworkController networkController;

    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;

    public int gpsIntervalHz = 100;
    public int attitudeIntervalHz = 500;
    public int imuIntervalHz = 100;
    public int homePositionIntervalHz = 1;
    */
    void Start()
    {
        Debug.Log("Starting Fixed Wing");
        //GameObject test = GameObject.Find("Plane Drone");
        drone = GameObject.Find("Plane Drone").GetComponent<PlaneDrone>();
        //GameObject.Find("Quad Drone").GetComponent<QuadVehicle>().StateUpdate();
        //GameObject.Find("Quad Drone").GetComponent<QuadSensors>()
        drone.SetHomePosition();// drone.GPSLongitude(), drone.GPSLatitude(), drone.GPSAltitude());
        /*
        //drone.ControlRemotely(false);
        messenger = new MAVLinkMessenger();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Conversions.HertzToMilliSeconds(gpsIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(gpsIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.AttitudeQuaternion, Conversions.HertzToMilliSeconds(attitudeIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.ScaledIMU, Conversions.HertzToMilliSeconds(imuIntervalHz));

        //Add scenario screen for selecting different scenarios
        */
        StartScenario1();
    }

    public void StartScenario1()
    {
        Debug.Log("Start Scenerio 3");

        //Initialize state
        Vector3 startLocation = new Vector3(900.0f, 150.0f, -700.0f);
        Vector3 startVelocity = new  Vector3(-41.0f*Mathf.Sqrt(2.0f)/2.0f, 0.0f, 41.0f*Mathf.Sqrt(2.0f) / 2.0f); //Vector3(0.0f, 0.0f, 40.0f);//
        Vector3 startEuler = new Vector3(-1.3f, -45.0f, 0.0f);
        
        drone.InitializeVehicle(startLocation, startVelocity, startEuler);
        drone.SetControlMode(4); //Stabilized Mode
        drone.SetGuided(false);
        
        //drone.CommandAttitude(new Vector3(0.0f, targetAltitude, 0.0f), targetAirspeed);
        success = true;
        initTime = Time.time;

    }

    private void FixedUpdate()
    {
        //elevatorTrim += Input.GetAxis("Trim")*0.001f;
        /*
        throttleTrim += Input.GetAxis("Trim") * 0.001f;
        drone.CommandAttitude(new Vector3(0.0f, elevatorTrim, 0.0f), 0.7f + throttleTrim);
        */
        drone.SetControlMode(5); //Stabilized Mode
        drone.SetGuided(true);
        if(Time.time-initTime < 5)
            drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), nominalThrust);
        else
            drone.CommandAttitude(new Vector3(0.0f, targetAirspeed, 0.0f), 1.0f);

        SuccessScenario3();
    }

    public void SuccessScenario3()
    {
        currTime = Time.time-initTime;
        currentAirspeed = drone.VelocityLocal().magnitude;
        currentClimbRate = -drone.VelocityLocal().z;
        if (currTime > finalTime - timeInterval && currTime <= finalTime)
        {
            if (Mathf.Abs(currentAirspeed - targetAirspeed) > airspeedThreshold)
            {
                success = false;
            }
                
        }

        if(currTime > finalTime)
        {
            Debug.Log("Sucess = " + success);
        }
        /*
        float currTime = Time.time;
        currentVelocity = drone.VelocityLocal().z*0.001f + currentVelocity*0.999f;
        if(Mathf.Abs(currentVelocity) > velocityThreshold)
        {
            lastVelocityTime = currTime;
        }

        currentAirspeedRate = drone.AccelerationBody().x * 0.001f + currentAirspeedRate * 0.999f;
        if (Mathf.Abs(currentAirspeedRate) > airspeedRateThreshold)
        {
            lastAirspeedRateTime = currTime;
        }

        if(((currTime-lastVelocityTime) > timeInterval) && ((currTime-lastAirspeedRateTime) > timeInterval))
        {
            Debug.Log("Scenario 0 Complete: Elevator = " + drone.MomentBody().y + " Pitch: " + drone.AttitudeEuler().y + " Airspeed: " + drone.VelocityLocal().magnitude);
        }
        */
    }

}