using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scenario : MonoBehaviour
{
	public bool IsRunning { get; protected set; }

	public ScenarioData data;
	public TuningParameter[] tuningParameters;

	public event System.Action onSuccess = delegate {};
	public event System.Action onFailure = delegate {};


	public void Init ()
	{
		Debug.Log ( "Initializing scenario: " + data.title );
		IsRunning = false;
		tuningParameters.ForEach ( x => x.Reset () );
		OnInit ();
	}

	public void Begin ()
	{
        OnBegin();
		IsRunning = true;
	}

	public void End ()
	{
        OnEnd();
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
    protected virtual void OnBegin() {}
    protected virtual void OnEnd() {}
	protected virtual bool OnCheckSuccess () { return false; }
	protected virtual bool OnCheckFailure () { return false; }
	protected virtual void OnCleanup () {}
}