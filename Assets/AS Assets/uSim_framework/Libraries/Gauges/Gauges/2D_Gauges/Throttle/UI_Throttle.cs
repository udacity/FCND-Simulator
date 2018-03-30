using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Throttle : MonoBehaviour {

	public Slider slider;
	public EnginesManager enginesManager;
	public float throttleInput;
	public bool useSliderValue;
	public bool useSelectorIndex;
	public int selectorIndex;


	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider> ();
		if (enginesManager == null) {
			Debug.LogError ("Engines manager has not been set. Select one from the aircraft hierarchy");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		if (enginesManager == null)
			return;
		if (useSliderValue) {



			if (useSelectorIndex)
				enginesManager.SetEngineThrottle(selectorIndex, slider.value);
			else
				enginesManager.throttleInput = slider.value;
			
				

		} else {

			if (useSelectorIndex)
				slider.value = enginesManager.engines [selectorIndex].throttle;
			else
				slider.value = enginesManager.throttle;
		}

	}
}
