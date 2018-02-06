using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;

[CreateAssetMenu (menuName="Mapbox/Udacity/Output Props Modifier")]
public class OutputProps : GameObjectModifier
{
	public override void Run (VectorEntity ve, Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
	{
//		if ( ve.GameObject.name.Contains ("68381") )
		Debug.Log ( ve.GameObject.name + " properties:\n" + ve.Feature.Properties.DictionaryToString () );
	}
}