using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightManager : MonoBehaviour {

	public bool setPositionOnStart;
	public Vector3 worldStartPosition;
	public bool autoStart;
	public bool taxiToRwy;
	public bool startFlying;
	public float waitTime;
	public AiFlightPlan initialFlightPlan;
	public GameObject flightAircraft;
	public int flightSize;
	public string originAirfieldId;
	public Transform originAirfield;
	public Transform parkingSpot;
	public List<Transform> waypoints;
	AutoPilotActionsManager manager ;
	void Awake () {
		
		initialFlightPlan.waypoints = waypoints;
		manager = flightAircraft.GetComponent<AutoPilotActionsManager> ();
		initialFlightPlan.formationLeader = manager;
		manager.flightPlanFeed = initialFlightPlan;
		manager.flightManager = this;
		if(setPositionOnStart)
		transform.position = worldStartPosition;
	}

	// Use this for initialization
	IEnumerator Start () {


		
		if (flightAircraft.GetComponentInChildren<Formation> () != null) {
			if (initialFlightPlan.formationLeader.gameObject == flightAircraft) {
				flightAircraft.GetComponentInChildren<Formation> ().leaderFlightplan = initialFlightPlan;
				InstantiateFlightWingmen ();
			}
		}


		yield return ReadyEngine ();

		yield return EngineStarter ();

		yield return new WaitForSeconds (waitTime);

		if (autoStart)
			StartFlight ();
		
	}

	IEnumerator ReadyEngine () {


		do {
			yield return new WaitForEndOfFrame ();
		} while (manager.autopilot.engines == null);

		foreach (Engine engine in manager.autopilot.engines.engines) {

			engine.contact = true;
			engine.fuelValveOpen = true;

		}

	}

	void DoReadyEngine (AutoPilotActionsManager tgtManager){

		StartCoroutine (ReadyEngine (tgtManager));

	}

	IEnumerator ReadyEngine (AutoPilotActionsManager tgtmanager) {


		do {
			yield return new WaitForEndOfFrame ();
		} while (tgtmanager.autopilot.engines == null);

		foreach (Engine engine in tgtmanager.autopilot.engines.engines) {

			engine.contact = true;
			engine.fuelValveOpen = true;

		}

	}

	IEnumerator EngineStarter (AutoPilotActionsManager manager) {

		manager.autopilot.inputs.engineOn = true;
		yield return new WaitForSeconds (15);
		manager.autopilot.inputs.engineOn = false;
	}

	IEnumerator EngineStarter () {

		manager.autopilot.inputs.engineOn = true;
		yield return new WaitForSeconds (15);
		manager.autopilot.inputs.engineOn = false;
	}

	IEnumerator StartEngine () {

		float seconds = 5f;
		float t = 0f;
		do {
			manager.autopilot.inputs.engineOn = true;
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		} while(t < 60f * seconds);				

		manager.autopilot.inputs.engineOn = false;
	}

	void InstantiateFlightWingmen () {

		if (flightAircraft.GetComponentInChildren<Formation> () != null){
			
			for (int i = 0; i < flightSize; i++) {
				Vector3 posVector = (flightAircraft.transform.forward * 500f);
				if (!startFlying)
					posVector = Vector3.zero;
				Vector3 instPos = flightAircraft.GetComponentInChildren<Formation>().formationPositions[i].position - posVector;
				instPos.y = flightAircraft.transform.position.y;
				GameObject newAircraft = (GameObject)Instantiate (flightAircraft.gameObject, instPos, flightAircraft.transform.rotation) as GameObject;
				foreach (Transform pos in newAircraft.GetComponentInChildren<Formation> ().formationPositions) {
					
					Destroy (pos.gameObject);

				}
				AutoPilotActionsManager newManager = newAircraft.GetComponent<AutoPilotActionsManager> ();
				/*do {
					yield return new WaitForEndOfFrame ();
				} while (newManager.autopilot.engines == null);*/
				newManager.formationFlightAction = newAircraft.GetComponentInChildren<Formation> ().formationAction.GetComponent<AutoPilotAction> ();
				newManager.autopilot.currentAction = newManager.formationFlightAction;
				Destroy (newAircraft.GetComponentInChildren<Formation> ());
				newManager.flightPlanFeed = initialFlightPlan;
				newManager.formationPosition = flightAircraft.GetComponentInChildren<Formation> ().formationPositions [i];
				flightAircraft.GetComponentInChildren<Formation> ().wingmans.Add (newAircraft.GetComponent<AutoPilotActionsManager> ());
				/*foreach (Engine engine in newManager.autopilot.engines.engines) {

					engine.fuelValveOpen = true;

				}*/
				DoReadyEngine (newManager);
				newManager.autopilot.inputs.engineOn = true;
				StartCoroutine (EngineStarter (newManager));
			}
		}

		flightAircraft.GetComponentInChildren<Formation> ().SetFormationLeader (flightAircraft.GetComponent<AutoPilotActionsManager> ());
	}
	AiFlightPlan originalFlightplan;
	void StartFlight () {

		Runway fromRwy = null;
		SpawnBaseData[] spawns = GameObject.FindObjectsOfType<SpawnBaseData> () as SpawnBaseData[];

		foreach (SpawnBaseData data in spawns){
			if (data.idName == originAirfieldId)
				originAirfield = data.transform;
			}
		if (originAirfield != null) {
			if (originAirfield.GetComponent<Runway> () != null)
				fromRwy = originAirfield.GetComponent<Runway> ();
		}
		AutoPilotActionsManager manager = flightAircraft.GetComponent<AutoPilotActionsManager> ();

		originalFlightplan = manager.flightPlanFeed;
		if (flightAircraft.GetComponent<UsimVehicle> ().vehicleType == UsimVehicle.VehicleTypes.Air) {
			if (taxiToRwy && !startFlying) {
		
				GameObject newFlightPlan = (GameObject)Instantiate (fromRwy.taxiToRunway.gameObject, transform) as GameObject;
				newFlightPlan.transform.parent = flightAircraft.transform;
				manager.flightPlanFeed = newFlightPlan.GetComponent<AiFlightPlan> ();
				manager.flightPlanFeed.manager = manager;
				manager.actions = newFlightPlan.GetComponent<AiFlightPlan> ().actions;
				manager.waypoints = newFlightPlan.GetComponent<AiFlightPlan> ().waypoints;
				manager.waypointIndex = 0;
				manager.index = 0;
				manager.autopilot.currentAction = manager.actions [0];
				manager.flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.Taxi;

			} else {

				StartCoroutine (TakeOffFlight ());
			}
		} else {

			manager.flightPlanFeed = originalFlightplan;
			manager.actions = originalFlightplan.actions;
			manager.waypoints = originalFlightplan.waypoints;
			manager.waypointIndex = 0;
			manager.index = 1;
			manager.flightPlanFeed.StartFlight(0);
		}
	
		if (setPositionOnStart)
			transform.position = worldStartPosition;
	}

	public IEnumerator HoldForTraffic () {

		do{
			if(originAirfield.GetComponent<Runway>().runwayState == Runway.RunwayStates.Traffic)
			flightAircraft.GetComponent<AutoPilotActionsManager> ().flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.StandBy;
			yield return new WaitForSeconds (5);
		}
		while (originAirfield.GetComponent<Runway>().runwayState == Runway.RunwayStates.Traffic);

		flightAircraft.GetComponent<AutoPilotActionsManager> ().flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.Taxi;
		flightAircraft.GetComponent<AutoPilotActionsManager> ().autopilot.ignoreOutsideTriggers = false;
	}

	public IEnumerator TakeOffFlight () {



		manager.flightPlanFeed = originalFlightplan;
		manager.actions = originalFlightplan.actions;
		manager.waypoints = originalFlightplan.waypoints;
		manager.waypointIndex = 0;
		manager.index = 1;
		manager.flightPlanFeed.StartFlight(1);
		yield return new WaitForEndOfFrame ();
		manager.autopilot.currentRoutine.targetHdg = originAirfield.transform.eulerAngles.y;
		if (flightAircraft.GetComponentInChildren<Formation> () != null)
			flightAircraft.GetComponentInChildren<Formation> ().TakeOffFormation ();
		manager.autopilot.ignoreOutsideTriggers = false;
		manager.autopilot.inputs.engineOn = false;
		manager.autopilot.inputs.SetflapsDown (true);
	}

	public IEnumerator Park () {

		yield return new WaitForEndOfFrame ();
		manager.flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.Park;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AddWaypoint (int atIndex) {

		GameObject newWp = (GameObject)Instantiate (waypoints [atIndex].gameObject) as GameObject;
		newWp.transform.parent = transform;
		newWp.transform.position = waypoints [atIndex].position + (Vector3.forward * 1000f);
		if(waypoints [atIndex].GetComponent<WaypointTrigger>() != null){
			if(waypoints [atIndex].GetComponent<WaypointTrigger>().triggerMode == WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch){
				
				DestroyImmediate (waypoints [atIndex].GetComponent<WaypointTrigger>());

			}
		}

		newWp.gameObject.name = "WP" + (atIndex + 1).ToString ();
		initialFlightPlan.waypoints.Insert (atIndex+1, newWp.transform);
		waypoints = initialFlightPlan.waypoints;
		RenameWaypoints ();

	}


	public void RemoveWaypoint (int index) {

		Transform waypoint = waypoints [index];
		waypoints.Remove (waypoint);
		DestroyImmediate (waypoint.gameObject);
		RenameWaypoints ();

	}

	public void RenameWaypoints () {

		for (int i = 0; i < waypoints.Count; i++) {

			GameObject go = waypoints [i].gameObject;
			go.name = "WP" + i.ToString ();

		}
	}

	void OnDrawGizmos() {

	
		Gizmos.color = Color.yellow;
		if (!startFlying) {
			if (waypoints.Count > 0 && originAirfield != null) {
				Gizmos.DrawIcon (originAirfield.position, "Origin_icon.png", true);
				Gizmos.DrawLine (originAirfield.position, waypoints [0].position);
			}
		} else {
			if (waypoints.Count > 0) {
				Gizmos.DrawIcon (flightAircraft.transform.position, "Origin_icon.png", true);
				Gizmos.DrawLine (flightAircraft.transform.position, waypoints [0].position);
			}
		}

		Gizmos.color = Color.green;
		for (int i = 0; i < waypoints.Count; i++){
			Transform waypoint = waypoints [i];
				Gizmos.DrawIcon (waypoint.position, "WP_icon.png", true);

			int nextWaypoint = i + 1;
			if (nextWaypoint < waypoints.Count) {
				Gizmos.DrawLine (waypoint.position, waypoints [nextWaypoint].position);
			}
			else if (originAirfield != null && waypoints [waypoints.Count - 1].GetComponent<WaypointTrigger> () != null) {	
				Gizmos.color = Color.yellow;
				if( waypoints [waypoints.Count - 1].GetComponent<WaypointTrigger> ().triggerMode == WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch)
				Gizmos.DrawLine (waypoints [waypoints.Count - 1].position, waypoints [waypoints.Count - 1].GetComponent<WaypointTrigger> ().runwayTarget.transform.position);
			}
				
		}
	}
}
