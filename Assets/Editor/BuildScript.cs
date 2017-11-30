using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class BuildScript : MonoBehaviour
{
	[MenuItem ("Udacity/Build Windows", false, 20)]
	static void BuildWindows ()
	{
		Build ( BuildTarget.StandaloneWindows64 );
	}

	[MenuItem ("Udacity/Build Mac", false, 21)]
	static void BuildMac ()
	{
		Build ( BuildTarget.StandaloneOSXIntel64 );
	}

	[MenuItem ("Udacity/Build Linux", false, 22)]
	static void BuildLinux ()
	{
		Build ( BuildTarget.StandaloneLinux64 );
	}

	[MenuItem ("Udacity/Build All", false, 23)]
	static void BuildAll ()
	{
		Build ( BuildTarget.StandaloneWindows64, false );
		Build ( BuildTarget.StandaloneOSXIntel64, false );
		Build ( BuildTarget.StandaloneLinux64 );
	}


	static void Build (BuildTarget target, bool showBuilt = true)
	{
		string[] scenes = new string[] {
			// "Assets/Scenes/Launcher.unity",
			"Assets/Scenes/MainMenu.unity",
//			"Assets/Scenes/ControlsMenu.unity",
			"Assets/Scenes/urban.unity",
			"Assets/Scenes/MapScene2.unity",
		};

		string basePath = "Builds/FlyingCarND-Sim/";
		string settingPath = "FlyingCarND-Sim_Win_Data";

		string extension = "FlyingCarND-Sim_Win.exe";
		if ( target == BuildTarget.StandaloneOSXIntel64 )
		{
			extension = "FlyingCarND-Sim_OSX.app";
			settingPath = extension + "/Contents";
		} else
		if ( target == BuildTarget.StandaloneLinux64 )
		{
			extension = "FlyingCarND-Sim_Lin.x86_64";
			settingPath = "FlyingCarND-Sim_Lin_Data";
		}
		
		BuildPipeline.BuildPlayer ( scenes, basePath + extension, target, showBuilt ? BuildOptions.ShowBuiltPlayer : BuildOptions.None );
	}
}