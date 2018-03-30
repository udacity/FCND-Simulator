using UnityEngine;
using System.Collections;

public class EngineStarter : MonoBehaviour {

	public Engine engine;
	public bool starting;
	public float torque;
	private float initialTorque;
	public InputsManager inputs;
	public AudioSource starterSound;

	// Use this for initialization
	void Start () {
		inputs = transform.root.GetComponent<InputsManager> ();
		starterSound = GetComponent<AudioSource> ();
		initialTorque = engine.addedTorque;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (inputs == null)
			return;

		if (!engine.selected)
			return;

		starting = inputs.engineOn;
	
		if (starting) {
			starterSound.volume = 1f;
			Energize ();

		} else {

			engine.addedTorque = initialTorque;
			starterSound.volume = 0f;

		}

		starterSound.pitch = (engine.rpm / engine.stallThreshold );

	}

	void Energize (){

		engine.rpm += (torque-engine.friction);
		engine.addedTorque = torque;
	}
}
