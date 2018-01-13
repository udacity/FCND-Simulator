using UnityEngine;
using System.Threading.Tasks;
using FlightUtils;
using Messaging;
using MavLink;
using System;

public class Rate : MonoBehaviour
{
    private MAVLinkMessenger messenger;
    public int hz = 500;
    public Mavlink mav { get; private set; }
    void Start()
    {
        mav = new Mavlink();
        messenger = new MAVLinkMessenger();
        // Run(Conversions.HertzToMilliSeconds(hz));
        Task.Run( () => Run(Conversions.HertzToMilliSeconds(hz)));
    }

    async void Run(int delay)
    {
        int total_commands = 0;
        DateTime prev_time;
        DateTime curr_time;
        prev_time = DateTime.Now;
        while (true)
        {
            curr_time = DateTime.Now;
            total_commands += 1;
            var interval = curr_time - prev_time;
            var diff = interval.TotalSeconds;
            if (diff > 1.0)
            {
                Debug.Log(diff);
                Debug.Log("Attitude Message Freq: " + (float)total_commands / diff);
                prev_time = curr_time;
                total_commands = 0;
            }
            await Task.Delay(2);
        }
    }
}