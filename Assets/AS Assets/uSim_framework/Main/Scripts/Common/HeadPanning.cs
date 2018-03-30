using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPanning : MonoBehaviour {

	public bool locked;
	Vector3 initialRot;
	// Use this for initialization
	void Start () {
		initialRot = transform.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKey (KeyCode.O) && locked) {
			locked = false;
		}
		else if (Input.GetKey (KeyCode.O) && !locked) {
			locked = true;
			transform.localEulerAngles = initialRot;
		}

		if (locked)
			return;

		Vector3 headEulers = transform.localEulerAngles;
		headEulers.x -= Input.GetAxis ("Mouse Y");
		headEulers.y += Input.GetAxis ("Mouse X");
		transform.localEulerAngles = headEulers;

	}
}
