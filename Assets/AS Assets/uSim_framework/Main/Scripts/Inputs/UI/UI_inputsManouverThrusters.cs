using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_inputsManouverThrusters : MonoBehaviour {

	public ManouverThrust thruster;
	public UI_buttonController thrustButton;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


			thruster.throttle = thrustButton.inputValue;
		
	}

	public void SetInputs (){

	
	}
}
