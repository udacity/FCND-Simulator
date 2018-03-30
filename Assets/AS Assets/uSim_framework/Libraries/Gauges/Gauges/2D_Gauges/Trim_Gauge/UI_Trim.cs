using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Trim : MonoBehaviour {
	public Slider slider;
	public InputsManager inputs;

	
	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider> ();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (inputs == null)
			return;
		
		slider.value = 0.5f + inputs.trim;
		
	}


}
