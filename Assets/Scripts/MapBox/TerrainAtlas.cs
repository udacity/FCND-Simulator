using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TerrainAtlas: persistent object to hold a list of indices into a texture atlas
// By Noam Weiss, for Udacity, Inc.

[System.Serializable]
public class TerrainAtlasEntity
{
	[Tooltip ("Types should match landcover/landuse tags (i.e. \"grass\", etc). Separate tags with ','.")]
	public string types;
	public Rect textureRect;
	public Vector2 tiling = Vector2.one;

	public bool IncludesType (string type)
	{
		return types.ToLowerInvariant ().Contains ( type.ToLowerInvariant () );
	}
}

[CreateAssetMenu(menuName = "Mapbox/Udacity/Terrain Atlas")]
public class TerrainAtlas : ScriptableObject
{
	public TerrainAtlasEntity defaultTexture;
	public List<TerrainAtlasEntity> Textures;
}