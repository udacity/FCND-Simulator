using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;

[CreateAssetMenu (menuName="Mapbox/Udacity/Road Name Modifier")]
public class RoadNameMod : PrefabModifier
{
	public UIRoadName roadNamePrefab;

	public override void Run (VectorEntity ve, Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
	{
		string featureName = GetFeatureName ( ve.Feature.Properties );
		if ( featureName == "road_label" )
			Debug.Log ( "has a road label!" );
//		UIRoadName instance = Instantiate ( roadNamePrefab, ve.Transform, false );

//		instance.SetName ( GetFeatureName ( ve.Feature.Properties ) );
	}

	string GetFeatureName (Dictionary<string, object> props)
	{
		if ( props.ContainsKey ( "name" ) )
		{
			return props [ "name" ].ToString ();
		} else
		if ( props.ContainsKey ( "house_num" ) )
		{
			return props [ "house_num" ].ToString ();
		} else
		if ( props.ContainsKey ( "type" ) )
		{
			return props [ "type" ].ToString ();
		}

		return "?";
	}
}