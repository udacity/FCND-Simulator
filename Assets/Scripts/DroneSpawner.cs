using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
	static DroneSpawner Instance { get; set; }

	public GameObject dronePrefab;
	public GameObject cameraPrefab;
	public bool spawnOnAwake;
	public LayerMask spawnMask;

	bool spawned;

	void OnEnable ()
	{
		Instance = this;
	}

	void Awake ()
	{
		if ( spawnOnAwake )
			Spawn ();
	}

	void OnDestroy ()
	{
		Instance = null;
	}

	void Spawn ()
	{
		if ( !spawned )
		{
			spawned = true;
			Vector3 spawnPoint = Vector3.zero;
			Ray ray = new Ray ( Vector3.up * 100, -Vector3.up );
			RaycastHit hit;
			if ( Physics.Raycast ( ray, out hit, 200, spawnMask ) )
			{
				spawnPoint = hit.point + Vector3.up * 0.5f;
			}
			var droneInst = Instantiate ( dronePrefab, spawnPoint, Quaternion.identity );
			var cameraInst = Instantiate ( cameraPrefab );
			cameraInst.GetComponent<FollowCamera> ().targetTransform = droneInst.transform;
		}
	}

	public static void SpawnDrone ()
	{
		if ( Instance != null )
			Instance.Spawn ();
	}
}