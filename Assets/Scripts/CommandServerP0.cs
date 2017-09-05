using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SocketIO;
using System;
using System.Text;
using MavLink;

public class CommandServerP0 : MonoBehaviour
{
	public Camera frontFacingCamera;
	private SocketIOComponent _socket;

	private QuadController _quadController;
	private Mavlink _mavlink;

	void Start()
	{
		_quadController = GameObject.Find("Quad Drone").GetComponent<QuadController>();
		_socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
		_socket.On("open", OnOpen);
		_socket.On("command", OnCommand);
		_mavlink = new Mavlink();
	}

	void OnOpen(SocketIOEvent ev)
	{
		Debug.Log("Connection Open");
	}

	void OnCommand(SocketIOEvent ev)
	{
		JSONObject obj = ev.data;
		var jmsg = obj.GetField("mavmsg");

		List<byte> bytes = new List<byte>();
		for (var i = 0; i < jmsg.Count; i++) {
			bytes.Add(byte.Parse((jmsg[i]).ToString()));
		}

		// Decode the MAVLink message and if needed perform
		// action on drone.
		// Mavlink 2 Packet Format
		// uint8_t magic;              ///< protocol magic marker
		// uint8_t len;                ///< Length of payload
		// uint8_t incompat_flags;     ///< flags that must be understood
		// uint8_t compat_flags;       ///< flags that can be ignored if not understood
		// uint8_t seq;                ///< Sequence of packet
		// uint8_t sysid;              ///< ID of message sender system/aircraft
		// uint8_t compid;             ///< ID of the message sender component
		// uint8_t msgid 0:7;          ///< first 8 bits of the ID of the message
		// uint8_t msgid 8:15;         ///< middle 8 bits of the ID of the message
		// uint8_t msgid 16:23;        ///< last 8 bits of the ID of the message
		// uint8_t target_sysid;       ///< Optional field for point-to-point messages, used for payload else
		// uint8_t target_compid;      ///< Optional field for point-to-point messages, used for payload else
		// uint8_t payload[max 253];   ///< A maximum of 253 payload bytes
		// uint16_t checksum;          ///< X.25 CRC
		// uint8_t signature[13];      ///< Signature which allows ensuring that the link is tamper-proof

		var offset = 7;
		var msg = _mavlink.Deserialize(bytes.ToArray(), offset);
		Debug.Log(string.Format("MAVLink message: {0}", msg));

		var msgstr = msg.ToString();
		Debug.Log(string.Format("Message in string form {0}", msgstr));

		// TODO: commands to implement
		//
		// MAV_CMD_NAV_TAKEOFF	Takeoff from ground / hand
		// Mission Param #1	Minimum pitch (if airspeed sensor present), desired pitch without sensor
		// Mission Param #2	Empty
		// Mission Param #3	Empty
		// Mission Param #4	Yaw angle (if magnetometer present), ignored without magnetometer. NaN for unchanged.
		// Mission Param #5	Latitude
		// Mission Param #6	Longitude
		// Mission Param #7	Altitude
		// 
		// MAV_CMD_NAV_LAND	Land at location
		// Mission Param #1	Abort Alt
		// Mission Param #2	Empty
		// Mission Param #3	Empty
		// Mission Param #4	Desired yaw angle. NaN for unchanged.
		// Mission Param #5	Latitude
		// Mission Param #6	Longitude
		// Mission Param #7	Altitude (ground level)

		// switch (msgstr) {
		// 	case "MavLink.Msg_heartbeat":
		// 		Debug.Log("Received hearbeat");
		// 		break;
		// 	case "MavLink.Msg_altitude":
		// 		Debug.Log("Received altitude");
		// 		break;
		// 	default:
		// 		Debug.Log("Received unindentified message");
		// 		break;
		// }

		var cmd = obj.GetField("cmd").ToString();
		switch (cmd) {
			case "takeoff":
				// Make drone move up
				// send telemetry back
				break;
			case "land":
				// Make drone land
				// send telemetry back
				break;
			case "waypoints":
				// Fly through waypoints
				// waypoints
				break;
			default:
				Debug.Log(string.Format("Cmd {0} is invalid", cmd));
				break;
		}
	}

	public void FixedUpdate() {
	}

	void EmitTelemetry(Dictionary<string, JSONObject> data)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			print("Attempting to Send...");
			_socket.Emit("telemetry", new JSONObject(data));
		});
	}
}