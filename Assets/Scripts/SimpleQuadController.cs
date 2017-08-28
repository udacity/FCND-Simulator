using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleQuadController : MonoBehaviour
{
	public Transform chassis;
	public Transform camTransform;
	public QuadController controller;
	public FollowCamera followCam;
	public PathFollower pather;
	public float moveSpeed = 10;
	public float thrustForce = 25.0f;
    public float thrustMoment = 2.0f;
	public float maxTilt = 22.5f;
    public float maxHdot = 5.0f;
	public float tiltSpeed = 22.5f;
	public float turnSpeed = 90;
    public bool stabilized = true;
    public bool posctl = true;
    public float posctl_band = 0.25f;

    public bool go_to = false;
    public float poshold_vel = 1.0f;
    



    public float Kp_hdot = 2.5f;
    //public float Kp_h = 1.5f;
        
    public float Kp_r = 1.75f;
    public float Kp_roll = 6.0f;
    public float Kp_p = 1.75f;
    public float Kp_pitch = 6.0f;
    public float Kp_q = 1.75f;
    public float Kp_pos = -0.5f;
    public float Kp_vel = -0.05f;

    public float Ixx = 0.004856f;
    public float Iyy = 0.004856f;
    public float Izz = 0.008801f;
    public float mass = 0.468f;
    

	Rigidbody rb;
	float tiltX;
	float tiltZ;
    
    private float h_des = 0.0f;
    private bool pos_set = false;
    private Vector3 pos_hold =new Vector3(0.0f,0.0f,0.0f);

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
				controller.UseGravity = false;
				controller.rb.isKinematic = true;
				controller.rb.isKinematic = false;
				controller.rb.freezeRotation = false;
				controller.rb.velocity = Vector3.zero;
			} else
				controller.rb.freezeRotation = false;
		}

		if ( Input.GetKeyDown ( KeyCode.R ) )
		{
			controller.ResetOrientation ();
			followCam.ChangePoseType ( CameraPoseType.Iso );
		}

		if ( Input.GetKeyDown ( KeyCode.G ) )
		{
			controller.UseGravity = !controller.UseGravity;
		}

		if ( !active )
			return;

        if( Input.GetKeyDown(KeyCode.Alpha9))
        {
            go_to = true;
            pos_hold.x = rb.position.x + 10.0f;
            pos_hold.y = rb.position.y;
            pos_hold.z = rb.position.z + 10.0f;
            pos_set = true;
        }




        //Direct Control of the moments
        Vector3 thrust = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 yaw_moment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 pitch_moment = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 roll_moment = new Vector3(0.0f,0.0f,0.0f);
        Vector4 angle_input = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        if (posctl)
        {
            Vector3 velCmd = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), Input.GetAxis("Thrust"));
            Vector3 vel = rb.transform.InverseTransformDirection(rb.velocity);

            if(go_to||(Mathf.Sqrt(Mathf.Pow(vel.x, 2.0f) + Mathf.Pow(vel.z, 2.0f)) < poshold_vel 
                && Mathf.Sqrt(Mathf.Pow(velCmd.x,2.0f) + Mathf.Pow(velCmd.y, 2.0f)) < posctl_band)){
                if (!pos_set)
                {
                    pos_hold.x = rb.position.x;
                    pos_hold.y = rb.position.y;
                    pos_hold.z = rb.position.z;
                    pos_set = true;
                }
                Vector3 posErrorRel = rb.transform.InverseTransformDirection(pos_hold-rb.position);


                angle_input[0] = velCmd.z;
                angle_input[1] = Input.GetAxis("Yaw");
                angle_input[2] = Kp_pos * posErrorRel.x;
                angle_input[3] = -Kp_pos * posErrorRel.z;
            }
            else
            {
                pos_set = false;
                angle_input[0] = velCmd.z;
                angle_input[1] = Input.GetAxis("Yaw");
                angle_input[2] = Kp_vel * (moveSpeed*velCmd.x-vel.x);
                angle_input[3] = -Kp_vel * (-moveSpeed*velCmd.y-vel.z);
            }
        }
        else
        {
            //Pilot Input: Hdot, Yawrate, pitch, roll
            angle_input = new Vector4(Input.GetAxis("Thrust"), Input.GetAxis("Yaw"), Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        }


        //Constrain the angle inputs between -1 and 1 (tilt, turning speed, and vert speed taken into account later)
        for(int i = 1; i < 4; i++)
        {
            if (angle_input[i] > 1.0f)
                angle_input[i] = 1.0f;
            else if (angle_input[i] < -1.0f)
                angle_input[i] = -1.0f;
        }

        
        float roll_deg = 0.0f;

        Vector3 prq = new Vector3(0.0f,0.0f,0.0f);
        if (stabilized)
        {

            float thrust_nom = -1.0f * rb.mass * Physics.gravity[1];

            prq = rb.transform.InverseTransformDirection(rb.angularVelocity)*180.0f/Mathf.PI;
            
            roll_deg = rb.transform.eulerAngles.x;
            if (roll_deg > 180.0)
                roll_deg = roll_deg - 360.0f;

            float pitch_deg = rb.transform.eulerAngles.z;
            if (pitch_deg > 180.0)
                pitch_deg = pitch_deg - 360.0f;

            thrust[1] = ( Kp_hdot * (maxHdot*angle_input[0] - 1.0f*rb.velocity[1]) + thrust_nom)/ (Mathf.Cos(roll_deg*Mathf.PI/180.0f)*Mathf.Cos(pitch_deg*Mathf.PI/180.0f));

            yaw_moment[1] = Kp_r * (turnSpeed*angle_input[1] - prq[1]);

            pitch_moment[2] = Kp_pitch * (maxTilt*angle_input[2] - pitch_deg) + Kp_q*(0.0f-prq[2]);

            roll_moment[0] = Kp_roll * (maxTilt*angle_input[3] - roll_deg) + Kp_p*(0.0f-prq[0]);


        }
        else
        {
            thrust = thrustForce * (new Vector3(0.0f, angle_input[0], 0.0f));
            yaw_moment = thrustMoment * (new Vector3(0.0f, angle_input[1], 0.0f));
            pitch_moment = thrustMoment * (new Vector3(angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, angle_input[2] * Mathf.Sqrt(2.0f) / 2.0f));
            roll_moment = thrustMoment * (new Vector3(angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f, 0.0f, -1.0f * angle_input[3] * Mathf.Sqrt(2.0f) / 2.0f));
        }
        rb.AddRelativeForce(thrust);
        rb.AddRelativeTorque(Izz*yaw_moment+Iyy*pitch_moment+Ixx*roll_moment);
        

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