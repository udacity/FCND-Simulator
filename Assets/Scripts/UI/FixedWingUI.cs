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

	// event for selection between Tuning mode and Python mode
	public void OnSelectOperationMode (int mode)
	{
		// 0 being tuning
//		if ( mode == 0 )
//			scenarioManager
		startObject.SetActive ( false );
		// temp: start the scenario
		scenarioStartTime = Time.time;
		scenarioManager.Begin ();
	}

	public void OnScenarioLoaded (Scenario s)
	{
		startTitle.text = s.data.title;
		startDescription.text = s.data.description;
		scenarioRuntime = s.data.runtime;
		if ( scenarioRuntime == -1 )
			scenarioRuntime = Mathf.Infinity;
		runtimeFill.fillAmount = 0;
		runtimeObject.SetActive ( true );
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
}