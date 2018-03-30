using UnityEngine;
using System.Collections;

public class ScreenGauge : MonoBehaviour {

	public enum gaugeMode {Airspeed, Altimeter, VerticalSpeed, Fuel, RPM, Horizont};
	public enum speedMode {Mph,Kmh,Knots};
	public Transform needle;
	public AnimationCurve needleAnimation;
	public Transform needle2;
	public float needle2Rate;
	public float inputValue;
	public Vector2 flightAttitude;
	public gaugeMode mode;
	public speedMode iasUnitsMode;
	public GameObject aircraft;
	public float angle;
	public float angle2;
	public float needleOffset;
	public float fullAngle;
	public float maxPitch = 45f;
	public FuelManager fuelManager;
	public bool useSelectorIndex;
	public int selectorIndex;
	public Engine engine;
	public int engineNumber;

	// Use this for initialization
	void Start () {

		if(aircraft != null){
			
			fuelManager = aircraft.GetComponentInChildren<FuelManager> ();
			if (mode == gaugeMode.RPM && engine == null) {
				engine = aircraft.GetComponentInChildren<EnginesManager> ().engines [engineNumber];
			}
		}
		else{

			Debug.LogError ("Panel aircraft not set!");

		}

	}
	
	// Update is called once per frame
	void Update () {

		switch (mode) {
			
		case gaugeMode.Airspeed :
			
			inputValue = GetSpeed();
			SetAnimtime ();

			break;
			
		case gaugeMode.Altimeter :
			
			inputValue = GetAlt();
			SetAnimtime ();

			break;
			
			
		case gaugeMode.VerticalSpeed :
			
			inputValue = Mathf.Lerp (inputValue, GetVSpeed(), Time.deltaTime * 5f);
			SetAnimtime ();

			break;

		case gaugeMode.Fuel:

			if (useSelectorIndex)
				inputValue = GetFuelTank (selectorIndex);
			else
				inputValue = GetFuel();

			SetAnimtime ();
			
			break;

		case gaugeMode.RPM :
			
			inputValue = GetRpm();
			SetAnimtime ();

			break;

		case gaugeMode.Horizont:

			flightAttitude = GetAttitude ();
			SetHorizontNeedle ();

			break;
		}


	}

	private float GetSpeed (){

		float unitsCoef = 0f;

		switch (iasUnitsMode) {

		case speedMode.Kmh: 

			unitsCoef = 3.60f;

			break;

		case speedMode.Knots: 

			unitsCoef = 1.94f;

			break;

		case speedMode.Mph: 

			unitsCoef = 2.23f;

			break;

		}
		
		Vector3 velo = aircraft.GetComponent<Rigidbody> ().GetPointVelocity (aircraft.transform.position);
		float velocity = aircraft.transform.InverseTransformDirection (velo).z * unitsCoef;

		return velocity;
		
	}
	
	private float GetAlt (){
		
		float altitudeSealvl = aircraft.transform.position.y;
		
		return altitudeSealvl;//mts
		
	}
	
	private float GetVSpeed(){
		
		float vspeed = aircraft.GetComponent<AircraftControl>().verticalSpeed;
		
		return vspeed;
	}

	private float GetFuel (){

		float rate = fuelManager.fuelRate;

			return rate;

	}
	private float GetFuelTank (int tank){

		float rate = fuelManager.fuelCompartments[tank].quantityRate;

		return rate;

	}

	private float GetRpm (){
		
		float rpm = engine.rpm / 1000f;
		
		return rpm;//mts
		
	}

	private Vector2 GetAttitude (){
		
		float pitch = aircraft.transform.eulerAngles.x;
		if (pitch > 180f)
			pitch = pitch - 360f;

		float bank = aircraft.transform.eulerAngles.z;
		if (bank > 180f)
			bank = bank - 360f;
		pitch = Mathf.Clamp (pitch, -maxPitch, maxPitch);
		Vector2 attitude = new Vector2 (pitch,bank);

		return attitude;//angles deg

	}


	private void SetAnimtime (){
					
			float animTime = needleAnimation.Evaluate (inputValue);
			angle = fullAngle * animTime;
		angle2 = fullAngle * (inputValue / needle2Rate);
		Vector3 localEulers = needle.localEulerAngles;
		localEulers.z = needleOffset + (angle * -1f);
		needle.localEulerAngles = localEulers;
		if (needle2 != null) {
			Vector3 localEulers2 = needle2.localEulerAngles;
			localEulers2.z = angle2 * -1f;
			needle2.localEulerAngles = localEulers2;
		}
	}

	private void SetHorizontNeedle () {

		Vector3 eulers = needle.localEulerAngles;
		eulers.z = -flightAttitude.y;
		needle.localEulerAngles = eulers;
		needle2.localEulerAngles = eulers;
		Vector3 pos = needle.localPosition;
		pos.y =  needleOffset + flightAttitude.x;
		needle.localPosition = pos;

	}

	void OnGUI(){
		if(mode == gaugeMode.Airspeed)
			GUI.Label (new Rect (2f, 2f, 150f, 50f), "Ias: " + Mathf.FloorToInt (GetSpeed ()).ToString () + " " + iasUnitsMode.ToString());

	}
}
