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
  

        public override void OnLateUpdate()
        {
            //var nav = controller.controller;
            Vector3 totalMoment = new Vector3(controller.guidedCommand.x, controller.guidedCommand.y, controller.guidedCommand.w);
            float totalThrust = controller.guidedCommand.z;
            controller.CommandTorque(totalMoment);
            controller.CommandThrust(totalThrust);



        }
        
            
            
    }
}