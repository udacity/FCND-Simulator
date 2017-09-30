using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Loader script: looks for command line args to load a scene. if none exist, chooses a scene by default
 */

public class Loader : MonoBehaviour
{
	public int defaultScene;

	void Awake ()
	{
		string[] args = System.Environment.GetCommandLineArgs ();
		int sceneSelect = defaultScene;
		UnityEngine.SceneManagement.SceneManager.LoadScene ( sceneSelect + 1 ); // 0 is the loader
	}
}