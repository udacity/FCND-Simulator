using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdacityNetworking;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Text;

#if UNITY_EDITOR || !UNITY_WEBGL
using System.Threading.Tasks;
using Profiler = UnityEngine.Profiling.Profiler;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
class RepeatingMessage
{
	public Func<List<byte[]>> func;
	public float delay;
	public float nextInvokeTime;

	public RepeatingMessage (Func<List<byte[]>> f, float callDelay)
	{
		func = f;
		delay = callDelay;
		nextInvokeTime = Time.unscaledTime + delay;
	}

	public bool ReadyToCall ()
	{
		return Time.unscaledTime >= nextInvokeTime;
	}

	public void OnCalled ()
	{
		nextInvokeTime = Time.unscaledTime + delay;
	}
}
#endif

namespace UdacityNetworking
{
	public enum ConnectionProtocol { TCP, UDP, WebSocket };
	public enum ConnectionState { Connecting, Connected, Disconnecting, Disconnected };

	public class NetworkController : MonoBehaviour
	{
		static Encoding enc = Encoding.ASCII;
		#if UNITY_WEBGL && !UNITY_EDITOR
		[System.Runtime.InteropServices.DllImport ("__Internal")]
		static extern string ObtainHost ();
		#else
		string ObtainHost () { return ""; }
		#endif
		public ConnectionState ConnectionState { get { return connection != null ? connection.ConnectionState : ConnectionState.Disconnected; } }

		public static float Timeout = 30;

		public bool autoStartServer;
		public Int32 serverPort = 5760;
		public string serverIP = "127.0.0.1";
		public bool autoStartClient;
		public Int32 remotePort = 5760;
		public string remoteIP = "127.0.0.1";
		public ConnectionProtocol protocol;
		public float timeout = 30;
		NetworkConnection connection;


		Dictionary<MessageType, Action<MessageInfo>> handlers = new Dictionary<MessageType, Action<MessageInfo>> ();
//		event Action<MessageInfo> messageHandler = delegate {};
		event Action<ConnectionState> connectionEvent = delegate {};

		ConnectionState lastConnectionState;
		bool closing;

		#if UNITY_WEBGL && !UNITY_EDITOR
		List<RepeatingMessage> repeatingMessages = new List<RepeatingMessage> ();
		#endif

		void Awake ()
		{
			byte[] values = (byte[]) Enum.GetValues ( typeof (MessageType) );
			for ( int i = 0; i < values.Length; i++ )
				handlers.Add ( (MessageType) values [ i ], delegate {} );
//			handlers.Add ( MessageType.Mavlink, delegate {} );
//			handlers.Add ( MessageType.Colliders, delegate {} );
		}

		void Start ()
		{
			closing = false;
			if ( autoStartServer && autoStartClient )
				Debug.LogWarning ( "AutoStartServer and AutoStartClient are both set. Server will override the client option" );
			Timeout = timeout;
			#if UNITY_WEBGL && !UNITY_EDITOR
			protocol = ConnectionProtocol.WebSocket;
			connection = new WebsocketConnection ();
			#else
			if ( protocol == ConnectionProtocol.TCP )
				connection = new TCPConnection ();
			else
			if ( protocol == ConnectionProtocol.UDP )
				connection = new UDPConnection ();
			else
				connection = new WebsocketConnection ();
			#endif

			connection.Controller = this;
			connection.AddMessageHandler ( MessageReceived );
			lastConnectionState = ConnectionState.Disconnected;

			if ( protocol == ConnectionProtocol.WebSocket && ( autoStartClient || autoStartServer ) )
				StartClient ();
			else
			if ( autoStartServer )
				StartServer ();
			else
			if ( autoStartClient )
				StartClient ();
		}

		void Update ()
		{
			if ( connection != null )
			{
				connection.DoUpdate ();
				ConnectionState newState = connection.ConnectionState;
				if ( newState != lastConnectionState )
				{
					connectionEvent ( newState );
					lastConnectionState = newState;
				}
			}
		}

