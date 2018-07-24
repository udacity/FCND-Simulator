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

	[System.NonSerialized]
	public LineRenderer[] pathLines;

    public IDrone drone;
    public PlaneControl planeControl;
    public float unityTimestep = 0.02f; // Physics timestep used when tuning gains within Unity
    public float pythonTimestep = 0.005f; // Physics timestep used when running Python control

    Vector3 inertiaTensor;

	public GameObject droneObject;

    public void Start()
    {
        droneObject = GameObject.Find("Plane Drone");
        inertiaTensor = droneObject.GetComponent<AircraftControl>().inertiaTensors;
        Debug.Log("Inertia Tensor: " + inertiaTensor);
    }

    void OnEnable ()
	{
		pathLines = gameObject.GetComponentsInChildren<LineRenderer> ( true );
		if ( Application.isPlaying )
			pathLines.ForEach ( x => x.enabled = false );
	}

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

        if (droneObject == null)
            droneObject = GameObject.Find("Plane Drone");
        droneObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        droneObject.GetComponent<Rigidbody>().inertiaTensor = inertiaTensor;

        drone.InitializeVehicle(data.vehiclePosition, data.vehicleVelocity, data.vehicleEulerAngles);
        FollowCamera.activeCamera.SetLookMode ( data.cameraLookMode, data.cameraDistance );
        drone.SetGuided(false);
        drone.ArmDisarm(false);

		pathLines.ForEach ( x => x.enabled = true );
        OnInit ();
	}

	#if UNITY_EDITOR
	void Update ()
	{
		if ( false && droneObject != null && !IsRunning )
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
        droneObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        Debug.Log("MOI: " + droneObject.GetComponent<Rigidbody>().inertiaTensor + " Constraints: " + droneObject.GetComponent<Rigidbody>().constraints);
        drone.InitializeVehicle(data.vehiclePosition, data.vehicleVelocity, data.vehicleEulerAngles);
        OnBegin();
        if (drone.MotorsArmed())
            Time.fixedDeltaTime = 0.005f;
        else
            Time.fixedDeltaTime = 0.02f;
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
		pathLines.ForEach ( x => x.enabled = false );
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
        //planeControl.SetScenarioParameters ( tunableParameters );
        planeControl.SetScenarioParameters(allParameters);

    }

    public void ApplyLineColor(Color color)
    {
        if (pathLines != null)
            pathLines.ForEach(x => x.material.color = color);
    }

	protected virtual void OnInit () {}
    protected virtual void OnBegin() {}
    protected virtual void OnEnd() {}
	protected virtual bool OnCheckSuccess () { return false; }
	protected virtual bool OnCheckFailure () { return false; }
	protected virtual void OnCleanup () {}
//	protected virtual void OnReset () {}
}