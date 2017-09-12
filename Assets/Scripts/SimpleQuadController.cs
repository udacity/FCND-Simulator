using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleQuadController : MonoBehaviour
{

    const float M2Latitude = 1.0f / 111111.0f;
    Vector3 initGPS = new Vector3(37.412939f, 0.0f,121.995635f);
    const float M2Longitude = 1.0f / (0.8f * 111111.0f);

    public Transform camTransform;
	public QuadController controller;
	public FollowCamera followCam;
	public PathFollower pather;

    //Vehicle status indicators
    public bool motors_armed = false;
    
    //Flight modes
    public bool guided = false;
    public bool stabilized = true;
    public bool posctl = true;

    //Control Gains
    public float Kp_hdot = 10.0f;
    public float Kp_r = 20.0f;
    public float Kp_roll = 6.5f;
    public float Kp_p = 10.0f;
    public float Kp_pitch = 6.5f;
    public float Kp_q = 10.0f;
    public float Kp_pos = 0.1f;
    public float Kp_vel = 0.3f;//-0.05f;
    public float Kd_vel = 0.0f;
    public float Kp_alt = 1.0f;
    public float Ki_hdot = 0.1f;

    //Vehicle control thresholds
    public float posctl_band = 0.1f;
    public float posHoldDeadband = 1.0f;
    public float moveSpeed = 10;
    public float turnSpeed = 2.0f; //In radians
    public float thrustForce = 25.0f;
    public float thrustMoment = 2.0f;
    public float maxTilt = 0.5f; //MaxTilt in Radians
    public float maxHdot = 5.0f;

    //Default inertia data
    public float Ixx = 0.004856f;
    public float Iyy = 0.004856f;
    public float Izz = 0.008801f;
    public float mass = 0.468f;
    

	Rigidbody rb;
	float tiltX;
	float tiltZ;
    
    private float h_des = 0.0f;



    //
    private bool pos_set = false;
    Vector3 posHoldLocal =new Vector3(0.0f,0.0f,0.0f);
    Vector3 lastVelocityErrorBody = new Vector3(0.0f,0.0f,0.0f);
    float hDotInt = 0.0f;

	public bool active;
	
	void Awake ()
	{
		rb = GetComponent<Rigidbody> ();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		if ( controller == null )
			controller = GetComponent<QuadController> ();
		if ( followCam == null )
			followCam = camTransform.GetComponent<FollowCamera> ();
		active = false;

        rb.mass = mass;
        rb.inertiaTensor.Set(Ixx,Izz,Iyy);

	}

	void FixedUpdate ()
	{
        moveSpeed = 15.0f;
        turnSpeed = 2.0f;
        maxTilt = 0.5f;
		if ( Input.GetKeyDown ( KeyCode.F12 ) )
		{
			active = !active;
			if ( active )
			{
				controller.UseGravity = true;
				controller.rb.isKinematic = false;
				controller.rb.freezeRotation = false;
				controller.rb.velocity = Vector3.zero;
			} else  {
				controller.rb.freezeRotation = false;
            }
		}

		if ( Input.GetKeyDown ( KeyCode.R ) )
		{
            pos_set = false;
			controller.ResetOrientation ();
			followCam.ChangePoseType ( CameraPoseType.Iso );
		}

		if ( Input.GetKeyDown ( KeyCode.G ) )
		{
			controller.UseGravity = !controller.UseGravity;
		}

		if ( !active )
			return;

        //TODO: Remove this, it is for testing the Guided Mode
        if( Input.GetKeyDown(KeyCode.Alpha9))
        {
            guided = true;
            posHoldLocal.x = rb.position.x + 20.0f;
            posHoldLocal.y = rb.position.y + 5.0f;
            posHoldLocal.z = rb.position.z + 20.0f;
            pos_set = true;
            Debug.Log(posHoldLocal);
        }


        //Arm with the
        if (Input.GetKeyDown(KeyCode.A))
            motors_armed = !motors_armed;


        Vector3 rollYawPitch = controller.navTransform.eulerAngles*Mathf.PI/180.0f;
        for (int i = 0;i < 3; i++)
        {
            if (rollYawPitch[i] > Mathf.PI)
                rollYawPitch[i] = rollYawPitch[i] - 2.0f*Mathf.PI;
            else if (rollYawPitch[i] < -Mathf.PI)
                rollYawPitch[i] = rollYawPitch[i] + 2.0f*Mathf.PI;
        }
        Vector3 prq = controller.AngularVelocityBody;
        Vector3 prqRate = controller.AngularAccelerationBody;
        Vector3 localPosition = controller.GPS;
        Vector3 bodyVelocity = controller.BodyVelocity;
        localPosition.x = (localPosition.x - initGPS.x) / M2Latitude;
        localPosition.y = (localPosition.y - initGPS.y);
        localPosition.z = (localPosition.z - initGPS.z) / M2Longitude;


        //Direct Control of the moments
        Vector3 thrust = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 yaw_moment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 pitch_moment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 roll_moment = new Vector3(0.0f,0.0f,0.0f);
        Vector4 angle_input = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        if (motors_armed)
        {
            if (posctl || guided)
            {
                /*
                Vector3 velCmd = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), Input.GetAxis("Thrust"));
                Vector3 vel = rb.transform.InverseTransformDirection(rb.velocity);
                */
                Vector3 velCmdBody = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Thrust"), -Input.GetAxis("Horizontal"));
                float yawCmd = Input.GetAxis("Yaw");
                if (guided|| Mathf.Sqrt(Mathf.Pow(velCmdBody.x, 2.0f) + Mathf.Pow(velCmdBody.y, 2.0f) + Mathf.Pow(velCmdBody.z, 2.0f)) < posctl_band)
                {
                    if (!pos_set)
                    {
                        /*
                        pos_hold.x = rb.position.x;
                        pos_hold.y = rb.position.y;
                        pos_hold.z = rb.position.z;
                        */
                        posHoldLocal = localPosition;
                        pos_set = true;
                        Debug.Log(posHoldLocal);
                    }
                    /*
                    Vector3 posErrorRel = rb.transform.InverseTransformDirection(pos_hold - rb.position);


                    velCmd[2] = Kp_alt* posErrorRel[1];
                    velCmd[0] = Kp_pos * posErrorRel[0];
                    velCmd[1] = -Kp_pos * posErrorRel[2];
                    */
                    Vector3 posErrorLocal = posHoldLocal - localPosition;
                    Vector3 velCmdLocal;

                    //Put a deadband around the position hold
                    if (Mathf.Sqrt(Mathf.Pow(posErrorLocal.x, 2.0f) + Mathf.Pow(posErrorLocal.z, 2.0f)) < posHoldDeadband)
                    {
                        velCmdLocal.x = 0.0f;
                        velCmdLocal.z = 0.0f;
                    }
                    else
                    {
                        velCmdLocal.x = Kp_pos * posErrorLocal.x;
                        velCmdLocal.z = Kp_pos * posErrorLocal.z;
                    }
                    
                    velCmdLocal.y = Kp_alt * posErrorLocal.y;
                   

                    float cosYaw = Mathf.Cos(rollYawPitch.y);
                    float sinYaw = Mathf.Sin(rollYawPitch.y);
                    velCmdBody.x =cosYaw * velCmdLocal.x - sinYaw*velCmdLocal.z;
                    velCmdBody.z = sinYaw * velCmdLocal.x + cosYaw* velCmdLocal.z;

                    velCmdBody.y = velCmdLocal.y;

                }
                else
                {
                    pos_set = false;
                    
                }

                Vector3 velocityErrorBody = new Vector3(0.0f, 0.0f, 0.0f);
                Vector3 velocityErrorBodyD = new Vector3(0.0f, 0.0f, 0.0f);
                velocityErrorBody.x = moveSpeed * velCmdBody.x - bodyVelocity.x;
                velocityErrorBody.z = moveSpeed * velCmdBody.z - bodyVelocity.z;
                velocityErrorBodyD = (velocityErrorBody - lastVelocityErrorBody) / Time.deltaTime;
                lastVelocityErrorBody = velocityErrorBody;

                angle_input[2] = -Kp_vel * velocityErrorBody.x - Kd_vel * velocityErrorBodyD.x;
                angle_input[3] = Kp_vel * velocityErrorBody.z + Kd_vel * velocityErrorBodyD.z;

                angle_input[0] = velCmdBody.y;
                angle_input[1] = yawCmd;
                /*
                angle_input[0] = velCmd.z;
                angle_input[1] = Input.GetAxis("Yaw");
                angle_input[2] = Kp_vel * (moveSpeed * velCmd.x - vel.x);
                angle_input[3] = -Kp_vel * (-moveSpeed * velCmd.y - vel.z);
                */
            }
            else
            {
                pos_set = false;

                //Pilot Input: Hdot, Yawrate, pitch, roll
                angle_input = new Vector4(Input.GetAxis("Thrust"), Input.GetAxis("Yaw"), -Input.GetAxis("Vertical"), -Input.GetAxis("Horizontal"));
            }


            //Constrain the angle inputs between -1 and 1 (tilt, turning speed, and vert speed taken into account later)
            for (int i = 1; i < 4; i++)
            {
                if (angle_input[i] > 1.0f)
                    angle_input[i] = 1.0f;
                else if (angle_input[i] < -1.0f)
                    angle_input[i] = -1.0f;
            }

            if (guided||posctl||stabilized)
            {
                
                float thrust_nom = -1.0f * rb.mass * Physics.gravity[1];

                float hDotError = (maxHdot * angle_input[0] - 1.0f * controller.LinearVelocity.y);
                hDotInt = hDotInt + hDotError * Time.deltaTime;

                thrust[1] = (Kp_hdot * hDotError + Ki_hdot*hDotInt+ thrust_nom) / (Mathf.Cos(rollYawPitch.x) * Mathf.Cos(rollYawPitch.z));
                
                yaw_moment[1] = Kp_r * (turnSpeed * angle_input[1] - prq[1]);

                Debug.Log("Pitch: " + rollYawPitch.z);
                float pitchError = maxTilt * angle_input[2] - rollYawPitch.z;
                float rollError = maxTilt * angle_input[3] - rollYawPitch.x;

                float pitchRateError = Kp_pitch * pitchError - prq.z;
                Debug.Log("Q: "+ prq.z );

                
                float rollRateError = Kp_roll * rollError - prq.x;

                pitch_moment[2] = Kp_q * pitchRateError;

                Debug.Log("Pitch Moment: " +  pitch_moment[2]);

                roll_moment[0] = Kp_p * rollRateError;




            }
            else
            {
                thrust = thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
                yaw_moment = thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
                pitch_moment = thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
                roll_moment = thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
            }
            rb.AddRelativeForce(thrust);
            rb.AddRelativeTorque(yaw_moment +  pitch_moment + roll_moment);
        }
        else
        {
            pos_set = false;
        }

        //rb.AddRelativeTorque(pitch_moment);
        //rb.AddRelativeTorque(yaw_moment);


        //rb.AddForceAtPosition(new Vector3(0.0f,0.0f,10.0f),)
        //controller.ApplyMotorForce(input);


        //transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        /*

        Vector3 input = new Vector3 ( Input.GetAxis ( "Horizontal" ), Input.GetAxis ( "Thrust" ), Input.GetAxis ( "Vertical" ) );

		Vector3 forwardVelocity = Vector3.forward * input.z * moveSpeed;
		Vector3 sidewaysVelocity = Vector3.right * input.x * moveSpeed;
		Vector3 upVelocity = Vector3.up * input.y * thrustForce;
		Vector3 inputVelo = forwardVelocity + sidewaysVelocity + upVelocity;

		Vector3 forward = transform.forward - transform.right;
		forward.y = 0;
		Quaternion rot = Quaternion.LookRotation ( forward.normalized, Vector3.up );

//		rb.AddRelativeForce ( chassis.rotation * inputVelo * Time.deltaTime, ForceMode.VelocityChange );
		rb.velocity = rot * inputVelo;
//		transform.Rotate ( Vector3.up * input.x * thrustForce * Time.deltaTime, Space.World );

		float x = input.z / 2 + input.x / 2;
		float z = input.z / 2 - input.x / 2;
		Vector3 euler = transform.localEulerAngles;
		euler.x = maxTilt * x;
		euler.z = maxTilt * z;
		transform.localEulerAngles = euler;

		float yaw = Input.GetAxis ( "Yaw" );
		if ( yaw != 0 )
		{
			transform.Rotate ( Vector3.up * yaw * turnSpeed * Time.deltaTime, Space.World );
			camTransform.Rotate ( Vector3.up * yaw * turnSpeed * Time.deltaTime, Space.World );
		}

		if ( Input.GetKeyDown ( KeyCode.P ) )
		{
			PathPlanner.AddNode ( controller.Position, controller.Rotation );
		}
		if ( Input.GetKeyDown ( KeyCode.O ) )
		{
			controller.ResetOrientation ();
			pather.SetPath ( new Pathing.Path ( PathPlanner.GetPath () ) );
			PathPlanner.Clear ( false ); // clear the path but keep the visualization
		}
		if ( Input.GetKeyDown ( KeyCode.I ) )
		{
			PathPlanner.Clear ();
		}
        */
    }

	void OnGUI ()
	{
		GUI.backgroundColor = active ? Color.green : Color.red;
//		GUI.contentColor = Color.white;
		Rect r = new Rect ( 10, Screen.height - 100, 60, 25 );
		if ( GUI.Button ( r, "Input " + ( active ? "on" : "off" ) ) )
		{
			active = !active;
		}
	}

    // public void CommandGPS(double latitude, double longitude, double altitude)
    // {
    //     pos_set = true;
    //     posHoldLocal.x = (float)(latitude-latitude0) / M2Latitude;
    //     posHoldLocal.y = (float)(altitude);
    //     posHoldLocal.z = (float)(longitude -longitude0) / M2Longitude;
    // }

    //Command the quad to a GPS location (latitude, relative_altitude, longitude)
    public void CommandGPS(Vector3 GPS)
    {
        posHoldLocal.x = (GPS.x - initGPS.x) / M2Latitude;
        posHoldLocal.y = (GPS.y - initGPS.y);
        posHoldLocal.z = (GPS.z - initGPS.z) / M2Longitude;
    }

    public void ArmVehicle()
    {
        motors_armed = true;
    }

    public void DisarmVehicle()
    {
        motors_armed = false;
    }

    public void SetGuidedMode(bool input_guided)
    {
        guided = input_guided;
    }
}