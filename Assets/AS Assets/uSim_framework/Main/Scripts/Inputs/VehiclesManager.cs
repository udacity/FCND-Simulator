using UnityEngine;
using System.Collections;

public class VehiclesManager : MonoBehaviour {
	
	public InputsManager playerVehicleInputs;
	public bool useRewired;
	//inputs handlers
	MouseJoystick mouseJoy;
	StandardInputs standardInputs;



	// Use this for initialization
	void Start () {
	
		mouseJoy = GetComponent<MouseJoystick> ();
		standardInputs = GetComponent<StandardInputs> ();

	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.F5)) {

			SetMouseJoystick ();
		}

		if (Input.GetKeyUp (KeyCode.F6)) {

			SetStandardInputs ();
		}

		if (Input.GetKeyUp (KeyCode.F7) && useRewired) {

			SetRewiredInputs ();
		}

	}

	void SetMouseJoystick (){

		mouseJoy.lockJoystick = false;
		mouseJoy.showReticle = true;
		standardInputs.mouseJoystick = true;
		StartCoroutine (ShowMessage ("Mouse Joystick selected..."));
		if (useRewired)
			SendMessage ("DisableRewired");
		
	}

	void SetStandardInputs () {

		mouseJoy.lockJoystick = true;
		mouseJoy.showReticle = false;
		standardInputs.mouseJoystick = false;
		StartCoroutine (ShowMessage ("Standard inputs selected..."));
		if (useRewired)
			SendMessage ("DisableRewired");

	}

	void SetRewiredInputs () {

		mouseJoy.lockJoystick = true;
		mouseJoy.showReticle = false;
		standardInputs.mouseJoystick = true;
		SendMessage ("EnableRewired");
		StartCoroutine (ShowMessage ("Rewired inputs selected..."));

	}

	public void SetPlayerVehicle (InputsManager inputs){

		playerVehicleInputs = inputs;
		SetPlayerInterface ();
		SetStandardInputs ();

	}

	void SetPlayerInterface (){

		gameObject.SendMessage ("GetInputs");

	}

	string messageText = "";
	bool showmessage;
	void OnGUI (){

		if(showmessage)
		GUI.Label (new Rect(Screen.width / 2 - 100f, Screen.height /2 - 20f, 100f, 40f), messageText);

	}

	IEnumerator ShowMessage (string text) {

		messageText = text;
		showmessage = true;
		yield return new WaitForSeconds (1);
		showmessage = false;
	}
}
