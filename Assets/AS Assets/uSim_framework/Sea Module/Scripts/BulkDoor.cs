using UnityEngine;
using System.Collections;

public class BulkDoor : MonoBehaviour {

	public float rate;
	public float threshold;
	public bool open;
	public ShipCompartment compartmentA;
	public ShipCompartment compartmentB;
	public float transferRate;
	public float difference;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (open) {

			ShipCompartment from;
			ShipCompartment to;

			if (compartmentA.floodingRate > compartmentB.floodingRate) {

				from = compartmentA;
				to = compartmentB; 	 
			} else {

				from = compartmentB;
				to = compartmentA;
			}

	
			if (from.floodingRate > threshold) {
				difference = Mathf.Abs (Mathf.Clamp01 (from.floodVolume - to.floodVolume));
				if (from.floodVolume > to.floodVolume) {
					var level = from.floodVolume / from.capacity;

					rate = transferRate * level;
					to.floodVolume += (rate * difference) * Time.deltaTime;
					from.floodVolume -= (rate * difference) * Time.deltaTime;

				} 
			} 	
		}
	}
}
