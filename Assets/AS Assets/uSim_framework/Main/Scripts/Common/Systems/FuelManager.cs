using UnityEngine;
using System.Collections;

public class FuelManager : MonoBehaviour {

	public FuelCompartment[] fuelCompartments;
	[HideInInspector]
	public float fullCapacity;
	[HideInInspector]
	public float currentQuantity;
	[HideInInspector]
	public float fuelRate;
	public int selectedTank;
	public bool allTanks;
	// Use this for initialization
	void Start () {
	
		foreach (FuelCompartment fuelComp in fuelCompartments) {
			
			fullCapacity += fuelComp.capacity;
			currentQuantity += fuelComp.fuelQuantity;
		}
	}

	float GetFuelStatus (){

		float fuelStatus = 0f;

		foreach (FuelCompartment fuelComp in fuelCompartments) {
					
			fuelStatus += fuelComp.fuelQuantity;
		}

		return fuelStatus;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		currentQuantity = GetFuelStatus ();
		fuelRate = currentQuantity / fullCapacity;

	}
	public void ReduceFuelQuantity (float amount){
		if (!allTanks)
			fuelCompartments [selectedTank].fuelQuantity -= amount * Time.deltaTime;
		else
			ReduceFromAll (amount);
	}

	public float GetAvailableFuelAmount (){
		if (!allTanks)
			return fuelCompartments [selectedTank].fuelQuantity;
		else
			return GetFuelStatus ();
	}

	void ReduceFromAll (float amount){

		foreach (FuelCompartment fuelComp in fuelCompartments) {

			fuelComp.fuelQuantity -= amount / fuelCompartments.Length;
		}

	}

	public void SetSelectedTank (int index){

		selectedTank = index;

	}
}
