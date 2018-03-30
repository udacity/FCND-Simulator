using UnityEngine;
using System.Collections;

public class ShipCompartment : MonoBehaviour {

	public Rigidbody attachedTo;
	public string compartmentName;
	public float capacity;
	public float emptyWeight;

	public float floodVolume;
	public float flowRate;
	public float floodingRate;
	public ShipCompartment[] connectedCompartments;

	// Use this for initialization
	void Start () {
		//liters
		capacity = (transform.localScale.z * transform.localScale.x * transform.localScale.y) * 1000;
	}
	

	void FixedUpdate () {
		
		if (floodVolume < capacity){
			floodVolume += flowRate * Time.deltaTime;	

		}

		if(floodVolume < 0 ) 
			floodVolume = 0;
		if(floodVolume > capacity) 
			floodVolume = capacity-1;

		floodingRate = floodVolume / capacity;	
		if (transform.Find ("floodLevel") != null) {
			Vector3 scale = transform.Find ("floodLevel").transform.localScale;
			scale.y = floodingRate;

		}

		attachedTo.AddForceAtPosition (Vector3.down * floodVolume * -Physics.gravity.y, transform.position);
	}
}
