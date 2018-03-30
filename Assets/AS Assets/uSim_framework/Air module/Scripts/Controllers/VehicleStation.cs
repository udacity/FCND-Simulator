using UnityEngine;
using System.Collections;

public class VehicleStation : MonoBehaviour {

	public bool isController;
	public Transform exitPos;

	public bool occupied;
	public bool isPilot;
	public GameObject player;
	public Transform cameraRoot;
	public Transform playerPos;
	public Vector3 stationPos;
	public VirtualCockpit vc;
	// Use this for initialization
	void Start () {
	
		stationPos = playerPos.localPosition;
		if (isController) {

			InputsManager controller = transform.root.GetComponent<InputsManager> ();
			controller.pedalBrake = 1;
		}
	}
	public bool SetOccupied (GameObject requestPlayer){

		if (player != null) {

			print ("Station Occupied");
			return false;
		}
		player = requestPlayer;
		player.transform.parent = transform.root;
		occupied = true;

		cameraRoot.gameObject.SetActive (true);

		if (isPilot) {
			if(vc != null)
			vc.show = true;
			//panel.SetActive(true);
			//player.GetComponent<PilotCockpit>().use = true;

		}
		if (isController) {

			InputsManager controller = transform.root.GetComponent<InputsManager>();
			if (controller != null){


			controller.occupied = true;
			controller.player = true;
			}
		}
		return true;
	}



	public void SetFree (){

		//player.GetComponent<PilotCockpit> ().use = false;
		//player.GetComponent<PilotCockpit> ().Resetweights ();
		player.transform.parent = null;
		player = null;
		occupied = false;
		cameraRoot.gameObject.SetActive (false);
		if(vc != null)
		vc.show = false;
		InputsManager controller = transform.root.GetComponent<InputsManager>();
		if (controller == null)
			return;

		controller.occupied = false;
		controller.player = false;
		controller.wheelBrake = 1;
	}


	
	// Update is called once per frame
	/*void Update () {
	
		if(occupied && player != null && Input.GetKeyUp ("y")) {
			
			player.GetComponent<EnterVehicle>().ExitStation();
			SetFree ();

		}

	}*/
}
