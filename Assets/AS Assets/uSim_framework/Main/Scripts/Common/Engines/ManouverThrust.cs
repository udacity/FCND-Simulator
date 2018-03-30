using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManouverThrust : MonoBehaviour {

	InputsManager inputs;
	Rigidbody vehicle;

	public float pushForce;
	public float throttle;
	// Use this for initialization
	void Start () {
		inputs = transform.root.GetComponent<InputsManager> ();
		vehicle = transform.root.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (inputs == null)
			return;
		if (vehicle == null)
			return;

		if (inputs.occupied && inputs.player) {
			if (throttle != 0f)
				AddThrust ();
		}

	}

	public void AddThrust () {
		
			vehicle.AddForceAtPosition (-transform.forward * pushForce * throttle, transform.position);

	}
}
