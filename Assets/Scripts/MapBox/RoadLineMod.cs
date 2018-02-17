using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;
using Mapbox.Unity.MeshGeneration.Data;

[System.Serializable]
public class StringFloatPair
{
	public string type;
	public float value;
}

/// <summary>
/// Road-specific line mod: separates primary, secondary, tertiary, etc. roads
/// </summary>
[CreateAssetMenu(menuName = "Mapbox/Udacity/Road Line Modifier")]
public class RoadLineMod : LineMeshModifier
{
	public StringFloatPair[] items;

	public override void Run (VectorFeatureUnity feature, MeshData md, float scale)
	{	
		SetWidth ( feature );
		base.Run (feature, md, scale);
	}

	public override void Run (VectorFeatureUnity feature, MeshData md, UnityTile tile)
	{
		SetWidth ( feature );
		base.Run (feature, md, tile);
	}

	protected void SetWidth (VectorFeatureUnity feature)
	{
		string curType = FindSelectorKey ( feature );
		var item = items.Find ( x => x.type.ToLowerInvariant () == curType );
		if ( item != null )
			Width = item.value;
		else
			Width = 2;
	}

	protected string FindSelectorKey (VectorFeatureUnity feature)
	{
		if ( feature.Properties.ContainsKey ( "type" ) )
		{
			return feature.Properties [ "type" ].ToString ().ToLowerInvariant ();
		} else
		if ( feature.Properties.ContainsKey ( "class" ) )
		{
			return feature.Properties [ "class" ].ToString ().ToLowerInvariant ();
		}

		return "";
	}
}