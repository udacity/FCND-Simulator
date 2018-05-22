using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;

public class ScenarioManager : MonoBehaviour
{
	// statics
	public static ScenarioManager Instance { get { return instance; } }
	static ScenarioManager instance;

	// properties
	public Scenario CurrentScenario { get { return curScenario; } }

	// events
	public event System.Action<Scenario> onScenarioLoaded = delegate {};
	public event System.Action<Scenario> onScenarioSucceeded = delegate {};
	public event System.Action<Scenario> onScenarioFailed = delegate {};

	public Scenario[] scenarios;
	public IDrone drone;


	Scenario curScenario;
	float startTime;
	float runtime;

	void Awake ()
	{
		if ( instance != null && instance != this )
		{
			Destroy ( gameObject );
			return;
		}

		instance = this;
	}

	void Start ()
	{
		drone = Simulation.ActiveDrone;
	}

	void Update ()
	{
		if ( curScenario != null && curScenario.IsRunning )
		{
			// check for success (can a scenario even succeed before end of runtime?)
			if ( curScenario.data.canSucceedBeforeRuntime && curScenario.CheckSuccess () )
			{
				End ();
				onScenarioSucceeded ( curScenario );
				return;
			}

			// check for failure (can it fail before end of runtime?)
			if ( curScenario.data.canFailBeforeRuntime && curScenario.CheckFailure () )
			{
				End ();
				onScenarioFailed ( curScenario );
				return;
			}

			// check for end of runtime
			float t = Time.time - startTime;
			if ( t >= runtime )
			{
				// again check for success
				if ( curScenario.CheckSuccess () )
				{
					End ();
					onScenarioSucceeded ( curScenario );
					return;
				}

				// again check for failure
				if ( curScenario.CheckFailure () )
				{
					End ();
					onScenarioFailed ( curScenario );
					return;
				}
			}
		}
	}

	public void SelectScenario (int id)
	{
		if ( id < 0 || id >= scenarios.Length )
		{
			Debug.LogException ( new System.IndexOutOfRangeException ( "Select Scenario: index out of range: " + id ) );
			return;
		}

		Debug.Log ( "selected scenario: " + id );

		if ( curScenario != null )
		{
			curScenario.Cleanup ();
		}

		curScenario = scenarios [ id ];
		runtime = curScenario.data.runtime;
		curScenario.drone = drone;
		drone.Frozen = true;
		curScenario.Init ();
		onScenarioLoaded ( curScenario );
	}

	public void Begin ()
	{
		startTime = Time.time;
		drone.Frozen = false;
		curScenario.Begin ();
	}

	public void End ()
	{
		curScenario.End ();
		drone.Frozen = true;
	}

	public void DoReset ()
	{
		curScenario.DoReset ();
	}

	void OnSuccess ()
	{
		onScenarioSucceeded ( curScenario );
	}

	void OnFailure ()
	{
		onScenarioFailed ( curScenario );
	}
}