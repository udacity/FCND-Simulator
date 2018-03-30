using UnityEngine;
using System.Collections;

public class TrailerInit : MonoBehaviour {

	public GameObject trailer;
	public Transform anchor;
	// Use this for initialization
	void Start () {
	
		SpawnTrailer ();

	}
	
	// Update is called once per frame
	void SpawnTrailer () {

		GameObject spawnedTrailer = Instantiate (trailer, anchor.position, anchor.rotation) as GameObject;
		spawnedTrailer.GetComponent<HingeJoint> ().connectedBody = GetComponent<Rigidbody> ();

	}
}
