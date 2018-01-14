using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.NonSerialized]
public class SimParameters : ISerializationCallbackReceiver
{
	static SimParameters Instance
	{
		get
		{
			if ( instance == null )
				instance = new SimParameters ();
			return instance;
		}
	}
	static SimParameters instance;

	public static SimParameter[] Parameters { get { return Instance.parameters.ToArray (); } }
	[System.NonSerialized]
	List<SimParameter> parameters = new List<SimParameter> ();
	bool serialized;
	
	void Prune ()
	{
		bool hasNulls = false;
		List<SimParameter> newList = new List<SimParameter> ();
		foreach ( SimParameter p in parameters )
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

	public static void AddParameter (SimParameter p)
	{
//		Debug.Log ( "adding parameter " + p.GetHashCode () + " " + p.displayName );
		Instance.Prune ();
//		if ( !Instance.serialized )
//			return;
		var list = Instance.parameters;
		if ( !list.Contains ( p ) )
			list.Add ( p );
	}

	public static void RemoveParameter (SimParameter p)
	{
		Instance.Prune ();
		var list = Instance.parameters;
		list.Remove ( p );
	}
}