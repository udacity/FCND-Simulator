using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour {

	public Transform[] shipRudders;
	public float maxRudderAngle;
	InputsManager inputs;
	public float steeringInput;
	EnginesManager engines;
	public float throttleInput;
	public bool useVC;

	// Use this for initialization
	void Start () {
	
		inputs = GetComponent<InputsManager> ();
		engines = GetComponentInChildren<EnginesManager> ();

	}
	
	// Update is called once per frame
	void Update () {	

		if (!useVC) {
			steeringInput = inputs.shipRudder;	
			throttleInput = inputs.throttle;
		} else {

			inputs.SetThrottle (throttleInput);
			inputs.shipRudder = steeringInput;
		}
		for (int i = 0; i < shipRudders.Length; i++) {

			Vector3 angles = shipRudders[i].localEulerAngles;
			angles.y = -maxRudderAngle * steeringInput;
				shipRudders[i].localEulerAngles  =  angles;

		}
	}

}
