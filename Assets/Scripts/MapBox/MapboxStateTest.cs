using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;

public class MapboxStateTest : MonoBehaviour
{
	public AbstractMap mapScript;
	AbstractMapVisualizer visualizer;


	void Awake ()
	{
		visualizer = mapScript.MapVisualizer;
		visualizer.OnMapVisualizerStateChanged += Visualizer_OnMapVisualizerStateChanged;
	}

	void Visualizer_OnMapVisualizerStateChanged (ModuleState obj)
	{
		Debug.Log ( obj + " " + Time.time );
	}
}