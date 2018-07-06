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
//		Debug.Log ( "build path " + buildPath );
		string fileName = "gains.txt";
		string location = buildPath;
		#if UNITY_2017_3_OR_NEWER
		if ( target == BuildTarget.StandaloneOSX )
		#else
		if ( target == BuildTarget.StandaloneOSXIntel64 )
		#endif
			location = location.TrimEnd ( '/' ) + "/Contents";
		else
			location = location.Substring ( 0, location.LastIndexOf ( '/' ) + 1 );

		string dest = Path.Combine ( location, fileName );
		string source = Application.dataPath + "/Resources/" + fileName;
		File.Copy ( source, dest, true );
		Debug.Log ( "file copied to " + dest );
	}
}