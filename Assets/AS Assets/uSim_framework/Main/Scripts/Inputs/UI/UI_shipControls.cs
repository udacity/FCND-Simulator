using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_shipControls : MonoBehaviour {

	public ShipController shipController;
	public Slider rudderSlider;


	void Update () {

		shipController.steeringInput = rudderSlider.value;

	}

}
