using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BuildWindow : EditorWindow
{
	BuildAssets buildAssets;

	bool loaderSelected;
	bool menuSelected;
	bool[] selectedProjectScenes;
	bool[] selectedBuildTargets;
	string[] buildTargetNames;
	BuildTarget[] buildTargets;


	[MenuItem ("Udacity/Build Window", false, 100)]
	static void ShowBuildWindow ()
	{
		BuildWindow window = EditorWindow.GetWindow<BuildWindow> ();
		window.buildAssets = AssetDatabase.LoadAssetAtPath<BuildAssets> ( "Assets/BuildAssets.asset" );
		window.Init ();
		window.minSize = window.maxSize = new Vector2 ( 600, 500 );
		window.Show ();
	}

	void OnEnable ()
	{
		loaderSelected = false;
		menuSelected = false;
	}

	void Init ()
	{
		selectedProjectScenes = new bool[buildAssets.projectScenes.Length];
		#if UNITY_2017_3_OR_NEWER
		selectedBuildTargets = new bool[6];
		buildTargetNames = new string[] {
			"Windows 32-bit",
			"Windows 64-bit",
			"Mac",
			"Linux 32-bit",
			"Linux 64-bit",
			"Linux Universal"
		};
		buildTargets = new BuildTarget[] {
			BuildTarget.StandaloneWindows,
			BuildTarget.StandaloneWindows64,
			BuildTarget.StandaloneOSX,
			BuildTarget.StandaloneLinux,
			BuildTarget.StandaloneLinux64,
			BuildTarget.StandaloneLinuxUniversal
		};
		#else
		selectedBuildTargets = new bool[8];
		buildTargetNames = new string[] {
			"Windows 32-bit",
			"Windows 64-bit",
			"Mac 32-bit",
			"Mac 64-bit",
			"Mac Universal",
			"Linux 32-bit",
			"Linux 64-bit",
			"Linux Universal"
		};
		buildTargets = new BuildTarget[] {
			BuildTarget.StandaloneWindows,
			BuildTarget.StandaloneWindows64,
			BuildTarget.StandaloneOSXIntel,
			BuildTarget.StandaloneOSXIntel64,
			BuildTarget.StandaloneOSXUniversal,
			BuildTarget.StandaloneLinux,
			BuildTarget.StandaloneLinux64,
			BuildTarget.StandaloneLinuxUniversal
		};
		#endif
	}

	void OnGUI ()
	{
		GUIContent content = new GUIContent ( "Select the scene(s) and platform(s) to build for." );
		Vector2 titleSize = GUI.skin.label.CalcSize ( content );
		GUI.Label ( new Rect ( position.size.x / 2 - titleSize.x / 2, 20, titleSize.x, 25 ), content );

		// list the scenes to select

		Rect line = new Rect ( 20, 50, 150, 22 );
		loaderSelected = GUI.Toggle ( line, loaderSelected, "Loader Scene" );
		line.y += line.height;
		menuSelected = GUI.Toggle ( line, menuSelected, "Menu Scene" );
		line.y += line.height * 2;
		GUI.Label ( line, "Project Scenes:" );
		line.y += line.height;
		int scenesSelected = 0;
		for ( int i = 0; i < selectedProjectScenes.Length; i++ )
		{
			selectedProjectScenes [ i ] = GUI.Toggle ( line, selectedProjectScenes [ i ], buildAssets.projectScenes [ i ].title );
			line.y += line.height;
			if ( selectedProjectScenes [ i ] )
				scenesSelected++;
		}

		line.y += line.height;
		bool allScenes = loaderSelected && menuSelected && selectedProjectScenes.TrueForAll ( x => x );
		allScenes = GUI.Toggle ( line, allScenes, "Select All" );
		if ( allScenes )
		{
			loaderSelected = true;
			menuSelected = true;
			selectedProjectScenes.ForEach ( x => x = true );
		}


		// list the build targets to select
		line = new Rect ( position.width / 2, 50, 150, 22 );
		int buildTargetCount = 0;
		for ( int i = 0; i < selectedBuildTargets.Length; i++ )
		{
			selectedBuildTargets [ i ] = GUI.Toggle ( line, selectedBuildTargets [ i ], buildTargetNames [ i ] );
			if ( selectedBuildTargets [ i ] )
				buildTargetCount++;
			line.y += line.height;
		}

		#if !UNITY_2017_3_OR_NEWER
		line.y += line.height;
		bool allWindows = selectedBuildTargets [ 0 ] && selectedBuildTargets [ 1 ];
		allWindows = GUI.Toggle ( line, allWindows, "All Windows" );
		if ( allWindows )
			selectedBuildTargets [ 0 ] = selectedBuildTargets [ 1 ] = true;
		line.y += line.height;
		bool allMac = selectedBuildTargets [ 2 ] && selectedBuildTargets [ 3 ] && selectedBuildTargets [ 4 ];
		allMac = GUI.Toggle ( line, allMac, "All Mac" );
		if ( allMac )
			selectedBuildTargets [ 2 ] = selectedBuildTargets [ 3 ] = selectedBuildTargets [ 4 ] = true;
		line.y += line.height;
		bool allLinux = selectedBuildTargets [ 6 ] && selectedBuildTargets [ 7 ] && selectedBuildTargets [ 8 ];
		allLinux = GUI.Toggle ( line, allLinux, "All Linux" );
		if ( allLinux )
			selectedBuildTargets [ 5 ] = selectedBuildTargets [ 6 ] = selectedBuildTargets [ 7 ] = true;
		#else
		line.y += line.height;
		bool allWindows = selectedBuildTargets[0] && selectedBuildTargets[1];
		allWindows = GUI.Toggle ( line, allWindows, "All Windows" );
		if ( allWindows )
			selectedBuildTargets [ 0 ] = selectedBuildTargets [ 1 ] = true;
		line.y += line.height;
		bool allLinux = selectedBuildTargets [ 3 ] && selectedBuildTargets [ 4 ] && selectedBuildTargets [ 5 ];
		allLinux = GUI.Toggle ( line, allLinux, "All Linux" );
		if ( allLinux )
			selectedBuildTargets [ 3 ] = selectedBuildTargets [ 4 ] = selectedBuildTargets [ 5 ] = true;
		#endif
		
		line.y += line.height;
		line.y += line.height;

		if ( GUI.Button ( new Rect ( 10, position.height - 40, 100, 25 ), "Clear" ) )
		{
			loaderSelected = false;
			menuSelected = false;
			selectedProjectScenes.ForEach ( x => x = false );
			selectedBuildTargets.ForEach ( x => x = false );
			scenesSelected = 0;
			buildTargetCount = 0;
		}

		bool allSelected = selectedBuildTargets.TrueForAll ( x => x );
		allSelected = GUI.Toggle ( line, allSelected, "Select All" );
		if ( allSelected )
			selectedBuildTargets.ForEach ( x => x = true );

		if ( scenesSelected > 0 )
		{
			if ( buildTargetCount == 0 )
			{
				content = new GUIContent ( "Please select at least one platform to build to." );
				titleSize = GUI.skin.label.CalcSize ( content );
				GUI.Label ( new Rect ( position.size.x / 2 - titleSize.x / 2, position.height - 40, 300, 25 ), content );
			} else
			if ( GUI.Button ( new Rect ( position.size.x / 2 - 75, position.height - 40, 150, 25 ), "Build Selected" ) )
			{
				Build ();
			}
		} else
		if ( buildTargetCount > 0 )
		{
			content = new GUIContent ( "Please select at least one scene to build." );
			titleSize = GUI.skin.label.CalcSize ( content );
			GUI.Label ( new Rect ( position.size.x / 2 - titleSize.x / 2, position.height - 40, 300, 25 ), content );
		}
	}

	void Build ()
	{
		List<string> scenes = new List<string> ();
		if ( loaderSelected )
			scenes.Add ( AssetDatabase.GetAssetPath ( buildAssets.loaderScene ) );
		if ( menuSelected )
			scenes.Add ( AssetDatabase.GetAssetPath ( buildAssets.menuScene ) );
		for ( int i = 0; i < selectedProjectScenes.Length; i++ )
		{
			if ( selectedProjectScenes [ i ] )
				scenes.Add ( AssetDatabase.GetAssetPath ( buildAssets.projectScenes [ i ].scene ) );
		}

		int lastSelected = 0;
		for ( int i = selectedBuildTargets.Length - 1; i >= 0; i-- )
		{
			if ( selectedBuildTargets [ i ] )
			{
				lastSelected = i;
				break;
			}
		}

		int buildTargetCount = selectedBuildTargets.Count ( x => x );
		if ( buildTargetCount > 1 )
		{
			if ( !EditorUtility.DisplayDialog ( "Multiple Platforms Selected", "You selected to build for multiple platforms. This could take a while, are you sure?", "Confirmative", "Negatory" ) )
				return;
		}
		
//		int selectedCount = 0;
//		selectedBuildTargets.ForEach ( x =>
//		{
//			if ( x )
//				selectedCount++;
//		} );

		string[] sceneArr = scenes.ToArray ();
		for ( int i = 0; i < selectedBuildTargets.Length; i++ )
		{
			if ( selectedBuildTargets [ i ] )
			{
				Build ( sceneArr, buildTargets [ i ], buildTargetNames [ i ].Replace ( " ", "_" ), i == lastSelected );
			}
		}
	}

	void Build (string[] scenes, BuildTarget target, string buildName, bool showBuilt)
	{
		string basePath = "Builds/FCND-Sim/";
		string extension = "FCND-Sim_" + buildName + ".exe";
		string settingPath = "FCND-Sim_" + buildName + "_Data";
//		string settingPath = "FCND-Sim_Windows_Data";
//		string extension = "FCND-Sim_Windows.exe";

		#if UNITY_2017_3_OR_NEWER
		if ( target == BuildTarget.StandaloneOSX )
		{
			extension = "FCND-Sim_MacOS.app";
			settingPath = extension + "/Contents";
		} else
		if ( target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal )
		{
			extension = "FCND-Sim_" + buildName;
			settingPath = "FCND-Sim_" + buildName + "_Data";
//			extension = "FCND-Sim_Linux";
//			settingPath = "FCND-Sim_Linux_Data";
		}
		#else
		if ( target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXIntel64 || target == BuildTarget.StandaloneOSXUniversal )
		{
			extension = "FCND-Sim_" + buildName + ".app";
			settingPath = extension + "/Contents";
//			extension = "FCND-Sim_MacOS.app";
//			settingPath = extension + "/Contents";
		} else
		if ( target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal )
		{
			extension = "FCND_Sim_" + buildName;
			settingPath = "FCND-Sim_" + buildName + "_Data";
//			extension = "FCND-Sim_Linux";
//			settingPath = "FCND-Sim_Linux_Data";
		}
		#endif

		BuildPipeline.BuildPlayer ( scenes, basePath + extension, target, showBuilt ? BuildOptions.ShowBuiltPlayer : BuildOptions.None );
	}
}