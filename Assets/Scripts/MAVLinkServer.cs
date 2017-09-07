using System;
using UnityEngine;
using UnityEngine.Networking;

public class MAVLinkServer : MonoBehaviour {

	private int _port = 1337;

	public void SetupServer() {
		NetworkServer.Listen (_port);
		Debug.Log (string.Format ("Starting MAVLink Server ... listening on port {0}", _port));
	}

	public void OnConnected(NetworkMessage msg) {
		Debug.Log (string.Format ("Connected to server - {0}", msg));
	}

	void Start() {
		SetupServer ();
	}

}

