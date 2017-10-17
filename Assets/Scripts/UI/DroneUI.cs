using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DroneControllers;

// TODO(dom): Move these parts into separate files

public class DroneUI : MonoBehaviour {

    public TMPro.TextMeshProUGUI gpsText;
	public Image needleImage;
	public Image minimapImage;
    public Camera minimapCamera;

    public Button armButton;
    public Button guideButton;
	private QuadController quadController;
    private float minimapCameraY;

    // Need this to reference the previous used to render
    // the last minimap frame in the UI.
    private Texture2D tex = null;

	void Awake () {
		quadController = GameObject.Find("Quad Drone").GetComponent<QuadController>();
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
        var x = ((Input.mousePosition.x - (Screen.width - rt.rect.width)) / rt.rect.width) * Screen.width;
        var y = Input.mousePosition.y / rt.rect.height * Screen.height;
        var wp = c.ScreenToWorldPoint(new Vector3(x, y, minimapCameraY));
        Debug.Log("world point " + wp);
    }

    void UpdateMinimapCameraPosition() {
        var quadPos = quadController.transform.position;
        minimapCamera.transform.position = new Vector3(quadPos.x, quadPos.y + minimapCameraY, quadPos.z);
    }

    void ArmButtonOnClick() {
        if (quadController.inputCtrl.motors_armed) {
            quadController.inputCtrl.DisarmVehicle();
            armButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Disarmed";
        } else {
            quadController.inputCtrl.ArmVehicle();
            armButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Armed";
        }
    }

    void GuideButtonOnClick() {
        if (quadController.inputCtrl.guided) {
            quadController.inputCtrl.SetGuidedMode(false);
            guideButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Manual";
        } else {
            quadController.inputCtrl.SetGuidedMode(true);
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

        var c = minimapCamera;
        // NOTE: I'm not sure why we need to use Screen.width and Screen.height here
        // instead of the dimensions of the camera.
        //
        // Dividing the initial resolution to save memory.
        var w = (int) Screen.width / 3;
        var h = (int) Screen.height / 3;
        var rt = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32);
        c.targetTexture = rt;
        c.Render();
        RenderTexture.active = rt;

        // Destroy the previous texture, otherwise this becomes a memory leak
        if (tex != null) {
            Object.Destroy(tex);
        }

        tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        minimapImage.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.0f, .0f));

        // Cleanup
        c.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
	}
}