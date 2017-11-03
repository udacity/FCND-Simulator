﻿using System.Collections;
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
			"Assets/Scenes/Launcher.unity",
			"Assets/Scenes/MainMenu.unity",
			"Assets/Scenes/ControlsMenu.unity",
			"Assets/Scenes/urban.unity",
			"Assets/CityMap/MapScene.unity",
		};

		string basePath = "Builds/QuadSim/";
		string settingPath = "QuadSim_Win_Data";

		string extension = "QuadSim_Win.exe";
		if ( target == BuildTarget.StandaloneOSXIntel64 )
		{
			extension = "QuadSim_OSX.app";
			settingPath = extension + "/Contents";
		} else
		if ( target == BuildTarget.StandaloneLinux64 )
		{
			extension = "QuadSim_Lin.x86_64";
			settingPath = "QuadSim_Lin_Data";
		}
		
		BuildPipeline.BuildPlayer ( scenes, basePath + extension, target, showBuilt ? BuildOptions.ShowBuiltPlayer : BuildOptions.None );
	}
}