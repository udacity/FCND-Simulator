using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdacityNetworking;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR || !UNITY_WEBGL
using System.Threading.Tasks;
#endif

namespace UdacityNetworking
{
	public enum ConnectionProtocol { TCP, UDP };

	public class NetworkController : MonoBehaviour
	{
		public static float Timeout = 30;

		public bool autoStartServer;
		public Int32 port = 5760;
		public string ip = "127.0.0.1";
		public ConnectionProtocol protocol;
		public float timeout = 30;

		NetworkConnection connection;
		event Action<MessageInfo> messageHandler = delegate {};

		void Awake ()
		{
			Timeout = timeout;
			#if UNITY_WEBGL && !UNITY_EDITOR
			connection = new WebsocketConnection ();
			#else
			if ( protocol == ConnectionProtocol.TCP )
			connection = new TCPConnection ();
			else
				connection = new UDPConnection ();
			#endif

			connection.AddMessageHandler ( MessageReceived );

			if ( autoStartServer )
				StartServer ();
		}

		void Update ()
		{
			if ( connection != null )
				connection.DoUpdate ();
		}
		
		public void StartServer ()
		{
			connection.StartServer ( ip, port );
		}

		public void AddMessageHandler (Action<MessageInfo> handler)
		{
			messageHandler += handler;
		}

		public void RemoveMessageHandler (Action<MessageInfo> handler)
		{
			messageHandler -= handler;
		}

		public void SendMessage (byte[] message)
		{
			connection.SendMessage ( message );
		}

		public void SendMessage (byte[] message, string ip, int port)
		{
			connection.SendMessage ( message, ip, port );
		}

		void MessageReceived (MessageInfo message)
		{
			messageHandler ( message );
		}

		public void EnqueueRecurringMessage (Func<List<byte[]>> messageFunc, int delayMilliseconds)
		{
			#if UNITY_WEBGL && !UNITY_EDITOR
			StartCoroutine ( RecurringMessage ( messageFunc, delayMilliseconds ) );
			#else
			RecurringMessage ( messageFunc, delayMilliseconds );
			#endif
		}

		#if UNITY_WEBGL && !UNITY_EDITOR
		IEnumerator RecurringMessage (Func<List<byte[]>> msgFunc, int delayMS)
		{
			float delay = 1f * delayMS / 1000f;
			while ( connection.IsServerStarted )
			{
				var msgs = msgFunc ();
				foreach (var msg in msgs)
				{
					connection.SendMessage ( msg );
				}
				yield return new WaitForSecondsRealtime ( delay );
			}
		}
		#else
		async Task RecurringMessage (Func<List<byte[]>> msgFunc, int delayMS)
		{
			while ( connection.IsServerStarted )
			{
				var msgs = msgFunc ();
				foreach (var msg in msgs)
				{
					connection.SendMessage ( msg );
				}
				await Task.Delay ( delayMS );
			}
		}
		#endif
	}
}