using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader
{
	static Scene sceneToReload;
	static Scene tempScene;

	static bool init;

	public static void Reload ()
	{
		Init ();

		tempScene = SceneManager.CreateScene ( "TempScene" );
		SceneManager.UnloadSceneAsync ( sceneToReload );
	}

	static void Init ()
	{
		if ( !init )
		{
			sceneToReload = SceneManager.GetActiveScene ();
//			tempScene = SceneManager.CreateScene ( "TempScene" );
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			init = true;
		}
	}

	static void OnSceneLoaded (Scene newScene, LoadSceneMode sceneMode)
	{
		Debug.Log ( "Scene loaded: " + newScene.name );
		if ( newScene.name == tempScene.name )
		{
			SceneManager.UnloadSceneAsync ( sceneToReload );
		} else
		{
			SceneManager.UnloadSceneAsync ( tempScene );
		}
	}

	static void OnSceneUnloaded (Scene oldScene)
	{
		Debug.Log ( "scene unloaded: " + oldScene.name );
		if ( oldScene.name == tempScene.name )
		{
		} else
		{
			SceneManager.LoadScene ( sceneToReload.name );
		}
	}
}