		#if UNITY_WEBGL && !UNITY_EDITOR
		void FixedUpdate ()
		{
			if ( !closing && connection != null && ( connection.IsServerStarted || connection.IsConnected ) )
			{
				int count = repeatingMessages.Count;
				for ( int i = 0; i < count; i++ )
				{
					if ( repeatingMessages [ i ].ReadyToCall () )
					{
//						Profiler.BeginSample ( "s1" );
						var msgs = repeatingMessages [ i ].func ();
//						Profiler.EndSample ();
						
//						Profiler.BeginSample ( "s2" );
						foreach ( var msg in msgs )
						{
							connection.SendMessage ( msg );
						}
//						Profiler.EndSample ();
						repeatingMessages [ i ].OnCalled ();
					}
				}
			}
		}
		#endif

		void OnDestroy ()
		{
			if ( connection != null )
			{
				connection.Destroy ();
				connection = null;
			}
			closing = true;
		}
		
		public void StartServer ()
		{
			connection.StartServer ( serverIP, serverPort );
		}

		public void StartClient ()
		{
			if ( protocol == ConnectionProtocol.WebSocket )
			{
				string host = ObtainHost ();
				Debug.Log ( "host is " + host );
				if ( string.IsNullOrWhiteSpace ( host ) )
					remoteIP = "ws://" + remoteIP + ":" + remotePort.ToString ();
				else
					remoteIP = host.Replace ( "####", remotePort.ToString () );
			}
			Debug.Log(string.Format("ip {0}, port {1}", remoteIP, remotePort));
			connection.Connect ( remoteIP, remotePort );
		}

		public void AddMessageHandler (MessageType messageType, Action<MessageInfo> handler)
		{
			handlers [ messageType ] += handler;
//			messageHandler += handler;
		}

		public void RemoveMessageHandler (MessageType messageType, Action<MessageInfo> handler)
		{
			handlers [ messageType ] -= handler;
//			messageHandler -= handler;
		}

		public void AddConnectionEvent (Action<ConnectionState> handler)
		{
			connectionEvent += handler;
		}

		public void SendMessage (MessageInfo message)
		{
			connection.SendMessage ( message );
		}

		public void SendMessage (MessageType type, byte[] message)
		{
			connection.SendMessage ( MessageInfo.Prepack ( type, message ) );
		}

		public void SendMessage (MessageType type, byte[] message, string ip, int port)
		{
			connection.SendMessage ( MessageInfo.Prepack ( type, message, ip, port ) );
		}

		void MessageReceived (MessageInfo message)
		{
			handlers [ message.type ] ( message );
//			messageHandler ( message );
		}

		public void EnqueueRecurringMessage (MessageType messageType, Func<List<byte[]>> messageFunc, int delayMilliseconds)
		{
			#if UNITY_WEBGL && !UNITY_EDITOR
			float delay = 1f * delayMilliseconds / 1000f;
			repeatingMessages.Add ( new RepeatingMessage ( messageFunc, delay ) );
//			StartCoroutine ( RecurringMessage ( messageFunc, delayMilliseconds ) );
			#else
//			Task.Factory.StartNew ( RecurringMessage ( messageFunc, delayMilliseconds ) );
			Task.Run ( () => RecurringMessage ( messageType, messageFunc, delayMilliseconds ) );
//			RecurringMessage ( messageFunc, delayMilliseconds );
			#endif
		}

		#if UNITY_WEBGL && !UNITY_EDITOR
/*		IEnumerator RecurringMessage (MessageType messageType, Func<List<byte[]>> msgFunc, int delayMS)
		{
			float delay = 1f * delayMS / 1000f;
			while ( !closing )
			{
				if ( connection != null && ( connection.IsServerStarted || connection.IsConnected ) )
				{
					var msgs = msgFunc ();
					foreach (var msg in msgs)
					{
						connection.SendMessage ( MessageInfo.Encode ( messageType, msg ) );
						connection.SendMessage ( msg );
					}
				}
				yield return new WaitForFixedUpdate ();
//				yield return new WaitForSecondsRealtime ( delay );
			}
		}*/
		#else
		async Task RecurringMessage (MessageType messageType, Func<List<byte[]>> msgFunc, int delayMS)
		{
			
			while ( !closing )
			{
				if ( connection != null && ( connection.IsServerStarted || connection.IsConnected ) )
				{
					var msgs = msgFunc ();
					foreach ( var msg in msgs )
					{
						connection.SendMessage ( MessageInfo.Encode ( messageType, msg ) );
//						connection.SendMessage ( msg );
					}
				}
				await Task.Delay ( delayMS );
			}
		}
		#endif
	}
}