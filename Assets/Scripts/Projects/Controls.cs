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
        drone.SetHomePosition(drone.GPSLongitude(), drone.GPSLatitude(), drone.GPSAltitude());
        //drone.ControlRemotely(true);
        messenger = new MAVLinkMessenger();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Conversions.HertzToMilliSeconds(positionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(positionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.AttitudeQuaternion, Conversions.HertzToMilliSeconds(attitudeIntervalHz));

    }

}