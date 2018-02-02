using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdaciPlot;
using Drones;

public class PlotViz : MonoBehaviour
{
	public static PlotViz Instance
	{
		get { return instance; }
	}
	static PlotViz instance;

	public int Count
	{
		get { return plotLines.Count; }
	}

	public LineRenderer line;
	public Camera cam;
	public float refreshRate = 20;
	public float lineWidth = 0.06f;
	public int sampleCount = 200;
	public float timeSample = 5f;


//	Plottable<float> plot;

	List<Plottable<float>> plots;
	List<LineRenderer> plotLines;

	float refreshDelay;
	float nextRefresh;
	FastNoise fn;
	QuadDrone drone;
	float minSampleValue;
	float maxSampleValue;
	Rect cameraRect;
	float lastLineWidth;

	void Awake ()
	{
		if ( instance != null && instance != this )
		{
			Destroy ( gameObject );
			return;
		}
		instance = this;
		drone = GameObject.Find ( "Quad Drone" ).GetComponent<QuadDrone> ();
		// get camera world bounds (if need to, use viewport to world point)
		// size line renderer to bounds
		// render camera to texture?
		// dynamic update on line as test
		// set vertical scale
		// store multiple sample sets (get circ buffer)
		// render multiple sample sets in different colors (define list X colors, after that random if needed)

//		SimpleRingBuf<float> buf = new SimpleRingBuf<float> ( 10 );
//		for ( int i = 0; i < 10; i++ )
//			buf.Add ( i );
//		buf.Add ( 10 );
//		buf.Add ( 11 );
//		Debug.Log ( buf.Values ().ArrayToString () );
//		Debug.Log ( buf.Get ( -1 ) );

		refreshDelay = 1f / refreshRate;
//		plot = new Plottable<float> ( "Test Plot" );
		fn = new FastNoise ();
		Vector2 min = cam.ViewportToWorldPoint ( Vector3.zero );
		Vector2 max = cam.ViewportToWorldPoint ( Vector3.one );
		cameraRect = new Rect ( min.x, Mathf.Min ( min.y, max.y ), max.x - min.x, Mathf.Abs ( max.y - min.y ) );
		plots = new List<Plottable<float>> ();
		plotLines = new List<LineRenderer> ();
//		for ( int i = 0; i < 300; i++ )
//		{
//			float stamp = -5f + 5f * i / 300;
//			Debug.Log ( fn.GetSimplex ( stamp, 0 ) );

//			plot.AddSample ( fn.GetSimplex ( stamp, 0 ), stamp );
//		}


//		AddPlottable ( plot );
//		Plotting.AddPlottable1D ( "Pitch" );
//		AddPlottable ( Plotting.GetPlottable1D ( "Pitch" ) );
	}

	void Update ()
	{
		if ( Time.time > nextRefresh )
		{
			RefreshPlots ();
		}
//		plot.AddSample ( (float) drone.Altitude (), Time.time );
//		Plotting.AddSample ( "Pitch", (float) drone.Pitch (), Time.time );
	}

	void RefreshPlots ()
	{
		float leftPos = cameraRect.x * 0.95f;
		float rightPos = cameraRect.xMax * 0.95f;
		float sampleInterval = timeSample / sampleCount;
		for ( int p = 0; p < plots.Count; p++ )
		{
			var _plot = plots [ p ];
			var samples = _plot.GetSamples ();
			if ( samples == null || samples.Length == 0 )
				continue;
			
			float lastStamp = (float) samples [ samples.Length - 1 ].timestamp;
			float curTime = 0;
			samples = CullSamplesByTime ( samples, lastStamp - 5f, lastStamp );

			// parse x samples only instead of thousands. but how to optimize that so it's not iterating 200x3000 for each plot 20 times a second?
//			Vector3[] points = new Vector3[sampleCount];
//			for (int i = 0; i < sampleCount; i++)
//			{
//				
//			}

			Vector3[] points = new Vector3[samples.Length];
			for ( int i = 0; i < points.Length; i++ )
			{
				float x = Rescale ( lastStamp - 5f, lastStamp, leftPos, rightPos, (float) samples [ i ].timestamp );
				points [ i ] = new Vector3 ( x, samples [ i ].value, 0 );
//				if ( samples [ i ].value > maxSampleValue )
//					maxSampleValue = samples [ i ].value;
//				if ( samples [ i ].value < minSampleValue )
//					minSampleValue = samples [ i ].value;
			}
			var _line = plotLines [ p ];
			_line.positionCount = points.Length;
			_line.SetPositions ( points );
		}

		if ( lineWidth != lastLineWidth )
		{
			for ( int i = 0; i < plotLines.Count; i++ )
				plotLines [ i ].startWidth = plotLines [ i ].endWidth = lineWidth;
			lastLineWidth = lineWidth;
		}

		nextRefresh = Time.time + refreshDelay;
	}

