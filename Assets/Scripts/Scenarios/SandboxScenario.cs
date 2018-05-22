using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxScenario : Scenario
{
	protected override void OnInit ()
	{
		base.OnInit ();
        drone.ArmDisarm(true);
        drone.SetControlMode(1);
        drone.SetGuided(false);
        drone.CommandControls(0.0f, 0.0f, 0.0f, 0.67f);
	}

	protected override bool OnCheckSuccess ()
	{
		return base.OnCheckSuccess ();
	}

	protected override bool OnCheckFailure ()
	{
		return base.OnCheckFailure ();
	}

	protected override void OnCleanup ()
	{
		base.OnCleanup ();
	}
}