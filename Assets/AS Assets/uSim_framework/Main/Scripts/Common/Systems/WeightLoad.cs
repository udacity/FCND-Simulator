using UnityEngine;
using System.Collections;

public class WeightLoad : MonoBehaviour {

	public Rigidbody targetVehicle;
	public float mass;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		targetVehicle.AddForceAtPosition (Vector3.down * mass * -Physics.gravity.y, transform.position);

	}
}
