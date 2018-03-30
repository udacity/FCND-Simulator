using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Formation : MonoBehaviour {

	public AiFlightPlan leaderFlightplan;
	public AutoPilotActionsManager leader;
	public Transform[] formationPositions;
	public GameObject formationAction;
	public List<AutoPilotActionsManager> wingmans;



	public void ToggleWingmans (bool toggle){

		foreach (AutoPilotActionsManager manager in wingmans) {

			if (manager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.FlyRoute || manager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.FlyFormation) {
				manager.flyFormation = toggle;
				if (toggle) {
					manager.flyFormation = true;
					//manager.flightPlanFeed.actions [1].action.routines [0].targetSpd = leader.autopilot.aircraftSpeed;
					manager.waypointIndex = leader.waypointIndex;
				}
				else{
					//if (manager.flightPlanFeed.allowFormation) {
						if (manager.flightPlanFeed.currentFlightState != AiFlightPlan.FlightStates.TakeOff && manager.flightPlanFeed.currentFlightState != AiFlightPlan.FlightStates.StandBy) {
							manager.flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.FlyRoute;
							manager.flyFormation = false;
							manager.autopilot.finishRoute = false;	
							//manager.waypointIndex = manager.flightPlanFeed.waypoints.Count - 1;
						}
					//}
					//manager.waypointIndex = manager.flightPlanFeed.waypoints.Count - 1;
				}
			}
		}

	}

	public void TakeOffFormation () {

	
	
		StartCoroutine (TakeOffFormationCoroutine ());

	}

	IEnumerator TakeOffFormationCoroutine () {

		yield return new WaitForSeconds (5);
	
		foreach (AutoPilotActionsManager manager in wingmans) {
			yield return new WaitForSeconds (5);
			manager.flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.TakeOff;
			manager.autopilot.inputs.SetflapsDown (true);
		
		}

	}

	public void SetFormationLeader (AutoPilotActionsManager setmanager){

		foreach (AutoPilotActionsManager manager in wingmans) {

			manager.flightPlanFeed.formationLeader = setmanager;
		}
	}

	public void LateUpdate () {

		if (leaderFlightplan == null)
			leaderFlightplan = leader.flightPlanFeed;
		if (leaderFlightplan.currentFlightState == AiFlightPlan.FlightStates.FlyRoute && leader.flightPlanFeed.allowFormation)
			ToggleWingmans (true);
		

	}

}
