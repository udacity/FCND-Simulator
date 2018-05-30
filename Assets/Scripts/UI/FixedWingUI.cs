using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DroneInterface;
using UdacityNetworking;

public class FixedWingUI : MonoBehaviour
{
	// network controller
	public NetworkController networkController;

	// UI variables
	public TMP_Dropdown scenarioDropdown;

	// scenario start panel
	public GameObject startObject;
	public TMP_Text startTitle;
	public TMP_Text startDescription;

	// scenario waiting for python panel
	public GameObject pythonObject;
//	public TMP_Text 

	// scenario tuning panel
	public GameObject tuningObject;


	// scenario success panel
	public GameObject successObject;
	public TMP_Text successDescription;

	// scenaril failure panel
	public GameObject failObject;
	public TMP_Text failDescription;

	// runtime progress bar
	public GameObject runtimeObject;
	public TMP_Text runtimeText;
	public Image runtimeFill;

	// other variables
	public ScenarioManager scenarioManager;

	float scenarioStartTime;
	float scenarioRuntime;
	IDrone drone;
	bool waitingForPython;


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
		drone = Simulation.ActiveDrone;
		networkController = NetworkController.instance;
		networkController.AddConnectionEvent ( new System.Action<ConnectionState> ( OnConnectionStateChanged ) );
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

		if ( waitingForPython )
		{
			if ( drone.MotorsArmed () )
			{
                scenarioStartTime = Time.time;
                waitingForPython = false;
				pythonObject.SetActive ( false );
				scenarioManager.Begin ();
			}
		}
	}


	void ClearPanels ()
	{
		startObject.SetActive ( false );
		successObject.SetActive ( false );
		failObject.SetActive ( false );
		pythonObject.SetActive ( false );
		tuningObject.SetActive ( false );
		waitingForPython = false;
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
		ClearPanels ();
		OnSelectOperationMode ( button );
	}

	// event for doing things in the "waiting for Python" panel
	public void OnPythonButton (int button)
	{
		// 0 is cancel
		if ( button == 0 )
		{
			networkController.StopServer ();
			ClearPanels ();
			startObject.SetActive ( true );
		}
	}

	// event for doing things in the Tuning window
	public void OnTuningButton (int button)
	{
		ClearPanels ();

		// 0 is cancel
		if ( button == 0 )
		{
            networkController.StopServer();
            startObject.SetActive ( true );
		}

		if ( button == 1 )
		{
			
		}
	}

	// event for pressing a button in the Success window
	public void OnSuccessButton (int button)
	{
		ClearPanels ();
	}

	// event for pressing a button in the Failure window
	public void OnFailureButton (int button)
	{
		ClearPanels ();
		scenarioManager.DoReset ();
	}

	public void OnSelectOperationMode (int mode)
	{
		if ( mode == 0 )
		{
			// temp: start the scenario
			scenarioStartTime = Time.time;
			scenarioManager.Begin ();
		}

		if ( mode == 1 )
		{
			waitingForPython = true;
			pythonObject.SetActive ( true );
			networkController.StartServer ();
		}
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
        runtimeObject.SetActive(false);
		//networkController.StopServer ();
	}

	void OnScenarioFailed (Scenario s)
	{
		failDescription.text = s.data.failText;
		failObject.SetActive ( true );
		runtimeObject.SetActive ( false );
		//networkController.StopServer ();
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

	void OnConnectionStateChanged (ConnectionState newState)
	{
		Debug.Log ( "Network connection state: " + newState );
		if ( newState == ConnectionState.Disconnected )
		{
			if ( drone.MotorsArmed () )
			{
				Debug.Log ( "Disarming from disconnection" );
				drone.ArmDisarm ( false );
			}
		}
	}
}