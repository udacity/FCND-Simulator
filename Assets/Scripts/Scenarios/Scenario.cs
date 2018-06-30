using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drones;
using DroneInterface;
using DroneControllers;

[ExecuteInEditMode]
public abstract class Scenario : MonoBehaviour
{
	public bool IsRunning { get; protected set; }

	public ScenarioData data;
//	public TuningParameter[] tuningParameters;
	public string[] tunableParameters;
    public string[] userParameters;
	public int vizParameters;
	public VisualizationParameter vizParameter1;
	public VisualizationParameter vizParameter2;
	public System.Action<float, int> onParameter1Update = delegate {};
	public System.Action<float, int> onParameter2Update = delegate {};

    public IDrone drone;
    public PlaneControl planeControl;

#if UNITY_EDITOR
    [Tooltip ("An editor-only field. Drag the desired drone here to store its current position and orientation")]
	public GameObject droneObject;
    #endif


    public void Init ()
	{
        drone = Simulation.ActiveDrone;
        planeControl = GameObject.Find("Plane Drone").GetComponent<PlaneAutopilot>().planeControl;
        if (drone == null)
            Debug.Log("Null Active Drone");

        Debug.Log ( "Initializing scenario: " + data.title );
		IsRunning = false;
//		tuningParameters.ForEach ( x => x.Reset () );

		// set default (fixed) values for scenario
		planeControl.SetScenarioParameters ( tunableParameters );

        drone.InitializeVehicle(data.vehiclePosition, data.vehicleVelocity, data.vehicleEulerAngles);
        FollowCamera.activeCamera.SetLookMode ( data.cameraLookMode, data.cameraDistance );
        drone.SetGuided(false);
        drone.ArmDisarm(false);
        OnInit ();
	}

	#if UNITY_EDITOR
	void Update ()
	{
		if ( droneObject != null && !IsRunning )
		{
			if ( data == null )
				data = new ScenarioData ();
			Transform t = droneObject.transform;
			data.vehiclePosition = t.position;
			data.vehicleOrientation = t.rotation;
			// velocity? probably manual
			droneObject = null;
		}

		if ( IsRunning )
		{
			if ( Input.GetKeyDown ( KeyCode.L ) )
			{
				drone.Frozen = !drone.Frozen;
//				( (PlaneDrone) drone ).FreezeDrone ( !( (PlaneDrone) drone ).IsFrozen () );
			}
		}
	}
	#endif

	public void Begin ()
	{
        
        drone.Frozen = false;
        drone.InitializeVehicle(data.vehiclePosition, data.vehicleVelocity, data.vehicleEulerAngles);
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

	public void DoReset ()
	{
//		OnReset ();
		Init ();
	}

	public virtual void OnApplyTunableValues ()
	{
        // set default (fixed) values for scenario

        string[] allParameters = new string[tunableParameters.Length + userParameters.Length];
        tunableParameters.CopyTo(allParameters,0);
        userParameters.CopyTo(allParameters, tunableParameters.Length);
        Debug.Log(allParameters);
        //planeControl.SetScenarioParameters ( tunableParameters );
        planeControl.SetScenarioParameters(allParameters);

    }

	protected virtual void OnInit () {}
    protected virtual void OnBegin() {}
    protected virtual void OnEnd() {}
	protected virtual bool OnCheckSuccess () { return false; }
	protected virtual bool OnCheckFailure () { return false; }
	protected virtual void OnCleanup () {}
//	protected virtual void OnReset () {}
}