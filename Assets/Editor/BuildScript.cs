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
		Build ( BuildTarget.StandaloneWindows );
	}

	[MenuItem ("Udacity/Build Mac", false, 21)]
	static void BuildMac ()
	{
		Build ( BuildTarget.StandaloneOSX );
	}

	[MenuItem ("Udacity/Build Linux", false, 22)]
	static void BuildLinux ()
	{
		Build ( BuildTarget.StandaloneLinuxUniversal );
	}

	[MenuItem ("Udacity/Build All", false, 23)]
	static void BuildAll ()
	{
		Build ( BuildTarget.StandaloneWindows, false );
		Build ( BuildTarget.StandaloneOSX, false );
		Build ( BuildTarget.StandaloneLinuxUniversal, false );
	}


	static void Build (BuildTarget target, bool showBuilt = true)
	{
		string[] scenes = new string[] {
			// "Assets/Scenes/Launcher.unity",
			// "Assets/Scenes/MainMenu.unity",
//			"Assets/Scenes/ControlsMenu.unity",
			"Assets/Scenes/urban.unity",
			// "Assets/Scenes/MapScene2.unity",
		};

		string basePath = "Builds/FCND-Sim/";
		string settingPath = "FCND-Sim_Windows_Data";

		string extension = "FCND-Sim_Windows.exe";
		if ( target == BuildTarget.StandaloneOSX)
		{
			extension = "FCND-Sim_MacOS.app";
			settingPath = extension + "/Contents";
		} else
		if ( target == BuildTarget.StandaloneLinuxUniversal )
		{
			extension = "FCND-Sim_Linux";
			settingPath = "FCND-Sim_Linux_Data";
		}
		
		BuildPipeline.BuildPlayer ( scenes, basePath + extension, target, showBuilt ? BuildOptions.ShowBuiltPlayer : BuildOptions.None );
	}
}