using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixedWingUI : MonoBehaviour
{
	// UI variables
	public TMP_Dropdown scenarioDropdown;
	public GameObject startObject;
	public TMP_Text startTitle;
	public TMP_Text startDescription;
	public GameObject successObject;
	public TMP_Text successDescription;
	public GameObject failObject;
	public TMP_Text failDescription;
	public GameObject runtimeObject;
	public TMP_Text runtimeText;
	public Image runtimeFill;

	// other variables
	public ScenarioManager scenarioManager;

	float scenarioStartTime;
	float scenarioRuntime;


	void Awake ()
	{
		startObject.SetActive ( false );
		successObject.SetActive ( false );
		failObject.SetActive ( false );
		runtimeObject.SetActive ( false );
	}

	void Start ()
	{
		if ( scenarioManager == null )
			scenarioManager = ScenarioManager.Instance;
		scenarioManager.onScenarioLoaded += OnScenarioLoaded;
		scenarioManager.onScenarioSucceeded += OnScenarioSucceeded;
		scenarioManager.onScenarioFailed += OnScenarioFailed;
		scenarioDropdown.value = -1;
	}

	void Update ()
	{
		if ( scenarioManager.CurrentScenario != null && scenarioManager.CurrentScenario.IsRunning )
		{
			float t = Time.time - scenarioStartTime;
			if ( t > scenarioRuntime )
				t = scenarioRuntime;
			runtimeFill.fillAmount = t / scenarioRuntime;
			runtimeText.text = "Runtime: " + t.ToString ( "F2" ) + "s";
		}
	}



	// event for scenario dropdown
	public void OnScenarioSelected (int id)
	{
		Debug.Log ( "Selected scenario: " + id );
		if ( id > 0 )
		{
			id--;
			scenarioManager.SelectScenario ( id );
		}
	}

	// event for selection between Tuning mode and Python mode from the start prompt
	public void OnStartButton (int button)
	{
		startObject.SetActive ( false );
		OnSelectOperationMode ( button );
	}

	// event for doing things in the Tuning window
	public void OnTuningButton (int button)
	{
		
	}

	// event for pressing a button in the Success window
	public void OnSuccessButton (int button)
	{
		successObject.SetActive ( false );
	}

	// event for pressing a button in the Failure window
	public void OnFailureButton (int button)
	{
		failObject.SetActive ( false );
		scenarioManager.DoReset ();
	}

	public void OnSelectOperationMode (int mode)
	{
		// temp: start the scenario
		scenarioStartTime = Time.time;
		scenarioManager.Begin ();
	}

	public void OnScenarioLoaded (Scenario s)
	{
		startTitle.text = s.data.title;
		string _runtime = s.data.runtime == Mathf.Infinity ?
			"indefinitely." :
			( (int) s.data.runtime ).ToString () + " seconds.";
		startDescription.text = s.data.description.Replace ( "^runtime", _runtime );
		SetRuntime ( s.data.runtime );
		startObject.SetActive ( true );
	}

	void OnScenarioSucceeded (Scenario s)
	{
		successDescription.text = s.data.successText;
		successObject.SetActive ( true );
		runtimeObject.SetActive ( false );
	}

	void OnScenarioFailed (Scenario s)
	{
		failDescription.text = s.data.failText;
		failObject.SetActive ( true );
		runtimeObject.SetActive ( false );
	}

	void SetRuntime (float runtime)
	{
		if ( runtime == -1 )
			scenarioRuntime = Mathf.Infinity;
		else
			scenarioRuntime = runtime;
		
		runtimeFill.fillAmount = 0;
		runtimeText.text = "Runtime: 0s";
		runtimeObject.SetActive ( true );
	}
}