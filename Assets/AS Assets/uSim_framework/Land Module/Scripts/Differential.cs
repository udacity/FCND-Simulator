using UnityEngine;
using System.Collections;

public class Differential : MonoBehaviour {

	public VehicleWheel[] wheels;
	public VehicleGearBox gearBox;
	public int axles;
	public float diffRatio;
	public float fRpm;
	public float inputForce;

	// Use this for initialization
	void Start () {
		if (axles == 0)
			axles = 1;

		if (gearBox == null)
		if (transform.parent.GetComponent<VehicleGearBox> () != null)
			gearBox = transform.parent.GetComponent<VehicleGearBox> ();
	}

	public void SetOutput (float input){

		inputForce = input;

	}

	// Update is called once per frame
	void FixedUpdate () {
		float rpmFromWheel = ((wheels [0].rpm * wheels [0].radius)+ (wheels [1].rpm * wheels [1].radius)) / 2;
		if (rpmFromWheel >= 0f)
			fRpm = wheels [0].rpm;
		else
			fRpm = 0f;

		if (gearBox.curRatio == 0)
			return;
		foreach (VehicleWheel wheel in wheels) {

			wheel.inputForce = ((inputForce * gearBox.curRatio * diffRatio) / wheel.radius) / axles; 				
			wheel.rpmFromEngine =  Mathf.Lerp (gearBox.attachedEngine.rpm  / gearBox.curRatio / diffRatio, wheel.trpm , gearBox.attachedEngine.clutch);

		}


	}
}
