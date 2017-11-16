using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UdacityNetworking;
using System.Collections.Concurrent;

namespace UdacityNetworking
{
	class ClientInfo
	{
		public int clientHash;
		public TcpClient client;
		public CancellationTokenSource cts;
		public CancellationToken token;
		public float lastRead;

		public ClientInfo (TcpClient c, Action<object> cancellationCallback)
		{
			client = c;
			clientHash = c.GetHashCode ();
			cts = new CancellationTokenSource ();
			token = cts.Token;
			token.Register ( cancellationCallback, this );
			OnRead ();
		}

		public void OnRead ()
		{
			lastRead = Time.unscaledTime;
		}

		public bool IsTimeout ()
		{
			return Time.unscaledTime - lastRead >= 5f;
		}

		public void CancelAndDispose ()
		{
			if ( !cts.IsCancellationRequested && client.Connected )
			{
				cts.Cancel ();
				if ( client != null )
				{
					var ns = client.GetStream ();
					if ( ns != null )
						ns.Close ();
					client.Close ();
				}
			}
		}
	}

	public class TCPConnection : NetworkConnection
	{
		public bool IsServerStarted { get { return listener != null; } }
		public bool IsConnected { get { return myClient != null && myClient.Connected; } }

		ConcurrentDictionary<int, ClientInfo> clients = new ConcurrentDictionary<int, ClientInfo> ();
//		ConcurrentDictionary<int, TcpClient> clients = new ConcurrentDictionary<int, TcpClient> ();
		string ip;
		int port;
		bool running;
		event Action<MessageInfo> messageHandler = delegate { };
		TcpListener listener;
		TcpClient myClient;
		ConcurrentQueue<MessageInfo> messages = new ConcurrentQueue<MessageInfo> ();
		ConcurrentQueue<string> errors = new ConcurrentQueue<string> ();
		float nextTimeoutCheck;

		public void StartServer (string ip, int port)
		{
			running = true;
			this.ip = ip;
			this.port = port;
			TcpListenAsync ();
			DispatchMessages ();
		}

		public void DoUpdate ()
		{
			if ( Input.GetKeyDown ( KeyCode.K ) )
			{
				OutputClientList ();
			}
			if ( Input.GetKeyDown ( KeyCode.L ) )
			{
				OutputErrors ();
			}


			if ( Time.unscaledTime > nextTimeoutCheck )
			{
				var keys = clients.Keys;
				var _keys = new int[keys.Count];
				keys.CopyTo ( _keys, 0 );
				ClientInfo dummy;
				foreach ( var key in _keys )
					if ( clients [ key ].IsTimeout () )
					{
						clients [ key ].CancelAndDispose ();
						bool removed = clients.TryRemove ( key, out dummy );
						Debug.LogWarning ( "timeout remove client " + key + " successful: " + removed );
					}
				nextTimeoutCheck = Time.unscaledTime + 0.5f;
			}
		}

		public void Connect (string ip, int port)
		{
			myClient = new TcpClient ();
			myClient.ConnectAsync ( ip, port );
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
			messages.Enqueue ( new MessageInfo ( message, destIP, destPort ) );
		}

		public async Task DispatchMessages ()
		{
			// make sure server is started
			while ( !IsServerStarted )
				await Task.Delay ( 10 );
			
			while ( IsServerStarted )
			{
//				Debug.Log ( "checking messages" );
				ClientInfo[] clientArr = new ClientInfo[clients.Count];
//				TcpClient[] clientArr = new TcpClient[clients.Count];
				clients.Values.CopyTo ( clientArr, 0 );
//				var clientArr = clients.ToArray ();
				while ( !messages.IsEmpty )
				{
					MessageInfo msg = null;
					if ( messages.TryDequeue ( out msg ) && msg != null )
					{
						foreach ( var ci in clientArr )
//						foreach ( var client in clientArr )
						{
							TcpClient c = ci.client;
							if ( c != null && c.Connected )
							{
								var stream = c.GetStream ();
								if ( running && stream != null && stream.CanWrite ) //&& stream.CanRead )
									stream.Write ( msg.message, 0, msg.message.Length );
							}
						}
					}
				}
				await Task.Delay ( 10 );
			}
		}

