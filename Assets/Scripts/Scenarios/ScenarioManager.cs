using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	void Update ()
	{
		if ( curScenario != null && curScenario.IsRunning )
		{
			// check for success (can a scenario even succeed before end of runtime?)
			if ( curScenario.data.canSucceedBeforeRuntime && curScenario.CheckSuccess () )
			{
				curScenario.End ();
				onScenarioSucceeded ( curScenario );
				return;
			}

			// check for failure (can it fail before end of runtime?)
			if ( curScenario.data.canFailBeforeRuntime && curScenario.CheckFailure () )
			{
				curScenario.End ();
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
					curScenario.End ();
					onScenarioSucceeded ( curScenario );
					return;
				}

				// again check for failure
				if ( curScenario.CheckFailure () )
				{
					curScenario.End ();
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
		curScenario.Init ();
		onScenarioLoaded ( curScenario );
	}

	public void Begin ()
	{
		startTime = Time.time;
		curScenario.Begin ();
	}

	public void End ()
	{
		curScenario.End ();
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