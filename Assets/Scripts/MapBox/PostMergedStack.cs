using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;
//using Mapbox.Map;
//using Mapbox.Unity.MeshGeneration.Modifiers;
//using Mapbox.Unity.MeshGeneration.Data;
//using Mapbox.Unity.MeshGeneration.Components;

[CreateAssetMenu(menuName = "Mapbox/Udacity/Post-Merged Stack")]
public class PostMergedStack : ModifierStack
{
	class MeshData
	{
		public List<Vector3> verts;
		public List<Vector2> uvs;
		public List<Vector3> normals;
		public List<int> indices;
		public Material material;

		public MeshData (Mesh mesh, int subMesh, Material mat, Vector3[] allVerts, Vector3[] allNormals, Vector2[] allUVs)
//		public MeshData (List<Vector3> _vertices, List<Vector3> _normals, List<Vector2> _uvs, int subMesh, Material mat)
		{
			verts = new List<Vector3> ();
			uvs = new List<Vector2> ();
			normals = new List<Vector3> ();
			material = mat;

			int[] indexes = mesh.GetIndices ( subMesh );
			uint start = mesh.GetIndexStart ( subMesh );

		}
	}

	protected static Dictionary<Mesh, List<MeshFilter>> objectGroups = new Dictionary<Mesh, List<MeshFilter>> ();

	public override GameObject Execute (Mapbox.Unity.MeshGeneration.Data.UnityTile tile, Mapbox.Unity.MeshGeneration.Data.VectorFeatureUnity feature, Mapbox.Unity.MeshGeneration.Data.MeshData meshData, GameObject parent, string type = "")
	{
		GameObject parentGo = base.Execute (tile, feature, meshData, parent, type);
//		Debug.Log ( parentGo.name );
		// this stack is not meant to render anything on the parent object, only to contain objects inside
		var filt = parentGo.GetComponent<MeshFilter> ();
		if ( filt != null )
			Destroy ( filt );
		else
			Debug.Log ( "filt null" );
		var rend = parentGo.GetComponent<MeshRenderer> ();
		if ( rend != null )
			Destroy ( rend );
		else
			Debug.Log ( "rend null" );

		MeshRenderer[] renderers = parent.GetComponentsInChildren<MeshRenderer> ();
		MeshFilter[] filters = new MeshFilter[renderers.Length];
		GameObject[] objects = new GameObject[renderers.Length];
		for ( int i = 0; i < renderers.Length; i++ )
		{
			objects [ i ] = renderers [ i ].gameObject;
			filters [ i ] = objects [ i ].GetComponent<MeshFilter> ();
		}

		Material mat;
		MeshFilter mf;
		MeshRenderer mr;
		Mesh mesh;
		Vector3[] verts;
		Vector3[] normals;
		Vector2[] uvs;
//		List<int> indices = new List<int> ();
		int[] tris;
		MeshData md;
		return parent;
		for ( int i = 0; i < objects.Length; i++ )
		{
			mf = filters [ i ];
			mr = renderers [ i ];
			mesh = mf.sharedMesh;
			verts = mesh.vertices;
			normals = mesh.normals;
			uvs = mesh.uv;
			for ( int m = 0; m < mr.sharedMaterials.Length; m++ )
			{
				mat = mr.sharedMaterials [ m ];
				md = new MeshData ( mesh, m, mat, verts, normals, uvs );
			}

		}

		return parent;
	}
}