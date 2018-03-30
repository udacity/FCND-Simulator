using UnityEngine;
using System.Collections;

public class EnginesManager : MonoBehaviour {

	public enum ThrottleInputTypes{Contribute, Direct};
	public ThrottleInputTypes throttleInputType;
	public bool allEngines;
	public Engine[] engines;
	//[HideInInspector]
	public float throttleInput;
	public float thrustDir = 1f;
	//[HideInInspector]
	public float throttle;
	//[HideInInspector]
	public bool engineSelectorOn;
	// Use this for initialization
	void Start () {
	
		thrustDir = 1f;
	}

	void FixedUpdate () {

		thrustDir = Mathf.Clamp (thrustDir, -1f, 1f);
	
		if(!engineSelectorOn)
		allEngines = true;

		if(allEngines)
			SetThrottleAll ();
		
	}

	public void SelectEngine (int engineId) {

		allEngines = false;
		engines [engineId].selected = true;
		engines [engineId].throttle += throttleInput * Time.deltaTime;
		engines [engineId].thrustDir = thrustDir;
		engineSelectorOn = true;
	}

	public void SetEngineThrottle (int engineId, float value){

		if (value < 0f)
			thrustDir = -1f;
		else
			thrustDir = 1f;

		value = Mathf.Abs (value);

		if(throttleInputType == ThrottleInputTypes.Contribute)
			engines [engineId].throttle += value * Time.deltaTime;
		if(throttleInputType == ThrottleInputTypes.Direct)
			engines [engineId].throttle = value;
		
		engines [engineId].thrustDir = thrustDir;
		engineSelectorOn = true;

	}


	public void DeselectAllEngines () {
		
		foreach (Engine e in engines) {

			e.selected = false;

		}


	}

	public void SetThrottleAll(){

		if(throttleInputType == ThrottleInputTypes.Contribute)
		throttle += throttleInput * Time.deltaTime;
		if(throttleInputType == ThrottleInputTypes.Direct)
			throttle = throttleInput;
		throttle = Mathf.Clamp(throttle,0f,1f);


		foreach (Engine e in engines) {

			e.thrustDir = thrustDir;

			if (throttle < e.idleThrottle)
				throttle = e.idleThrottle;
			e.selected = true;
			if (throttleInputType == ThrottleInputTypes.Contribute) {
				if (throttleInput > 0f && throttle > e.throttle)
					e.throttle = throttle;

				if (throttleInput < 0f && throttle < e.throttle)
					e.throttle = throttle;
			} else {
				e.throttle = throttle;
			}
		}

	}
}
