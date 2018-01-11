using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Guided Motor")]
    public class QuadMB_GuidedMotor : QuadMovementBehavior
    {
        public override void OnSelect(SimpleQuadController _controller)
        {
            base.OnSelect(_controller);
        }

        public override void RemoteUpdate(float rollMoment, float pitchMoment, float yawMoment, float thrust)
        {
            var nav = controller.controller;
            Vector3 totalMoment = new Vector3(rollMoment, pitchMoment, yawMoment);
            float totalThrust = thrust;
            nav.CmdTorque(totalMoment);
            nav.CmdThrust(totalThrust);
            Debug.Log("Motor Command: " + totalThrust);
        }

        

        public override void OnLateUpdate()
        {
            
            
        }
        
            
            
    }
}