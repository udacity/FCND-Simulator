using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleRecorder : MonoBehaviour {

	public enum RecorderMode {Record, Play};

	[System.Serializable]
	public class RecFrame
	{
		public Vector3 velocity;
		public Quaternion rotation;
		public float aileronInput;
		public float elevatorInput;
		public float rudderInput;
		public float throttleInput;
		public int gearState;
		public int flapsIndex;
	}
	public bool rec;
	public int frameIndex;
	public RecorderMode mode;
	public List<RecFrame> recordedFrames;
	public ControlAnimator controls;
	public Rigidbody vehicle;
	public EnginesManager engines;
	public Vector3 initialPosition;
	public Quaternion initialRotation;

	// Use this for initialization
	void Start () {
		vehicle = GetComponent<Rigidbody> ();
		if (mode == RecorderMode.Record) {
			initialPosition = transform.position;
			initialRotation = transform.rotation;
		}
			if (mode == RecorderMode.Play) {
			transform.position = initialPosition;
			transform.rotation = initialRotation;
		}
			
	}

	
	
	// Update is called once per frame
		void FixedUpdate () {
	
		switch (mode) {

		case RecorderMode.Record :

			if(rec)
			RecordFrame ();

			break;


		case RecorderMode.Play:
			
			controls.inputs.player = false;

			if(frameIndex < recordedFrames.Count){
				if(frameIndex < 300)
					controls.inputs.engineOn = true;
				else 
					controls.inputs.engineOn = false;
				
				PlayFrame(recordedFrames[frameIndex]);
				frameIndex++;
			}

			break;
		}

	}

	void RecordFrame (){

		RecFrame newFrame = new RecFrame ();

		newFrame.aileronInput = controls.inputs.aileron;
		newFrame.elevatorInput = controls.inputs.elevator;
		newFrame.rudderInput = controls.inputs.rudder;
		newFrame.throttleInput = controls.inputs.throttle;
		newFrame.velocity = vehicle.velocity;
		newFrame.rotation = transform.rotation;
		newFrame.gearState = 1;
		if(!controls.inputs.gearDwn)
			newFrame.gearState = 0;
		newFrame.flapsIndex = controls.flapsIndex;

		recordedFrames.Add (newFrame);

	}

	void PlayFrame (RecFrame frame){

		vehicle.velocity = Vector3.Lerp (vehicle.velocity, frame.velocity, Time.deltaTime * 15f);
		vehicle.rotation = Quaternion.Lerp (transform.rotation, frame.rotation, Time.deltaTime * 15f);
		controls.inputs.aileron = frame.aileronInput;
		controls.inputs.elevator = frame.elevatorInput;
		controls.inputs.rudder = frame.rudderInput;
		engines.throttleInput = frame.throttleInput;

		controls.inputs.flaps = frame.flapsIndex;

	}

}
