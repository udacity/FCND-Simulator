using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;
using DroneVehicles;
using DroneSensors;

public class Estimation : MonoBehaviour
{
    private IDrone drone;
    private MAVLinkMessenger messenger;
    public NetworkController networkController;

    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;

    public int gpsIntervalHz = 10;
    public int attitudeIntervalHz = 50;
    public int imuIntervalHz = 100;
    public int homePositionIntervalHz = 1;

    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
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
    }
}