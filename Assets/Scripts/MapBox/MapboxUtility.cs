using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapboxUtility
{
	static MapboxUtility Instance
	{
		get
		{
			if ( instance == null )
				instance = new MapboxUtility ();
			return instance;
		}
	}

	public static bool IsCompleted
	{
		get { return Instance.started && Instance.stacksCompleted == Instance.totalStacks; }
	}

	static MapboxUtility instance;

	bool started;
	int totalStacks;
	int stacksCompleted;

	public static void Reset ()
	{
		Debug.Log ( "mapboxutility resetting" );
		Instance.totalStacks = 0;
		Instance.stacksCompleted = 0;
		Instance.started = false;
	}

	public static void AddStack ()
	{
		Instance.started = true;
		Instance.totalStacks++;
		Debug.Log ( "mapboxutility addstack total: " + Instance.totalStacks );
	}

	public static void StackCompleted ()
	{
		Instance.stacksCompleted++;
		Debug.Log ( "mapboxutility completed: " + Instance.stacksCompleted + " of " + Instance.totalStacks );
	}
}