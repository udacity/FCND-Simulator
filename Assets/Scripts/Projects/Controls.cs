using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;

public class Controls : MonoBehaviour
{
    private MAVLinkMessenger messenger;
    private IDrone drone;
    public NetworkController networkController;
    public int heartbeatIntervalHz = 1;
    public int positionIntervalHz = 100;
    public int attitudeIntervalHz = 500;
    public int homePositionIntervalHz = 1;


    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        GameObject.Find("Quad Drone").GetComponent<QuadController>().NavigationUpdate();
        drone.SetHome(drone.Longitude(), drone.Latitude(), drone.Altitude());
        drone.ControlRemotely(true);
        messenger = new MAVLinkMessenger();

		networkController.AddMessageHandler ( MessageType.Mavlink, messenger.ParseMessageInfo );
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.GlobalPosition, Conversions.HertzToMilliSeconds(positionIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(positionIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
		networkController.EnqueueRecurringMessage(MessageType.Mavlink, messenger.AttitudeQuaternion, Conversions.HertzToMilliSeconds(attitudeIntervalHz));

    }

}