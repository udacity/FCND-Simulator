using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCombatManouver : MonoBehaviour {

	public bool enableACM;
	AutoPilotActionsManager autopilotManager;
	AiRadar aiRadar;
	public float localPosX;
	// Use this for initialization
	void Start () {

		autopilotManager = GetComponent<AutoPilotActionsManager> ();
		aiRadar = GetComponent<AiRadar> ();
		time = 25f;
	}
	float time;
	// Update is called once per frame
	void FixedUpdate () {

		if (!enableACM)
			return;

		if (aiRadar.target == null)
			return;

		float distToTarget = Vector3.Distance (transform.position, aiRadar.target.transform.position);
		/*float targetPitch = ElevationToTarget ();
		targetPitch = Mathf.Clamp (targetPitch, -25f, autopilotManager.autopilot.optimusAoa * (autopilotManager.autopilot.climbSpeed / autopilotManager.autopilot.aircraftSpeed));
		float targetBank = BankToTarget ();*/

		//autopilotManager.autopilot.currentRoutine.targetPitch = targetPitch;
		Vector3 localPos = transform.InverseTransformPoint (aiRadar.target.transform.position);
		//if (localPos.z > 0) {
	//	if (time <= 0f) {
	//		time = 0f;
				autopilotManager.autopilot.currentRoutine.verticalFlightMode = AutoPilotSystem.VerticalFlightModes.WaypointLevel;
				autopilotManager.autopilot.currentRoutine.horizontalFlightMode = AutoPilotSystem.HorizontalFlightModes.AlignToWp;
				
	//		}
	//	}
		/*if(localPos.z < 0 && distToTarget < 100f && time > 0){
							
			autopilotManager.autopilot.currentRoutine.verticalFlightMode = AutoPilotSystem.VerticalFlightModes.Loop;
			autopilotManager.autopilot.currentRoutine.horizontalFlightMode = AutoPilotSystem.HorizontalFlightModes.LevelWings;
			time -= Time.deltaTime;
		}*/

		float minAngle =  Mathf.Lerp (20f, Random.Range (25f, 75f), 500f / transform.position.y);
		float maxAngle =  Mathf.Lerp (45f, Random.Range (89f, 179f), 500f / transform.position.y);
		autopilotManager.autopilot.normalBankAngle = Mathf.Lerp (minAngle, maxAngle, 500f /distToTarget);
		autopilotManager.autopilot.inputs.trim = Mathf.Lerp (0f, 0.35f, 150f / distToTarget);


	}

	float BankToTarget (){

		Vector3 localPos = transform.InverseTransformPoint (aiRadar.target.transform.position);
		localPos.z = 0f;
		Vector3 vectorToTgt = localPos - transform.position;	

		return Vector3.Angle (transform.up, vectorToTgt) * (Mathf.Clamp (localPos.x, -1f, 1f));

	}

	float ElevationToTarget (){

		Vector3 localPos = transform.InverseTransformPoint (aiRadar.target.transform.position);
		localPos.z = 10f;
		localPos.x = 0f;
		Vector3 vectorToTgt =  aiRadar.target.transform.position - transform.position;

		return localPos.y * 0.07f;

	}

	float  SignedAngle(Vector3 v1,Vector3 v2,Vector3 normal) {
		var perp = Vector3.Cross(normal, v1);
		var angle = Vector3.Angle(v1, v2);
		angle *= Mathf.Sign(Vector3.Dot(perp, v2));
		return angle;
	}

}
