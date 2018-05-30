using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;
using DroneVehicles;
using DroneSensors;

public class FixedWing : MonoBehaviour
{
    private IDrone drone;
    private MAVLinkMessenger messenger;
    public NetworkController networkController;

    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;

    public int gpsIntervalHz = 50;
    public int attitudeIntervalHz = 100;
    public int imuIntervalHz = 100;
    public int homePositionIntervalHz = 1;

    void Start()
    {
        //Debug.Log("Starting Fixed Wing");
        //GameObject test = GameObject.Find("Plane Drone");
        drone = GameObject.Find("Plane Drone").GetComponent<PlaneDrone>();
        //GameObject.Find("Quad Drone").GetComponent<QuadVehicle>().StateUpdate();
        //GameObject.Find("Quad Drone").GetComponent<QuadSensors>()
        drone.SetHomePosition();// drone.GPSLongitude(), drone.GPSLatitude(), drone.GPSAltitude());

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
        //StartScenario0();
    }
    
    public void StartScenario0()
    {
        //Debug.Log("Start Scenerio 0");

        //Initialize state
        //Vector3 startLocation = new Vector3(900.0f, 150.0f, -700.0f);
        
        //Vector3 startVelocity = new  Vector3(-41.0f*Mathf.Sqrt(2.0f)/2.0f, 0.0f, 41.0f*Mathf.Sqrt(2.0f) / 2.0f); //Vector3(0.0f, 0.0f, 40.0f);//
        
        //Vector3 startEuler = new Vector3(-1.3f, -45.0f, 0.0f);
        

    }
    /*
    public void SuccessScenario0()
    {
        //To be run in the fixed update function
        //Check some success criterion
    }
    */

}