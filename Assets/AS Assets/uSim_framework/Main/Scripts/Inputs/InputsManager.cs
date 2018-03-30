using UnityEngine;
using System.Collections;

public class InputsManager : MonoBehaviour {

	public bool occupied;
	public bool player;
	public float elevator;
	public float aileron;
	public float rudder;
	public float steering;
	public float throttle;
	public float accelerator;
	public int flaps;
	public bool flapsUp;
	public bool flapsDown;
	public float shipRudder;
	public bool tailwheelLock;
	public float airBrakes;
	public float slats;
	public float swept;
	public float colectivePitch;
	public float thrustDir;
	public float trim;
	public float trimauto;
	public float wheelBrake;
	public float pedalBrake;
	public float parkingBrake;
	public bool setParkingbrake;
	public float engineClutch;
	public bool engineOn;
	public bool gearDwn;
	public bool autoTrim;
	public AnimationCurve trimCurve;
	public AircraftControl aircraftControl;	
	public EnginesManager enginesManager;
	public UsimVehicle usimVeh;


	void Start () {
		usimVeh = GetComponent <UsimVehicle>();
		aircraftControl = GetComponent <AircraftControl>();
		enginesManager = GetComponentInChildren<EnginesManager> ();
		thrustDir = 1f;
	
	}

	public void SetLandingGear (bool set){

		gearDwn = set;
		aircraftControl.gearDwn = gearDwn;
		LandingGearAnimation[] gears = transform.GetComponentsInChildren<LandingGearAnimation> () as LandingGearAnimation[];
		foreach (LandingGearAnimation lga in gears) {
			lga.transform.SendMessage ("ToggleGear", !gearDwn);
		}
	}

	public void SetElevator (float set){

		elevator = set ;

	}

	public void SetAileron (float set){

		aileron = set ;

	}

	public void SetRudder (float set){

		rudder = set;
		shipRudder = set;
	}

	public void SetSteering (float set){

		steering = set;
	}

	public void SetThrottle (float set){

		throttle = set;

	}

	public void SetAccelerator (float set){

		accelerator = set;

	}

	public void SetColecivePitch (float set){

		colectivePitch = set ;

	}

	public void SetEngineStart (bool set){
		
		engineOn = set;
	}

	public void SelectEngine (int engineId){

		enginesManager.SelectEngine (engineId);

	}

	public void SetClutch (float set) {

		engineClutch = set;

	}


	public void SetWheelbrake (float set){

		wheelBrake = set;

	}

	public void SetPedalBrake (float set){

		pedalBrake = set;

	}

	public void SetParkingBrake (float set){

		parkingBrake = set;
		if (aircraftControl != null && set == 1f)
			aircraftControl.parkingBrake = true;
		else
			aircraftControl.parkingBrake = false;
	}

	public void SetflapsUp (bool set){

		flapsUp = set;

	}

	public void SetflapsDown (bool set){

		flapsDown = set;

	}

	public void SetAirBrakes (float set) {

		airBrakes = set;

	}

	public void SetSlats (float set) {

		slats = set;

	}

	public void SetSwept (float set) {

		swept = set;

	}

	public void SetShipRudder (float set) {

		shipRudder = set;

	}

	public void FlapUp (){
		
		flaps--;
		flapsUp = false;
	}

	public void FlapDown (){

		flaps++;
		flapsDown = false;

	}




	void Update () {
		if (occupied ){
			if (player) {
				if (aircraftControl != null) {
					if (Input.GetKeyDown ("a") && autoTrim)
						autoTrim = false;
					else if (Input.GetKeyDown ("a") && !autoTrim)
						autoTrim = true;
			
					if (autoTrim)
						trimauto = trimCurve.Evaluate (aircraftControl.ias);


					if (setParkingbrake)
						SetParkingBrake (1f);
					else
						SetParkingBrake (0f);

					if (aircraftControl.isTailWheel)
						aircraftControl.tailwheelLock = tailwheelLock;
			
				}
			}
			if (usimVeh.vehicleType == UsimVehicle.VehicleTypes.Air || usimVeh.vehicleType == UsimVehicle.VehicleTypes.Sea) {
				enginesManager.throttleInput = throttle;	
				enginesManager.thrustDir = thrustDir;
			}
			if (usimVeh.vehicleType == UsimVehicle.VehicleTypes.Land) {
				enginesManager.throttleInput = accelerator;
			}
			/*if(heliControl == null) return;
			heliControl.pitchInput = elevator;
			heliControl.rollInput = aileron;
			heliControl.yawInput = rudder;*/

		}

		
	}
}
