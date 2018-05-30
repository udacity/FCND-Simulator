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
using Encoding = System.Text.Encoding;

namespace UdacityNetworking
{
	//Server
	class UdpListener
	{
		UdpClient client;
		private IPEndPoint _listenOn;

		public UdpListener (IPAddress addr, int port)
		{
			_listenOn = new IPEndPoint ( addr, port );
			client = new UdpClient ( _listenOn );
		}

		public async Task<UdpReceiveResult> ReceiveAsync ()
		{
			return await client.ReceiveAsync ();
		}

		public async Task SendMessage (byte[] bytes, IPEndPoint target)
		{
			await client.SendAsync ( bytes, bytes.Length, target );
		}

		public void Stop ()
		{
			client.Close ();
			client.Dispose ();
			client = null;
			_listenOn = null;
		}
	}

	class UDPClientInfo
	{
		public int clientHash;
		public IPEndPoint client;
		public float lastRead;

		public UDPClientInfo (IPEndPoint ep)
		{
			client = ep;
			clientHash = ep.GetHashCode ();
			OnRead ();
		}

		public void OnRead ()
		{
			lastRead = Time.unscaledTime;
		}

		public bool IsTimeout ()
		{
			return Time.unscaledTime - lastRead >= NetworkController.Timeout;
		}

		public void CancelAndDispose ()
		{
			// nothing to really do here
//			if ( !cts.IsCancellationRequested && client.Connected )
//			{
//				cts.Cancel ();
//				if ( client != null )
//				{
//					var ns = client.GetStream ();
//					if ( ns != null )
//						ns.Close ();
//					client.Close ();
//				}
//			}
		}
	}

	public class UDPConnection : INetworkConnection
	{
		public NetworkController Controller { get; set; }
		public ConnectionState ConnectionState { get { return connectionState; } }
		public bool IsServerStarted { get { return listener != null; } }
		public bool IsConnected { get { return myClient != null && myClient.Client.Connected; } }

		ConcurrentDictionary<int, UDPClientInfo> clients = new ConcurrentDictionary<int, UDPClientInfo> ();
		string ip;
		int port;
		bool running;
		event Action<MessageInfo> messageHandler = delegate { };
		UdpListener listener;
		UdpClient myClient;
		ConcurrentQueue<MessageInfo> messages = new ConcurrentQueue<MessageInfo> ();
		float nextTimeoutCheck;
		ConnectionState connectionState;

		public void StartServer (string ip, int port)
		{
			running = true;
			this.ip = ip;
			this.port = port;
			connectionState = ConnectionState.Connecting;
//			Task.Run ( UdpListenAsync );
			Task.Run ( DispatchMessages );
			UdpListenAsync ();
//			DispatchMessages ();
		}

		public void StopServer ()
		{
			running = false;
			listener.Stop ();
			listener = null;
		}

		public void DoUpdate ()
		{
			if ( Time.unscaledTime > nextTimeoutCheck )
			{
				var keys = clients.Keys;
				var _keys = new int[keys.Count];
				keys.CopyTo ( _keys, 0 );
				UDPClientInfo dummy;
				foreach ( var key in _keys )
					if ( clients [ key ].IsTimeout () )
					{
						clients [ key ].CancelAndDispose ();
						bool removed = clients.TryRemove ( key, out dummy );
						Debug.LogWarning ( "timeout remove client " + key + " successful: " + removed );
					}
				nextTimeoutCheck = Time.unscaledTime + 5f;
//				nextTimeoutCheck = Time.unscaledTime + NetworkController.Timeout;
			}
			if ( connectionState == ConnectionState.Connected && myClient != null && !myClient.Client.Connected )
				connectionState = ConnectionState.Disconnected;
		}

		public void Connect (string ip, int port)
		{
			connectionState = ConnectionState.Connecting;
			myClient = new UdpClient ( ip, port );
			WaitConnect ();

		}
		async Task WaitConnect ()
		{
			while ( !myClient.Client.Connected )
				await Task.Delay ( 5 );
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
			
			while ( IsServerStarted )
			{
				Debug.Log ( "checking messages" );
				UDPClientInfo[] clientArr = new UDPClientInfo[clients.Count];
				clients.Values.CopyTo ( clientArr, 0 );
				try
				{
					while ( !messages.IsEmpty )
					{
						MessageInfo msg = null;
						if ( messages.TryDequeue ( out msg ) && msg != null )
						{
							foreach ( var ci in clientArr )
							{
								if ( ci != null && !ci.IsTimeout () )
								{
									await listener.SendMessage ( msg.message, ci.client );
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogException ( e );
				}
				await Task.Delay ( 10 );
			}
		}

		// Starts an UDP server and listens for new client connections.
		async Task UdpListenAsync()
		{
			try
			{
				if ( listener != null )
				{
					listener.Stop ();
					listener = null;
				}
				// Setup the UdpListener 
				var addr = IPAddress.Parse ( ip );
				listener = new UdpListener ( addr, port );
				// Start listening for client requests.
//				listener.Start ();
				Debug.Log ( "Starting UDP server ..." );
				connectionState = ConnectionState.Connected;

				while ( running )
				{
					// udp is essentially connection-less, so receiving will just check if there's a new ip/port 
					// clients will be identified by the hash code of their IPEndPoint
					var receive = await listener.ReceiveAsync ();
					Debug.Log ( "received something" );
					UDPClientInfo clientInfo;
					if ( receive.Buffer.Length > 0 )
					{
						if ( !clients.TryGetValue ( receive.RemoteEndPoint.GetHashCode (), out clientInfo ) )
						{
							Debug.Log ( "Accepted a connection." );
							clientInfo = new UDPClientInfo ( receive.RemoteEndPoint );
							clients [ clientInfo.clientHash ] = clientInfo;
						}
						clientInfo.OnRead ();
						messageHandler ( new MessageInfo ( receive.Buffer ) );
						
					} else
					{
						Debug.Log ( "received an empty message (close), attempting to remove client" );
						clients.TryRemove ( receive.RemoteEndPoint.GetHashCode (), out clientInfo );
						if ( clientInfo != null )
							clientInfo.CancelAndDispose ();
					}
				}
			}
			catch ( SocketException e )
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
				connectionState = ConnectionState.Disconnected;
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
				myClient.Close ();
		}

		void OnClientTimeout (object obj)
		{
			UDPClientInfo info = (UDPClientInfo) obj;
			UDPClientInfo ci;
			bool removed = clients.TryRemove ( info.clientHash, out ci );
//			bool removed = clients.TryRemove ( info.clientHash, out client );
			Debug.LogWarning ( "client " + info.clientHash + " was removed? " + removed );
		}
	}
}