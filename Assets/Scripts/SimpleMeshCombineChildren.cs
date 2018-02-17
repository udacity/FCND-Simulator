#if UNITY_EDITOR
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
		var renderers = gameObject.GetComponentsInChildren<MeshRenderer> ( true );
		var filters = gameObject.GetComponentsInChildren<MeshFilter> ( true );

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
			Transform parent = filters [ i ].transform.parent;
			Matrix4x4 matrix = Matrix4x4.TRS ( parent.localPosition, parent.localRotation, parent.localScale );
			comb.transform = matrix;
//			comb.transform = filters [ i ].transform.localToWorldMatrix;
			combines [ i ] = comb;
			filters [ i ].gameObject.SetActive ( false );
		}

		mesh.CombineMeshes ( combines );
		Material mat = renderers [ 0 ].sharedMaterial;
		MeshFilter filter = gameObject.AddComponent<MeshFilter> ();
		MeshRenderer rend = gameObject.AddComponent<MeshRenderer> ();
		filter.sharedMesh = mesh;
		rend.sharedMaterial = mat;
		rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
	}
}
#endif