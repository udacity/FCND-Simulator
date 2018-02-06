using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.MeshGeneration.Modifiers;

[CreateAssetMenu(menuName = "Mapbox/Udacity/Flat Terrain MultiTex")]
public class FlatTerrainMultiTex : FlatTerrainFactory
{
	static int keyHash = Shader.PropertyToID ( "_KeyTex" );
	internal override void OnRegistered (Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
	{
		base.OnRegistered (tile);

//		tile.GetComponent<Renderer> ().material.mainTexture = TerrainRasterizer.GetOrAddTileTexture ( tile.name );
		Material mat = tile.MeshRenderer.material;
//		Material mat = tile.GetComponent<Renderer> ().material;
		if ( mat.HasProperty ( keyHash ) )
			mat.SetTexture ( keyHash, TerrainRasterizer.GetOrAddTileTexture ( tile.name ) );
//		tile.GetComponent<Renderer> ().material = mat;
	}
}