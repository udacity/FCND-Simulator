using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InputsInstall : Editor {

	[MenuItem ("Window/Usim/Auto-configure Inputs")]
	static void InstallStandardInputs () {

		AssetDatabase.CopyAsset("Assets/uSim_framework/Configuration files/InputManager.asset", "Assets/../ProjectSettings/InputManager.asset");
		AssetDatabase.Refresh();
		Debug.Log ("Project InputManager.asset has been updated. Restart project");
	}
}
