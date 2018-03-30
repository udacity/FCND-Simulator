using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Runway : MonoBehaviour {

	public enum RunwayStates {Clear, Traffic, Inoperative};
	public RunwayStates runwayState;
	public Transform ilsVectorPointer;
	public Transform runwayEnd;
	public float glideslopeAngle;
	public List<GameObject> OnWaitFlights;
	public GameObject onAprouchAircraft;
	public AiFlightPlan[] aprouchRoutes;
	public AiFlightPlan waitCircuit;
	public AiFlightPlan taxiToRunway;

	public AiFlightPlan GetAprouchByIndex(int index){

		return aprouchRoutes [index];

	}
}
