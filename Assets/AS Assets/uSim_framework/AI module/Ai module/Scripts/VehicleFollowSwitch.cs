using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleFollowSwitch : MonoBehaviour {

	public List<GameObject> aiVehicles;
	public GameObject currentVeh;
	public int selected;
	public Transform camTransform;

	// Use this for initialization
	void Start () {
		followAi = true;
	}

	public void NextVeh () {

		if (selected < aiVehicles.Count-1)
			selected++;
		else
			selected = 0;
	}

	public void PrevVeh () {

		if (selected > 0)
			selected--;
		else
			selected = aiVehicles.Count;

	}
	public bool followAi;
	// Update is called once per frame
	void Update () {

		if (!followAi)
			return;
		
		currentVeh = aiVehicles [selected];
		camTransform.parent = currentVeh.transform;
		camTransform.GetComponent<SmoothOrbit> ().target = currentVeh.transform.Find ("Cog");
	}
}
