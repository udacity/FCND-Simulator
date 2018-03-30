using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VsfAiSpawner : MonoBehaviour {
	[System.Serializable]
	public class aiVehicleData
	{
		public string idName;
		public GameObject prefab;
	}

	public aiVehicleData[] aiVehiclesPool;
	public int secondsToWait;
	VSF_Unet_DemoMain main;

	// Use this for initialization
	IEnumerator Start () {

		main = GameObject.FindObjectOfType<VSF_Unet_DemoMain> ();

		do {
			yield return new WaitForEndOfFrame ();
		} while(main.playerEntity == null);

		yield return new WaitForSeconds (secondsToWait);
		if(main.playerEntity.isServer)
			yield return SpawnAiPool();

	}

	IEnumerator SpawnAiPool () {

		foreach (aiVehicleData data in aiVehiclesPool) {
			main.playerEntity.CmdSpawnAiVehicle (data.idName, main.playerEntity.gameObject);
			yield return new WaitForSeconds (5f);
		}

		yield return new WaitForEndOfFrame ();

	}

	public GameObject GetAiVehicleById (string id){

		foreach (aiVehicleData data in aiVehiclesPool) {

			if (data.idName == id)
				return data.prefab;
		}

		return null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
