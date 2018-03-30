using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotorsController : MonoBehaviour {

	public InputsManager inputs;
	public float fwdInput;
	public float sideInput;
	public float colectiveInput;
	public float tailRotorThrottle;
	public float maxXDeflection;
	public float maxZDeflection;
	public Transform forcePoint;
	public Prop mainRotor;
	public Transform tailRotor;
	public float tailRotorForce;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		fwdInput = inputs.elevator;
		sideInput = inputs.aileron;
		colectiveInput = inputs.colectivePitch;
		tailRotorThrottle = inputs.rudder;

		Vector3 fpPos = forcePoint.localPosition;
		fpPos.y = -maxZDeflection * fwdInput;
		fpPos.x = -maxXDeflection * sideInput;
		forcePoint.localPosition = fpPos;

	}
	void FixedUpdate () {

		colectiveInput = Mathf.Clamp01 (colectiveInput);
		mainRotor.densityCoef = 1f * colectiveInput;
		mainRotor.targetBody.AddRelativeTorque (Vector3.up * tailRotorForce * tailRotorThrottle);
		tailRotor.Rotate (Vector3.forward, mainRotor.rpm * 60f * Time.deltaTime, Space.Self);

	}
}
