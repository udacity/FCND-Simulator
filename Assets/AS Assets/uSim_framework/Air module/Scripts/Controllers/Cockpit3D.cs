using UnityEngine;
using System.Collections;

public class Cockpit3D : MonoBehaviour {

	public enum StickType {Stick, Yoke, DriveWheel, ShipRudder};
	public enum ThrottleType {PivotLever, ControlRod};

	public Transform flightController;
	public StickType controllerType;
	public InputsManager inputsManager;
	public float aileronInput;
	public float steeringInput;
	public float aileronMax;
	public bool useElevator;
	public float elevatorInput;
	public float elevatorMax;
	public bool useThrottle;
	public Transform throttleController;
	public ThrottleType throttlecontrollerType;
	public float throttleInput;
	public float throttleMin;
	public float throttleMax;
	Vector3 initialThrottlePos;
	public ShipController shipController;

	// Use this for initialization
	void Start () {

		if (useThrottle && throttlecontrollerType == ThrottleType.ControlRod) {

			initialThrottlePos = throttleController.localPosition;

		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		aileronInput = inputsManager.aileron;
		if (controllerType != StickType.ShipRudder)
			steeringInput = inputsManager.steering;
		else
			steeringInput = shipController.steeringInput;
		elevatorInput = inputsManager.elevator;

		float hInput = aileronInput;

		throttleInput = inputsManager.enginesManager.throttle;
		float tInput = throttleInput;

		if (controllerType == StickType.DriveWheel)
			hInput = steeringInput;
		if (controllerType == StickType.ShipRudder)
			hInput = -steeringInput;
		Vector3 eulers = flightController.localEulerAngles;

		if(useElevator)
		eulers.x = Mathf.Lerp ( elevatorMax , -elevatorMax ,(1 + inputsManager.elevator)/2);
		
		eulers.z = Mathf.Lerp ( aileronMax , -aileronMax ,(1 + hInput)/2);
		flightController.localEulerAngles = eulers;

		if (useThrottle) {

			if (throttlecontrollerType == ThrottleType.PivotLever) {
				Vector3 throttleEulers = throttleController.localEulerAngles;
				throttleEulers.x = Mathf.Lerp ( throttleMin , throttleMax , tInput);
				throttleController.localEulerAngles = throttleEulers;
			}

			if (throttlecontrollerType == ThrottleType.ControlRod) {
				Vector3 thottlePos = throttleController.localPosition;
				thottlePos.z = Mathf.Lerp ( throttleMin , throttleMax , tInput);
				throttleController.localPosition = thottlePos;
			}

		}

	}
}
