using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_inputs : MonoBehaviour {

	public GameObject inputObj;
	public UI_buttonController rightButton;
	public UI_buttonController leftButton;
	public UI_buttonController upButton;
	public UI_buttonController downButton;
	public UI_buttonController jumpButton;
	public UI_buttonController action1Button;
	bool jump = false;
	bool action1 = false;
	bool buttonPressed;
	// Use this for initialization
	void Start () {
		jump = false;
	}
	
	// Update is called once per frame
	void Update () {
	
		SetInputs ();

	}
	bool canJump;
	public void SetInputs (){

		inputObj.SendMessage("SetHorizontalInput", (rightButton.inputValue + leftButton.inputValue));
		inputObj.SendMessage("SetVerticalInput", (upButton.inputValue + downButton.inputValue));


		if (action1Button.inputValue > 0f && !buttonPressed) {

			buttonPressed = true;
			StartCoroutine (SendAction1());

		} else if (action1Button.inputValue == 0f) {
			action1 = false;
			buttonPressed = false;
		}
	}


	IEnumerator SendAction1 (){

		action1 = true;		
		inputObj.SendMessage("SetAction1Input", (action1));
		yield return new WaitForEndOfFrame ();
		action1 = false;		
		inputObj.SendMessage("SetAction1Input", (action1));
	}
}
