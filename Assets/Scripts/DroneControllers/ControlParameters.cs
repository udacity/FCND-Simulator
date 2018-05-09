using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DroneControllers
{
    public class ControlParameters : MonoBehaviour
    {
        QuadAutopilot ctrl;

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
        public SimParameter paramKpHdot;
        [System.NonSerialized]
        public SimParameter paramKiHdot;

        [System.NonSerialized]
        public SimParameter paramKpPos;
        [System.NonSerialized]
        public SimParameter paramKpPos2;
        [System.NonSerialized]
        public SimParameter paramKpAlt;
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

        void Awake()
        {
            ctrl = GetComponent<QuadAutopilot>();

            paramKpRollrate = new SimParameter("Control:rollrate_gain_P", ctrl.attCtrl.Kp_p,OnRollrateKpChanged);
            paramKpPitchrate = new SimParameter("Control:pitchrate_gain_P", ctrl.attCtrl.Kp_q, OnPitchrateKpChanged);
            paramKpYawrate = new SimParameter("Control:yawrate_gain_P", ctrl.attCtrl.Kp_r, OnYawrateKpChanged);

            paramKpRoll = new SimParameter("Control:roll_gain_P", ctrl.attCtrl.Kp_roll, OnRollKpChanged);
            paramKpPitch = new SimParameter("Control:pitch_gain_P", ctrl.attCtrl.Kp_pitch, OnPitchKpChanged);

            paramKpHdot = new SimParameter("Control:hdot_gain_P", ctrl.attCtrl.Kp_hdot, OnHdotKpChanged);
            paramKiHdot = new SimParameter("Control:hdot_gain_I", ctrl.attCtrl.Ki_hdot, OnHdotKiChanged);

            paramKpPos = new SimParameter("Control:position_gain_P", ctrl.posCtrl.Kp_pos, OnPosKpChanged);
            paramKpPos2 = new SimParameter("Control:position_gain_P2", ctrl.posCtrl.Kp_pos2, OnPosKp2Changed);

            paramKpAlt = new SimParameter("Control:altitude_gain_P", ctrl.posCtrl.Kp_alt, OnAltKpChanged);

            paramKpVel = new SimParameter("Control:velocity_gain_P", ctrl.posCtrl.Kp_vel, OnVelKpChanged);

            paramKpYaw = new SimParameter("Control:yaw_gain_P", ctrl.posCtrl.Kp_yaw, OnYawKpChanged);

            paramMaxTilt = new SimParameter("Control:Max Tilt (rad)", ctrl.attCtrl.maxTilt, OnMaxTiltChanged);
            paramMaxAscentRate = new SimParameter("Control:Max Ascent Rate (m/s)", ctrl.attCtrl.maxAscentRate, OnMaxAscentRateChanged);
            paramMaxDescentRate = new SimParameter("Control:Max Descent Rate (m/s)", ctrl.attCtrl.maxDescentRate, OnMaxDescentRateChanged);
            paramPosHoldDeadband = new SimParameter("Control:Position Gain Radius (m)", ctrl.posCtrl.posHoldDeadband, OnPosHoldDeadbandChanged);
            paramMaxSpeed = new SimParameter("Control:Max Speed (m/s)", ctrl.posCtrl.maxSpeed, OnMaxSpeedChanged);
        }
        

        public void OnRollrateKpChanged(SimParameter p)
        {
            Debug.Log("Kp_p changed from: " + ctrl.attCtrl.Kp_p + " to: " + p.Value);
            ctrl.attCtrl.Kp_p = p.Value;
        }

        public void OnPitchrateKpChanged(SimParameter p)
        {
            Debug.Log("Kp_q changed from: " + ctrl.attCtrl.Kp_q + " to: " + p.Value);
            ctrl.attCtrl.Kp_q = p.Value;
        }

        public void OnYawrateKpChanged(SimParameter p)
        {
            Debug.Log("Kp_r changed from: " + ctrl.attCtrl.Kp_r + " to: " + p.Value);
            ctrl.attCtrl.Kp_p = p.Value;
        }

        public void OnPitchKpChanged(SimParameter p)
        {
            Debug.Log("Kp_pitch changed from: " + ctrl.attCtrl.Kp_pitch + " to: " + p.Value);
            ctrl.attCtrl.Kp_pitch = p.Value;
        }

        public void OnRollKpChanged(SimParameter p)
        {
            Debug.Log("Kp_roll changed from: " + ctrl.attCtrl.Kp_roll + " to: " + p.Value);
            ctrl.attCtrl.Kp_roll = p.Value;
        }

        public void OnHdotKpChanged(SimParameter p)
        {
            Debug.Log("Kp_hdot changed from: " + ctrl.attCtrl.Kp_hdot + " to: " + p.Value);
            ctrl.attCtrl.Kp_hdot = p.Value;
        }

        public void OnHdotKiChanged(SimParameter p)
        {
            Debug.Log("Ki_hdot changed from: " + ctrl.attCtrl.Ki_hdot + " to: " + p.Value);
            ctrl.attCtrl.Ki_hdot = p.Value;
        }

        public void OnPosKpChanged(SimParameter p)
        {
            Debug.Log("Kp_pos changed from: " + ctrl.posCtrl.Kp_pos + " to: " + p.Value);
            ctrl.posCtrl.Kp_pos = p.Value;
        }

        public void OnPosKp2Changed(SimParameter p)
        {
            Debug.Log("Kp_pos2 changed from: " + ctrl.posCtrl.Kp_pos2 + " to: " + p.Value);
            ctrl.posCtrl.Kp_pos2 = p.Value;
        }

        public void OnAltKpChanged(SimParameter p)
        {
            Debug.Log("Kp_alt changed from: " + ctrl.posCtrl.Kp_alt + " to: " + p.Value);
            ctrl.posCtrl.Kp_alt = p.Value;
        }

        public void OnVelKpChanged(SimParameter p)
        {
            Debug.Log("Kp_vel changed from: " + ctrl.posCtrl.Kp_vel + " to: " + p.Value);
            ctrl.posCtrl.Kp_vel= p.Value;
        }

        public void OnYawKpChanged(SimParameter p)
        {
            Debug.Log("Kp_yaw changed from: " + ctrl.posCtrl.Kp_yaw + " to: " + p.Value);
            ctrl.posCtrl.Kp_yaw = p.Value;
        }

        public void OnMaxTiltChanged(SimParameter p)
        {
            Debug.Log("Max Tilt changed from: " + ctrl.attCtrl.maxTilt + "  to: " + p.Value);
            ctrl.attCtrl.maxTilt= p.Value;
        }

        public void OnMaxAscentRateChanged(SimParameter p)
        {
            Debug.Log("Max Ascent Rate changed from: " + ctrl.attCtrl.maxAscentRate + " to: " + p.Value);
            ctrl.attCtrl.maxAscentRate = p.Value;
        }

        public void OnMaxDescentRateChanged(SimParameter p)
        {
            Debug.Log("Max Descent Rate changed from: " + ctrl.attCtrl.maxDescentRate + " to: " + p.Value);
            ctrl.attCtrl.maxDescentRate = p.Value;
        }

        public void OnPosHoldDeadbandChanged(SimParameter p)
        {
            Debug.Log("PosHoldDeadband changed from: " + ctrl.posCtrl.posHoldDeadband + " to: " + p.Value);
            ctrl.posCtrl.posHoldDeadband = p.Value;
        }

        public void OnMaxSpeedChanged(SimParameter p)
        {
            Debug.Log("Max Speed changed from: " + ctrl.posCtrl.maxSpeed + " to: " + p.Value);
            ctrl.posCtrl.maxSpeed = p.Value;
        }
    }
}
