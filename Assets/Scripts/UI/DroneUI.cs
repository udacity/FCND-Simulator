using UnityEngine;
using UnityEngine.UI;
using DroneInterface;
using Drones;
using FlightUtils;

// NOTE: The minimap is currently disabled since
// we don't use it for anything.

public class DroneUI : MonoBehaviour
{

    public TMPro.TextMeshProUGUI gpsText;
    public Image needleImage;
    public Button armButton;
    public Button guideButton;
    private IDrone drone;
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
        if (v)
        {
            armButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Armed";
        }
        else
        {
            armButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Disarmed";
        }
    }

    void UpdateGuidedButton()
    {
        var v = drone.Guided();
        if (v)
        {
            guideButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Guided";
        }
        else
        {
            guideButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Manual";
        }
    }

    // Might be able to move this over to `LateUpdate`.
    void FixedUpdate()
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
        var oldHdg = needleImage.rectTransform.rotation.eulerAngles.z;
        // rotate the needle by the yaw difference
        needleImage.rectTransform.Rotate(0, 0, -(-hdg - -oldHdg));

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
}