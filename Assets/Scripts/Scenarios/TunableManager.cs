using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class TunableParameter
{
	public string name;
	public float minValue;
	public float maxValue;
	public float fixedValue;
	[System.NonSerialized]
	public float value;
	[Tooltip ("Number of decimal digits")]
	public int resolution = 3;
}

// only needed this once to create the asset
//[CreateAssetMenu (menuName="Scriptables/TunableManager")]
public class TunableManager : ScriptableObject
{
	static TunableManager Instance
	{
		get
		{
			if ( instance == null )
				LoadInstance ();
			return instance;
		}
	}
	static TunableManager instance;

	public static List<TunableParameter> Parameters { get { return Instance.parameters; } }
	public List<TunableParameter> parameters;


	public static TunableParameter GetParameter (string name)
	{
		return Parameters.Find ( x => x.name.ToLower () == name.ToLower () );
	}

	static void LoadInstance ()
	{
		instance = Resources.Load<TunableManager> ( "TunableManager" );
		instance.LoadFromFile ();
	}

	public static void Init ()
	{
		if ( instance == null )
			LoadInstance ();
	}

	public void LoadFromFile ()
	{
//		string line;
		string[] split;

		string path = "";
		// editor
		#if UNITY_EDITOR
		path = "Assets/Resources/gains.txt";
		#else
		path = Application.dataPath.TrimEnd ( '/' );
//		if ( Application.platform == RuntimePlatform.OSXPlayer )
//			path += "/../../";
//		else
//			path += "/../";
		path += "/gains.txt";
		#endif

		string[] lines = File.ReadAllLines ( path );
		foreach ( string line in lines )
		{
			split = line.Split ( ':' );
			TunableParameter p = parameters.Find ( x => x.name.ToLower () == split [ 0 ].ToLower () );
			if ( p != null )
			{
				p.value = float.Parse ( split [ 1 ] );
			} else
				Debug.Log ( "TunableManager can't find parameter named " + split [ 0 ] );
		}

		Debug.Log ( "Gains loaded from file" );
	}

	public static void SaveGains ()
	{
		string path = Application.dataPath.TrimEnd ( '/' );
//		path = path.Substring ( 0, path.LastIndexOf ( '/' ) );
		#if !UNITY_EDITOR
		if ( Application.platform == RuntimePlatform.OSXPlayer )
			path = Application.persistentDataPath.TrimEnd ( '/' );
//			path = path.Substring ( 0, path.LastIndexOf ( '/' ) );
		#endif
		path += "/gains_new.txt";
		using ( StreamWriter s = File.CreateText ( path ) )
		{
			foreach ( var p in instance.parameters )
			{
				string line = p.name + ":" + p.value;
				s.WriteLine ( line );
			}
		}
		Debug.Log ( "Saved gains out to " + path );
	}
}