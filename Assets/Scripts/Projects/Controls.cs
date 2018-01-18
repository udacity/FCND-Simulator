using UnityEngine;

using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;

public class Controls : MonoBehaviour
{
    private MAVLinkMessenger messenger;
    private IDrone drone;
    public NetworkController networkController;
    public int heartbeatIntervalHz = 1;
    public int positionIntervalHz = 100;
    public int attitudeIntervalHz = 500;
    public int homePositionIntervalHz = 1;

    public SimParameter exampleParameter1;
    //	public SimParameter exampleParameter2;
    public SimParameter exampleParameter3;
    public SimParameter altitudeParameter;


    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        drone.ControlRemotely(true);
        messenger = new MAVLinkMessenger();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Conversions.HertzToMilliSeconds(positionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(positionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.AttitudeQuaternion, Conversions.HertzToMilliSeconds(attitudeIntervalHz));

        /// Example of observing parameter changes
        exampleParameter1.Observe(OnParameterChanged);
    }

    void OnParameterChanged(SimParameter p)
    {
        Debug.Log("Parameter changed: " + p.displayName + "! New value: " + p.Value);
    }
}