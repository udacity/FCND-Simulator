using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiRadar : MonoBehaviour {

	public int ownerId;
	public float radarRange;
	public bool enableIff;
	public List<AiRadar> contacts;
	public AutoPilotActionsManager autopilotManager;

	//ACM
	public GameObject target;
	public AiFlightPlan acmFlightplan;

	// Use this for initialization
	void Start () {

		autopilotManager = GetComponent<AutoPilotActionsManager> ();

	}
	
	// Update is called once per frame
	void LateUpdate () {

		//if (!gettingObjs)
			GetObjsVisualRange ();

		if (enableIff)
			CheckIff ();

	}

	bool gettingObjs;
	void GetObjsVisualRange () {

		gettingObjs = true;

		contacts = new List<AiRadar> ();
		AiRadar[] radarsInRange = FindObjectsOfType<AiRadar> () as AiRadar[];

		int i = 0;

		do {
			
			if (Vector3.Distance (transform.position, radarsInRange [i].transform.position) < radarRange) {

				AiRadar ufo = radarsInRange [i].gameObject.GetComponent<AiRadar>();
				if (!contacts.Contains (ufo))
					contacts.Add (ufo);

			}

			i++;
			//yield return new WaitForEndOfFrame ();

		} while (i < radarsInRange.Length);


		gettingObjs = false;
	}

	bool checkingIff;
	void CheckIff () {

		checkingIff = true;

		int i = 0;

		do {

			if (contacts[i].ownerId != ownerId && target == null) {

				if(autopilotManager.GetComponent<AirCombatManouver>() != null){
					AirCombatManouver acm = autopilotManager.GetComponent<AirCombatManouver>();

					if(acm.enabled){
						
						target = contacts[i].gameObject;					
						autopilotManager.flightPlanFeed = acmFlightplan;
						autopilotManager.actions = acmFlightplan.actions;
						autopilotManager.autopilot.currentAction = autopilotManager.actions[0];
						autopilotManager.autopilot.routineIndex = 0;
						autopilotManager.currentWp = target.transform;
						autopilotManager.autopilot.currentAction.action.routines[0].targetWp = target.transform;
						autopilotManager.flyFormation = false;
						if(autopilotManager.flightPlanFeed.formationLeader != null)
						autopilotManager.flightPlanFeed.formationLeader.GetComponentInChildren<Formation>().ToggleWingmans(false);

			
					acm.enableACM = true;
					}
				}
			}

			i++;
			//yield return new WaitForEndOfFrame ();

		} while (i < contacts.Count);

		checkingIff = false;
	}

}
