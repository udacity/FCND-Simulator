using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DictionaryUtility
{
	public static string DictionaryToString<T> (this Dictionary<string, T> dict)
	{
		StringBuilder sb = new StringBuilder ();
		foreach ( var pair in dict )
			sb.Append ( pair.Key + ": " + pair.Value.ToString () + "\n" );

		return sb.ToString ();
	}
}