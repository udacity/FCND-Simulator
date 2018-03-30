using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirdragObject : MonoBehaviour {

	public Rigidbody targetRigidbody;
	public float dragForce;
	public float dragCoef;
	public float activeRate = 1f;
	public float baseValue;
	public float airDensity;
	public float airSpeed;

	public bool useCustomDragCurve;
	public AnimationCurve customDragCurve;

	AtmosphericGlobals atmosphere;
	bool dynamicAtmosphere;

	// Use this for initialization
	void Start () {

		dynamicAtmosphere = false;

		if (GameObject.FindObjectOfType<AtmosphericGlobals> () != null) {

			atmosphere = GameObject.FindObjectOfType<AtmosphericGlobals> ();
			if (atmosphere.fixedAtmosphere)
				airDensity = atmosphere.fixedDensity;
			else
				dynamicAtmosphere = true;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (targetRigidbody == null)
			return;
				
		Vector3 objectVel = targetRigidbody.transform.InverseTransformDirection (targetRigidbody.GetPointVelocity (transform.position));
		airSpeed = objectVel.z;

		if (dynamicAtmosphere)
			airDensity = atmosphere.GetDensity(transform.position.y);

		if (useCustomDragCurve)
			dragCoef = customDragCurve.Evaluate (airSpeed / 10f );
		else
			dragCoef = 1f;

		dragForce = (airDensity * ((dragCoef * baseValue ) * 0.5f) * (airSpeed * airSpeed)) * activeRate;

		if (dragForce > 0.1f)
			targetRigidbody.AddForceAtPosition (transform.forward * -dragForce, transform.position);
	}
}


