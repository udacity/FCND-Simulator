using UnityEngine;
using System.Collections;

public class StandardInputs : MonoBehaviour {

	public InputsManager inputs;
	public bool mouseJoystick;

	// Use this for initialization
	void Start () {
		if (GetComponent<MouseJoystick> () != null)
			mouseJoystick = true;
	}


	// Update is called once per frame
	void Update () {
	
		if (inputs == null)
			return;
		
		//INPUTS	
		inputs.trim = Input.GetAxis ("trim");
		if (!mouseJoystick) {
			inputs.elevator = Mathf.Lerp (inputs.elevator, Input.GetAxis ("elevator") + inputs.trim + inputs.trimauto, Time.deltaTime * 5f);
			inputs.aileron = Input.GetAxis ("aileron");
		}
		inputs.rudder = Input.GetAxis ("rudder");
		inputs.steering = Input.GetAxis ("steering");
		inputs.shipRudder = Mathf.Lerp (inputs.rudder,Input.GetAxis ("shipRudder"), Time.deltaTime * 5f);

		inputs.airBrakes = Mathf.Lerp (inputs.airBrakes, Input.GetAxis ("airBrakes") , Time.deltaTime*3f);
		inputs.slats = Mathf.Lerp (inputs.slats, Input.GetAxis ("slats") , Time.deltaTime*3f);
		inputs.swept = Mathf.Lerp (inputs.swept, Input.GetAxis ("swept") , Time.deltaTime*3f);
		inputs.colectivePitch = Mathf.Lerp (inputs.colectivePitch, Input.GetAxis ("colective") , Time.deltaTime*10f);
		if (Input.GetKeyUp (KeyCode.Q) || inputs.flapsDown) {

			inputs.FlapDown ();

		}
		if (Input.GetKeyUp (KeyCode.W) || inputs.flapsUp) {

			inputs.FlapUp ();
		}

		if (inputs.slats < 0f) inputs.slats = 0f;
		if (inputs.usimVeh != null) {
			if (inputs.usimVeh.vehicleType == UsimVehicle.VehicleTypes.Air) {
				inputs.wheelBrake = Input.GetAxis ("wheelBrake");				
				inputs.throttle = Input.GetAxis ("throttle");
				if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyUp (KeyCode.Space) && !inputs.setParkingbrake) {
					inputs.setParkingbrake = true;
				} else if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyUp (KeyCode.Space) && inputs.setParkingbrake) {
					inputs.setParkingbrake = false;
				}
				if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyUp (KeyCode.T) && !inputs.tailwheelLock) {
					inputs.tailwheelLock = true;
				} else if (Input.GetKey (KeyCode.LeftControl) && Input.GetKeyUp (KeyCode.T) && inputs.tailwheelLock) {
					inputs.tailwheelLock = false;
				}
			}
			if (inputs.usimVeh.vehicleType == UsimVehicle.VehicleTypes.Land) {

				inputs.pedalBrake = Input.GetAxis ("carBrake");			
				inputs.accelerator = Mathf.Lerp(-1f,1f, Input.GetAxis ("carAccelerator"));
				inputs.engineClutch = Input.GetAxis ("engineClutch");
			}
			if (inputs.usimVeh.vehicleType == UsimVehicle.VehicleTypes.Sea) {

				inputs.throttle = Input.GetAxis ("throttle");

			}
		}

		if(Input.GetButtonDown ("engineToggle") && !inputs.engineOn) inputs.engineOn = true;
		else if(Input.GetButtonUp ("engineToggle") && inputs.engineOn) inputs.engineOn = false;



	
		if(Input.GetKey("1"))		
			inputs.SelectEngine (0);
		if (Input.GetKeyUp ("1")) {
			inputs.enginesManager.DeselectAllEngines ();
			inputs.enginesManager.engineSelectorOn = false;
		}
		
		if(Input.GetKey("2"))	
			inputs.SelectEngine (1);

		if (Input.GetKeyUp ("2")) {
			inputs.enginesManager.DeselectAllEngines ();
			inputs.enginesManager.engineSelectorOn = false;
		}
		
		if(Input.GetKey("3"))			
			inputs.SelectEngine (2);

		if (Input.GetKeyUp ("3")) {
			inputs.enginesManager.DeselectAllEngines ();
			inputs.enginesManager.engineSelectorOn = false;
		}
		
		if(Input.GetKey("4"))		
			inputs.SelectEngine (3);

		if (Input.GetKeyUp ("4")) {
			inputs.enginesManager.DeselectAllEngines ();
			inputs.enginesManager.engineSelectorOn = false;
		}

		

		if (inputs.aircraftControl != null) {
			if (inputs.aircraftControl.isRGear) {
				if (Input.GetButtonUp ("gear") && !inputs.gearDwn) {
					inputs.SetLandingGear (true);

				} else if (Input.GetButtonUp ("gear") && inputs.gearDwn) {
					inputs.SetLandingGear (false);

				}

			}
		}

	}


	public void GetInputs(){

		inputs = GetComponent<VehiclesManager> ().playerVehicleInputs;

	}
}
