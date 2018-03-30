using UnityEngine;
using System.Collections;
//1.0
public class PilotHead : MonoBehaviour {

	public float defaultAngle;
	public float targetRot;
	public float targetRotX;
	public float headLeanAngle;
	public Transform pilotHead;
	public Transform pilotHeadPivot;
	public HeadPanning headPanning;
	Vector3 initialPos;
	Vector3 headInitialPos;

	void Start () {

		if(pilotHead.GetComponent<HeadPanning> () != null)
			headPanning = pilotHead.GetComponent<HeadPanning> ();

		transform.position = pilotHead.position;
		transform.parent = pilotHead.transform;
		initialPos = transform.localPosition;
		headInitialPos = pilotHead.localPosition;
	}

	void LateUpdate(){

		transform.parent = pilotHead;

		//transform.position = Vector3.Lerp (transform.position, pilotHead.position, Time.deltaTime * 5f);



	}

	private Vector3 curRot;
	void Update () {

		if (headPanning != null) {
			if (!headPanning.locked)
				return;
		}

		if (Input.GetKey (KeyCode.Keypad1)) {
			pilotHead.transform.Translate (-Vector3.right * 1f * Time.deltaTime);
		} else if (Input.GetKey (KeyCode.Keypad3)) {
			pilotHead.transform.Translate (Vector3.right * 1f * Time.deltaTime);
		} else if (Input.GetKeyUp (KeyCode.KeypadPlus)) {
			pilotHead.transform.Translate (Vector3.up * 0.1f);
		} else if (Input.GetKey (KeyCode.KeypadMinus)) {
			pilotHead.transform.Translate (-Vector3.up * 0.1f);
		} else if (Input.GetKeyUp (KeyCode.KeypadPeriod)) {
			pilotHead.transform.Translate (-Vector3.forward * 0.1f);
		} else if (Input.GetKey (KeyCode.Keypad0)) {
			pilotHead.transform.Translate (Vector3.forward * 0.1f);
		} 

		if (Input.GetKeyUp (KeyCode.KeypadEnter))
			ResetPos ();


		if (Input.GetKey (KeyCode.Keypad7)) {

			headLeanAngle = 23f;

		} else if (Input.GetKey (KeyCode.Keypad9)) {

			headLeanAngle = -23f;
		} else {

			headLeanAngle = 0f;
		}

		if (Input.GetKey (KeyCode.Keypad8)) {

			targetRotX = 20f;

		}
		else if(Input.GetKey (KeyCode.Keypad5)) {

			targetRotX = -90f;

		}

		else {
			targetRotX = defaultAngle;
		}

		if (Input.GetKey (KeyCode.Keypad6)) {

			targetRot = 90f;



		} else if (Input.GetKey (KeyCode.Keypad4)) {

			targetRot = 270f;	


		}
		else if (Input.GetKey (KeyCode.Keypad4) && Input.GetKey (KeyCode.Keypad8) ) {

			targetRot = 315f;	


		} 
		else if (Input.GetKey (KeyCode.Keypad6) && Input.GetKey (KeyCode.Keypad8)) {

			targetRot = 45f;	


		} else if (Input.GetKey (KeyCode.Keypad2)) {

			targetRot = 180f;



		} else {

			targetRot = 0f;


		}


		curRot = transform.localEulerAngles;
		transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (targetRotX, targetRot, transform.localRotation.z), Time.deltaTime * 5f);
		pilotHeadPivot.transform.localRotation = Quaternion.Lerp (pilotHeadPivot.transform.localRotation, Quaternion.Euler (pilotHeadPivot.localEulerAngles.x, pilotHeadPivot.localEulerAngles.y, headLeanAngle), Time.deltaTime * 5f);
		//Vector3 newPos = new Vector3( pilotHead.transform.localPosition.x + (headLeanAngle * -0.01f), pilotHead.transform.localPosition.y,pilotHead.transform.localPosition.z);
		//pilotHead.transform.localPosition = Vector3.Lerp (pilotHead.transform.localPosition, newPos, Time.deltaTime * 5f);
	}

	public void ResetPos (){
		if (!enabled)
			return;
		transform.localPosition = initialPos;
		pilotHead.localPosition = headInitialPos;
	}
}
