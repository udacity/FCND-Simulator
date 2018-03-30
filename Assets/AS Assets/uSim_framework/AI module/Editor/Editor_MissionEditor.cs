using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FlightManager))]
public class Editor_MissionEditor : Editor {

	FlightManager flightManager;
	public int currentWaypointIndex;
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		flightManager = (FlightManager)target;

		GUILayout.Label ("WAYPOINTS");


		for(int i = 0; i < flightManager.waypoints.Count; i++){

			GUILayout.BeginHorizontal ();

			Color defaultColor = GUI.color;


			if (i == currentWaypointIndex)
				GUI.color = Color.green;
				
		
			if(GUILayout.Button(">>"))
			{
				currentWaypointIndex = i;
			}
			GUILayout.Label ("Id: " + i.ToString ()); 
			GUILayout.Label ("Alt: "); 
			Vector3 pos = flightManager.waypoints [i].position;
			pos.y = EditorGUILayout.IntField ((int)flightManager.waypoints[i].position.y); 
			flightManager.waypoints [i].position = pos;
			if(GUILayout.Button("Up"))
			{
				int index = i;
				if (index > 0)
					index--;
				
				Transform movingWp = flightManager.waypoints [i];
				movingWp.gameObject.name = "WP" + index;					

				Transform cloneWp = flightManager.waypoints [index];
				cloneWp.gameObject.name = "WP" + i;

				if (movingWp.GetComponent<WaypointTrigger> () != null) {

					if (movingWp.GetComponent<WaypointTrigger> ().triggerMode == WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch) {
					
						if (cloneWp.gameObject.GetComponent<WaypointTrigger> () != null)
							DestroyImmediate (cloneWp.gameObject.GetComponent<WaypointTrigger> ());

						WaypointTrigger trigger = cloneWp.gameObject.AddComponent<WaypointTrigger> ();
						trigger.triggerDistance = 1500f;
						trigger.runwayTarget = flightManager.originAirfield.GetComponent<Runway> ();
						trigger.triggerFlightplan = trigger.runwayTarget.aprouchRoutes [0];
						trigger.triggerMode = WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch;
						DestroyImmediate (movingWp.GetComponent<WaypointTrigger>());
					}
				}

				flightManager.waypoints [index] = movingWp;
				flightManager.waypoints [i] = cloneWp;

				currentWaypointIndex = index;
				
			}
			if(GUILayout.Button("Down"))
			{
				int index = i;
				if (index < flightManager.waypoints.Count)
					index++;

				Transform movingWp = flightManager.waypoints [i];
				movingWp.gameObject.name = "WP" + index;					

				Transform cloneWp = flightManager.waypoints [index];
				cloneWp.gameObject.name = "WP" + i;

				if (movingWp.GetComponent<WaypointTrigger> () != null) {

					if (movingWp.GetComponent<WaypointTrigger> ().triggerMode == WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch) {

						if (cloneWp.gameObject.GetComponent<WaypointTrigger> () != null)
							DestroyImmediate (cloneWp.gameObject.GetComponent<WaypointTrigger> ());

						WaypointTrigger trigger = cloneWp.gameObject.AddComponent<WaypointTrigger> ();
						trigger.triggerDistance = 1500f;
						trigger.runwayTarget = flightManager.originAirfield.GetComponent<Runway> ();
						trigger.triggerFlightplan = trigger.runwayTarget.aprouchRoutes [0];
						trigger.triggerMode = WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch;
						DestroyImmediate (movingWp.GetComponent<WaypointTrigger>());
					}
				}

				flightManager.waypoints [index] = movingWp;
				flightManager.waypoints [i] = cloneWp;

				currentWaypointIndex = index;
			}

			GUILayout.EndHorizontal ();
				
			GUI.color = defaultColor;

			}

		GUILayout.BeginHorizontal ();

		if(GUILayout.Button("Remove Waypoint"))
		{
			flightManager.RemoveWaypoint (currentWaypointIndex);
		}
		if(GUILayout.Button("Add Waypoint"))
		{
			if (currentWaypointIndex < flightManager.waypoints.Count) {
				flightManager.AddWaypoint (currentWaypointIndex);
				currentWaypointIndex++;
			}
		}

		GUILayout.EndHorizontal ();
	}

	void OnSceneGUI ()
	{
		if (flightManager != null) {
			for (int i = 0; i < flightManager.waypoints.Count; i++) {
				Transform waypoint = flightManager.waypoints [i];

				Handles.Label (waypoint.position, i.ToString ());
			}
		}
	}
}
