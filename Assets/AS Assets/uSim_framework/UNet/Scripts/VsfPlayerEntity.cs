using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class VsfPlayerEntity : NetworkBehaviour {

	VSF_Unet_DemoMain main;
	public string playerName;


	void Start () {
		main = GameObject.FindObjectOfType<VSF_Unet_DemoMain> ();
		if(isLocalPlayer)
			main.playerEntity = this;
		
	}

	public void SpawnVehicle (GameObject requestPlayer){
		
		print ("server spawning : " + requestPlayer.name);

		CmdSpawnVehicle (requestPlayer, playerName);

	}

	[Command]
	public void CmdDestroyPlayer (GameObject player){

		NetworkServer.Destroy (player);

	}




	[Command]
	public void CmdSpawnVehicle (GameObject requestPlayer, string playerTag) {
		
		Vector3 startPos = main.currentSpawnPoint.position;
		if(main.availableVehicles [main.selectedIndex].GetComponent<AutoPilotActionsManager> ().flightManager.setPositionOnStart)
			startPos = main.availableVehicles [main.selectedIndex].GetComponent<AutoPilotActionsManager> ().flightManager.worldStartPosition;

		GameObject player = Instantiate (main.availableVehicles [main.selectedIndex],startPos, main.currentSpawnPoint.rotation) as GameObject;
		player.GetComponent<NetworkPosition> ().syncPos = main.currentSpawnPoint.position;
		player.GetComponentInChildren<NetworkVehicleTag> ().SetTagText (playerTag);

		NetworkServer.SpawnWithClientAuthority (player, requestPlayer);

	}

	[Command]
	public void CmdSpawnAiVehicle (string prefabId, GameObject requestPlayer) {

		GameObject prefab = GameObject.FindObjectOfType<VsfAiSpawner> ().GetAiVehicleById (prefabId);
		Vector3 startPos = prefab.GetComponent<AutoPilotActionsManager> ().flightManager.worldStartPosition;

		GameObject vehicle = Instantiate (prefab, startPos, prefab.transform.rotation) as GameObject;
		vehicle.GetComponentInChildren<NetworkVehicleTag> ().SetTagText (prefabId + " (Ai)");

		NetworkServer.Spawn (vehicle);



	}
}
