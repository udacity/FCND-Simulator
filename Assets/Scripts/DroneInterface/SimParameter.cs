using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ParameterAction = System.Action<SimParameter>;

[System.Serializable]
public class SimParameter : ISerializationCallbackReceiver
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


	public SimParameter ()
	{
//		SimParameters.AddParameter ( this );
	}

//	~SimParameter ()
//	{
//		SimParameters.RemoveParameter ( this );
//	}

	public void Observe (ParameterAction changeObserver)
	{
		onChanged += changeObserver;
	}

	public void Unobserve (ParameterAction changeObserver)
	{
		onChanged -= changeObserver;
	}

	/// <summary>
	/// Unity methods. Do not use!
	/// </summary>
	public void OnBeforeSerialize ()
	{
		SimParameters.RemoveParameter ( this );
//		Debug.Log ( "param before ser on " + displayName + " " + GetHashCode () );
	}

	/// <summary>
	/// Unity methods. Do not use!
	/// </summary>
	public void OnAfterDeserialize ()
	{
		if ( !init )
		{
			init = true;
			SimParameters.AddParameter ( this );
		}
//		Debug.Log ( "param after ser on " + displayName + " " + GetHashCode () );
	}
}