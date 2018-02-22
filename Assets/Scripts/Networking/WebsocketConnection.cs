using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdacityNetworking;

// for now, the websocket will only run a client, and in such a case python will run the server

namespace UdacityNetworking
{
	public class WebsocketConnection : NetworkConnection
	{
		public NetworkController Controller { get; set; }
		public ConnectionState ConnectionState { get { return socket != null ? ( (ConnectionState) (int) socket.State ) : ConnectionState.Disconnected; } }
		public bool IsServerStarted { get { return false; } }
		public bool IsConnected { get { return connected; } }

		WebSocket socket;
		float reconnectDelay = 5;
		bool connected;

		event Action<MessageInfo> messageHandler = delegate { };

		public void StartServer (string ip, int port)
		{
			throw new NotImplementedException ( "WebSocket server not supported at this time." );
		}

		public void Connect (string ip, int port)
		{
			connected = false;
			Debug.Log ( "websocket connecting to " + ip );
			socket = new WebSocket ( new Uri ( ip ) );
			Controller.StartCoroutine ( socket.Connect () );
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
			socket.Send ( message );
		}

		public void DoUpdate ()
		{
			if ( socket == null )
				return;
			
			if ( !string.IsNullOrWhiteSpace ( socket.error ) )
			{
				Debug.LogError ( "Socket error: `" + socket.error + "`, reconnecting in " + reconnectDelay + " seconds." );
				socket = null;
				return;
			}
			if ( socket.State == WebSocketState.Closed || socket.State == WebSocketState.Closing )
			{
				if ( connected )
					connected = false;
				Debug.Log ( "Socket is closed or closing." );
				return;
			}
			if ( socket.State == WebSocketState.Open )
			{
				if ( !connected )
					connected = true;
				byte[] message = socket.Receive ();
				if ( message != null )
				{
					messageHandler ( new MessageInfo ( message ) );
				}
			}
		}

		public void Destroy ()
		{
			if ( socket.State == WebSocketState.Connecting || socket.State == WebSocketState.Open )
			{
				socket.Close ();
				socket = null;
			}
		}
	}
}