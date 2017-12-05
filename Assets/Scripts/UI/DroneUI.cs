using UnityEngine;
using UnityEngine.UI;
using DroneInterface;
using Drones;
using FlightUtils;
using TMPText = TMPro.TextMeshProUGUI;

public class DroneUI : MonoBehaviour
{

    public TMPText gpsText;
    public Image needleImage;
    public Image windArrow;
    public Button armButton;
    public Button guideButton;

    public bool localizeWind;

    private IDrone drone;

    void Awake()
    {
        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
    }

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
            //            armButton.GetComponentInChildren<TMPText>().text = "Armed";
        }
        else
        {
            //            armButton.GetComponentInChildren<TMPText>().text = "Disarmed";
        }
    }

    void UpdateGuidedButton()
    {
        var v = drone.Guided();
        if (v)
        {
            //            guideButton.GetComponentInChildren<TMPText>().text = "Guided";
        }
        else
        {
            //            guideButton.GetComponentInChildren<TMPText>().text = "Manual";
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
        //        var oldHdg = needleImage.rectTransform.rotation.eulerAngles.z;
        // rotate the needle by the yaw difference
        //        needleImage.rectTransform.Rotate(0, 0, -(-hdg - -oldHdg));
        needleImage.rectTransform.eulerAngles = Vector3.forward * hdg;

        // update wind direction arrow
        if (WindDisturbance.Enabled)
        {
            windArrow.enabled = true;
            float angle = -WindDisturbance.Angle() + 90;
            if (localizeWind)
                angle -= hdg;
            //			float angle = ( -WindDisturbance.Angle () + 90 - hdg );
            windArrow.rectTransform.eulerAngles = Vector3.forward * angle;
            Vector2 size = windArrow.rectTransform.sizeDelta;
            size.x = size.y * 0.5f + size.y * WindDisturbance.StrengthPercent();
            windArrow.rectTransform.sizeDelta = size;
            float radAngle = angle * Mathf.Deg2Rad;
            Vector2 anchor = new Vector2(0.5f + Mathf.Cos(radAngle) * 0.15f, 0.5f + Mathf.Sin(radAngle) * 0.15f);
            //			Vector2 anchor = new Vector2 ( 0.5f + Mathf.Cos ( radAngle ) * 0.55f, 0.5f + Mathf.Sin ( radAngle ) * 0.55f );
            windArrow.rectTransform.anchorMin = windArrow.rectTransform.anchorMax = anchor;

        }
        else
        {
            windArrow.enabled = false;
        }


    }
}