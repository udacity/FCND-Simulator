using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiFlightPlan : MonoBehaviour {

	//states
	public enum FlightStates {StandBy, Park, Taxi, TakeOff, FlyRoute, FlyFormation, Aprouch, Land, Other};


	public AutoPilotAction[] actions;
	public List<Transform> waypoints;
	public Runway fromRunway;
	public Runway toRunway;
	public FlightStates currentFlightState;
	public AutoPilotActionsManager manager;
	public AutoPilotActionsManager formationLeader;
	public bool allowFormation;
	public AiFlightPlan formationFlightPlan;


	public void StartFlight (int index){

		if (index >= actions.Length)
			index = 0;
			
		currentFlightState = actions [index].action.flightState;

	}

	// Update is called once per frame
	void LateUpdate () {

		if (manager == null)
			return;

		if (manager.flightPlanFeed == this) {
			foreach (AutoPilotAction action in actions) {

				if (action.action.flightState == currentFlightState) {
					manager.index =	manager.GetActionIdByName (action.action.actionName);
					if(actions.Length > 0 && !manager.flyFormation )
						manager.autopilot.currentAction = actions [manager.index];
				}
			}
		}

	}


}
