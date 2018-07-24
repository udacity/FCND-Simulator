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
            Vector3 totalMoment = new Vector3(controller.MomentThrustTarget.x, controller.MomentThrustTarget.y, controller.MomentThrustTarget.z);
            float totalThrust = controller.MomentThrustTarget.z;
            controller.CommandMoment(totalMoment, totalThrust);
        }
        
            
            
    }
}