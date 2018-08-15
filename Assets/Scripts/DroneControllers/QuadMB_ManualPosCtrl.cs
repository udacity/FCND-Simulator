using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
    [CreateAssetMenu(menuName = "MovementBehaviors/Quad Manual Pos Ctrl")]
    public class QuadMB_ManualPosCtrl : QuadMovementBehavior
    {
        float prevTime = 0.0f;
        float maxSpeed = 10f;
        float stoppingTime = 0.0f;

        [SerializeField]
        float yawStep = 5.0f*Mathf.PI/180f;

        public override void OnSelect(IDroneController _controller)
        {
            base.OnSelect(_controller);
            controller.AttitudeTarget = controller.ControlAttitude;
            //controller.PositionTarget = controller.ControlPosition;
        }

        public override void OnLateUpdate()
        {
            yawStep = 5f * Mathf.PI / 180f;

            QuadControl QuadControl = (QuadControl)controller.control;

            Vector3 attitude = controller.ControlAttitude;// new Vector3(nav.GetRoll(), nav.GetPitch(), nav.GetYaw());
            Vector3 angularVelocity = controller.ControlBodyRate;// new Vector3(nav.GetRollrate(), nav.GetPitchrate(), nav.GetYawrate());
            Vector3 localVelocity = controller.ControlVelocity;// new Vector3(nav.GetNorthVelocity(), nav.GetEastVelocity(), nav.GetDownVelocity());
            Vector3 localPosition = controller.ControlPosition;// new Vector3(nav.GetLocalNorth(), nav.GetLocalEast(), nav.GetLocalDown());

            Vector3 velocityCmd;
            float yawCmd;
            Vector3 targetPosition;
            if (!controller.Guided())
            {
                velocityCmd = maxSpeed * (new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), -Input.GetAxis("Thrust"))).normalized;
                float yawInput = Input.GetAxis("Yaw");
                if (Mathf.Abs(yawInput) > 0)
                {
                    yawCmd = attitude.z + yawStep * yawInput;
                    Debug.Log("Yaw Cmd2: " + yawCmd + " attitude.z: " + attitude.z + " yawStep: " + yawStep + " yawInput: " + yawInput);
                }
                else
                    yawCmd = controller.AttitudeTarget.z;

                targetPosition = localPosition + localVelocity*stoppingTime;

                if (velocityCmd.x == 0 && velocityCmd.y == 0)
                {
                    targetPosition.x = controller.PositionTarget.x;
                    targetPosition.y = controller.PositionTarget.y;
                }

                if (velocityCmd.z == 0)
                {
                    targetPosition.z = controller.PositionTarget.z;
                }
            }
            else
            {
                velocityCmd = controller.VelocityTarget;
                targetPosition = controller.PositionTarget;
                yawCmd = controller.AttitudeTarget.z;//controller.BodyRateTarget.z;

            }

            Vector3 targetVelocity;
            targetVelocity.x = velocityCmd.x * Mathf.Cos(attitude.z) - velocityCmd.y * Mathf.Sin(attitude.z);
            targetVelocity.y = velocityCmd.x * Mathf.Sin(attitude.z) + velocityCmd.y * Mathf.Cos(attitude.z);
            targetVelocity.z = velocityCmd.z;
            Vector3 outerLoop = QuadControl.PositionVelocityLoop(targetPosition, targetVelocity, localPosition, localVelocity, attitude.z);

            controller.VelocityTarget = targetVelocity;
            controller.PositionTarget = targetPosition;
            
            Vector2 targetRollPitch = new Vector2(outerLoop.x, outerLoop.y);

            Vector3 attitudeTarget = controller.AttitudeTarget;
            attitudeTarget.x = targetRollPitch.x;
            attitudeTarget.y = targetRollPitch.y;
            attitudeTarget.z = yawCmd;
            controller.AttitudeTarget = attitudeTarget;


            Vector2 targetRate = QuadControl.RollPitchLoop(targetRollPitch, attitude);

            float yawRateCmd = QuadControl.YawLoop(yawCmd, attitude.z);

            Vector3 bodyRateTarget = controller.BodyRateTarget;
            bodyRateTarget.x = targetRate.x;
            bodyRateTarget.y = targetRate.y;
            bodyRateTarget.z = yawRateCmd;
            controller.BodyRateTarget = bodyRateTarget;

            Vector2 rollPitchMoment = QuadControl.RollPitchRateLoop(targetRate, angularVelocity);
            
            float yawMoment = QuadControl.YawRateLoop(yawRateCmd, angularVelocity.z);

            float dt = Time.fixedDeltaTime;

            float thrust = QuadControl.VerticalVelocityLoop(-(outerLoop.z+velocityCmd.z), attitude, -localVelocity.z, dt, 0.5f);

            Vector3 totalMoment = new Vector3(rollPitchMoment.x, rollPitchMoment.y, yawMoment);
            controller.CommandMoment(totalMoment, thrust);

        }

    }
}