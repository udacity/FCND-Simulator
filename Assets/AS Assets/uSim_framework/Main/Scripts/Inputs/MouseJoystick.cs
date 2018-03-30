using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseJoystick : MonoBehaviour {

	public bool showReticle;
	public bool lockJoystick;
	public RectTransform reticle;
	public float reticleSize;
	public RectTransform mouseBall;

	public float movementX;
	public float movementY;

	public Vector2 outputValues;

	public InputsManager inputs;

	// Use this for initialization
	void Start () {
	
		mouseBall.localPosition = Vector3.zero;
		reticle.sizeDelta = new Vector2(reticleSize,reticleSize);

	}

	public void MoveBall (Vector2 movement){

		Vector3 newPos = mouseBall.localPosition;
		newPos.x += movement.x;
		newPos.x = Mathf.Clamp (newPos.x, -reticleSize / 2, reticleSize / 2);
		newPos.y += movement.y;
		newPos.y = Mathf.Clamp (newPos.y, -reticleSize / 2, reticleSize / 2);
		mouseBall.localPosition = newPos;

		outputValues = new Vector2 (newPos.x / (reticleSize / 2), newPos.y / (reticleSize / 2));
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.LeftControl)) {

			reticle.gameObject.SetActive (false);
			lockJoystick = true;

		}
		if (Input.GetKeyUp (KeyCode.LeftControl)) {

			reticle.gameObject.SetActive (true);
			lockJoystick = false;

		}

		if (!showReticle && reticle.gameObject.activeSelf) {

			reticle.gameObject.SetActive (false);

		} else if (showReticle && !reticle.gameObject.activeSelf) {

			reticle.gameObject.SetActive (true);
		}
			
		float inputX = Input.GetAxis ("Mouse X");
		float inputY = Input.GetAxis ("Mouse Y");

		if (!lockJoystick) {
			MoveBall (new Vector2 (inputX, inputY));
			Cursor.visible = false;
		} else {

			Cursor.visible = true;
		}

		
		if (inputs != null && !lockJoystick)
			SetInputs ();
		
	}

	void SetInputs (){

		inputs.SetAileron (outputValues.x);
		inputs.SetElevator (-outputValues.y);

	}

	public void GetInputs(){

		inputs = GetComponent<VehiclesManager> ().playerVehicleInputs;

	}
}
