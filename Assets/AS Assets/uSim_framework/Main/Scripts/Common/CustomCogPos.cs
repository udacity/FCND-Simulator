using UnityEngine;
using System.Collections;

public class CustomCogPos : MonoBehaviour {
	Rigidbody aircraft;
	Transform cog;
	// Use this for initialization
	void Start () {
		aircraft = GetComponent<Rigidbody> ();
		cog = transform.Find ("Cog").transform;
		aircraft.centerOfMass = cog.localPosition; 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
