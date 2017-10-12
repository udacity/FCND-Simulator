using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// TODO(dom): Move these parts into separate files

public class DroneUI : MonoBehaviour {

    public TMPro.TextMeshProUGUI gpsText;
	public Image needleImage;
	public Image minimapImage;
    public Camera minimapCamera;

    public Button armButton;
    public Button guideButton;
	private QuadController quadController;
    private GameObject droneObj;
    private float minimapCameraY;



	void Awake () {
        droneObj = GameObject.Find("Quad Drone");
		quadController = droneObj.GetComponent<QuadController>();
        armButton.onClick.AddListener(ArmButtonOnClick);
        guideButton.onClick.AddListener(GuideButtonOnClick);
        minimapImage.GetComponent<Button>().onClick.AddListener(RenderMinimap);
        minimapCameraY = minimapCamera.transform.position.y;
        Debug.Log(minimapCameraY);
        UpdateMinimapCameraPosition();
	}

    void RenderMinimap() {
        var c = minimapCamera;
        var rt = minimapImage.GetComponent<RectTransform>();
        Debug.Log("Rect " + rt.rect);
        Debug.Log(string.Format("global x = {0}, y = {1}", Input.mousePosition.x, Input.mousePosition.y));
        var x = ((Input.mousePosition.x - (Screen.width - rt.rect.width)) / rt.rect.width) * c.targetTexture.width;
        var y = Input.mousePosition.y / rt.rect.height * c.targetTexture.height;
        Debug.Log(string.Format("x = {0}, y = {1}", (int) x, (int) y));
        var wp = c.ScreenToWorldPoint(new Vector3(x, y, minimapCameraY));
        Debug.Log("world point " + wp);
    }

    void UpdateMinimapCameraPosition() {
        var quadPos = droneObj.transform.position;
        minimapCamera.transform.position = new Vector3(quadPos.x, quadPos.y + minimapCameraY, quadPos.z);
    }

    void ArmButtonOnClick() {
		var _quadController = droneObj.GetComponent<QuadController>();
        if (_quadController.inputCtrl.motors_armed) {
            _quadController.inputCtrl.DisarmVehicle();
            armButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Disarmed";
        } else {
            _quadController.inputCtrl.ArmVehicle();
            armButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Armed";
        }
    }

    void GuideButtonOnClick() {
		var _quadController = droneObj.GetComponent<QuadController>();
        if (_quadController.inputCtrl.guided) {
            _quadController.inputCtrl.SetGuidedMode(false);
            guideButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Manual";
        } else {
            _quadController.inputCtrl.SetGuidedMode(true);
            guideButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Autonomous";
        }
    }

	void LateUpdate () {
        var lat = quadController.getLatitude();
        var lon = quadController.getLongitude();
        var alt = quadController.getAltitude();
		gpsText.text = string.Format("Latitude = {0:0.000}\nLongitude = {1:0.000}\nAltitude = {2:0.000} (meters)", lat, lon, alt);
        // _gpsText.color = new Color(255, 255, 255, 0);

        // North -> 0/360
        // East -> 90
        // South -> 180
        // West - 270
        var hdg = quadController.getYaw();
        var oldHdg = needleImage.rectTransform.rotation.eulerAngles.z;
        // rotate the needle by the yaw difference
        needleImage.rectTransform.Rotate(0, 0, -(-hdg - -oldHdg));

        // update minimap cam
        UpdateMinimapCameraPosition();

        RenderTexture.active = minimapCamera.targetTexture;
        var width = minimapCamera.targetTexture.width;
        var height = minimapCamera.targetTexture.height;

        Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        minimapImage.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

        // cleanup, doesn't quite work
        // RenderTexture.active = null;
        // Object.Destroy(texture2D);
	}
}