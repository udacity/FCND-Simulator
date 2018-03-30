using UnityEngine;
using System.Collections;

public class SimpleGrip : MonoBehaviour {

	public Rigidbody robot;
	private Vector3 velocity;
	private float veloX;
	private float veloZ;
	public float gripFactor;
	public float maxForce;

	// Use this for initialization
	void Start () {
	
		robot = GetComponent<Rigidbody> ();

	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
	 velocity =	robot.GetPointVelocity (transform.position);
		veloX = transform.InverseTransformDirection (velocity).x;
		veloZ = transform.InverseTransformDirection (velocity).z;
		float forceX = veloX * gripFactor;
		float forceZ = veloZ * gripFactor;
		if (forceX > maxForce)
			forceX = maxForce;
		if (forceZ > maxForce)
			forceZ = maxForce;
		robot.AddForceAtPosition (transform.TransformDirection (new Vector3( -forceX,0f,-forceZ)), transform.position);
	}
}
