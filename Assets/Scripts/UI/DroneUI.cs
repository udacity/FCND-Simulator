using UnityEngine;
using UnityEngine.UI;
using DroneInterface;
using Drones;
using FlightUtils;
using TMPText = TMPro.TextMeshProUGUI;

// NOTE: The minimap is currently disabled since
// we don't use it for anything.

public class DroneUI : MonoBehaviour
{

    public TMPText gpsText;
    public Image needleImage;
	public Image windArrow;
    public Button armButton;
    public Button guideButton;
	public ToggleGroup qualityGroup;
	Toggle[] toggles;

	public bool localizeWind;

    private IDrone drone;
	ButtonStateWatcher armWatcher;
	ButtonStateWatcher guideWatcher;
    // public Image minimapImage;
    // public Camera minimapCamera;
    // private float initialCameraY;

    // // Need this to reference the previous used to render
    // // the last minimap frame in the UI.
    // private Texture2D tex = null;


    void Awake()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
        // initialCameraY = minimapCamera.transform.position.y;
        // UpdateMinimapCameraPosition();
		int quality = QualitySettings.GetQualityLevel ();
		toggles = qualityGroup.transform.GetComponentsInChildren<Toggle> ();
		for ( int i = 0; i < toggles.Length; i++ )
			toggles [ i ].isOn = ( i == quality );
		qualityGroup.NotifyToggleOn ( toggles [ quality ] );
		armWatcher = armButton.GetComponent<ButtonStateWatcher> ();
		guideWatcher = guideButton.GetComponent<ButtonStateWatcher> ();
    }

    // Might be able to move this over to `LateUpdate`.
    void Update()
    {
        UpdateArmedButton();
        UpdateGuidedButton();
    }

    void LateUpdate()
    {
        // Updates UI drone position
        var lat = drone.Latitude();
        var lon = drone.Longitude();
        var alt = drone.Altitude();
        gpsText.text = string.Format("Latitude = {0:0.000000}\nLongitude = {1:0.000000}\nAltitude = {2:0.000} (meters)", lat, lon, alt);
        // _gpsText.color = new Color(255, 255, 255, 0);

        // Updates UI compass drone heading
        // North -> 0/360
        // East -> 90
        // South -> 180
        // West - 270
        var hdg = -(float)drone.Yaw();
//        var oldHdg = needleImage.rectTransform.rotation.eulerAngles.z;
        // rotate the needle by the yaw difference
//        needleImage.rectTransform.Rotate(0, 0, -(-hdg - -oldHdg));
		needleImage.rectTransform.eulerAngles = Vector3.forward * hdg;

		// update wind direction arrow
		if ( WindDisturbance.Enabled )
		{
			windArrow.enabled = true;
			float angle = -WindDisturbance.Angle () + 90;
			if ( localizeWind )
				angle -= hdg;
//			float angle = ( -WindDisturbance.Angle () + 90 - hdg );
			windArrow.rectTransform.eulerAngles = Vector3.forward * angle;
			Vector2 size = windArrow.rectTransform.sizeDelta;
			size.x = size.y * 0.5f + size.y * WindDisturbance.StrengthPercent ();
			windArrow.rectTransform.sizeDelta = size;
			float radAngle = angle * Mathf.Deg2Rad;
			Vector2 anchor = new Vector2 ( 0.5f + Mathf.Cos ( radAngle ) * 0.15f, 0.5f + Mathf.Sin ( radAngle ) * 0.15f );
//			Vector2 anchor = new Vector2 ( 0.5f + Mathf.Cos ( radAngle ) * 0.55f, 0.5f + Mathf.Sin ( radAngle ) * 0.55f );
			windArrow.rectTransform.anchorMin = windArrow.rectTransform.anchorMax = anchor;

		} else
			windArrow.enabled = false;

        // Update minimap UI
        // UpdateMinimapCameraPosition();
        // // Renders a new camera image on the minimap
        // var c = minimapCamera;
        // // NOTE: I'm not sure why we need to use Screen.width and Screen.height here
        // // instead of the dimensions of the camera.
        // //
        // // Dividing the initial resolution to save memory.
        // var w = (int)Screen.width / 3;
        // var h = (int)Screen.height / 3;
        // var rt = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32);
        // c.targetTexture = rt;
        // c.Render();
        // RenderTexture.active = rt;

        // // Destroy the previous texture, otherwise this becomes a memory leak
        // if (tex != null)
        // {
        //     Object.Destroy(tex);
        // }

        // tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        // tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        // tex.Apply();
        // minimapImage.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(.0f, .0f));

        // // Cleanup
        // c.targetTexture = null;
        // RenderTexture.active = null;
        // rt.Release();
    }

	// When the minimap is clicked, the point "birds-eye" is converted to the in game
	// 3D point, "world point".
	//
	// TODO: Use the world point for things, i.e. move the drone to that point.

	// Updates the minimap camera position to the new location of the drone.
	// void UpdateMinimapCameraPosition()
	// {
	//     var pos = drone.UnityCoords();
	//     minimapCamera.transform.position = new Vector3(pos.x, pos.y + initialCameraY, pos.z);
	// }

	// public void MinimapOnClick()
	// {
	//     var c = minimapCamera;
	//     var rt = minimapImage.GetComponent<RectTransform>();
	//     var x = ((Input.mousePosition.x - (Screen.width - rt.rect.width)) / rt.rect.width) * Screen.width;
	//     var y = Input.mousePosition.y / rt.rect.height * Screen.height;
	//     var wp = c.ScreenToWorldPoint(new Vector3(x, y, initialCameraY));
	//     Debug.Log("world point " + wp);
	// }

	// Toggles whether the drone is armed or disarmed.
	public void ArmButtonOnClick()
	{
		drone.Arm(!drone.Armed());
	}

	// Toggles whether the drone is guided (autonomously controlled) or unguided (manually controlled).
	public void GuideButtonOnClick()
	{
		drone.TakeControl(!drone.Guided());
	}

	void UpdateArmedButton()
	{
		var v = drone.Armed();
		if ( v != ( armWatcher.CurrentState == 1 ) )
			armWatcher.OnClick ();
	}

	void UpdateGuidedButton()
	{
		var v = drone.Guided();
		if ( v != ( guideWatcher.CurrentState == 1 ) )
			guideWatcher.OnClick ();
	}

	public void OnQualityToggle (int toggle)
	{
		if ( toggles [ toggle ].isOn && toggle != QualitySettings.GetQualityLevel () )
			QualitySettings.SetQualityLevel ( toggle );
	}
}