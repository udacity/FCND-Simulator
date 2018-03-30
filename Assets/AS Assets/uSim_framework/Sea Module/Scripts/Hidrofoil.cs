using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hidrofoil : MonoBehaviour {

	public Rigidbody targetRigidbody;
	public AnimationCurve curve;
	public float area;
	public float deltaAngle;
	public float coef;
	public Transform oceanRef;
	public float deltaFloat;
	public float applyForce;

	void Start () {

	}

	void FixedUpdate () {

		Vector3 pointVelocity = targetRigidbody.GetPointVelocity (transform.position);
		float velocityZ = transform.InverseTransformDirection (pointVelocity).z;
		float velocityX = transform.InverseTransformDirection (pointVelocity).x;

		deltaFloat = oceanRef.localPosition.y - transform.localPosition.y;

		deltaAngle = (Mathf.Atan2(velocityX, velocityZ ) * Mathf.Rad2Deg);
		coef = curve.Evaluate (deltaAngle);	
		applyForce = (1.015f * coef * (velocityZ * velocityZ) * area) * Mathf.Clamp01(deltaFloat);
		targetRigidbody.AddForceAtPosition (transform.TransformDirection(new Vector3(-applyForce,0f,0f)), transform.position);

	}


	void OnDrawGizmosSelected () {

		Gizmos.color = Color.blue;
		Gizmos.DrawIcon (transform.position , "hidrofoil.png", true);

	}
}
