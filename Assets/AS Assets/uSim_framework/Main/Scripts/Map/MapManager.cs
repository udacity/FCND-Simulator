using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour {

	public enum NavDir {N, S, E, W};

	public Transform worldParent;
	public float navRadius;
	public Transform player;
	public List<Transform> dynamicObjects;

	IEnumerator Start () {

		do {
			yield return new WaitForSeconds (1);
		} while(player == null);

		UsimVehicle[] vehicles = GameObject.FindObjectsOfType<UsimVehicle> () as UsimVehicle[];
		foreach (UsimVehicle veh in vehicles) {
			if (!dynamicObjects.Contains (veh.transform)) {
				if(veh.transform != player)
				dynamicObjects.Add (veh.transform);			
			}
		}

	}

	bool updating = false;
	IEnumerator UpdateMap() {
	


		updating = true;
		Vector3 playerPos = player.position;
		Vector3 worldPos = worldParent.position;


		if (playerPos.x > navRadius) {

			playerPos.x -= navRadius;
			worldPos.x -= navRadius;

			yield return StartCoroutine (SetPosition (worldPos, playerPos));
			//yield return StartCoroutine (MoveDynamicObjects (NavDir.E));
		}
		else if (playerPos.x < -navRadius) {

			playerPos.x += navRadius;
			worldPos.x += navRadius;

			yield return StartCoroutine (SetPosition (worldPos, playerPos));
			//yield return StartCoroutine (MoveDynamicObjects (NavDir.W));
		}
		else if (playerPos.z > navRadius) {

			playerPos.z -= navRadius;
			worldPos.z -= navRadius;

			yield return StartCoroutine (SetPosition (worldPos, playerPos));
			//yield return StartCoroutine (MoveDynamicObjects (NavDir.N));
		}
		else if (playerPos.z < -navRadius) {

			playerPos.z += navRadius;
			worldPos.z += navRadius;

			yield return StartCoroutine (SetPosition (worldPos, playerPos));
			//yield return StartCoroutine (MoveDynamicObjects (NavDir.S));
		}

		UnParentDynamicObjects ();
		updating = false;
	}

	void LateUpdate (){

		if (player == null) {
			worldParent.position = Vector3.zero;
			return;
		}

		if (!updating)
			StartCoroutine (UpdateMap ());

	}

	IEnumerator SetPosition(Vector3 worldpos, Vector3 playerpos){

		ParentDynamicObjects ();
		worldParent.position = worldpos;
		player.position = playerpos;

		yield return new WaitForEndOfFrame ();
	}

	IEnumerator MoveDynamicObjects (NavDir dir){

		int moveX = 0;
		int moveY = 0;

		switch (dir) {

		case NavDir.N:
			moveY = -1;
			break;
		case NavDir.S:
			moveY = 1;
			break;
		case NavDir.E:
			moveX = -1;
			break;
		case NavDir.W:
			moveY = 1;
			break;

		}

		foreach (Transform dynamicObj in dynamicObjects) {

			Vector3 pos = dynamicObj.transform.position;
			pos.x += navRadius * moveX;
			pos.z += navRadius * moveY;
			dynamicObj.transform.position = pos;

		}

		yield return new WaitForEndOfFrame ();
	}

	void ParentDynamicObjects () {
			
			foreach (Transform dynamicObj in dynamicObjects) {
			dynamicObj.parent = worldParent;
			}

	}

	void UnParentDynamicObjects () {

		foreach (Transform dynamicObj in dynamicObjects) {
			dynamicObj.parent = null;
		}

	}
}
