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
	public UITunable tunablePrefab;
	public UITunable[] tunables;
	public RectTransform tunableGridParent;

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

	// parameter visualization
	public UIVizParameter parameter1;
	public UIVizParameter parameter2;

	// other values
	public UIBarValue throttle;
	public UIBarValue elevator;
	public UIBarValue rudder;
	public UIBarValue aileron;

	// control mode text
	public TMP_Text controlModeText;

	// other variables
	public ScenarioManager scenarioManager;


	float scenarioStartTime;
	float scenarioRuntime;
	IDrone drone;
	bool waitingForPython;
	int curScenarioMode;


	void Awake ()
	{
		startObject.SetActive ( false );
		successObject.SetActive ( false );
		failObject.SetActive ( false );
		runtimeObject.SetActive ( false );

		Simulation.FixedWingUI = this;
		Simulation.DroneUI = GetComponent<DroneUI> ();
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
		SetControlModeText ( 2 );
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
                waitingForPython = false;
				pythonObject.SetActive ( false );
				scenarioManager.Begin ();
				scenarioStartTime = Time.time;
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
			ClearPanels ();
            startObject.SetActive ( true );
		}

		if ( button == 1 )
		{
			if ( tunables != null )
				tunables.ForEach ( x => x.ApplyValue () );
			
			scenarioStartTime = Time.time;
			scenarioManager.CurrentScenario.OnApplyTunableValues ();
			scenarioManager.Begin ();
		}
	}

	// event for pressing a button in the Success window
	public void OnSuccessButton (int button)
	{
		ClearPanels ();
		if ( button == 0 )
		{
			scenarioManager.SelectScenario ( scenarioDropdown.value - 1 );
//			scenarioManager.DoReset ();
//			SetRuntime ( scenarioManager.CurrentScenario.data.runtime );
			
		} else
		{
			scenarioDropdown.value = ++scenarioDropdown.value % scenarioDropdown.options.Count;
//			scenarioDropdown.value++;
			scenarioDropdown.RefreshShownValue ();
			scenarioManager.SelectScenario ( scenarioDropdown.value - 1 );
		}
	}

	// event for pressing a button in the Failure window
	public void OnFailureButton (int button)
	{
		ClearPanels ();
		scenarioManager.DoReset ();
		if ( button == 0 )
		{
			OnSelectOperationMode ( curScenarioMode );
//			tuningObject.SetActive ( true );
			SetRuntime ( scenarioManager.CurrentScenario.data.runtime );
		} else
		{
			
			scenarioManager.SelectScenario ( scenarioDropdown.value - 1 );
//			scenarioRuntime 
		}
	}

	// event for saving tuned parameters
	public void OnSaveParameterButton ()
	{
		if ( tunables != null )
			tunables.ForEach ( x => x.ApplyValue () );
		
		TunableManager.SaveGains ();
	}

	public void OnSelectOperationMode (int mode)
	{
		curScenarioMode = mode;
		if ( mode == 0 )
		{
			tuningObject.SetActive ( true );
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
		s.onParameter1Update = OnVizParameter1Update;
		s.onParameter2Update = OnVizParameter2Update;

		if ( s.vizParameters > 0 )
			parameter1.Set ( s.vizParameter1 );
		else
			parameter1.SetActive ( false );
		if ( s.vizParameters > 1 )
			parameter2.Set ( s.vizParameter2 );
		else
			parameter2.SetActive ( false );

		ClearPanels ();
		startTitle.text = s.data.title;
		string _runtime = s.data.runtime == Mathf.Infinity ?
			"indefinitely." :
			( (int) s.data.runtime ).ToString () + " seconds.";
		startDescription.text = s.data.description.Replace ( "^runtime", _runtime );
		SetRuntime ( s.data.runtime );
		startObject.SetActive ( true );
		// initialize the tunable stuff
		if (tunables != null && tunables.Length > 0)
		{
			tunables.ForEach ( x => Destroy ( x.gameObject ) );
			tunables = null;
		}

		if ( s.tunableParameters != null && s.tunableParameters.Length > 0 )
		{
			var list = new List<UITunable> ();
			var tp = s.tunableParameters;
			var parameters = TunableManager.Parameters;
			foreach ( var p in tp )
			{
				if ( string.IsNullOrWhiteSpace ( p ) )
				{
					Debug.LogError ( "empty tunable parameter name in " + s.name );
					continue;
				}

				var tunable = parameters.Find ( x => x.name.ToLower () == p.ToLower () );
				if ( tunable != null )
				{
					UITunable uit = Instantiate ( tunablePrefab, tunableGridParent );
					uit.Set ( tunable );
//					uit.Set ( tunable.name, tunable.value, tunable.minValue, tunable.maxValue, tunable );
					uit.gameObject.SetActive ( true );
					list.Add ( uit );
				}
			}
			tunables = list.ToArray ();
		}
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

	void OnVizParameter1Update (float value, int decimals = 1)
	{
		parameter1.UpdateValue ( value, decimals );
	}

	void OnVizParameter2Update (float value, int decimals = 1)
	{
		parameter2.UpdateValue ( value, decimals );
	}

	/// <summary>
	/// Sets the label for control mode:
	/// 0=vtol, 1=hybrid, 2=fixedwing
	/// </summary>
	public void SetControlModeText (int mode)
	{
		if ( controlModeText != null )
		{
			if ( mode == 0 )
				controlModeText.text = "VTOL";
			else
			if ( mode == 1 )
				controlModeText.text = "HYBRID";
			else
				controlModeText.text = "FIXED-WING";
		}
	}
}