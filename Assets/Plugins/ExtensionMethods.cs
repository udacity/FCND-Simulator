﻿using UnityEngine;
using System;
using System.Collections;

public static class ExtensionMethods
{
	/*******************************************************************************
	* LAYERMASK methods
	*******************************************************************************/

	// check if a LayerMask contains a layer
	public static bool ContainsLayer (this LayerMask mask, int layer)
	{
		return ( mask.value & ( 1 << layer ) ) != 0;
	}

	// check if a LayerMask contains a layer by name
	public static bool ContainsLayer (this LayerMask mask, string layer)
	{
		return ( mask.value & ( 1 << LayerMask.NameToLayer ( layer ) ) ) != 0;
	}

	// check if a LayerMask contains a LayerMask
	public static bool ContainsMask (this LayerMask mask, LayerMask testMask)
	{
		return ( mask.value & testMask.value ) != 0;
	}


	/*******************************************************************************
	* STRING methods
	*******************************************************************************/

	// get a byte array from a string
	public static byte[] GetBytes (this string s)
	{
		return System.Text.Encoding.UTF8.GetBytes ( s );
	}

	// get a string form a byte array
	public static string GetString (this byte[] bytes)
	{
		return System.Text.Encoding.UTF8.GetString ( bytes );
	}


	/*******************************************************************************
	* ARRAY methods
	*******************************************************************************/

	// check if an array contains a specific value
	public static bool Contains<T> (this T[] array, T value)
	{
		for ( int i = 0; i < array.Length; i++ )
			if ( array [ i ].Equals ( value ) )
				return true;
		return false;
	}

	// find the index of an item in an array
	public static int IndexOf<T> (this T[] array, T item)
	{
		int length = array.Length;
		for ( int i = 0; i < length; i++ )
			if ( array [ i ].Equals ( item ) )
				return i;
		
		return -1;
	}

	// return an item matching a predicate in an array
	public static T Find<T> (this T[] array, Predicate<T> condition)
	{
		int length = array.Length;
		for ( int i = 0; i < length; i++ )
			if ( condition ( array [ i ] ) )
				return array [ i ];
		return default(T);
	}

	// find the index of an item matching a predicate in an array
	public static int FindIndex<T> (this T[] array, Predicate<T> condition)
	{
		int length = array.Length;
		for ( int i = 0; i < length; i++ )
			if ( condition ( array [ i ] ) )
				return i;

		return -1;
	}

	// run an action on each item in an array
	public static void ForEach<T> (this T[] array, Action<T> action) where T : class
	{
		int length = array.Length;
		for ( int i = 0; i < length; i++ )
			action ( array [ i ] );
	}

	// run a func on each item in an array (useful for value types)
	public static void ForEach<T> (this T[] array, Func<T, T> func) where T : struct
	{
		int length = array.Length;
		for ( int i = 0; i < length; i++ )
			array [ i ] = func ( array [ i ] );
	}

	// check if a condition is true for every element in an array
	public static bool TrueForAll<T> (this T[] array, Predicate<T> condition)
	{
		int length = array.Length;
		for ( int i = 0; i < length; i++ )
			if ( !condition ( array [ i ] ) )
				return false;
		return true;
	}

	// copy X elements from one array to another
	public static void CopyFrom<T> (this T[] array, T[] source)
	{
		int count = array.Length;
		for ( int i = 0; i < count; i++ )
			array [ i ] = source [ i ];
	}

	/*******************************************************************************
	* FLOAT methods
	*******************************************************************************/
	public static short ToShort (this double value)
	{
		int cnt = 0;
		while ( value != Math.Floor ( value ) )
		{
			value *= 10.0;
			cnt++;
		}

		return (short) ( ( cnt << 12 ) + (int) value );
	}

	public static double ToDouble (this short value)
	{
		int cnt = value >> 12;
		double result = value & 0xfff;
		while ( cnt > 0 )
		{
			result /= 10.0;
			cnt--;
		}

		return result;
	}
}

public static class VectorExtensions
{
	public static Vector2 NaN2 { get { return new Vector2 ( float.NaN, float.NaN ); } }

	public static Vector3 NaN3 { get { return new Vector3 ( float.NaN, float.NaN, float.NaN ); } }

	public static Vector2 Infinity2 { get { return new Vector2 ( Mathf.Infinity, Mathf.Infinity ); } }

	public static Vector3 Infinity3 { get { return new Vector3 ( Mathf.Infinity, Mathf.Infinity, Mathf.Infinity ); } }
}