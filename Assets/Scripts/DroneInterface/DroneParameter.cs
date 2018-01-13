using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ParameterAction = System.Action<DroneParameter>;

[System.Serializable]
public class DroneParameter : ISerializationCallbackReceiver
{
	public string displayName = "New Parameter";
	public float Value
	{
		get { return thisValue; }
		set
		{
			if ( thisValue != value )
			{
				thisValue = value;
				onChanged ( this );
			}
		}
	}

	[SerializeField]
	float thisValue;

	ParameterAction onChanged = delegate {};
	bool init;


	public DroneParameter ()
	{
//		DroneParameters.AddParameter ( this );
	}

//	~DroneParameter ()
//	{
//		DroneParameters.RemoveParameter ( this );
//	}

	public void Observe (ParameterAction changeObserver)
	{
		onChanged += changeObserver;
	}

	public void Unobserve (ParameterAction changeObserver)
	{
		onChanged -= changeObserver;
	}


	public void OnBeforeSerialize ()
	{
		DroneParameters.RemoveParameter ( this );
//		Debug.Log ( "param before ser on " + displayName + " " + GetHashCode () );
	}

	public void OnAfterDeserialize ()
	{
		if ( !init )
		{
			init = true;
			DroneParameters.AddParameter ( this );
		}
//		Debug.Log ( "param after ser on " + displayName + " " + GetHashCode () );
	}
}