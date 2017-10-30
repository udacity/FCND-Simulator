using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CollectionUtility
{
	public static string DictionaryToString<T> (this Dictionary<string, T> dict)
	{
		StringBuilder sb = new StringBuilder ();
		foreach ( var pair in dict )
			sb.Append ( pair.Key + ": " + pair.Value.ToString () + "\n" );

		return sb.ToString ();
	}

	public static string ListToString<T> (this List<T> list)
	{
		StringBuilder sb = new StringBuilder ();
		foreach ( var x in list )
			sb.Append ( x.ToString () + ", " );

		return sb.ToString ();
	}

	public static string ArrayToString<T> (this T[] array)
	{
		StringBuilder sb = new StringBuilder ();
		foreach ( var x in array )
			sb.Append ( x.ToString () + ", " );

		return sb.ToString ();
	}
}