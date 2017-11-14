using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdacityNetworking;

namespace UdacityNetworking
{
	public class WebsocketConnection : NetworkConnection
	{
		public bool IsServerStarted { get { return false; } }
		public bool IsConnected { get { return false; } }

		event Action<MessageInfo> messageHandler = delegate { };

		public void StartServer (string ip, int port)
		{
			throw new NotImplementedException ();
		}

		public void Connect (string ip, int port)
		{
			throw new NotImplementedException ();
		}

		public void AddMessageHandler (Action<MessageInfo> handler)
		{
			messageHandler += handler;
		}

		public void RemoveMessageHandler (Action<MessageInfo> handler)
		{
			messageHandler -= handler;
		}

		public void SendMessage (byte[] message, string destIP = "", int destPort = -1)
		{
			throw new NotImplementedException ();
		}
	}
}