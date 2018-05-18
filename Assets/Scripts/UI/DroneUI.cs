using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DroneInterface;
using Drones;
using FlightUtils;
using TMPText = TMPro.TextMeshProUGUI;

public class DroneUI : MonoBehaviour
{
	public Mapbox.Unity.Map.AbstractMap mapScript;
    public TMPText gpsText;
    public Image needleImage;
    public Image windArrow;
    public Button armButton;
    public Button guideButton;
    public ToggleGroup qualityGroup;
    Toggle[] toggles;
    public UIParameter parameterPrefab;
    public RectTransform parametersParent;
    public GameObject pauseText;
	public RawImage graphImage;

	public GameObject controlsOverlay;
	public GameObject parametersOverlay;
	public GameObject plotOverlay;

    public bool localizeWind;

    private IDrone drone;
    ButtonStateWatcher armWatcher;
    ButtonStateWatcher guideWatcher;
    List<UIParameter> uiParameters;

    void Awake()
	{
//        drone = GameObject.Find("Quad Drone").GetComponent<QuadDrone>();
//        int quality = QualitySettings.GetQualityLevel();
//        toggles = qualityGroup.transform.GetComponentsInChildren<Toggle>();
//        for (int i = 0; i < toggles.Length; i++)
//        {
//            toggles[i].isOn = (i == quality);
//        }
//        qualityGroup.NotifyToggleOn(toggles[quality]);
		armWatcher = armButton.GetComponent<ButtonStateWatcher> ();
		guideWatcher = guideButton.GetComponent<ButtonStateWatcher> ();
		pauseText.gameObject.SetActive ( false );
		Simulation.Observe ( OnSimulationPause );
		// mapScript isn't available in every scene, so make sure to only try this if it's assigned
		if ( mapScript != null )
			mapScript.OnInitialized += OnMapInitialized;
	}

    void Start ()
    {
		int quality = QualitySettings.GetQualityLevel();
		toggles = qualityGroup.transform.GetComponentsInChildren<Toggle>();
		for (int i = 0; i < toggles.Length; i++)
		{
			toggles[i].isOn = (i == quality);
		}
		qualityGroup.NotifyToggleOn(toggles[quality]);
		drone = Simulation.ActiveDrone;
        var parameters = SimParameters.Parameters;
        //		Debug.Log ( "there are " + parameters.Length + " parameters" );
        foreach (SimParameter p in parameters)
        {
            UIParameter up = Instantiate(parameterPrefab, parametersParent);
            //			up.transform.SetParent ( parametersParent );
            up.Init(p, false);
            up.gameObject.SetActive(true);
            //			Debug.Log ( "parameter is " + p.displayName );
        }
		graphImage.enabled = PlotViz.Count > 0;
    }

    void Update ()
    {
		if ( drone == null )
		{
			drone = Simulation.ActiveDrone;
			return;
		}
        UpdateArmedButton();
        UpdateGuidedButton();
    }

    void LateUpdate ()
    {
		if ( drone == null )
			return;

        // Updates UI drone position
        var lat = drone.GPSLatitude();
        var lon = drone.GPSLongitude();
        var alt = drone.GPSAltitude();
        var airspeed = drone.VelocityLocal().magnitude;
        gpsText.text = string.Format("Latitude = {0:0.000000}\nLongitude = {1:0.000000}\nAltitude = {2:0.000} (meters)\nAirspeed = {3:0.0} (meters/sec)", lat, lon, alt, airspeed);
        // _gpsText.color = new Color(255, 255, 255, 0);

        // Updates UI compass drone heading
        // North -> 0/360
        // East -> 90
        // South -> 180
        // West - 270
        var hdg = -(float)drone.AttitudeEuler().z * Mathf.Rad2Deg;
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

	void OnMapInitialized ()
	{
		var centerCoords = mapScript.CenterLatitudeLongitude;
		Simulation.latitude0 = centerCoords.x;
		Simulation.longitude0 = centerCoords.y;
		drone.SetHomePosition ( drone.GPSLongitude (), drone.GPSLatitude (), drone.GPSAltitude () );
	}

    // Toggles whether the drone is armed or disarmed.
    public void ArmButtonOnClick()
    {
        drone.ArmDisarm(!drone.MotorsArmed());
    }

    // Toggles whether the drone is guided (autonomously controlled) or unguided (manually controlled).
    public void GuideButtonOnClick()
    {
        drone.SetGuided(!drone.Guided());
    }

    void UpdateArmedButton()
    {
        var v = drone.MotorsArmed();
        if (v && !armWatcher.active)
        {
            armWatcher.OnClick();
        }
        else if (!v && armWatcher.active)
        {
            armWatcher.OnClick();
        }
    }

    void UpdateGuidedButton()
    {

        var v = drone.Guided();
        if (v && !guideWatcher.active)
        {
            guideWatcher.OnClick();
        }
        else if (!v && guideWatcher.active)
        {
            guideWatcher.OnClick();
        }
    }

    public void OnQualityToggle(int toggle)
    {
        if (toggles[toggle].isOn && toggle != QualitySettings.GetQualityLevel())
        {
            QualitySettings.SetQualityLevel(toggle);
        }
    }

	public void OnOpenResolution (bool open)
	{
		Simulation.UIIsOpen = open;
	}

	public void OpenControlsOverlay ()
	{
		controlsOverlay.SetActive ( true );
		parametersOverlay.SetActive ( false );
		plotOverlay.SetActive ( false );
		plotOverlay.GetComponent<PlottingUI> ().OnClose ();
		PauseSimulation ( true );
		Simulation.UIIsOpen = true;
	}

	public void OpenParametersOverlay ()
	{
		controlsOverlay.SetActive ( false );
		parametersOverlay.SetActive ( true );
		plotOverlay.SetActive ( false );
		plotOverlay.GetComponent<PlottingUI> ().OnClose ();
		graphImage.enabled = false;
		PauseSimulation ( true );
		Simulation.UIIsOpen = true;
	}

	public void OpenPlotOverlay ()
	{
		controlsOverlay.SetActive ( false );
		parametersOverlay.SetActive ( false );
		plotOverlay.SetActive ( true );
		graphImage.enabled = false;
		PauseSimulation ( true );
		Simulation.UIIsOpen = true;
	}

	public void CloseOverlay ()
	{
		controlsOverlay.SetActive ( false );
		parametersOverlay.SetActive ( false );
		plotOverlay.SetActive ( false );
		plotOverlay.GetComponent<PlottingUI> ().OnClose ();
		graphImage.enabled = PlotViz.Count > 0;
		PauseSimulation ( false );
		Simulation.UIIsOpen = false;
	}

    public void PauseSimulation(bool pause)
    {
        Simulation.Paused = pause;
    }

    void OnSimulationPause(bool paused)
    {
        pauseText.SetActive(paused);
    }
}