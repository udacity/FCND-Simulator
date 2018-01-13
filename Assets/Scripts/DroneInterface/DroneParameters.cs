using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.NonSerialized]
public class DroneParameters : ISerializationCallbackReceiver
{
	static DroneParameters Instance
	{
		get
		{
			if ( instance == null )
				instance = new DroneParameters ();
			return instance;
		}
	}
	static DroneParameters instance;

	public static DroneParameter[] Parameters { get { return Instance.parameters.ToArray (); } }
	[System.NonSerialized]
	List<DroneParameter> parameters = new List<DroneParameter> ();
	bool serialized;
	
	void Prune ()
	{
		bool hasNulls = false;
		List<DroneParameter> newList = new List<DroneParameter> ();
		foreach ( DroneParameter p in parameters )
		{
			if ( p == null )
			{
				hasNulls = true;
				continue;
			}
			newList.Add ( p );
		}

		if ( hasNulls )
		{
			parameters = newList;
		}
	}

	public void OnBeforeSerialize ()
	{
		parameters.Clear ();
		serialized = false;
//		Debug.Log ( "before ser" );
	}

	public void OnAfterDeserialize ()
	{
//		parameters.Clear ();
		serialized = true;
//		Debug.Log ( "it has " + parameters.Count + " parameters" );
//		Debug.Log ( "after ser" );
	}

	public static void AddParameter (DroneParameter p)
	{
//		Debug.Log ( "adding parameter " + p.GetHashCode () + " " + p.displayName );
		Instance.Prune ();
//		if ( !Instance.serialized )
//			return;
		var list = Instance.parameters;
		if ( !list.Contains ( p ) )
			list.Add ( p );
	}

	public static void RemoveParameter (DroneParameter p)
	{
		Instance.Prune ();
		var list = Instance.parameters;
		list.Remove ( p );
	}
}