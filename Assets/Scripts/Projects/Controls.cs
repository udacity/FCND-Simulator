using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

// usings needed for TCP/IP
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using MavLink;
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
    public int telemetryIntervalHz = 20;
    public int homePositionIntervalHz = 1;

    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        drone.ControlRemotely(true);
        messenger = new MAVLinkMessenger();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Utils.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Utils.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Utils.HertzToMilliSeconds(homePositionIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.AttitudeTarget, Utils.HertzToMilliSeconds(telemetryIntervalHz));
    }
}