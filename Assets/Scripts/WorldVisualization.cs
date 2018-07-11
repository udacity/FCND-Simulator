using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldVisualization : MonoBehaviour
{
	public static WorldVisualization Instance
	{
		get
		{
			if ( instance == null )
				instance = GameObject.Find ( "World Visualization" ).GetComponent<WorldVisualization> ();
			return instance;
		}
	}
	static WorldVisualization instance;


	public LineRenderer trajectory;
	public int trajectoryPointCount = 10;
	public float trajectoryDistance = 50;
	Vector3[] positions;

	void Awake ()
	{
		if ( instance != null && instance != this )
		{
			Destroy ( gameObject );
			return;
		}

		instance = this;

		positions = new Vector3[trajectoryPointCount];
		trajectory.positionCount = trajectoryPointCount;
		trajectory.SetPositions ( positions );
	}


	public void SetTrajectory (Vector3 position, Vector3 velocity, Vector3 angVelocity)
	{
		Vector3 direction = velocity.normalized;
		float speed = velocity.magnitude;
		for ( int i = 0; i < trajectoryPointCount; i++ )
		{
			float percent = 1f * i / trajectoryPointCount;
			float distance = percent * trajectoryDistance;
			Vector3 frameDirection = direction * speed * distance * Time.fixedDeltaTime;

			positions [ i ] = position + frameDirection;
			position += frameDirection;
			direction += ( angVelocity + velocity ) * Time.fixedDeltaTime;
		}
		trajectory.SetPositions ( positions );
	}
}