using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkVehicle : NetworkBehaviour {

	public VSF_Unet_DemoMain main;

	public bool isAi;

	public EnginesManager enginesManager;
	public float engine1rpm;
	public float engine2rpm;
	public float engine3rpm;
	public float engine4rpm;


	// Use this for initialization
	IEnumerator Start () {

		main = GameObject.FindObjectOfType<VSF_Unet_DemoMain> ();

		enginesManager = GetComponentInChildren<EnginesManager> ();

		do {
			yield return new WaitForEndOfFrame ();
		} while(main.playerEntity == null);


		if (hasAuthority) {
			if (!isAi) {
				GameObject.FindObjectOfType<MapManager> ().player = transform;
				GetComponent<InputsManager> ().occupied = false;
				GetComponent<UsimVehicle> ().ToggleVehCamera (false);
				main.mainCamera.GetComponent<SmoothOrbit> ().target = transform;
				main.mainCamera.SetActive (true);
				main.mapCamera.enabled = false;
				main.menu.SetActive (true);
				main.player = gameObject;
			}
		} else {	
			if(!isAi)
				SetAsRemote ();
		}

	}


	public void SetAsRemote () {

		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<UsimVehicle> ().ToggleVehCamera (false);

		if (enginesManager != null)
			foreach (Engine eng in enginesManager.engines) {

				eng.remoteKinematic = true;

			}

	}


	[Command]
	void CmdSetEnginesRpm (float eng1Rpm, float eng2Rpm, float eng3Rpm, float eng4Rpm) {

		if (enginesManager.engines [0] != null)
			enginesManager.engines [0].rpm = eng1Rpm;

		if (enginesManager.engines.Length > 1 && enginesManager.engines [1] != null)
			enginesManager.engines [1].rpm = eng2Rpm;

		if (enginesManager.engines.Length > 2 && enginesManager.engines [2] != null)
			enginesManager.engines [2].rpm = eng3Rpm;

		if (enginesManager.engines.Length > 3 && enginesManager.engines [3] != null)
			enginesManager.engines [3].rpm = eng4Rpm;

	}

	[ClientRpc]
	public void RpcSetEnginesRpm (float eng1Rpm, float eng2Rpm, float eng3Rpm, float eng4Rpm) {

		if (!isServer) {
			
			if (enginesManager.engines [0] != null)
				enginesManager.engines [0].rpm = eng1Rpm;

			if (enginesManager.engines.Length > 1 && enginesManager.engines [1] != null)
				enginesManager.engines [1].rpm = eng2Rpm;

			if (enginesManager.engines.Length > 2 && enginesManager.engines [2] != null)
				enginesManager.engines [2].rpm = eng3Rpm;

			if (enginesManager.engines.Length > 3 && enginesManager.engines [3] != null)
				enginesManager.engines [3].rpm = eng4Rpm;

		}

	}

	// Update is called once per frame
	void FixedUpdate () {

		if (enginesManager == null)
			return;

		if (hasAuthority) {

			if (enginesManager.engines [0] != null)
				engine1rpm = enginesManager.engines [0].rpm;

			if (enginesManager.engines.Length > 1 && enginesManager.engines [1] != null)
				engine1rpm = enginesManager.engines [1].rpm;

			if (enginesManager.engines.Length > 2 && enginesManager.engines [2] != null)
				engine1rpm = enginesManager.engines [2].rpm;

			if (enginesManager.engines.Length > 3 && enginesManager.engines [3] != null)
				engine1rpm = enginesManager.engines [3].rpm;

			CmdSetEnginesRpm (engine1rpm, engine2rpm, engine3rpm, engine4rpm);


		}

	}
}
