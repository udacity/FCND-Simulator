using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Action = System.Action<bool>;

public class Simulation
{
	static Simulation Instance
	{
		get
		{
			if ( instance == null )
				instance = new Simulation ();
			return instance;
		}
	}
	static Simulation instance;

	public static bool Paused
	{
		get { return Instance.paused; }
		set 
		{
			if ( value != Instance.paused )
			{
				Instance.paused = value;
				if ( value )
					Instance.Pause ();
				else
					Instance.Resume ();
			}
		}
	}

	public static bool UIIsOpen
	{
		get;
		set;
	}

	Action pauseEvent = delegate (bool pause) {};
	bool paused;
	float lastTimeScale = 1;

	void Pause ()
	{
		lastTimeScale = Time.timeScale;
		Time.timeScale = 0;
		pauseEvent ( true );
	}

	void Resume ()
	{
		Time.timeScale = lastTimeScale;
		pauseEvent ( false );
	}

	public static void Observe (Action pauseListener)
	{
		Instance.pauseEvent += pauseListener;
	}
}