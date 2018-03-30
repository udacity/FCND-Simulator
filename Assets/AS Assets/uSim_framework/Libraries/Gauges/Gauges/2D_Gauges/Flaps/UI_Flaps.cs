using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Flaps : MonoBehaviour {

	public Slider slider;
	public ControlAnimator inputs;
	
	
	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider> ();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (inputs == null)
			return;

		float maxFlapAnlges = inputs.flapangles [inputs.flapangles.Length - 1];
		slider.value = Mathf.Lerp (1f,0f, inputs.flapangle / maxFlapAnlges);
		
	}

}
