using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkVehicleDamage : NetworkBehaviour {

	public bool destroyed;
	public VsfVehicleDamage[] damagableParts;
	int destroyedParts;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	[Command]
	public void CmdApplyDamageById (float amount, string id){

		foreach (VsfVehicleDamage damagePart in damagableParts) {

			if (damagePart.partId == id) {
				damagePart.ApplyDamage (amount);
				if (isServer)
					RpcApplyDamage (amount, id);
			}

		}

	}

	[ClientRpc]
	public void RpcApplyDamage (float amount, string id) {

		foreach (VsfVehicleDamage damagePart in damagableParts) {

			if (damagePart.partId == id) {
				damagePart.ApplyDamage (amount);
			}

		}


	}

	[Command]
	public void CmdDestroyById (string id){

		foreach (VsfVehicleDamage damagePart in damagableParts) {

			if (damagePart.partId == id) {
				damagePart.DestroyPart ();
				destroyedParts++;
			}
		}

		if (destroyedParts >= 2)
			destroyed = true;
	}
}
