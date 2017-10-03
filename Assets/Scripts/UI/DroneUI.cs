using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneUI : MonoBehaviour {
	public Text _gpsText;
	public Image _needleImage;
	public Image _minimapImage;
	private QuadController _quadController;

	void Awake () {
		_quadController = GameObject.Find("Quad Drone").GetComponent<QuadController>();
	}

	void LateUpdate () {
        var lat = _quadController.getLatitude();
        var lon = _quadController.getLongitude();
        var alt = _quadController.getAltitude();
		_gpsText.text = string.Format("Latitude = {0:0.000}\nLongitude = {1:0.000}\nAltitude = {2:0.000} (meters)", lat, lon, alt);

        // North -> 0/360
        // East -> 90
        // South -> 180
        // West - 270
        var hdg = _quadController.getYaw();
        var oldHdg = _needleImage.rectTransform.rotation.eulerAngles.z;
        // rotate the needle by the yaw difference
        _needleImage.rectTransform.Rotate(0, 0, -(-hdg - -oldHdg));
	}
}