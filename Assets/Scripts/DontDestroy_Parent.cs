using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy_Parent : MonoBehaviour
{
	static DontDestroy_Parent instance;

	public bool useSingleton = true;
	public bool activateChildren = true;
	
	void Awake ()
	{
		if ( instance != null && useSingleton )
		{
			Destroy ( gameObject );
			return;
		}

		instance = this;
		DontDestroyOnLoad ( gameObject );
		if ( activateChildren )
		{
			foreach ( Transform t in transform )
				t.gameObject.SetActive ( true );
		}
	}
}