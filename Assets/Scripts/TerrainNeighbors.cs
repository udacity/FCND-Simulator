using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainNeighbors : MonoBehaviour
{
	public Terrain center;
	public Terrain left;
	public Terrain right;
	public Terrain top;
	public Terrain bottom;

	void OnEnable ()
	{
		center.SetNeighbors ( left, top, right, bottom );
	}
}