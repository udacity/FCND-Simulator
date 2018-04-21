using UnityEngine;

using System;
using System.Linq;
using System.Collections.Generic;
// using System.IO;

using MavLink;
using FlightUtils;
using Drones;
using DroneInterface;
using UdacityNetworking;
using Messaging;
using DroneControllers;


public class MotionPlanning : MonoBehaviour
{
    private GameObject droneGO;
    private IDrone drone;
    private MAVLinkMessenger messenger;
    public NetworkController networkController;
    private string collidersFile = "colliders.csv";
    public int heartbeatIntervalHz = 1;
    public int telemetryIntervalHz = 4;
    public int sensorIntervalHz = 1;
    public int homePositionIntervalHz = 1;
    public float sensorRange = 50;
    private Dictionary<Quaternion, MAV_SENSOR_ORIENTATION> mavSensorLookup = new Dictionary<Quaternion, MAV_SENSOR_ORIENTATION>();

    void Start()
    {
        droneGO = GameObject.Find("Quad Drone");
        drone = droneGO.GetComponent<QuadDrone>();
        drone.ControlRemotely(false);
        messenger = new MAVLinkMessenger();

        // Quaternion.identity
        // var buildings = Resources.FindObjectsOfTypeAll<Mesh>();
        // Debug.Log(buildings.Length);
        // SetupLidarRays();

        networkController.AddMessageHandler(messenger.ParseMessageInfo);
        networkController.EnqueueRecurringMessage(messenger.GlobalPosition, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.LocalPositionNED, Conversions.HertzToMilliSeconds(telemetryIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.Heartbeat, Conversions.HertzToMilliSeconds(heartbeatIntervalHz));
        networkController.EnqueueRecurringMessage(messenger.HomePosition, Conversions.HertzToMilliSeconds(homePositionIntervalHz));
//        networkController.EnqueueRecurringMessage(SensorInfo, Conversions.HertzToMilliSeconds(sensorIntervalHz));

    }

    //
    // Sensor Misc
    //

    List<byte[]> SensorInfo()
    {
        // Send multiple messages for different orientations
        var msgs = new List<byte[]>();
        //        print("Sensing distances ...");
        var pos = drone.UnityCoords();
        var collisions = Sensors.Lidar.Sense(droneGO, mavSensorLookup.Keys.ToList(), sensorRange);

        for (int i = 0; i < collisions.Count; i++)
        {
            var c = collisions[i];
            //print(string.Format("ray hit - drone loc {0}, rotation {1}, distance (meters) {2}, collision loc {3}", c.origin, c.rotation.eulerAngles, c.distance, c.target));
            var mo = mavSensorLookup[c.rotation];
            var dist = c.distance;
            var msg = new Msg_distance_sensor
            {
                // A unity unit is 1m and the distance unit
                // required by this message is centimeters,
                // hence the 100x multiplication.
                min_distance = 0,
                max_distance = (UInt16)(sensorRange * 100),
                current_distance = (UInt16)(dist * 100),
                type = (byte)MAV_DISTANCE_SENSOR.MAV_DISTANCE_SENSOR_LASER,
                id = 0,
                orientation = (byte)mo,
                // TODO: add variance model, likely using FastNoise
                covariance = 0,
            };
            var serializedPacket = messenger.mav.SendV2(msg);
            msgs.Add(serializedPacket);
        }
        return msgs;
    }

	void CollidersToCSV(bool regenerate = false)
    {
        var go = GameObject.Find("ColliderGatherer");
        if (go == null)
        {
            Debug.Log("ColliderGatherer GameObject not found in scene ...");
            return;
        }

		var raycastColliderGen = go.GetComponent<RaycastGenerateColliders> ();
		raycastColliderGen.GenerateColliders ( OnCollidersGenerated );
//        var collidersGenerator = go.GetComponent<GenerateColliderList>();
//		if ( regenerate )
//			collidersGenerator.GenerateColliders ();
//		var colliders = collidersGenerator.colliders;

        SimpleFileBrowser.ShowSaveDialog(CreateFile, null, true, null, "Select Folder", "Save");
    }

	void OnCollidersGenerated ()
	{
		// SimpleFileBrowser.ShowSaveDialog(CreateFile, null, true, null, "Select Folder", "Save");
	}

	System.Text.StringBuilder GetCollidersString ()
	{
		var colliders = GameObject.Find("ColliderGatherer").GetComponent<RaycastGenerateColliders>().colliders;
		var header = "posX,posY,posZ,halfSizeX,halfSizeY,halfSizeZ\n";
		string latString = Simulation.latitude0.ToString ();
		string lonString = Simulation.longitude0.ToString ();
		System.Text.StringBuilder sb = new System.Text.StringBuilder ();

		int length = latString.Split ( '.' ) [ 1 ].Length;
		for ( int i = 0; i < ( 6 - length ); i++ )
			latString += "0";

		length = lonString.Split ( '.' ) [ 1 ].Length;
		for ( int i = 0; i < ( 6 - length ); i++ )
			lonString += "0";
		sb.Append ( "lat0 " + latString + ", lon0 " + lonString + "\n" );
		sb.Append ( header );

		foreach (var c in colliders)
		{
			var pos = c.position;
			var hsize = c.halfSize;
			var row = string.Format("{0},{1},{2},{3},{4},{5}\n", pos.z, pos.x, pos.y, hsize.z, hsize.x, hsize.y);
			sb.Append ( row );
		}

		return sb;
	}

    void CreateFile(string path)
    {
		System.Text.StringBuilder sb = GetCollidersString ();

		var filepath = System.IO.Path.Combine(path, collidersFile);
        Debug.Log(string.Format("Writing colliders to {0} ...", filepath));
		if (System.IO.File.Exists(filepath))
        {
            Debug.Log("Overwriting previous file");
        }


		System.IO.File.Create(filepath).Close();
		System.IO.File.AppendAllText ( filepath, sb.ToString () );
    }

    void SetupLidarRays()
    {

        // x, y, z -> pitch, yaw, roll - https://en.wikibooks.org/wiki/Cg_Programming/Unity/Rotations
        // EUN frame
        mavSensorLookup[Quaternion.Euler(new Vector3(0, 0, 0))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_NONE;
        mavSensorLookup[Quaternion.Euler(new Vector3(0, 90, 0))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_90;
        mavSensorLookup[Quaternion.Euler(new Vector3(0, 180, 0))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_180;
        mavSensorLookup[Quaternion.Euler(new Vector3(0, 270, 0))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_270;
        mavSensorLookup[Quaternion.Euler(new Vector3(90, 0, 0))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_90;
        mavSensorLookup[Quaternion.Euler(new Vector3(270, 0, 0))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_270;
        mavSensorLookup[Quaternion.Euler(new Vector3(0, 0, 90))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90;
        mavSensorLookup[Quaternion.Euler(new Vector3(0, 0, 270))] = MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270;

        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 45, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_45;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 135, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_135;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 225, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_225;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 315, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_YAW_315;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 0, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 45, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_45;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 90, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 135, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_135;
        // mavSensorLookup[Quaternion.Euler(new Vector3(180, 0, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_180;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 225, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_225;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 270, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_270;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 315, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_YAW_315;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 45, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_45;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 90, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 135, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_135;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 45, 270))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_YAW_45;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 90, 270))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_YAW_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 135, 270))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_YAW_135;
        // mavSensorLookup[Quaternion.Euler(new Vector3(180, 90, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_180_YAW_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(180, 270, 0))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_PITCH_180_YAW_270;
        // mavSensorLookup[Quaternion.Euler(new Vector3(90, 0, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(90, 0, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_PITCH_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(90, 0, 270))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_PITCH_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(180, 0, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_180;
        // mavSensorLookup[Quaternion.Euler(new Vector3(180, 0, 270))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_PITCH_180;
        // mavSensorLookup[Quaternion.Euler(new Vector3(270, 0, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_270;
        // mavSensorLookup[Quaternion.Euler(new Vector3(270, 0, 180))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_180_PITCH_270;
        // mavSensorLookup[Quaternion.Euler(new Vector3(270, 0, 270))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_270_PITCH_270;
        // mavSensorLookup[Quaternion.Euler(new Vector3(180, 90, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_PITCH_180_YAW_90;
        // mavSensorLookup[Quaternion.Euler(new Vector3(0, 270, 90))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_90_YAW_270;
        // mavSensorLookup[Quaternion.Euler(new Vector3(315, 315, 315))] =  MAV_SENSOR_ORIENTATION.MAV_SENSOR_ROTATION_ROLL_315_PITCH_315_YAW_315;


    }

    void LateUpdate()
    {
        // Save colliders file
        if (Input.GetButton("Shift Modifier") && Input.GetButtonDown("Save"))
        {
			CollidersToCSV(true);
        }
    }
}