	void RefreshItem (Plottable<float> item, LineRenderer itemLine)
	{
		float leftPos = cameraRect.x * 0.95f;
		float rightPos = cameraRect.xMax * 0.95f;
		var samples = item.GetSamples ();
		if ( samples == null || samples.Length == 0 )
			return;
		float lastStamp = (float) samples [ samples.Length - 1 ].timestamp;
		samples = CullSamplesByTime ( samples, lastStamp - 5f, lastStamp );

		Vector3[] points = new Vector3[samples.Length];
		for ( int i = 0; i < points.Length; i++ )
		{
			float x = Rescale ( lastStamp - 5f, lastStamp, leftPos, rightPos, (float) samples [ i ].timestamp );
			points [ i ] = new Vector3 ( x, samples [ i ].value, 0 );
//			float delta = lastStamp - (float) samples [ i ].timestamp;
//			points [ i ] = new Vector3 ( 5f - delta, samples [ i ].value, 0 );
			if ( samples [ i ].value > maxSampleValue )
				maxSampleValue = samples [ i ].value;
			if ( samples [ i ].value < minSampleValue )
				minSampleValue = samples [ i ].value;
		}
		itemLine.positionCount = points.Length;
		itemLine.SetPositions ( points );
	}

	TimedSample<float>[] CullSamplesByTime (TimedSample<float>[] samples, float start, float end)
	{
		double dst = Mathf.Min ( start, end );
		double dend = Mathf.Max ( start, end );
		Queue<TimedSample<float>> q = new Queue<TimedSample<float>> ();
		for ( int i = samples.Length - 1; i >= 0; i-- )
			if ( samples [ i ].timestamp >= start && samples [ i ].timestamp <= end )
				q.Enqueue ( samples [ i ] );
		return q.ToArray ();
	}

	public void AddPlottable (Plottable<float> p)
	{
		minSampleValue = 0;
		maxSampleValue = 0;
		if ( plots == null )
			plots = new List<Plottable<float>> ();
		plots.Add ( p );
		if ( plotLines == null )
			plotLines = new List<LineRenderer> ();
		LineRenderer newLine = Instantiate<LineRenderer> ( line, transform );
		newLine.gameObject.SetActive ( true );
		newLine.startWidth = newLine.endWidth = lineWidth;

		plotLines.Add ( newLine );
		RefreshItem ( p, newLine );
	}

	public void RemovePlottable (Plottable<float> p)
	{
		int index = plots.IndexOf ( p );
		if ( index != -1 )
		{
			LineRenderer plot = plotLines [ index ];
			plots.RemoveAt ( index );
			plotLines.RemoveAt ( index );
			Destroy ( plot.gameObject );
		}
	}

	public void SetColor (string item, Color c)
	{
		int index = plots.FindIndex ( x => x.label == item );
		if ( index != -1 )
			plotLines [ index ].startColor = plotLines [ index ].endColor = c;
	}

	void GetMinMax (TimedSample<float>[] samples, out float min, out float max)
	{
		min = max = 0;
		int count = samples.Length;
		for ( int i = 0; i < count; i++ )
		{
			if ( samples [ i ].value > max )
				max = samples [ i ].value;
			if ( samples [ i ].value < min )
				min = samples [ i ].value;
		}
	}

	float Rescale (float inMin, float inMax, float outMin, float outMax, float value)
	{
		return Mathf.Lerp ( outMin, outMax, Mathf.InverseLerp ( inMin, inMax, value ) );
	}

	void Scale ()
	{
		// scale viz
	}

	void FitToCamera ()
	{
		// fit height to camera
	}
}