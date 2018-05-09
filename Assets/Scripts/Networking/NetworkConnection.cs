using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UdacityNetworking
{
	public class MessageInfo
	{
		public string ip;
		public int port;
		public byte[] message;

		public MessageInfo (byte[] msg, string _ip = "", int _port = -1)
		{
			ip = _ip;
			port = _port;
			message = msg;
		}
	}

	public interface INetworkConnection
	{
		ConnectionState ConnectionState { get; }
		NetworkController Controller { get; set; }
		bool IsServerStarted { get; }
		bool IsConnected { get; }

		void StartServer ( string ip, int port );
		void Connect ( string ip, int port );
		void AddMessageHandler ( Action<MessageInfo> handler );
		void RemoveMessageHandler ( Action<MessageInfo> handler );
		void SendMessage (byte[] message, string destIP = "", int destPort = -1);
		void DoUpdate ();
		void Destroy ();
	}
}