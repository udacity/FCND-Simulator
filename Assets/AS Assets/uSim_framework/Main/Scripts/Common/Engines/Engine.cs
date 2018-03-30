using UnityEngine;
using System.Collections;

public class Engine : MonoBehaviour {


	public bool remoteKinematic;

	public AnimationCurve torqueCurve;
	public AnimationCurve frictionCurve;

//	[HideInInspector]
	public float addedTorque;
	public float baseTorque;
	float initialBaseTorque;

	public float minRpm;
	public float totalMass;
	
	public float idleThrottle;
	//[HideInInspector]
	public float throttle;
	public float thrustDir;
	//[HideInInspector]
	public float rpm;
	public float inertia;
	//[HideInInspector]
	public float outputForce;
	[HideInInspector]
	public bool contact;
	public float stallThreshold;
	[HideInInspector]
	public bool stall;

	public float torque;
	public float friction;
	public float addedFriction;
	[HideInInspector]
	public float angularAccel;
	[HideInInspector]
	public float deltaRotation;
	public float rpmLimit;

	public float flyWheelRadius = 1f;

	//fuel

	public FuelManager fuelManager;
	public int fuelIndex;
	public float maxFuelConsumption;
	float availableFuel;
	public bool fuelValveOpen = true;
	//clutch
	public bool useClutch;
	public float clutch;

	public float rpmFromAttach;
	public float brakeFromAttach;
	float freeRpm ;


	//[HideInInspector]
	public bool selected;

	// Use this for initialization
	void Start () {
		addedTorque = 0f;
		initialBaseTorque = baseTorque;
	}

	void Update () {

		throttle = Mathf.Clamp01 (throttle);

	}

	void FixedUpdate () {
	
		if (remoteKinematic)
			return;

		if(throttle < idleThrottle) throttle = idleThrottle;
		if(addedTorque < 0f) addedTorque = 0f;

		if(!contact){	

			torque = 0f;
			if(rpm >= 0f) rpm -= 300 * Time.deltaTime;
		}

		float fuelConsuptionUnit = maxFuelConsumption * throttle;
		float availableFuel = fuelManager.GetAvailableFuelAmount ();
		if (availableFuel >= fuelConsuptionUnit) 
			fuelManager.ReduceFuelQuantity (fuelConsuptionUnit);

		availableFuel += Mathf.Lerp (0f, 1f * Time.deltaTime, rpm / stallThreshold);
		//act as sort of fuel pressure
		availableFuel = Mathf.Clamp01(availableFuel);

		if (!fuelValveOpen)
			availableFuel = 0f;
		

		torque = (torqueCurve.Evaluate(rpm /1000f)  * 1000f) * availableFuel;
		friction = frictionCurve.Evaluate(rpm/1000f) * 1000f;

		angularAccel = ((((torque) * throttle) + addedTorque + (-friction - addedFriction - brakeFromAttach)) / totalMass) * 60f ;

		float deltaRpm = (angularAccel  * -Physics.gravity.y) * Time.deltaTime * inertia;
		freeRpm += deltaRpm;
		freeRpm = Mathf.Clamp (freeRpm, 0f, rpmLimit);


		if (useClutch) {		
			
			rpm = Mathf.Lerp (rpmFromAttach, freeRpm, clutch / 1f);
							
		} else {
			
			rpm = freeRpm;
		}
		if(rpm < 0f) rpm = 0f;
		if (brakeFromAttach <= 0f)
			brakeFromAttach = 0f;
		float output = ((((torque  + addedTorque) * throttle) - friction ) * flyWheelRadius );
		outputForce = output * Mathf.Lerp (1f,0f, clutch /1f);
		if(rpm < stallThreshold) stall = true;
		else if (contact) stall = false;

		rpm = Mathf.Clamp (rpm, 0f, rpmLimit);

	}
}
