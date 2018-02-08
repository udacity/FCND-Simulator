﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class to store and manage items that can be graphed
// the class can store 1D (float), 2D (Vector2), and 3D (Vector3) plots
// each plot is contained in a Plottable<T>
// plots and samples can be added at runtime, and a list of plots (by type) can be gotten to help select which plots to graph

namespace UdaciPlot
{
	public class Plotting
	{
		static Plotting instance;
		static Plotting Instance
		{
			get
			{
				if ( instance == null )
					instance = new Plotting ();
				
				return instance;
			}
		}

		Dictionary<string, Plottable<float>> items1D = new Dictionary<string, Plottable<float>> ();
		Dictionary<string, Plottable<Vector2>> items2D = new Dictionary<string, Plottable<Vector2>> ();
		Dictionary<string, Plottable<Vector3>> items3D = new Dictionary<string, Plottable<Vector3>> ();

		public static void Clear ()
		{
			Instance.items1D.Clear ();
			Instance.items2D.Clear ();
			Instance.items3D.Clear ();
		}

		public static void AddPlottable1D (string title)
		{
			if ( !Instance.items1D.ContainsKey ( title ) )
				Instance.items1D.Add ( title, new Plottable<float> ( title, _comparer: CompareFloats ) );
		}

		public static void AddPlottable2D (string title)
		{
			if ( !Instance.items2D.ContainsKey ( title ) )
				Instance.items2D.Add ( title, new Plottable<Vector2> ( title, _comparer:CompareVector2 ) );
		}

		public static void AddPlottable3D (string title)
		{
			if ( !Instance.items3D.ContainsKey ( title ) )
				Instance.items3D.Add ( title, new Plottable<Vector3> ( title, _comparer:CompareVector3 ) );
		}

		public static void AddSample (string title, float value, double timestamp) {
			Instance._AddSample ( title, value, timestamp );
		}

		public static void AddSample (string title, Vector2 value, double timestamp) {
			Instance._AddSample ( title, value, timestamp );
		}

		public static void AddSample (string title, Vector3 value, double timestamp) {
			Instance._AddSample ( title, value, timestamp );
		}

		public void _AddSample (string title, float value, double timestamp)
		{
			Plottable<float> p;
			if ( items1D.TryGetValue ( title, out p ) )
			{
				p.AddSample ( value, timestamp );
			} else
			{
				p = new Plottable<float> ( title, _comparer:CompareFloats );
				p.AddSample ( value, timestamp );
				items1D.Add ( title, p );
			}
		}

		public void _AddSample (string title, Vector2 value, double timestamp)
		{
			Plottable<Vector2> p;
			if ( items2D.TryGetValue ( title, out p ) )
			{
				p.AddSample ( value, timestamp );
			} else
			{
				p = new Plottable<Vector2> ( title, _comparer:CompareVector2 );
				p.AddSample ( value, timestamp );
				items2D.Add ( title, p );
			}
		}

		public void _AddSample (string title, Vector3 value, double timestamp)
		{
			Plottable<Vector3> p;
			if ( items3D.TryGetValue ( title, out p ) )
			{
				p.AddSample ( value, timestamp );
			} else
			{
				p = new Plottable<Vector3> ( title, _comparer:CompareVector3 );
				p.AddSample ( value, timestamp );
				items3D.Add ( title, p );
			}
		}

		public static Plottable<float> GetPlottable1D (string title)
		{
			return Instance.items1D [ title ];
		}

		public static Plottable<Vector2> GetPlottable2D (string title)
		{
			return Instance.items2D [ title ];
		}

		public static Plottable<Vector3> GetPlottable3D (string title)
		{
			return Instance.items3D [ title ];
		}

		public static string[] ListPlottables1D ()
		{
			return new List<string> ( Instance.items1D.Keys ).ToArray ();
		}

		public static string[] ListPlottables2D ()
		{
			return new List<string> ( Instance.items2D.Keys ).ToArray ();
		}

		public static string[] ListPlottables3D ()
		{
			return new List<string> ( Instance.items3D.Keys ).ToArray ();
		}

		/// <summary>
		/// Compares two float values. Returns 1 if a>b, -1 if b>a, 0 if equal
		/// </summary>
		public static int CompareFloats (float a, float b)
		{
			if ( a > b )
				return 1;
			else
			if ( b > a )
				return -1;
			
			return 0;
		}

		/// <summary>
		/// Compares two Vector2's vertical component. Returns 1 if a>b, -1 if b>a, 0 if equal
		/// </summary>
		public static int CompareVector2 (Vector2 a, Vector2 b)
		{
			if ( a.y > b.y )
				return 1;
			else
			if ( b.y > a.y )
				return -1;

			return 0;
		}

		/// <summary>
		/// Compares two Vector3's vertical component. Returns 1 if a>b, -1 if b>a, 0 if equal
		/// </summary>
		public static int CompareVector3 (Vector3 a, Vector3 b)
		{
			if ( a.y > b.y )
				return 1;
			else
				if ( b.y > a.y )
					return -1;

			return 0;
		}
	}
}