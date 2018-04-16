using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;

public class BackyardFlyer : MonoBehaviour
{
    private IDrone drone;
    private MAVLinkMessenger messenger;
    public NetworkController networkController;

    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;
    public int homePositionIntervalHz = 1;

    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        GameObject.Find("Quad Drone").GetComponent<QuadController>().NavigationUpdate();
        drone.SetHome(drone.Longitude(), drone.Latitude(), drone.Altitude());

        drone.ControlRemotely(false);
        messenger = new MAVLinkMessenger();

		networkController.AddMessageHandler ( MessageType.Mavlink, messenger.ParseMessageInfo );
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.GlobalPosition, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
    }
}