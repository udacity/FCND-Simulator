using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.MeshGeneration.Modifiers;


// helper class to filter features by a specific property key/value (ex: height)
[System.Serializable]
public class FilteredPrefab
{
	public FeatureTags tag;
	public GameObject[] prefabs;
//	public bool 
}

// FeatureReplacer: a GameObject modifier that simple takes an existing feature object, gets the boundaries and replaces with one a few assigned prefabs

[CreateAssetMenu(menuName = "Mapbox/Udacity/Feature Replacer Modifier")]
public class FeatureReplacer : GameObjectModifier
{
	public FilteredPrefab[] filters;
	public GameObject[] defaultPrefabs;
	public bool randomRotation = true;

	public override void Run (VectorEntity ve, Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
//	public override void Run (Mapbox.Unity.MeshGeneration.Components.FeatureBehaviour fb, Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
	{
		if ( ( filters == null || filters.Length == 0 ) && ( defaultPrefabs == null || defaultPrefabs.Length == 0 ) )
		{
			#if UNITY_EDITOR
			Debug.LogError ( "FeatureReplacer " + name + ": must assign at least one filter or one prefab to replace" );
			#endif
			return;
		}

//		string props = ve.Feature.Properties.DictionaryToString ();
//		string props = fb.Data.Properties.DictionaryToString ();
//		if ( props.Contains ( "university" ) )
//			Debug.Log ( "feture is \n" + props );


		GameObject[] prefabs = defaultPrefabs;
		foreach ( var filter in filters )
		{
			if ( filter.tag.IsMatch ( ve.Feature.Properties ) )
//			if ( filter.tag.IsMatch ( fb.Data.Properties ) )
			{
				prefabs = filter.prefabs;
				break;
			}
		}

		GameObject go = ve.GameObject;
//		GameObject go = fb.gameObject;
		Transform container = tile.transform.Find ( ve.Transform.parent.name + "_Replacement" );
//		Transform container = tile.transform.Find ( fb.transform.parent.name + "_Replacement" );
		if ( container == null )
		{
			container = new GameObject ( ve.Transform.parent.name + "_Replacement" ).transform;
//			container = new GameObject ( fb.transform.parent.name + "_Replacement" ).transform;
			container.SetParent ( tile.transform );
			container.localPosition = ve.Transform.parent.localPosition;
//			container.localPosition = fb.transform.parent.localPosition;
		}
		MeshRenderer rend = go.GetComponent<MeshRenderer> ();
		Vector3 origSize = rend.bounds.size;
		go.SetActive ( false );
		Vector3 position = rend.bounds.center - Vector3.up * rend.bounds.extents.y;
		int prefab = Random.Range ( 0, prefabs.Length );
		GameObject instance = Instantiate ( prefabs [ prefab ] );
		instance.name = go.name;
		instance.transform.position = position;
		instance.transform.parent = container;
		rend = instance.GetComponentInChildren<MeshRenderer> ();
		Vector3 curSize = rend.bounds.size;
		float sizeRatio = Mathf.Max ( curSize.x, curSize.z ) / Mathf.Min ( origSize.x, origSize.z );
//		float sizeRatio = rend.bounds.size.magnitude / origSize.magnitude;
//		float sizeRatio = Mathf.Min ( curSize.x / origSize.x, curSize.z / origSize.z );
		instance.transform.localScale /= sizeRatio;
//		if ( randomRotation )
//			instance.transform.eulerAngles = new Vector3 ( 0, Random.Range ( 0, 4 ) * 90, 0 );
	}
}