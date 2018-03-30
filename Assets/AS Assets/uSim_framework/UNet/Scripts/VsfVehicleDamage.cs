using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VsfVehicleDamage : MonoBehaviour {

	public string partId;
	public float health = 100f;
	public bool detachable;
	public Rigidbody partBody;
	FixedJoint partAnchor;
	float initialHealth;
	NetworkVehicleDamage damageManager;
	public bool addExplosionFx;
	public GameObject explosionPrefab;
	public bool addFireFx;
	public GameObject firePrefab;
	public bool addSmokeFx;
	public GameObject smokePrefab;

	// Use this for initialization
	void Start () {
		
		initialHealth = health;
		if(partBody != null)
			partAnchor = partBody.GetComponent<FixedJoint> ();
		damageManager = transform.root.GetComponent<NetworkVehicleDamage> ();

	}


	public void ResetState () {

		health = initialHealth;
		destroyed = false;
	}

	public void ApplyDamage (float amount){

		health -= amount;
		if (health <= 0f && !destroyed)
			DestroyPart ();

	}

	public bool destroyed;
	public void DestroyPart () {

		if (!destroyed) {
			destroyed = true;

			StartCoroutine (DoDestroy ());
		}
	}

	IEnumerator DoDestroy () {

		if (partBody != null ){
			if (detachable) {
				partBody.transform.parent = null;
				partBody.isKinematic = false;
				yield return new WaitForEndOfFrame ();
				Destroy (partAnchor);
			} 

				if(addExplosionFx)
					AddExplosionFx ();

				if(addFireFx)
					AddFireFx ();

				if(addSmokeFx)
					AddSmokeFx ();
			
		}

		if(!detachable)
			GetComponent<InputsManager> ().enabled = false;

		print ("Vehicle Destroyed");


	}

	void AddExplosionFx () {

		Instantiate (explosionPrefab, partBody.transform.position, Quaternion.identity);


	}

	void AddFireFx () {


		GameObject fire = (GameObject) Instantiate (firePrefab, partBody.transform.position, Quaternion.identity) as GameObject;
		if (partBody != null)
			fire.transform.parent = partBody.transform;


	}

	void AddSmokeFx () {

		GameObject smoke = (GameObject) Instantiate (smokePrefab, partBody.transform.position, Quaternion.identity) as GameObject;
		if (partBody != null)
			smoke.transform.parent = partBody.transform;

	}


	public void OnCollisionEnter (Collision collision){

		print ("Damage triggered!!");
	
			if (collision.rigidbody != null) {
			if (VSF_Unet_DemoMain.main.playerEntity.isServer) {
				float impactPoints = collision.rigidbody.velocity.magnitude;
				if (impactPoints < 7f)
					return;
				float massPoints = collision.rigidbody.mass;
				float damage = (impactPoints + massPoints);
				//print ("Hit " + damageManager.gameObject.name + " with " + damage + " damage points!");

				damageManager.CmdApplyDamageById (damage, partId);
			}

			} else {

				if (collision.relativeVelocity.magnitude > 10f)
					damageManager.CmdApplyDamageById (collision.relativeVelocity.magnitude * 10f, partId);

			}
		
	}


}
