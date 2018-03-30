using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Reflection;

public class CustomPanelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public enum ButtonActionModes {Bool, FixedValue, DynamicValue};
	public enum ButtonModes {PushButton, SwitchButton};
	public ButtonModes buttonMode;
	public ButtonActionModes buttonActionMode;
	public Button uiButton;
	public Sprite buttonUp;
	public Sprite buttonDown;
	public GameObject vehicle;
	public GameObject targetObject;
	public string targetObjectName;
	public string parameterName;
	public float dynamicFlow;
	public float targetValue;
	public bool invertBool;
	float dynamicValue = 0f;

	//button state
	public bool buttonDepressed;

	public Component[] classesAttached;
	public Component targetClass;
	public PropertyInfo targetInfo;

	// Use this for initialization
	IEnumerator Start () {

		do{
			yield return new WaitForSeconds(1);
		}while(vehicle == null);
		GetTargetObject ();
		GetClasses ();
	}
	
	// Update is called once per frame
	void LateUpdate () {
	
	

			switch (buttonActionMode) {

		case ButtonActionModes.Bool:

			if(!invertBool)
				SetTgtParameter (buttonDepressed);
			else
				SetTgtParameter (!buttonDepressed);

				break;

		case ButtonActionModes.DynamicValue:

			GetTgtParameter ();
			dynamicValue += dynamicFlow;
			SetTgtParameter (dynamicValue);

				break;

			case ButtonActionModes.FixedValue:

			SetTgtParameter (targetValue);

				break;
			}

	}

	public void OnPointerDown(PointerEventData eventData) {
		if (buttonMode == ButtonModes.PushButton) {
			buttonDepressed = true;
			return;
		}
		if (buttonDepressed && buttonMode == ButtonModes.SwitchButton) {
			buttonDepressed = false;
			return;
		}
		if (!buttonDepressed && buttonMode == ButtonModes.SwitchButton) {
			buttonDepressed = true;
			return;
		}
	}

	public void OnPointerUp(PointerEventData eventData) {
		if(buttonMode == ButtonModes.PushButton)
			buttonDepressed = false;
		
	}

	void GetTargetObject (){

		targetObject = vehicle.transform.Find (targetObjectName).gameObject;

	}

	void GetClasses () {

		classesAttached = (Component[]) targetObject.GetComponents(typeof(Component));

	}

	void SetTgtParameter (float set) {

		foreach (Component c in classesAttached) {

			foreach (FieldInfo fi in c.GetType().GetFields()) {
				if (fi.Name == parameterName) {
					targetClass = c;
					System.Object obj = (System.Object)c;
					fi.SetValue (obj,(float) set);

				}
			}
		}
	}

	void SetTgtParameter (bool set) {

		foreach (Component c in classesAttached) {

			foreach (FieldInfo fi in c.GetType().GetFields()) {
				if (fi.Name == parameterName) {
					targetClass = c;
					System.Object obj = (System.Object)c;
					fi.SetValue (obj,(bool) set);

				}
			}
		}
	}

	void GetTgtParameter () {

		foreach (Component c in classesAttached) {

			foreach (FieldInfo fi in c.GetType().GetFields()) {
				if (fi.Name == parameterName) {
					targetClass = c;
					System.Object obj = (System.Object)c;
					dynamicValue = (float)fi.GetValue (obj);

				}
			}
		}
	}

	public void SetVehicle (GameObject veh){

		vehicle = veh;

	}

}
