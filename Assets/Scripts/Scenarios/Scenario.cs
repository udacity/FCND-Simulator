using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneInterface;

[ExecuteInEditMode]
public abstract class Scenario : MonoBehaviour
{
	public bool IsRunning { get; protected set; }

	public ScenarioData data;
	public TuningParameter[] tuningParameters;
	public IDrone drone;

	#if UNITY_EDITOR
	public GameObject droneObject;
	#endif

	public void Init ()
	{
		Debug.Log ( "Initializing scenario: " + data.title );
		IsRunning = false;
		tuningParameters.ForEach ( x => x.Reset () );
		OnInit ();
	}

	#if UNITY_EDITOR
	void Update ()
	{
		if ( droneObject != null )
		{
			if ( data == null )
				data = new ScenarioData ();
			Transform t = droneObject.transform;
			data.vehiclePosition = t.position;
			data.vehicleOrientation = t.rotation;
			// velocity? probably manual
			droneObject = null;
		}
	}
	#endif

	public void Begin ()
	{
		IsRunning = true;
	}

	public void End ()
	{
		IsRunning = false;
	}

	public bool CheckSuccess ()
	{
		return OnCheckSuccess ();
	}

	public bool CheckFailure ()
	{
		return OnCheckFailure ();
	}

	public void Cleanup ()
	{
		OnCleanup ();
	}

	protected virtual void OnInit () {}
	protected virtual bool OnCheckSuccess () { return false; }
	protected virtual bool OnCheckFailure () { return false; }
	protected virtual void OnCleanup () {}
}