using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoPilotActionsManager : MonoBehaviour {

	public FlightManager flightManager;
	public AiFlightPlan flightPlanFeed;
	public FpStartButton startButton;
	public AutoPilotSystem autopilot;
	public AutoPilotAction[] actions;
	public List<Transform> waypoints;
	public List<Transform> auxWaypoints;
	public Transform currentWp;
	public Runway currentRunway;
	public int waypointIndex;
	public int index;
	public bool flyFormation;
	public Transform formationPosition;
	public AutoPilotAction formationFlightAction;
	// Use this for initialization
	IEnumerator Start () {

		yield return new WaitForSeconds (1);

		auxWaypoints = new List<Transform> ();
		if (flightPlanFeed != null) {

			GameObject newFlightPlan = (GameObject) Instantiate (flightPlanFeed.gameObject,transform) as GameObject;
			flightPlanFeed = newFlightPlan.GetComponent<AiFlightPlan> ();
			flightPlanFeed.manager = this;
			actions = flightPlanFeed.actions;
			waypoints = flightPlanFeed.waypoints;
			waypointIndex = 0;
			if(startButton != null)
			startButton.flightPlan = flightPlanFeed;
			
			if (GetComponentInChildren<Formation> () != null) {
				GetComponentInChildren<Formation> ().leaderFlightplan = flightPlanFeed;
				//flightPlanFeed.formationLeader = this;
				//GetComponent<Formation> ().SetFormationLeader (this);
			}
		}
		if (formationFlightAction != null) {
			GameObject newActionGo = (GameObject)Instantiate (formationFlightAction.gameObject, flightPlanFeed.transform) as GameObject;
			AutoPilotAction newAction = newActionGo.GetComponent<AutoPilotAction> ();
			formationFlightAction = newAction;
		}
	}


	// Update is called once per frame
	void LateUpdate () {
		if (flightPlanFeed.manager == null)
			flightPlanFeed.manager = this;
		//if(actions.Length > 0 && !flyFormation)
		//autopilot.currentAction = actions [index];
		if (waypoints.Count > 0 && !flyFormation && flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.FlyRoute) {
			if(waypointIndex < waypoints.Count)
				currentWp = waypoints [waypointIndex];
			else
				currentWp = waypoints [0];
		}

		if (flyFormation && autopilot.currentAction != null) {

			currentWp = formationPosition;
			autopilot.currentAction = formationFlightAction;
			autopilot.currentRoutine = autopilot.currentAction.action.routines [0];		
			flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.FlyFormation;
		//	flightPlanFeed.actions [0].action.routines [0].targetSpd = flightPlanFeed.formationLeader.autopilot.aircraftSpeed;
			waypointIndex = flightPlanFeed.formationLeader.waypointIndex;
			//index = flightPlanFeed.formationLeader.index;
		} 
	}

	public int GetActionIdByName (string name) {

		for ( int i = 0; i < actions.Length;i++) {
			AutoPilotAction action = actions [i];
			if (action.action.actionName == name)
				return i;
		}
		return 0;
	}
}
