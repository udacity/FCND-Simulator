using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Manual")]
    public class QuadMB_Manual : QuadMovementBehavior
    {
        float totalThrust=100.0f;
        

        public override void OnLateUpdate()
        {
            //var nav = controller.controller;
            float rollMoment = Input.GetAxis("Horizontal");
            float pitchMoment = -Input.GetAxis("Vertical");
            float yawMoment = Input.GetAxis("Yaw");
            float thrust = totalThrust * Input.GetAxis("Thrust");
            Vector3 totalMoment = new Vector3(rollMoment, pitchMoment, yawMoment);
            controller.CommandMoment(totalMoment, thrust);
            
            
        }
    }
}