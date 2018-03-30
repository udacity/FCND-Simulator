using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterModeToggler : MonoBehaviour {

	VehicleController controller;
	public bool waterModeEnabled;

	// Use this for initialization
	void Start () {
		controller = GetComponent<VehicleController> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyUp (KeyCode.D) && !waterModeEnabled)
			waterModeEnabled = true;
		else if (Input.GetKeyUp (KeyCode.D) && waterModeEnabled)
			waterModeEnabled = false;

		controller.engine.useClutch = true;

		if (!waterModeEnabled)
			return;

		controller.engine.useClutch = false;
		controller.steeringInput *= -1f;

	}
}
