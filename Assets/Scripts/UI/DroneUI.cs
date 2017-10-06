using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneUI : MonoBehaviour {
	public Text _gpsText;
	public Image _needleImage;
	public Image _minimapImage;
    public Camera _minimapCamera;
	private QuadController _quadController;
    private GameObject _droneObj;


	void Awake () {
        _droneObj = GameObject.Find("Quad Drone");
		_quadController = _droneObj.GetComponent<QuadController>();
        UpdateMinimapCamPosition();
	}

    void UpdateMinimapCamPosition() {
        var quadPos = _droneObj.transform.position;
        _minimapCamera.transform.position = new Vector3(quadPos.x, quadPos.y + 30, quadPos.z);
    }

	void LateUpdate () {
        var lat = _quadController.getLatitude();
        var lon = _quadController.getLongitude();
        var alt = _quadController.getAltitude();
		_gpsText.text = string.Format("Latitude = {0:0.000}\nLongitude = {1:0.000}\nAltitude = {2:0.000} (meters)", lat, lon, alt);
        // _gpsText.color = new Color(255, 255, 255, 0);

        // North -> 0/360
        // East -> 90
        // South -> 180
        // West - 270
        var hdg = _quadController.getYaw();
        var oldHdg = _needleImage.rectTransform.rotation.eulerAngles.z;
        // rotate the needle by the yaw difference
        _needleImage.rectTransform.Rotate(0, 0, -(-hdg - -oldHdg));

        // update minimap cam
        UpdateMinimapCamPosition();

        // display above sky camera frame on minimap
        var targetTexture = _minimapCamera.targetTexture;
        RenderTexture.active = targetTexture;
        Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
        texture2D.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        texture2D.Apply();
        var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        _minimapImage.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        // Object.Destroy(texture2D);
        // Object.Destroy(sprite);
	}
}