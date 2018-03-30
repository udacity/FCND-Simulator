using UnityEngine;
using System.Collections;

public class FpStartButton : MonoBehaviour {

	public AiFlightPlan flightPlan;
	public int startActionIndex;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ButtonAction (int actionIndex){

		flightPlan.StartFlight (actionIndex);

	}
}
