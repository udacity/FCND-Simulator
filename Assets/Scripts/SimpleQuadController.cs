using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleQuadController : MonoBehaviour
{
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
    public float Kp_hdot = 20.0f;
    public float Kp_r = 10.0f;
    public float Kp_roll = 4.0f;
    public float Kp_p = 6.0f;
    public float Kp_pitch = 4.0f;
    public float Kp_q = 6.0f;
    public float Kp_pos = 0.05f;
    public float Kp_vel = -0.05f;

    //Vehicle control thresholds
    public float posctl_band = 0.25f;
    public float moveSpeed = 10;
    public float turnSpeed = 90;
    public float thrustForce = 25.0f;
    public float thrustMoment = 2.0f;
    public float maxTilt = 22.5f;
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
    Vector3 pos_hold =new Vector3(0.0f,0.0f,0.0f);

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

	void LateUpdate ()
	{
		if ( Input.GetKeyDown ( KeyCode.F12 ) )
		{
			active = !active;
            
			if ( active )
			{
				controller.UseGravity = true;
				controller.rb.isKinematic = false;
				controller.rb.freezeRotation = false;
				controller.rb.velocity = Vector3.zero;
			} else
				controller.rb.freezeRotation = false;
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

        //Test Input for GoTo mode
        if( Input.GetKeyDown(KeyCode.Alpha9))
        {
            guided = true;
            pos_hold.x = rb.position.x + 20.0f;
            pos_hold.y = rb.position.y + 5.0f;
            pos_hold.z = rb.position.z + 20.0f;
            pos_set = true;
            Debug.Log(pos_hold);
        }


        //Arm with the
        if (Input.GetKeyDown(KeyCode.A))
            motors_armed = !motors_armed;





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
                Vector3 velCmd = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), Input.GetAxis("Thrust"));
                Vector3 vel = rb.transform.InverseTransformDirection(rb.velocity);

                if (guided|| Mathf.Sqrt(Mathf.Pow(velCmd.x, 2.0f) + Mathf.Pow(velCmd.y, 2.0f) + Mathf.Pow(velCmd.z, 2.0f)) < posctl_band)
                {
                    if (!pos_set)
                    {
                        pos_hold.x = rb.position.x;
                        pos_hold.y = rb.position.y;
                        pos_hold.z = rb.position.z;
                        pos_set = true;
                        Debug.Log(pos_hold);
                    }
                    Vector3 posErrorRel = rb.transform.InverseTransformDirection(pos_hold - rb.position);


                    velCmd[2] = Kp_pos * posErrorRel[1];
                    velCmd[0] = Kp_pos * posErrorRel[0];
                    velCmd[1] = -Kp_pos * posErrorRel[2];

                }
                else
                {
                    pos_set = false;
                }
                angle_input[0] = velCmd.z;
                angle_input[1] = Input.GetAxis("Yaw");
                angle_input[2] = Kp_vel * (moveSpeed * velCmd.x - vel.x);
                angle_input[3] = -Kp_vel * (-moveSpeed * velCmd.y - vel.z);
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

                Vector3 prq = rb.transform.InverseTransformDirection(rb.angularVelocity) * 180.0f / Mathf.PI;

                float roll_deg = rb.transform.eulerAngles.x;
                if (roll_deg > 180.0)
                    roll_deg = roll_deg - 360.0f;

                float pitch_deg = rb.transform.eulerAngles.z;
                if (pitch_deg > 180.0)
                    pitch_deg = pitch_deg - 360.0f;

                thrust[1] = (Kp_hdot * (maxHdot * angle_input[0] - 1.0f * rb.velocity[1]) + thrust_nom) / (Mathf.Cos(roll_deg * Mathf.PI / 180.0f) * Mathf.Cos(pitch_deg * Mathf.PI / 180.0f));

                yaw_moment[1] = Kp_r * (turnSpeed * angle_input[1] - prq[1]);

                pitch_moment[2] = Kp_q * (Kp_pitch * (maxTilt * angle_input[2] - pitch_deg) - prq[2]);

                roll_moment[0] = Kp_p * (Kp_roll * (maxTilt * angle_input[3] - roll_deg) - prq[0]);


            }
            else
            {
                thrust = thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
                yaw_moment = thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
                pitch_moment = thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
                roll_moment = thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
            }
            rb.AddRelativeForce(thrust);
            rb.AddRelativeTorque(Izz * yaw_moment + Iyy * pitch_moment + Ixx * roll_moment);
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
}