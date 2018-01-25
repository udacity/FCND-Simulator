using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Data;
//using System;
using Mapbox.Unity.MeshGeneration.Modifiers;

// TerrainTexturizer: assigns uvs to pieces of terrain, based on a terrain texture atlas.
// By Noam Weiss, for Udacity, Inc.

[CreateAssetMenu(menuName = "Mapbox/Udacity/Terrain Texturizer")]
public class TerrainTexturizer : MeshModifier
{
	public TerrainAtlas atlasInfo;

	TerrainAtlasEntity currentSection;
	Rect currentTextureRect;
	Vector2 currentSectionTiling;

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Run(VectorFeatureUnity feature, MeshData md, UnityTile tile = null)
	{
		if (md.Vertices.Count == 0 || feature == null || feature.Points.Count < 1)
			return;

		// start by finding the correct 'piece' (quadrant, etc) of the texture, by matching our feature type to one in the atlas
		currentSection = FindTexture ( feature.Properties [ "type" ].ToString () );
		currentTextureRect = currentSection.textureRect;
		currentSectionTiling = currentSection.tiling;

		// for now, just figure out each vert's horizontal position relative to the current texture rect, and assign that as uv
		// start by grabbing the tile's corners (for now using its renderer's bounds) and offset the area by the tile's actual position
		// the tile will be moved because of the *AtSpecificLocation map script, which literally just moves the parent map object so our coordinates are centered
		Bounds bounds = tile.MeshRenderer.bounds;
		Vector3 positionOffset = tile.transform.position;
		bounds.min -= positionOffset;
		bounds.max -= positionOffset;

		// rather than corners, we'll just use the minimum x and z, and the delta to the max x and z. we'll essentialy inverse lerp each vert by this to figure out its position relative to the tile itself
		float xMin = bounds.min.x;
		float xDist = bounds.max.x - xMin;
		float zMin = bounds.min.z;
		float zDist = bounds.max.z - zMin;

		// i used uvCount here because the uv count _should_ match the vert count. and we're using .UV[0] because .UV is the list of uv *channels*, and each one the actual list of uvs
		int uvCount = md.UV [ 0 ].Count;
		md.Colors = new List<Color> ( new Color[uvCount] );
		for ( int i = 0; i < uvCount; i++ )
		{
			Vector3 vert = md.Vertices [ i ];

			// temp: raise everything just a bit off the ground
			vert.y += 0.01f;
			md.Vertices [ i ] = vert;

			// do the inverse lerp here
			float x = ( vert.x - xMin ) / xDist;
			float z = ( vert.z - zMin ) / zDist;
			Vector2 uv = new Vector2 ( x, z );

			// for use with Standard shader
			// and then just use this lerp value in texture space
//			uv = currentTextureRect.position + Mult ( uv, currentTextureRect.size );
//			md.UV [ 0 ] [ i ] = uv;

			// for use with Terrain/TilingAtlas shader. try to use uv2
			md.UV [ 0 ] [ i ] = currentTextureRect.position + Mult ( uv, currentTextureRect.size );
			md.Colors [ i ] = new Color ( currentSectionTiling.x, currentSectionTiling.y, 0, 1 );
//			md.UV [ 1 ] [ i ] = new Vector2 ( 0.5f, 0.75f );
//			md.UV [ 1 ] [ i ] = currentSectionTiling;
			// we'll borrow this line from our shader to do the tiling
//			fixed2 uv2 = frac (IN.uv_KeyTex * tiling) * 0.5 + uv;
//			md.UV [ 0 ] [ i ] = Mult ( uv, currentSectionTiling );

//			Vector2 frac = Frac ( Mult ( uv, currentSectionTiling ) );
//			Vector2 uv2 = frac * 0.5f + currentTextureRect.position;
//			Vector2 uv2 = Mult ( frac, currentTextureRect.size ) + currentTextureRect.position;
//			md.UV [ 0 ] [ i ] = uv2;
		}
	}

	TerrainAtlasEntity FindTexture (string typeProperty)
	{
		foreach ( TerrainAtlasEntity tex in atlasInfo.Textures )
			if ( tex.IncludesType ( typeProperty ) )
				return tex;
		
		return atlasInfo.defaultTexture;
	}

	Vector2 Frac (Vector2 v)
	{
		return new Vector2 (
			v.x - (int) v.x,
			v.y - (int) v.y
		);
	}

	Vector2 Mult (Vector2 v1, Vector2 v2)
	{
		return new Vector2 (
			v1.x * v2.x,
			v1.y * v2.y
		);
	}
}