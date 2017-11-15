using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UdacityNetworking;
using System.Collections.Concurrent;

namespace UdacityNetworking
{
	public class TCPConnection : NetworkConnection
	{
		public bool IsServerStarted { get { return listener != null; } }
		public bool IsConnected { get { return client != null && client.Connected; } }

		List<TcpClient> clients = new List<TcpClient> ();
		object clientLock = new object ();
//		ConcurrentDictionary<int, TcpClient> clients = new ConcurrentDictionary<int, TcpClient> ();
//		ConcurrentBag<TcpClient> clients = new ConcurrentBag<TcpClient>();
		string ip;
		int port;
		bool running;
		event Action<MessageInfo> messageHandler = delegate { };
		TcpListener listener;
		TcpClient client;
		ConcurrentQueue<MessageInfo> messages = new ConcurrentQueue<MessageInfo> ();

		void Start ()
		{
		}

		void Update ()
		{
		}

		public void StartServer (string ip, int port)
		{
			running = true;
			this.ip = ip;
			this.port = port;
			TcpListenAsync ();
			DispatchMessages ();
		}

		public void Connect (string ip, int port)
		{
			client = new TcpClient ();
			client.ConnectAsync ( ip, port );
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
//				TcpClient[] clientArr = new TcpClient[clients.Count];
//				clients.Values.CopyTo ( clientArr, 0 );
				var clientArr = clients.ToArray ();
				while ( !messages.IsEmpty )
				{
					MessageInfo msg = null;
					if ( messages.TryDequeue ( out msg ) && msg != null )
					{
						foreach ( var client in clientArr )
						{
							if ( client != null && client.Connected )
							{
								var stream = client.GetStream ();
								if ( running && stream != null && stream.CanWrite && stream.CanRead )
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
					var client = await listener.AcceptTcpClientAsync();
					Debug.Log ("Accepted a connection.");
//					if ( !clients.TryAdd ( client.GetHashCode (), client ) )
//						clients [ client.GetHashCode () ] = client;
					lock ( clientLock )
					{
						clients.Add ( client );
					}
					HandleClient ( client );
				}
			}
			catch (SocketException e)
			{
				Debug.Log (string.Format("SocketException: {0}", e));
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
					messageHandler ( new MessageInfo ( dest ) );
//					mav.ParseBytesV2 ( dest );
				} else
				{
					break;
				}
			}
			if ( stream != null )
				stream.Close ();
//			int hash = client.GetHashCode ();
//			clients.TryRemove ( hash, out client );
			if ( client != null )
			{
				client.Close ();
				lock ( clientLock )
				{
					clients.Remove ( client );
				}
			}
		}

		void OnDestroy ()
		{
			running = false;
			listener.Stop ();
			listener = null;
		}
	}
}