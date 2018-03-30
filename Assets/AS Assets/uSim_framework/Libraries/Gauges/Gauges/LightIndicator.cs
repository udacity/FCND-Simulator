using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class LightIndicator : MonoBehaviour {

	public GameObject onState;
	public GameObject vehicle;
	public GameObject targetObject;
	public string targetObjectName;
	public string parameterName;
	public bool triggerByValue;
	public bool offAtValue;
	public bool triggerOnExceed;
	public float offValue;
	public float triggerAtValue;
	public float triggerValue;
	public bool triggerByFlag;
	bool triggerFlag;

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
		GetTgtParameter ();
	}

	void GetTargetObject (){

		targetObject = vehicle.transform.Find (targetObjectName).gameObject;

	}

	void GetClasses () {

		classesAttached = (Component[]) targetObject.GetComponents(typeof(Component));

	}

	void GetTgtParameter () {

		foreach (Component c in classesAttached) {

			foreach (FieldInfo fi in c.GetType().GetFields()) {
				if (fi.Name == parameterName) {
					targetClass = c;
					System.Object obj = (System.Object)c;
					if (triggerByValue)
						triggerValue = (float)fi.GetValue (obj);

				}
			}
		}
	}



	public void SetVehicle (GameObject veh){

		vehicle = veh;

	}

	
	// Update is called once per frame
	void LateUpdate () {
		if (vehicle == null)
			return;
		GetTgtParameter ();

		onState.SetActive (false);

		if (triggerValue < triggerAtValue && !triggerOnExceed) {
			onState.SetActive (true);
		} else if (triggerValue > triggerAtValue && triggerOnExceed) {
			onState.SetActive (true);
		}
		if (offAtValue && triggerValue > offValue && triggerOnExceed)
			onState.SetActive (false);
	}
}
