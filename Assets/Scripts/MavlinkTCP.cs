using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// usings needed for TCP/IP
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// needed for mavlink
using MavLink;

public class MavlinkTCP : MonoBehaviour {

    // the server
    private TcpListener _server = null;

    // the connected client - right now will only have 1 client connected
    private TcpClient _client = null;
    private NetworkStream _stream = null;

    // mavlink instance
    private Mavlink _mavlink;

    // thread for the tcp connection
    private Thread _tcpListenerThread;

    // flag to say the read thread should be reading
    private Boolean _running = true;

    // Use this for initialization
    void Start ()
    {
        // create a new instance of mavlink
        _mavlink = new Mavlink();

        // set up vent listener for packets being decoded
        _mavlink.PacketReceived += new PacketReceivedEventHandler(OnPacketReceived);

        // start the thread for the tcp connection
        _tcpListenerThread = new Thread(() => TcpThread());
        _tcpListenerThread.Start();

        
    }

    // Update is called once per frame
    void Update ()
    {
        /*
        // if we have a client, want to send the desired mavlink data here
        if (_client != null)
        {
            // first check to see if there are any new commands that have come in
            ReadFromClient();

            // TODO: send the correct packets at an appropriate rate

            // for now just send a heartbeat
            Msg_heartbeat hrtbt = new Msg_heartbeat
            {
                type = 1,
                autopilot = 1,
                system_status = 1,
                base_mode = 1,
                custom_mode = 1,
                mavlink_version = 3
            };
            var serializedPacket = _mavlink.SendV2(hrtbt);
            _stream.Write(serializedPacket, 0, serializedPacket.Length);
        }
        */
    }

    // this is called at a fixed rate
    /*
    void FixedUpdate()
    {
        // if we have a client, want to send the desired mavlink data here
        if (_client != null)
        {
            // first check to see if there are any new commands that have come in
            ReadFromClient();

            // TODO: send the correct packets at an appropriate rate

            // for now just send a heartbeat
            Msg_heartbeat hrtbt = new Msg_heartbeat
            {
                type = 1,
                autopilot = 1,
                system_status = 1,
                base_mode = 1,
                custom_mode = 1,
                mavlink_version = 3
            };
            var serializedPacket = _mavlink.SendV2(hrtbt);
            _stream.Write(serializedPacket, 0, serializedPacket.Length);
        }
    }
    */

    // called when this is destroyed
    private void OnDestroy()
    {
        // tell everything to stop running
        _running = false;

        // make sure that our threads are all finished
        _tcpListenerThread.Join();
        
        // want to make sure we close the client connection and stop the server
        if (_client != null)
        {
            _client.Close();
        }

        if (_server != null)
        {
            _server.Stop();
        }

    }

    void TcpThread()
    {
        try
        {
            // Set the TcpListener on port 5760 on the localhost (127.0.0.1)
            Int32 port = 5760;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            // create the listener
            _server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            _server.Start();

            // wait for a connection to be made 
            // NOTE: this is a blocking call!!!!
            // Enter the listening loop.
            print("Waiting for a connection... ");

            // Perform a blocking call to accept requests.
            // You could also user server.AcceptSocket() here.
            _client = _server.AcceptTcpClient();
            print("Connected!");

            // at this point we have a client, so let's get the stream to make this easier
            _stream = _client.GetStream();

            while (_running)
            {
                // read and write from this thread for the moment
                //ReadFromClient();

                // TODO: would need to rate limit if doing things here

                // for now just send a heartbeat
                Msg_heartbeat hrtbt = new Msg_heartbeat
                {
                    type = 1,
                    autopilot = 1,
                    system_status = 1,
                    base_mode = 1,
                    custom_mode = 1,
                    mavlink_version = 3
                };
                var serializedPacket = _mavlink.SendV2(hrtbt);
                _stream.Write(serializedPacket, 0, serializedPacket.Length);
                print("wrote to tcp");
            }

        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        } finally
        {
            // this is to ensure the sever stops once a disconnection happens, or when done with everything
            _server.Stop();
        }

        // TODO: probably want to kill the thread at this point...?

        // TODO: need to stop the server at some point
        //_server.Stop();
    }
    

    // this will simply continually read from the client in a thread?
    // TODO: can also have this simply read on every frame update....
    void ReadFromClient()
    {
        // make a fairly large buffer
        byte[] bytes = new byte[1024];
        

        // check for data, and while there is still data, parse it
        while (_stream.Read(bytes, 0, bytes.Length) != 0)
        {
            // have mavlink parse the incoming bytes
            // this will trigger the new packet events
            _mavlink.ParseBytes(bytes);
        }


    }


    void OnPacketReceived(object sender, MavlinkPacket packet)
    {
        print("received a packet!!!");
    }

}
