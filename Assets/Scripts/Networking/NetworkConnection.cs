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
		public MessageType type;
		public bool prepacked;

		public MessageInfo (MessageType messageType, byte[] msg, string _ip = "", int _port = -1, bool _prepacked = false)
		{
			ip = _ip;
			port = _port;
			message = msg;
			type = messageType;
			prepacked = _prepacked;
		}

		public MessageInfo (byte[] msg)
		{
			prepacked = true;
			type = (MessageType) msg [ 0 ];
			message = msg;
		}

		public byte[] Encode ()
		{
			byte[] bytes = new byte[message.Length + 1];
			bytes [ 0 ] = (byte) type;
			Array.Copy ( message, 0, bytes, 1, message.Length );
			return bytes;
		}

		public static byte[] Encode (MessageType messageType, byte[] _message)
		{
			byte[] bytes = new byte[_message.Length + 1];
			bytes [ 0 ] = (byte) messageType;
			Array.Copy ( _message, 0, bytes, 1, _message.Length );
			return bytes;
		}

		public static MessageInfo Prepack (MessageType messageType, byte[] _message, string _ip = "", int _port = -1)
		{
			byte[] bytes = new byte[_message.Length + 1];
			bytes [ 0 ] = (byte) messageType;
			Array.Copy ( _message, 0, bytes, 1, _message.Length );
			return new MessageInfo ( messageType, bytes, _ip, _port, true );
		}
	}

	public enum MessageType
	{
		Mavlink,
		Colliders
	}

	public interface NetworkConnection
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
		void SendMessage ( MessageInfo message );
		void DoUpdate ();
		void Destroy ();
	}
}