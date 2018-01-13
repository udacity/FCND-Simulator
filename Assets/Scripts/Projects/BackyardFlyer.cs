// Comment this one to remove changing physics step at runtime
#define PHYSICS_TEST
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

	public SimParameter exampleParameter1;
//	public SimParameter exampleParameter2;
	public SimParameter exampleParameter3;
	public SimParameter altitudeParameter;


    // Use this for initialization
    void Start()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        drone.ControlRemotely(false);
        messenger = new MAVLinkMessenger();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
    }

	#if PHYSICS_TEST
	void LateUpdate ()
	{
		if ( Input.GetKeyDown ( KeyCode.Alpha1 ) )
			Time.fixedDeltaTime = 0.02f;
		if ( Input.GetKeyDown ( KeyCode.Alpha2 ) )
			Time.fixedDeltaTime = 0.01f;
		if ( Input.GetKeyDown ( KeyCode.Alpha3 ) )
			Time.fixedDeltaTime = 0.005f;
		if ( Input.GetKeyDown ( KeyCode.Alpha4 ) )
			Time.fixedDeltaTime = 0.002f;
		if ( Input.GetKeyDown ( KeyCode.Alpha5 ) )
			Time.fixedDeltaTime = 0.001f;
	}
	#endif
}