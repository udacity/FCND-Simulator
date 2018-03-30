using UnityEngine;
using System.Collections;

public class WaypointTrigger : MonoBehaviour {
	
	public enum WaypointTriggerModes{GetRunwayAprouch, UseTriggerFlightPlan, RunwayHold, RunwayTakeoff, Park};
	public WaypointTriggerModes triggerMode;
	public float triggerDistance;
	public AiFlightPlan triggerFlightplan;
	public Runway runwayTarget;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public AiFlightPlan GetAprouch (){

		return runwayTarget.GetAprouchByIndex (0);

	}
}
