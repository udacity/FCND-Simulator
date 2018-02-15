#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class SceneAssetObject
{
	public string title;
	public SceneAsset scene;
}

[CreateAssetMenu (menuName="Udacity/Build Assets")]
public class BuildAssets : ScriptableObject
{
	public SceneAsset loaderScene;
	public SceneAsset menuScene;

	public SceneAssetObject[] projectScenes;
}
#endif