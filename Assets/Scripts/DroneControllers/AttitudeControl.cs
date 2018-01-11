using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttitudeControl {


    public float Kp_r = 20.0f;
    
    public float Kp_hdot = 5.0f;
    public float Ki_hdot = 0.5f;

    public float Kp_p = 10.0f;
    public float Ki_p = 0.0f;
    public float Kd_p = 0.0f;
    public float Kp_roll = 6.5f;

    public float Kp_q = 10.0f;
    public float Ki_q = 0.0f;
    public float Kd_q = 0.0f;
    public float Kp_pitch = 6.5f;

    public float maxTilt = 0.5f;
    public float maxAscentRate = 5.0f;
    public float maxDescentRate = 2.0f;

    private float hDotInt;
    private Vector2 lastError_pq = Vector2.zero;
    private Vector2 pqInt = Vector2.zero;



	// Use this for initialization
	public AttitudeControl () {
        hDotInt = 0.0f;
	}
	
    /// <summary>
    /// Closes the loop on the vehicle yaw rate returning a desired yaw moment
    /// </summary>
    public float YawRateLoop(float targetYawrate, float yawrate)
    {
        float yawMoment = Kp_r * (targetYawrate - yawrate);
        return yawMoment;
    }

    /// <summary>
    /// Closes the loop on vertical velocity using a PI controller to output a thrust command. Optionally a ff command can be passed in.
    /// </summary>   
    public float VerticalVelocityLoop(float targetVerticalVelocity, Vector3 attitude, float verticalVelocity, float dt, float thrustNom=0.0f)
    {
        
        if (targetVerticalVelocity > maxAscentRate)
            targetVerticalVelocity = maxAscentRate;
        else if (targetVerticalVelocity < -maxDescentRate)
            targetVerticalVelocity = -maxDescentRate;

        float hDotError = targetVerticalVelocity - verticalVelocity;

        hDotInt += hDotError * dt;
        float thrust = (Kp_hdot * hDotError + Ki_hdot * hDotInt + thrustNom) / (Mathf.Cos(attitude.x) * Mathf.Cos(attitude.y));
        return thrust;
    }

    /// <summary>
    /// Command a desired roll and pitch using a PD cascading controller. Returns a roll and pitch rate
    /// </summary>
    public Vector2 RollPitchLoop(Vector2 targetRollPitch, Vector3 attitude)
    {
        /*
        //Enfoce a maximum tilt angle
        float angleMagnitude = Mathf.Sqrt(targetRollPitch.x * targetRollPitch.x + targetRollPitch.y * targetRollPitch.y);
        if (angleMagnitude > maxTilt)
        {
            targetRollPitch = maxTilt * targetRollPitch / angleMagnitude;
        }

        //PD control on roll
        float roll = attitude.x;
        float targetRoll = targetRollPitch.x;
        targetRoll = targetRoll % (2.0f * Mathf.PI);

        float rollError = targetRoll - roll;
        if (rollError > Mathf.PI)
            rollError = rollError - 2.0f * Mathf.PI;
        else if (rollError < -Mathf.PI)
            rollError = rollError + 2.0f * Mathf.PI;

        float rollrateCmd = Kp_roll * rollError;

        //PD control on pitch
        float pitch = attitude.y;
        float targetPitch = targetRollPitch.y;
        targetPitch = targetPitch % (2.0f * Mathf.PI);

        float pitchError = targetPitch - pitch;
        if (pitchError > Mathf.PI)
            pitchError = pitchError - 2.0f * Mathf.PI;
        else if (pitchError < -Mathf.PI)
            pitchError = pitchError + 2.0f * Mathf.PI;

        float pitchrateCmd = Kp_pitch * pitchError;


        return new Vector2(rollrateCmd, pitchrateCmd);
        */
        float angleMagnitude = Mathf.Sqrt(targetRollPitch.x * targetRollPitch.x + targetRollPitch.y * targetRollPitch.y);
        if (angleMagnitude > maxTilt)
        {
            targetRollPitch = maxTilt * targetRollPitch / angleMagnitude;
        }
        attitude.z = 0.0f;
        Vector3 R1 = euler2RM(attitude.x, attitude.y, attitude.z, 1);
        Vector3 R2 = euler2RM(attitude.x, attitude.y, attitude.z, 2);
        Vector3 R3 = euler2RM(attitude.x, attitude.y, attitude.z, 3);
        Vector3 R3d = euler2RM(targetRollPitch.x, targetRollPitch.y, 0.0f, 3);
        float target_p = (1 / R3.z) * (R1.y * Kp_roll * (R3.x - R3d.x) - R1.x * Kp_pitch * (R3.y - R3d.y));
        float target_q = (1 / R3.z) * (R2.y * Kp_roll * (R3.x - R3d.x) - R2.x * Kp_pitch * (R3.y - R3d.y));

        return new Vector2(target_p, target_q);
    }

    /// <summary>
    /// Command a desired roll/pitch rate using PID control. Returns a desired roll/pitch moment
    /// </summary>
    public Vector2 RollPitchRateLoop(Vector2 targetRate,Vector3 angularRate,float dt=0.0f)
    {
        Vector2 rateError = targetRate - new Vector2(angularRate.x, angularRate.y);
        Vector2 rateErrorD = Vector2.zero;
        if(dt > 0.0f)
        {
            rateErrorD = (rateError - lastError_pq) / dt;
            pqInt = pqInt + rateError * dt;
        }
        
        float rollMoment = Kp_p * rateError.x + Kd_p* rateErrorD.x + Ki_p*pqInt.x;
        float pitchMoment = Kp_q * rateError.y + Kd_q * rateErrorD.y + Ki_q * pqInt.y;
        return new Vector2(rollMoment, pitchMoment);   
    }

    private Vector3 euler2RM(float roll, float pitch, float yaw, int column)
    {
        Vector3 columnRM = Vector3.zero;
        if (column == 1)
        {
            columnRM.x = Mathf.Cos(pitch) * Mathf.Cos(yaw);
            columnRM.y = -Mathf.Cos(roll) * Mathf.Sin(yaw) + Mathf.Sin(roll) * Mathf.Sin(pitch) * Mathf.Cos(yaw);
            columnRM.z = Mathf.Sin(roll) * Mathf.Sin(yaw) + Mathf.Cos(roll) * Mathf.Sin(pitch) * Mathf.Cos(yaw);
        }else if (column == 2)
        {
            columnRM.x = Mathf.Cos(pitch) * Mathf.Sin(yaw);
            columnRM.y = Mathf.Cos(roll) * Mathf.Cos(yaw) + Mathf.Sin(roll) * Mathf.Sin(pitch) * Mathf.Sin(yaw);
            columnRM.z =- Mathf.Sin(roll) * Mathf.Cos(yaw) + Mathf.Cos(roll) * Mathf.Sin(pitch) * Mathf.Sin(yaw);
        }else if (column == 3)
        {
            columnRM.x = -Mathf.Sin(pitch);
            columnRM.y = Mathf.Sin(roll) * Mathf.Cos(pitch);
            columnRM.z = Mathf.Cos(roll) * Mathf.Cos(pitch);
        }
        return columnRM;
    }
}
