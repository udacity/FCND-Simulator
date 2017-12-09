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
        drone.ControlRemotely(false);
        messenger = new MAVLinkMessenger();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Utils.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Utils.HertzToMilliSeconds(homePositionIntervalHz));
    }
}