using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DemoSceneManager : MonoBehaviour {


	public GameObject mainCamera;
	public GameObject[] availableVehicles;
	public GameObject menu;
	public GameObject pauseMenu;
	public GameObject keymapScreen;
	public GameObject player;
	public int selectedIndex;
	public Transform currentSpawnPoint;
	public SpawnBaseData[] locations;
	SpawnBaseData currentLocation;
	public List<SpawnData> availableSpawns;
	SpawnData selectedSpawn;
	int selectorIndex;
	public bool pause;

	public Dropdown vehiclesDropdown;
	public Dropdown locationsDropdown;
	public Dropdown spawnsDropdown;

	public GameObject uSimPlayerInterface;

	IEnumerator Start () {

		uSimPlayerInterface.SetActive (false);
		GameObject.FindObjectOfType<MapManager> ().player = mainCamera.transform;
		ClearDropdowns ();
		FillVehicles ();
		FillLocations ();
		yield return new WaitForEndOfFrame ();
		SetLocation (0);
		SetSpawnPoint (0);
		yield return new WaitForSeconds (2f);
		SetSelectedVehicle (0);


	}

	void ClearDropdowns (){

		vehiclesDropdown.ClearOptions ();
		locationsDropdown.ClearOptions ();
		spawnsDropdown.ClearOptions ();
	}

	public void SelectAircraft (int index){

		selectedIndex = index;
		InstantiatePlayer ();

	}

	public void SetSelectedVehicle (int index){


		selectedIndex = vehiclesDropdown.value;
		if (selectedSpawn.spawnType == availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().vehicleType) {
			SpawnSelectedVehicle ();
		} else {
			if (player != null) {
				mainCamera.transform.parent = null;
				Destroy (player);
			}
			StartCoroutine (GetAvailableSpawn ());
		}
	}

	IEnumerator GetAvailableSpawn () {

		int i = 0;
		do {						
			SetLocation (i);
			yield return new WaitForEndOfFrame();
			if(selectedSpawn.spawnType == UsimVehicle.VehicleTypes.Sea &&
				availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().vehicleType == UsimVehicle.VehicleTypes.Air && 
				availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().isSeaPlane){
				SpawnSelectedVehicle();
				player.GetComponentInChildren<LandingGearAnimation> ().SetStart (false);
				break;
			}
			if(selectedSpawn.spawnType == availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().vehicleType){
				SpawnSelectedVehicle();
				break;
			}
			i++;
			yield return new WaitForEndOfFrame();
		} while(i < locationsDropdown.options.Count);


		yield return true;
	}

	public void SetSelectedLocation (){

		currentLocation = locations [locationsDropdown.value];

		GetLocationSpawns ();
	}

	public void SetLocation (int index){

		currentLocation = locations [index];
		GetLocationSpawns ();
	}



	void GetLocationSpawns (){

		print ("val :  " + locationsDropdown.value);
		spawnsDropdown.ClearOptions ();
		availableSpawns = new List<SpawnData> ();
		foreach (SpawnData sd in currentLocation.spawnPoints) {
			if (availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().vehicleType == UsimVehicle.VehicleTypes.Air &&
				availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().isSeaPlane &&
				sd.spawnType == UsimVehicle.VehicleTypes.Sea)
				availableSpawns.Add (sd);
			if (availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().vehicleType == sd.spawnType)
				availableSpawns.Add (sd);
			
		}
		if (availableSpawns.Count > 0) {
			
			selectedSpawn = availableSpawns [0];
			currentSpawnPoint = selectedSpawn.transform;
			if (player != null) {
				if (availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().vehicleType == UsimVehicle.VehicleTypes.Air &&
				    availableVehicles [selectedIndex].GetComponent<UsimVehicle> ().isSeaPlane &&
				    selectedSpawn.spawnType == UsimVehicle.VehicleTypes.Sea)
					player.GetComponentInChildren<LandingGearAnimation> ().SetStart (true);
				StartCoroutine (MovePlayer ());
			}
		}
		FillLocationSpawns ();

	
	}

	void FillVehicles (){

		List<string> options = new List<string>();
		foreach (GameObject veh in availableVehicles) {

			UsimVehicle vehicle = veh.GetComponent<UsimVehicle> ();
			options.Add (vehicle.name + " | " + vehicle.description);

		}
		vehiclesDropdown.AddOptions (options);
	}

	void FillLocations (){

		List<string> options = new List<string>();
		foreach (SpawnBaseData obj in locations) {

			options.Add (obj.baseDescription);

		}
		locationsDropdown.AddOptions (options);
	}

	void FillLocationSpawns (){

		List<string> options = new List<string>();
		foreach (SpawnData sd in availableSpawns) {

			options.Add (sd.name);

		}
		spawnsDropdown.AddOptions (options);
	}

	void GetSelectedSpawn () {

	
		selectedSpawn = availableSpawns [selectedIndex];

	}

	public void SetSpawnPoint (int index) {

		currentSpawnPoint = availableSpawns [spawnsDropdown.value].transform;
		if (player != null) {
			StartCoroutine (MovePlayer ());
		}
	}

	IEnumerator MovePlayer (){

		do{			
			yield return new WaitForEndOfFrame ();
		}
		while(player == null);

		player.transform.position = currentSpawnPoint.position;
		player.transform.rotation = currentSpawnPoint.rotation;

	}

	void SpawnSelectedVehicle (){
		if (player != null) {
			mainCamera.transform.parent = null;
			Destroy (player);
		}
		player = Instantiate (availableVehicles [selectedIndex], currentSpawnPoint.position, currentSpawnPoint.rotation) as GameObject;
	
		GameObject.FindObjectOfType<MapManager> ().player = player.transform;
		player.GetComponent<InputsManager> ().occupied = false;
		player.GetComponent<UsimVehicle> ().ToggleVehCamera (false);
		mainCamera.transform.parent = player.transform;
		mainCamera.GetComponent<SmoothOrbit> ().target = player.transform;
	}


	public void InstantiatePlayer (){

	//	player = Instantiate (availableVehicles [selectedIndex], currentSpawnPoint.position, currentSpawnPoint.rotation) as GameObject;
		GameObject.FindObjectOfType<VehicleFollowSwitch> ().followAi = false;
		//if(player.GetComponent<UsimVehicle> ().spawnPanelOnStart)
	//	player.GetComponent<UsimVehicle> ().SpawnPanel ();
		GameObject.FindObjectOfType<VehiclesManager> ().SetPlayerVehicle (player.GetComponent<InputsManager> ());
		GameObject.FindObjectOfType<MapManager> ().player = player.transform;
		mainCamera.SetActive (false);
		ToggleMenu (false);
		ToggleMouseControl (true);


	}

	void ToggleMouseControl (bool toggle){

		MouseJoystick mouseJoy = GameObject.FindObjectOfType<MouseJoystick> ();
		mouseJoy.showReticle = !toggle;
		mouseJoy.lockJoystick = toggle;

	}

	public void ToggleKeymapScreen (bool toggle) {

		keymapScreen.SetActive (toggle);

	}


	public void ToggleMenu (bool toggle) {
		
		menu.SetActive (toggle);

	}

	public void TogglePauseMenu (bool toggle) {

		pauseMenu.SetActive (toggle);

	}

	void Update(){



		if (Input.GetKeyUp (KeyCode.Escape) && pause) {

			pause = false;	
			TogglePauseMenu (pause);
			ToggleMouseControl (pause);

				
		}
		else if (Input.GetKeyUp (KeyCode.Escape) && !pause) {

			pause = true;
			TogglePauseMenu (pause);
			ToggleMouseControl (pause);

		}

		if (Input.GetKeyUp (KeyCode.F12) && keymapScreen.activeSelf ) {

			ToggleMouseControl (false);
			ToggleKeymapScreen (false);


		}
		else if (Input.GetKeyUp (KeyCode.F12) && !keymapScreen.activeSelf) {

			ToggleMouseControl (true);
			ToggleKeymapScreen (true);

		}


	}

	public void ExitFlight (){
		
		mainCamera.transform.parent = null;
		GameObject panel = player.GetComponent<UsimVehicle> ().panelInstance;
		if (panel != null) {
			panel.SetActive (false);
			Destroy (panel);
		}
		player.SetActive (false);
		Destroy (player);
		mainCamera.SetActive (true);
		TogglePauseMenu (false);
		ToggleMenu (true);
		ToggleMouseControl (true);


	}

	public void StartFlight () {

		StartCoroutine (SetFlight ());
	}

	IEnumerator SetFlight (){

		uSimPlayerInterface.SetActive (true);
		yield return new WaitForEndOfFrame ();
		UsimVehicle vehicle = player.GetComponent<UsimVehicle> ();
		if(vehicle.spawnPanelOnStart)
		vehicle.SpawnPanel ();
		InputsManager inputs = player.GetComponent<InputsManager> ();
		inputs.occupied = true;
		GameObject.FindObjectOfType<VehiclesManager> ().SetPlayerVehicle (inputs);
		GameObject.FindObjectOfType<MapManager> ().player = player.transform;
		mainCamera.SetActive (false);
		vehicle.ToggleVehCamera (true);
		ToggleMenu (false);
		ToggleMouseControl (true);

	}

	public void ResumeFlight (){


		TogglePauseMenu (false);
		ToggleMouseControl (true);

	}

	public void ExitApp () {

		Application.Quit ();

	}

}
