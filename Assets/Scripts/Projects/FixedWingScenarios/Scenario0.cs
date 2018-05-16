using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;
using DroneVehicles;
using DroneSensors;

public class Scenario0 : MonoBehaviour
{
    private IDrone drone;
    public float currentVelocity = 0.0f;
    private float lastVelocityTime = 0.0f;
    private float velocityThreshold = 0.5f;

    public float currentAirspeedRate = 0.0f;
    private float lastAirspeedRateTime = 0.0f;
    private float airspeedRateThreshold = 0.1f;

    private float timeInterval = 5.0f;

    public float elevatorTrim = 0.0f;
    public float throttleTrim = 0.0f;
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
        StartScenario0();
    }

    public void StartScenario0()
    {
        Debug.Log("Start Scenerio 0");

        //Initialize state
        Vector3 startLocation = new Vector3(900.0f, 150.0f, -700.0f);
        Vector3 startVelocity = new  Vector3(-41.0f*Mathf.Sqrt(2.0f)/2.0f, 0.0f, 41.0f*Mathf.Sqrt(2.0f) / 2.0f); //Vector3(0.0f, 0.0f, 40.0f);//
        Vector3 startEuler = new Vector3(-1.3f, -45.0f, 0.0f);
        
        drone.InitializeVehicle(startLocation, startVelocity, startEuler);
        drone.SetControlMode(2); //Longitude mode
        drone.SetGuided(true);
        drone.CommandAttitude(new Vector3(0.0f, elevatorTrim, 0.0f), 0.7f);

    }

    private void FixedUpdate()
    {
        //elevatorTrim += Input.GetAxis("Trim")*0.001f;
        throttleTrim += Input.GetAxis("Trim") * 0.001f;
        drone.CommandAttitude(new Vector3(0.0f, elevatorTrim, 0.0f), 0.7f + throttleTrim);

        SuccessScenario0();
    }

    public void SuccessScenario0()
    {
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
    }

}