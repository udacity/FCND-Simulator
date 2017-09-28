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
		if (Input.GetKey(KeyCode.Escape)) {
			Debug.Log("reloading scene");
			SceneManager.LoadScene("urban");
		}
	}

	public void OnModeSelect(int mode)
	{
	}

	public void OnExitButton()
	{
	}
}