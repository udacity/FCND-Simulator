using UnityEngine;
using System.Collections;

public class Panel2D : MonoBehaviour {

	public bool use2Dpanel;
	public bool detachPanel;
	public PilotHead pilotHead;
	public GameObject panel;
	UsimVehicle objectvehicle;

	IEnumerator Start () {

		if (use2Dpanel) {
		
			do {
				yield return new WaitForSeconds (1);
			} while (objectvehicle == null);

			pilotHead = objectvehicle.GetComponentInChildren<PilotHead> ();


		}
			
		if (detachPanel) {

			transform.parent.parent = null;

		}
	}

	public void SetAircraftGaugesData (UsimVehicle vehicle){

		objectvehicle = vehicle;
		ScreenGauge[] gauges = GetComponentsInChildren<ScreenGauge> () as ScreenGauge[];

		foreach (ScreenGauge gauge in gauges) {
			
			gauge.aircraft = vehicle.gameObject;		
			//gauge.engine = vehicle.engines.engines [0];
			gauge.fuelManager = vehicle.fuel;

		}

		UI_Throttle[] throttles = GetComponentsInChildren<UI_Throttle> () as UI_Throttle[];

		foreach (UI_Throttle throttle in throttles) {

			throttle.enginesManager = vehicle.engines;

		}
		if(GetComponentInChildren<UI_Trim> () != null)
		GetComponentInChildren<UI_Trim> ().inputs = vehicle.GetComponent<InputsManager> ();
		if(GetComponentInChildren<UI_Flaps> () != null)
		GetComponentInChildren<UI_Flaps> ().inputs = vehicle.GetComponent<ControlAnimator> ();
	
		LightIndicator[] lightIndicators = GetComponentsInChildren<LightIndicator> () as LightIndicator[];

		foreach (LightIndicator light in lightIndicators) {

			light.SetVehicle (vehicle.gameObject);			

		}

		CustomPanelButton[] customButtons = GetComponentsInChildren<CustomPanelButton> () as CustomPanelButton[];

		foreach (CustomPanelButton button in customButtons) {

			button.SetVehicle (vehicle.gameObject);			

		}

	}

	void Update () {

		if (!use2Dpanel)
			return;

		Vector3 panelPos = panel.transform.position;

		if (Input.GetKeyUp (KeyCode.PageUp)) 
			panelPos.y += 2f ;

		if (Input.GetKeyUp (KeyCode.PageDown))
			panelPos.y -= 2f ;

		panel.transform.position = panelPos;

		if (pilotHead == null)
			return;

		TogglePanel (pilotHead.enabled);

		if (pilotHead.targetRotX == pilotHead.defaultAngle && pilotHead.targetRot == 0f && pilotHead.enabled)
			TogglePanel (true);
		else
			TogglePanel (false);



	}

	void TogglePanel (bool toggle){

		panel.SetActive (toggle);

	}

}
