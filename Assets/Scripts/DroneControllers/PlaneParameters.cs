using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DroneControllers
{
    public class PlaneParameters : MonoBehaviour
    {
        PlaneAutopilot ctrl;

        [System.NonSerialized]
        public SimParameter paramKpRollrate;
        [System.NonSerialized]
        public SimParameter paramKpPitchrate;
        [System.NonSerialized]
        public SimParameter paramKpYawrate;

        [System.NonSerialized]
        public SimParameter paramKpRoll;
        [System.NonSerialized]
        public SimParameter paramKpPitch;

        [System.NonSerialized]
        public SimParameter paramKpSpeed;
        [System.NonSerialized]
        public SimParameter paramKiSpeed;

        [System.NonSerialized]
        public SimParameter paramKpPos;
        [System.NonSerialized]
        public SimParameter paramKpPos2;
        [System.NonSerialized]
        public SimParameter paramKpAlt;
        [System.NonSerialized]
        public SimParameter paramKiAlt;
        [System.NonSerialized]
        public SimParameter paramKpVel;
        [System.NonSerialized]
        public SimParameter paramKpYaw;

        [System.NonSerialized]
        public SimParameter paramMaxTilt;
        [System.NonSerialized]
        public SimParameter paramMaxAscentRate;
        [System.NonSerialized]
        public SimParameter paramMaxDescentRate;
        [System.NonSerialized]
        public SimParameter paramPosHoldDeadband;
        [System.NonSerialized]
        public SimParameter paramMaxSpeed;

        [System.NonSerialized]
        public SimParameter paramKpSideslip;
        [System.NonSerialized]
        public SimParameter paramKiSideslip;

        void Awake()
        {
            ctrl = GetComponent<PlaneAutopilot>();
            if (ctrl != null)
            {
                paramKpRollrate = new SimParameter("Control:rollrate_gain_P", ctrl.planeControl.Kp_p, OnRollrateKpChanged);
                paramKpPitchrate = new SimParameter("Control:pitchrate_gain_P", ctrl.planeControl.Kp_q, OnPitchrateKpChanged);
                //paramKpYawrate = new SimParameter("Control:yawrate_gain_P", ctrl.planeControl.Kp_r, OnYawrateKpChanged);

                paramKpRoll = new SimParameter("Control:roll_gain_P", ctrl.planeControl.Kp_roll, OnRollKpChanged);
                paramKpPitch = new SimParameter("Control:pitch_gain_P", ctrl.planeControl.Kp_pitch, OnPitchKpChanged);

                paramKpSpeed = new SimParameter("Control:speed_gain_P", ctrl.planeControl.Kp_speed, OnSpeedKpChanged);
                paramKiSpeed = new SimParameter("Control:speed_gain_I", ctrl.planeControl.Ki_speed, OnSpeedKiChanged);

                //paramKpPos = new SimParameter("Control:position_gain_P", ctrl.planeControl.Kp_pos, OnPosKpChanged);
                //paramKpPos2 = new SimParameter("Control:position_gain_P2", ctrl.planeControl.Kp_pos2, OnPosKp2Changed);

                paramKpAlt = new SimParameter("Control:altitude_gain_P", ctrl.planeControl.Kp_alt, OnAltKpChanged);
                paramKiAlt = new SimParameter("Control:altitude_gain_I", ctrl.planeControl.Ki_alt, OnAltKiChanged);

                //paramKpVel = new SimParameter("Control:velocity_gain_P", ctrl.planeControl.Kp_vel, OnVelKpChanged);

                //paramKpYaw = new SimParameter("Control:yaw_gain_P", ctrl.planeControl.Kp_yaw, OnYawKpChanged);

                //paramMaxTilt = new SimParameter("Control:Max Tilt (rad)", ctrl.planeControl.maxTilt, OnMaxTiltChanged);
                //paramMaxAscentRate = new SimParameter("Control:Max Ascent Rate (m/s)", ctrl.planeControl.maxAscentRate, OnMaxAscentRateChanged);
                //paramMaxDescentRate = new SimParameter("Control:Max Descent Rate (m/s)", ctrl.planeControl.maxDescentRate, OnMaxDescentRateChanged);
                //paramPosHoldDeadband = new SimParameter("Control:Position Gain Radius (m)", ctrl.planeControl.posHoldDeadband, OnPosHoldDeadbandChanged);
                //paramMaxSpeed = new SimParameter("Control:Max Speed (m/s)", ctrl.planeControl.maxSpeed, OnMaxSpeedChanged);

                paramKpSideslip = new SimParameter("Control:sideslip_gain_P", ctrl.planeControl.Kp_sideslip, OnSideslipKpChanged);
                paramKiSideslip = new SimParameter("Control:sideslip_gain_I", ctrl.planeControl.Ki_sideslip, OnSideslipKiChanged);
            }
        }
        
        
        public void OnRollrateKpChanged(SimParameter p)
        {
            Debug.Log("Kp_p changed from: " + ctrl.planeControl.Kp_p + " to: " + p.Value);
            ctrl.planeControl.Kp_p = p.Value;
        }
        
        public void OnPitchrateKpChanged(SimParameter p)
        {
            Debug.Log("Kp_q changed from: " + ctrl.planeControl.Kp_q + " to: " + p.Value);
            ctrl.planeControl.Kp_q = p.Value;
        }
        /*
        public void OnYawrateKpChanged(SimParameter p)
        {
            Debug.Log("Kp_r changed from: " + ctrl.planeControl.Kp_r + " to: " + p.Value);
            ctrl.planeControl.Kp_p = p.Value;
        }
        */
        public void OnPitchKpChanged(SimParameter p)
        {
            Debug.Log("Kp_pitch changed from: " + ctrl.planeControl.Kp_pitch + " to: " + p.Value);
            ctrl.planeControl.Kp_pitch = p.Value;
        }


        
        public void OnRollKpChanged(SimParameter p)
        {
            Debug.Log("Kp_roll changed from: " + ctrl.planeControl.Kp_roll + " to: " + p.Value);
            ctrl.planeControl.Kp_roll = p.Value;
        }
        
        public void OnSpeedKpChanged(SimParameter p)
        {
            Debug.Log("Kp_speed changed from: " + ctrl.planeControl.Kp_speed + " to: " + p.Value);
            ctrl.planeControl.Kp_speed = p.Value;
        }

        public void OnSpeedKiChanged(SimParameter p)
        {
            Debug.Log("Ki_speed changed from: " + ctrl.planeControl.Ki_speed + " to: " + p.Value);
            ctrl.planeControl.Ki_speed = p.Value;
        }
        /*
        public void OnPosKpChanged(SimParameter p)
        {
            Debug.Log("Kp_pos changed from: " + ctrl.planeControl.Kp_pos + " to: " + p.Value);
            ctrl.planeControl.Kp_pos = p.Value;
        }

        public void OnPosKp2Changed(SimParameter p)
        {
            Debug.Log("Kp_pos2 changed from: " + ctrl.planeControl.Kp_pos2 + " to: " + p.Value);
            ctrl.planeControl.Kp_pos2 = p.Value;
        }
        */
        public void OnAltKpChanged(SimParameter p)
        {
            Debug.Log("Kp_alt changed from: " + ctrl.planeControl.Kp_alt + " to: " + p.Value);
            ctrl.planeControl.Kp_alt = p.Value;
        }

        public void OnAltKiChanged(SimParameter p)
        {
            Debug.Log("Ki_alt changed from: " + ctrl.planeControl.Ki_alt + " to: " + p.Value);
            ctrl.planeControl.Ki_alt = p.Value;
        }
        /*
        public void OnVelKpChanged(SimParameter p)
        {
            Debug.Log("Kp_vel changed from: " + ctrl.planeControl.Kp_vel + " to: " + p.Value);
            ctrl.planeControl.Kp_vel= p.Value;
        }

        public void OnYawKpChanged(SimParameter p)
        {
            Debug.Log("Kp_yaw changed from: " + ctrl.planeControl.Kp_yaw + " to: " + p.Value);
            ctrl.planeControl.Kp_yaw = p.Value;
        }

        public void OnMaxTiltChanged(SimParameter p)
        {
            Debug.Log("Max Tilt changed from: " + ctrl.planeControl.maxTilt + "  to: " + p.Value);
            ctrl.planeControl.maxTilt= p.Value;
        }

        public void OnMaxAscentRateChanged(SimParameter p)
        {
            Debug.Log("Max Ascent Rate changed from: " + ctrl.planeControl.maxAscentRate + " to: " + p.Value);
            ctrl.planeControl.maxAscentRate = p.Value;
        }

        public void OnMaxDescentRateChanged(SimParameter p)
        {
            Debug.Log("Max Descent Rate changed from: " + ctrl.planeControl.maxDescentRate + " to: " + p.Value);
            ctrl.planeControl.maxDescentRate = p.Value;
        }

        public void OnPosHoldDeadbandChanged(SimParameter p)
        {
            Debug.Log("PosHoldDeadband changed from: " + ctrl.planeControl.posHoldDeadband + " to: " + p.Value);
            ctrl.planeControl.posHoldDeadband = p.Value;
        }

        public void OnMaxSpeedChanged(SimParameter p)
        {
            Debug.Log("Max Speed changed from: " + ctrl.planeControl.maxSpeed + " to: " + p.Value);
            ctrl.planeControl.maxSpeed = p.Value;
        }
        */
        public void OnSideslipKpChanged(SimParameter p)
        {
            Debug.Log("Kp_sideslip changed from: " + ctrl.planeControl.Kp_sideslip+ " to: " + p.Value);
            ctrl.planeControl.Kp_sideslip = p.Value;
        }

        public void OnSideslipKiChanged(SimParameter p)
        {
            Debug.Log("Ki_sideslip changed from: " + ctrl.planeControl.Ki_sideslip+ " to: " + p.Value);
            ctrl.planeControl.Ki_sideslip = p.Value;
        }
    }
}
