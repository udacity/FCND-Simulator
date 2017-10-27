using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
	void Start()
	{
		DontDestroyOnLoad(this);
	}

	void LateUpdate()
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
			if (Input.GetKeyDown(KeyCode.R)) {
				print("RELOADING SCENE");
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
		}
	}

	public void OnModeSelect(int mode)
	{
	}

	public void OnExitButton()
	{
	}
}