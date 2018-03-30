using UnityEngine;
using System.Collections;

public class RollSound : MonoBehaviour {

	public AudioSource sound;
	public float inputSpeed;
	public float threshold = 20f;
	VehicleWheel wheel;
	// Use this for initialization
	void Start () {
	
		wheel = GetComponent<VehicleWheel> ();
	}


	public void SetRollSpeed (float speed){

		inputSpeed = speed;

	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (wheel.onGround) {
			sound.volume = Mathf.Lerp (sound.volume, inputSpeed / threshold, Time.deltaTime * 2.5f);
			SetRollSpeed (wheel.Mz);
		} else {

			sound.volume = 0f;
		}
	}
}
