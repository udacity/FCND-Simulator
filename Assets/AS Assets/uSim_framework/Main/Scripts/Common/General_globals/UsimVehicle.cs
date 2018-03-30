using UnityEngine;
using System.Collections;

public class UsimVehicle : MonoBehaviour {

	public enum VehicleTypes { Air, Land, Sea}; 

	public VehicleTypes vehicleType;
	public bool isSeaPlane;
	public string name;
	public string description;
	public GameObject panelPrefab;
	public bool spawnPanelOnStart;
	[HideInInspector]
	public GameObject panelInstance;
	public Vector3 panelPosition;
	[HideInInspector]
	public AircraftControl aircraftController;
	[HideInInspector]
	public VehicleController vehicleController;
	[HideInInspector]
	public ShipController shipController;
	[HideInInspector]
	public EnginesManager engines;
	[HideInInspector]
	public FuelManager fuel;

	void Awake () {

		switch (vehicleType) {

		case VehicleTypes.Air:

			aircraftController = GetComponent<AircraftControl> ();

			break;

		case VehicleTypes.Land:

			vehicleController = GetComponent<VehicleController> ();

			break;

		case VehicleTypes.Sea:

			shipController = GetComponent<ShipController> ();

			break;

		}

		engines = GetComponentInChildren<EnginesManager> ();
		fuel = GetComponentInChildren<FuelManager> ();

	}

	void Start () {



		//if (spawnPanelOnStart)
		//	SpawnPanel ();

	}

	public void SpawnPanel () {	

		aircraftController = GetComponent<AircraftControl> ();
		engines = GetComponentInChildren<EnginesManager> ();
		fuel = GetComponentInChildren<FuelManager> ();
		panelInstance = (GameObject) Instantiate (panelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		panelInstance.transform.parent = GameObject.Find ("_uSim_PlayerInterface").transform;
		Panel2D panel = panelInstance.GetComponentInChildren<Panel2D> ();
		panel.SetAircraftGaugesData (this);
		panel.transform.localPosition = panelPosition;

		if (vehicleType != VehicleTypes.Air) {

			GameObject.FindObjectOfType<MouseJoystick> ().enabled = false;

		}
	}

	public void ToggleVehCamera (bool toggle){

		transform.Find ("vehcam").gameObject.SetActive (toggle);

	}



}
