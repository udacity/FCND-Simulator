using UnityEngine;
using System.Collections;

public class AircraftSounds : MonoBehaviour {

	AircraftControl controller;
	public AudioSource wind;
//	public AudioSource airframeWind;

	// Use this for initialization
	IEnumerator Start () {
		controller = GetComponent<AircraftControl> ();
		wind.enabled = false;
		yield return new WaitForSeconds (2f);
		wind.enabled = true;

	}
	
	// Update is called once per frame
	void FixedUpdate () {
	if (controller.speed > 0) {
			wind.pitch = controller.speed / 40f;
			wind.volume = controller.speed / 30f;
		}
	//	airframeWind.pitch = controller.speed / 40f;
	//	airframeWind.volume =  controller.speed / 40f;				

	}
}
