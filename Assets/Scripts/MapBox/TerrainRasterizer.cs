﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;

// TerrainRasterizer: a GameObject mod to turn chunks of terrain geometry into a lookup texture for the main terrain object

[CreateAssetMenu(menuName = "Mapbox/Udacity/Terrain Rasterizer Modifier")]
public class TerrainRasterizer : GameObjectModifier
{
	const int TexSize = 256;
	static Dictionary<string, Texture2D> terrainTextures = new Dictionary<string, Texture2D> ();
	static int texCount = 0;
//	static bool test = false;

	public static Texture2D GetOrAddTileTexture (string tileName)
	{
		Texture2D tex = null;
		Color alpha = new Color ( 0, 0, 0, 1 );
		if ( !terrainTextures.TryGetValue ( tileName, out tex ) )
		{
			tex = new Texture2D ( TexSize, TexSize );
			Color[] colors = tex.GetPixels ();
			colors.ForEach ( (x ) =>
			{
				return alpha;
//				return Color.clear;
			} );
			tex.SetPixels ( colors );
			tex.name = tileName; //"tex " + ++texCount;
			tex.filterMode = FilterMode.Point;
			tex.Apply ();
			terrainTextures.Add ( tileName, tex );
		}

		return tex;
	}

	public override void Run (VectorEntity ve, Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
//	public override void Run (Mapbox.Unity.MeshGeneration.Components.FeatureBehaviour fb, Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
	{
		Texture2D tex = GetOrAddTileTexture ( tile.name );

		// what we want here is to map the texture onto the appropriate tile's bounds

		Bounds tileBounds = tile.GetComponent<Renderer> ().bounds;
		Mesh featureMesh = ve.Mesh; // ve.MeshFilter.mesh?
//		Mesh featureMesh = fb.GetComponent<MeshFilter> ().mesh;

		Vector3[] verts = featureMesh.vertices;
		Color[] pixels = tex.GetPixels ();

		Vector2 tileCorner = new Vector2 ( tileBounds.center.x - tileBounds.extents.x, tileBounds.center.z - tileBounds.extents.z );

		float tileSize = tileBounds.size.x / TexSize;
		string vertList = verts.ArrayToString ();

		for ( int i = 0; i < verts.Length; i++ )
		{
			verts [ i ] += tileBounds.center;
		}

		Color sample = ve.MeshRenderer.material.color;
//		Color sample = fb.GetComponent<Renderer> ().material.color;
		for ( int y = 0; y < TexSize; y++ )
		{
			for ( int x = 0; x < TexSize; x++ )
			{
				Vector2 pos = tileCorner + new Vector2 ( x * tileSize, y * tileSize );

				// this somewhat works
//				if ( !test )
				int idx = y * TexSize + x;
				if ( PolyUtils.PointInPoly2D ( new Vector3 ( pos.x, 0, pos.y ), new List<Vector3> ( verts ) ) )// && pixels [ idx ] == Color.clear )
					pixels [ idx ] = sample;
//					pixels [ idx ] = Color.red;
			}
		}
//		test = true;

		tex.SetPixels ( pixels );
		tex.Apply ();
//		r.material.mainTexture = tex;

		ve.GameObject.SetActive ( false );
//		fb.gameObject.SetActive ( false );
	}
}