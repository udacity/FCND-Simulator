using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;
//using Mapbox.Map;
//using Mapbox.Unity.MeshGeneration.Modifiers;
//using Mapbox.Unity.MeshGeneration.Data;
//using Mapbox.Unity.MeshGeneration.Components;

[CreateAssetMenu(menuName = "Mapbox/Udacity/Batching Modifier Stack")]
public class ObjectBatcherStack : ModifierStack
{
	public override GameObject Execute (Mapbox.Unity.MeshGeneration.Data.UnityTile tile, Mapbox.Unity.MeshGeneration.Data.VectorFeatureUnity feature, Mapbox.Unity.MeshGeneration.Data.MeshData meshData, GameObject parent, string type = "")
	{
		GameObject parentGo = base.Execute (tile, feature, meshData, parent, type);
		MeshRenderer[] renderers = parent.GetComponentsInChildren<MeshRenderer> ();
		GameObject[] objects = new GameObject[renderers.Length];
		for ( int i = 0; i < renderers.Length; i++ )
			objects [ i ] = renderers [ i ].gameObject;
		StaticBatchingUtility.Combine ( objects, parentGo );
		
		return parent;
	}
}