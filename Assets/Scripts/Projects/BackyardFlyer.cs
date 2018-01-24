using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;

public class BackyardFlyer : MonoBehaviour
{
    private IDrone drone;
    private MAVLinkMessenger messenger;
    public NetworkController networkController;

    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;
    public int homePositionIntervalHz = 1;


    // Use this for initialization
    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        Debug.Log(drone.HomeLongitude() + " " + drone.HomeLatitude());
        Debug.Log(drone.Longitude() + " " + drone.Latitude());
        drone.SetHome(drone.Longitude(), drone.Latitude(), drone.Altitude());
        drone.ControlRemotely(false);
        messenger = new MAVLinkMessenger();
        Debug.Log(drone.HomeLongitude() + " " + drone.HomeLatitude());
        Debug.Log(drone.Longitude() + " " + drone.Latitude());

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
    }
}