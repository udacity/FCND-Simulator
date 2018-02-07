//#if UNITY_EDITOR
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SimpleMeshCombineChildren : MonoBehaviour
{
	[MenuItem ("Utility/Combine Children for Selected", false, 60)]
	static void CombineSelected ()
	{
		SimpleMeshCombineChildren[] scripts = Selection.gameObjects.Select ( (x ) =>
		{
			return x.GetComponent<SimpleMeshCombineChildren> ();
		} ).ToArray ();

		scripts.ForEach ( x => x.CombineChildren () );
	}

	void CombineChildren ()
	{
		var renderers = gameObject.GetComponentsInChildren<MeshRenderer> ();
		var filters = gameObject.GetComponentsInChildren<MeshFilter> ();

		CombineInstance[] combines = new CombineInstance[filters.Length];
		Mesh mesh = new Mesh ();
		List<Vector3> verts = new List<Vector3> ();
		List<int> tris = new List<int> ();
		List<Vector3> normals = new List<Vector3> ();
		List<Vector2> uvs = new List<Vector2> ();
		for (int i = 0; i < combines.Length; i++)
		{
			CombineInstance comb = new CombineInstance ();
			comb.mesh = filters [ i ].sharedMesh;
			comb.transform = filters [ i ].transform.localToWorldMatrix;
			combines [ i ] = comb;
			filters [ i ].gameObject.SetActive ( false );
		}
//		foreach ( MeshFilter mf in filters )
//		{
//			Mesh m = mf.sharedMesh;
//			verts.AddRange ( m.vertices );
//			normals.AddRange ( m.normals );
//			uvs.AddRange ( m.uv );
//			tris.AddRange ( m.triangles );
//		}
//
//		if ( verts.Count < 60000 )
//		{
//			mesh.vertices = verts.ToArray ();
//			mesh.triangles = tris.ToArray ();
//			mesh.normals = normals.ToArray ();
//			mesh.uv = uvs.ToArray ();
//			mesh.RecalculateBounds ();
//
//		} else
//		{
//			Debug.LogError ( "too many verts!" );
//			return;
//		}

		mesh.CombineMeshes ( combines );
		Material mat = renderers [ 0 ].sharedMaterial;
		MeshFilter filter = gameObject.AddComponent<MeshFilter> ();
		MeshRenderer rend = gameObject.AddComponent<MeshRenderer> ();
		filter.sharedMesh = mesh;
		rend.sharedMaterial = mat;
		rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
	}
}
//#endif