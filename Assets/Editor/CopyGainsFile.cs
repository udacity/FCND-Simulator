using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class CopyGainsFile
{
	[PostProcessBuild]
	public static void OnBuildCopyGainsFile (BuildTarget target, string buildPath)
	{
		string fileName = "gains.txt";
		string location = buildPath;
		if ( target != BuildTarget.StandaloneOSX )
			location = location.Substring ( 0, location.LastIndexOf ( '/' ) + 1 );
//		if ( target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64 )
//			location += "/../";
//		else
//		if ( target == BuildTarget.StandaloneOSX )
//			location += "/";
//		else
//		if ( target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinux64 || target == BuildTarget.StandaloneLinuxUniversal )
//			location += "/../";

		string dest = Path.Combine ( location, fileName );
		string source = Application.dataPath + "/Resources/" + fileName;
		File.Copy ( source, dest );
		Debug.Log ( "file copied to " + dest );
	}
}