		// Starts an HTTP server and listens for new client connections.
		async Task TcpListenAsync()
		{
			try
			{
				if ( listener != null )
				{
					listener.Stop ();
					listener = null;
				}
				// Setup the TcpListener 
				var addr = IPAddress.Parse(ip);
				listener = new TcpListener(addr, port);
				// Start listening for client requests.
				listener.Start ();
				Debug.Log ("Starting TCP MAVLink server ...");

				while (running)
				{
					var client = await listener.AcceptTcpClientAsync ();
					Debug.Log ("Accepted a connection.");
					ClientInfo ci = new ClientInfo ( client, OnClientTimeout );
					ClientInfo dummy;
					foreach ( var pair in clients )
					{
						if ( pair.Value.client.Equals ( client ) && pair.Key != client.GetHashCode () )
						{
							Debug.LogWarning ( "removing an old identical client" );
							clients.TryRemove ( pair.Key, out dummy );
							break;
						}
					}
					clients[ client.GetHashCode () ] = ci;
//					if ( !clients.TryAdd ( client.GetHashCode (), client ) )
//						clients [ client.GetHashCode () ] = client;
//					lock ( clientLock )
//					{
//						clients.Add ( client );
//					}
					HandleClient ( client );
				}
			}
			catch (SocketException e)
			{
				errors.Enqueue ( string.Format ( "SocketException: {0}", e ) );
//				Debug.Log (string.Format("SocketException: {0}", e));
				listener.Stop ();
				listener = null;
			}
			finally
			{
				// this is to ensure the sever stops once a disconnection happens, or when done with everything
			}
		}

		async Task HandleClient (TcpClient client)
		{
			NetworkStream stream = client.GetStream ();
			while ( running && client != null && client.Connected && stream != null && stream.CanRead )
			{
//				Debug.Log ( "Reading from stream ... " );
				var buf = new byte[1024];

				var bytesRead = await stream.ReadAsync ( buf, 0, buf.Length );
				if ( bytesRead > 0 )
				{
					var dest = new byte[bytesRead];
					Array.Copy ( buf, dest, bytesRead );
					clients [ client.GetHashCode () ].OnRead (); // update the last read time from this client
					messageHandler ( new MessageInfo ( dest ) );
//					mav.ParseBytesV2 ( dest );
				} else
				{
					Debug.LogWarning ( "stream ended" );
					break;
				}
				errors.Enqueue ( "post stream read" );
			}
			if ( stream != null )
				stream.Close ();
			if ( client != null )
			{
				foreach ( var c in clients.Values )
				{
					if ( c != null )
						Debug.LogWarning ( c.GetHashCode () + " " + c.client.Connected );
					else
						Debug.LogWarning ( "a null client" );
				}
				int hash = client.GetHashCode ();
				ClientInfo dummy = null;
				Debug.LogWarning ( "trying to remove client " + hash );
				if ( !clients.TryRemove ( hash, out dummy ) )
//				if ( !clients.TryRemove ( hash, out client ) )
					errors.Enqueue ( "couldn't remove a client? " + hash + " " + ( dummy == null ).ToString () );
//					Debug.LogError ( "couldn't remove a client? " + hash + " " + ( client == null ).ToString () );
				client.Close ();
//				lock ( clientLock )
//				{
//					clients.Remove ( client );
//				}
			} else
			{
				Debug.LogWarning ("null client?");
			}
		}

		void OnDestroy ()
		{
			running = false;
			listener.Stop ();
			listener = null;
		}

		void OnClientTimeout (object obj)
		{
			ClientInfo info = (ClientInfo) obj;
			TcpClient client;
			ClientInfo ci;
			bool removed = clients.TryRemove ( info.clientHash, out ci );
//			bool removed = clients.TryRemove ( info.clientHash, out client );
			Debug.LogWarning ( "client " + info.clientHash + " was removed? " + removed );
		}

		public void OutputClientList ()
		{
			if ( clients.IsEmpty )
				Debug.Log ( "no clients" );
			foreach ( var pair in clients )
			{
				Debug.Log ( pair.Value == null ? "null client" : ( pair.Key + " " + pair.Value.client.Connected ) );
			}
		}

		public void OutputErrors ()
		{
			if ( errors.IsEmpty )
				Debug.LogWarning ( "no errors" );
			while ( !errors.IsEmpty )
			{
				string s = "";
				if ( errors.TryDequeue ( out s ) )
					Debug.LogError ( s );
			}
		}
	}
}