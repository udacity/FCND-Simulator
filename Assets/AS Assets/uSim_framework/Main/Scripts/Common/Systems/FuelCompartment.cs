using UnityEngine;
using System.Collections;

public class FuelCompartment : MonoBehaviour {

	public float emptyWeight;
	[HideInInspector]
	public float totalWeight;
	public float unitWeight = 1f;
	[HideInInspector]
	public float fuelWeight;
	//[HideInInspector]
	public float capacity;
	public float fuelQuantity;
	[HideInInspector]
	public float quantityRate;

	public Rigidbody targetRigidbody;

	// Use this for initialization
	void Start () {
	

		//capacity = (Mathf.PI * transform.localScale.x * transform.localScale.y) * 10f;
		if (fuelQuantity >= capacity)
			fuelQuantity = capacity;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		quantityRate = fuelQuantity / capacity;
		fuelWeight = fuelQuantity * unitWeight;
		totalWeight = fuelWeight + emptyWeight;
		targetRigidbody.AddForceAtPosition (Vector3.down * (totalWeight * Physics.gravity.y), transform.position);

	}
}
