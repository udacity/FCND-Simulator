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
	class TCPClientInfo
	{
		public int clientHash;
		public TcpClient client;
		public CancellationTokenSource cts;
		public CancellationToken token;
		public float lastRead;

		public TCPClientInfo (TcpClient c, Action<object> cancellationCallback)
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
		public NetworkController Controller { get; set; }
		public ConnectionState ConnectionState { get { return connectionState; } }
		public bool IsServerStarted { get { return listener != null; } }
		public bool IsConnected { get { return myClient != null && myClient.Connected; } }

		ConcurrentDictionary<int, TCPClientInfo> clients = new ConcurrentDictionary<int, TCPClientInfo> ();
//		ConcurrentDictionary<int, TcpClient> clients = new ConcurrentDictionary<int, TcpClient> ();
		string ip;
		int port;
		bool running;
		event Action<MessageInfo> messageHandler = delegate { };
		TcpListener listener;
		TcpClient myClient;
		ConcurrentQueue<MessageInfo> messages = new ConcurrentQueue<MessageInfo> ();
		float nextTimeoutCheck;
		ConnectionState connectionState;

		public void StartServer (string ip, int port)
		{
			running = true;
			this.ip = ip;
			this.port = port;
			connectionState = ConnectionState.Connecting;
			TcpListenAsync ();
			Task.Run ( DispatchMessages );
//			DispatchMessages ();
		}

		public void DoUpdate ()
		{
			if ( Time.unscaledTime > nextTimeoutCheck )
			{
				var keys = clients.Keys;
				var _keys = new int[keys.Count];
				keys.CopyTo ( _keys, 0 );
				TCPClientInfo dummy;
				foreach ( var key in _keys )
					if ( clients [ key ].IsTimeout () )
					{
						clients [ key ].CancelAndDispose ();
						bool removed = clients.TryRemove ( key, out dummy );
						Debug.LogWarning ( "timeout remove client " + key + " successful: " + removed );
					}
				nextTimeoutCheck = Time.unscaledTime + NetworkController.Timeout;
			}
			if ( connectionState == ConnectionState.Connected && myClient != null && !myClient.Connected )
				connectionState = ConnectionState.Disconnected;
		}

		public void Connect (string ip, int port)
		{
			myClient = new TcpClient ();
			_Connect ( ip, port );
		}
		async Task _Connect (string ip, int port)
		{
			connectionState = ConnectionState.Connecting;
			await myClient.ConnectAsync ( ip, port );
			connectionState = ConnectionState.Connected;
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

//			SpinWait sw = new SpinWait ();
//			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch ();
			while ( IsServerStarted )
			{
//				Debug.Log ( "checking messages" );
				TCPClientInfo[] clientArr = new TCPClientInfo[clients.Count];
//				TcpClient[] clientArr = new TcpClient[clients.Count];
				clients.Values.CopyTo ( clientArr, 0 );
//				var clientArr = clients.ToArray ();
				try
				{
					while ( !messages.IsEmpty )
					{
						MessageInfo msg = null;
						if ( messages.TryDequeue ( out msg ) && msg != null )
						{
							foreach ( var ci in clientArr )
//							foreach ( var client in clientArr )
							{
								TcpClient c = ci.client;
								if ( c != null && c.Connected )
								{
									var stream = c.GetStream ();
									if ( running && stream != null && stream.CanWrite ) //&& stream.CanRead )
										await stream.WriteAsync ( msg.message, 0, msg.message.Length );
//										stream.Write ( msg.message, 0, msg.message.Length );
								}
							}
						}
					}
					
				}
				catch (Exception e)
				{
					Debug.LogException ( e );
				}
//				sw.Reset ();
//				watch.Reset ();
//				watch.Start ();
//				while ( watch.ElapsedMilliseconds <= 2 )
//					sw.SpinOnce ();
//				watch.Stop ();
//				Debug.Log ( "spun " + sw.Count + " times" );

				await Task.Delay ( 2 );
			}
		}

		// Starts an TCP server and listens for new client connections.
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
				Debug.Log ("Starting TCP server ...");
				connectionState = ConnectionState.Connected;

				while ( running )
				{
					var client = await listener.AcceptTcpClientAsync ();
					Debug.Log ("Accepted a connection.");
					TCPClientInfo ci = new TCPClientInfo ( client, OnClientTimeout );
//					TCPClientInfo dummy;
//					foreach ( var pair in clients )
//					{
//						if ( pair.Value.client.Equals ( client ) && pair.Key != client.GetHashCode () )
//						{
//							Debug.LogWarning ( "removing an old identical client" );
//							clients.TryRemove ( pair.Key, out dummy );
//							break;
//						}
//					}
					clients[ client.GetHashCode () ] = ci;
					HandleClient ( client );
				}
			}
			catch (SocketException e)
			{
				Debug.LogException ( e );
//				Debug.Log (string.Format("SocketException: {0}", e));
				listener.Stop ();
				listener = null;
				connectionState = ConnectionState.Disconnected;
			}
			finally
			{
				// this is to ensure the sever stops once a disconnection happens, or when done with everything
				if ( listener != null )
				{
					listener.Stop ();
					listener = null;
				}
					connectionState = ConnectionState.Disconnected;
			}
		}

		async Task HandleClient (TcpClient client)
		{
			var buf = new byte[16384];
			NetworkStream stream = client.GetStream ();
			while ( running && client != null && client.Connected && stream != null && stream.CanRead )
			{
//				Debug.Log ( "Reading from stream ... " );

				var bytesRead = await stream.ReadAsync ( buf, 0, buf.Length );
				if ( bytesRead > 0 )
				{
					var dest = new byte[bytesRead];
					Array.Copy ( buf, dest, bytesRead );
					clients [ client.GetHashCode () ].OnRead (); // update the last read time from this client
					messageHandler ( new MessageInfo ( dest ) );
				} else
				{
					Debug.Log ( "stream ended" );
					break;
				}
			}
			if ( stream != null )
				stream.Close ();
			if ( client != null )
			{
				int hash = client.GetHashCode ();
				TCPClientInfo dummy = null;
				Debug.LogWarning ( "trying to remove client " + hash );
				if ( !clients.TryRemove ( hash, out dummy ) )
					Debug.LogError ( "couldn't remove a client? " + hash + " " + ( dummy == null ).ToString () );
				client.Close ();
			} else
			{
				Debug.LogWarning ("null client?");
			}
		}

		public void Destroy ()
		{
			running = false;
			if ( listener != null )
			{
				listener.Stop ();
				listener = null;
			}
			if ( myClient != null )
			{
				myClient.Close ();
			}
		}

		void OnClientTimeout (object obj)
		{
			TCPClientInfo info = (TCPClientInfo) obj;
			TcpClient client;
			TCPClientInfo ci;
			bool removed = clients.TryRemove ( info.clientHash, out ci );
//			bool removed = clients.TryRemove ( info.clientHash, out client );
			Debug.LogWarning ( "client " + info.clientHash + " was removed? " + removed );
		}
	}
}