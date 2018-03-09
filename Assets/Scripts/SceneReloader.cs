using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

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

//	public static void Reload (int ms)
//	{
//		Task.Run ( () => DelayedReload ( ms ) );
//	}
//
//	static async Task DelayedReload (int ms)
//	{
//		await Task.Delay ( ms );
//		Reload ();
//	}

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
//			var mapScript = GameObject.Find ( "Map" ).GetComponent<Mapbox.Unity.Map.AbstractMap> ();
//			mapScript.Reset ();
			// guess this temp scene doesn't need unloading
//			SceneManager.UnloadSceneAsync ( tempScene );
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