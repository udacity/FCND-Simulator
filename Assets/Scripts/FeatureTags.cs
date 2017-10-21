using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TagComparison
{
	AtMost,
	Exactly,
	AtLeast
}

public enum PropValueType
{
	Number,
	String
}

[System.Serializable]
public class FeatureTags
{
	public string name;
	public string propertyKey;
	public TagComparison comparison;
	public string propertyValue;
	public PropValueType valueType;

	public bool IsMatch (Dictionary<string, object> properties)
	{
		foreach ( var pair in properties )
		{
			if ( pair.Key.ToLowerInvariant () == propertyKey.ToLowerInvariant () )
			{
				return IsMatch ( pair.Value.ToString () );
			}
		}

		return false;
	}

	public bool IsMatch (string propValue)
	{
		if ( valueType == PropValueType.String )
			return propValue.ToLowerInvariant () == propertyValue.ToLowerInvariant ();
		else
			return NumberMatch ( float.Parse ( propValue ) );
	}

	bool NumberMatch (float val)
	{
		float thisValue = float.Parse ( propertyValue );
		switch ( comparison )
		{
		case TagComparison.AtLeast:
			return val >= thisValue;
			break;
		case TagComparison.AtMost:
			return val <= thisValue;
			break;
		default:
			return Mathf.Approximately ( thisValue, val );
			break;
		}
	}
}