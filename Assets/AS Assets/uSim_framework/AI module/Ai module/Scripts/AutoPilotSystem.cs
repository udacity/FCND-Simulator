using UnityEngine;
using System.Collections;

public class AutoPilotSystem : MonoBehaviour {

	public enum ExitTriggers {None, Altitude, BelowAltitude, AboveVspeed, AboveAirspeed, ArrivedWaypoint, ArrivedFinalWaypoint, BelowVspeed, BelowAirspeed, BelowDistance, BelowDistanceToIls, Time};
	public enum ExitModes {None, NextRoutine, NextAction, ExitRoutine, NextWaypoint, ExitAction, SetToNone};
	public enum VerticalFlightModes	{None, ClimbOut, ChangeFlightLevel, WaypointLevel, LevelFlight, HoldPitch, HoldVspeed, HoldAngleAttack, Loop, InvertFlight, LandingAprouch, Flare, StallOut};
	public enum HorizontalFlightModes {None, LevelWings, HoldHeding, AlignToWp, AlignToRunway, HoldCourse, HoldBank, RadialInbound, InvertWings};
	public enum LongitudinalFlightModes {None, HoldSpeed, FullThrottle, IdleThrottle,FormationFlight, MatchOther};



	//pilot settings
	public AnimationCurve elevatorResponse;
	public AnimationCurve aileronResponse;
	//public Aerofoil referenceWing;

	//aircraft references
	public InputsManager inputs;
	public Rigidbody aircraft;
	AutoPilotActionsManager actionsManager;
	UsimVehicle vehicle;
	public AircraftControl controller;
	public EnginesManager engines;

	public bool verticalEnabled;
	public bool horizontalEnabled;

	public float stallSpeed;
	public float climbSpeed;
	public float bestClimbAngle;
	public float optimusAoa;
	public float cruiseSpeed;
	public float aprouchSpeed;
	public int landingFlapIndex;
	public float flareHeight;
	public float v1Speed;
	public float normalBankAngle;

	//current Data
	public float aircraftSpeed;
	public float aircraftAltitude;
	public float aircraftVerticalSpeed;
	public float aircraftAoa;
	public float aircraftBank;
	public float deltaAlt;
	public float deltaAoa;
	public float deltaPitch;
	public float deltaBank;
	public float deltaHdg;
	public float deltaSpd;
	public float deltaVspd;
	public float distToWp;

	public float elevatorOutput;
	public float aileronOutput;
	public float throttleOutput;

	//engine data
	public float engineThrottle;

	[System.Serializable]
	public class PilotRoutine
	{				
		public VerticalFlightModes verticalFlightMode;
		public float targetAlt;
		public float targetAoa;
		public float targetPitch;
		public float targetVspd;
		public HorizontalFlightModes horizontalFlightMode;
		public bool useSteering;
		public bool useRoutineTarget;
		public float targetBnk;
		public float targetHdg;
		public float targetCrs;
		public float targetRdl;
		public Transform targetWp;
		public LongitudinalFlightModes longitudinalFlightMode;
		public float targetSpd;
		public float targetThr;
		public ExitTriggers exitTrigger;
		public float exitValue;
		public ExitModes exitMode;
	}

	[System.Serializable]
	public class AutoPilotActionGroup
	{	
		public string actionName;	
		public AiFlightPlan.FlightStates flightState;
		public int routineIndex;
		public PilotRoutine[] routines;	
		public ExitTriggers actionExitTrigger;
		public ExitModes actionExitMode;
		public string exitAction;

	}

	public AutoPilotAction currentAction;
	public PilotRoutine currentRoutine;
	public int routineIndex;
	public bool finishRoute = false;
	public bool ignoreOutsideTriggers = false;
	public bool allowAirCombatManouvers;
	bool gearDown;
	// Use this for initialization
	void Start () {
		actionsManager = GetComponent<AutoPilotActionsManager> ();
		controller = GetComponent<AircraftControl> ();
		engines = GetComponentInChildren<EnginesManager> ();
		gearDown = inputs.gearDwn;
		vehicle = aircraft.GetComponent<UsimVehicle> ();
		bankFactor = 1f;
	}

	public float t;
	public float courseDev;

	// Update is called once per frame
	void FixedUpdate () {

		//reads
		GetAoa ();
		aircraftVerticalSpeed = GetVSpeed ();
		aircraftAltitude = GetAlt ();
		aircraftSpeed = GetSpeed ();


		if (actionsManager.flightPlanFeed != null)
		if (actionsManager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.StandBy) {
		
			aileronOutput = 0f;
			elevatorOutput = 0f;
			inputs.rudder = inputs.steering = 0f;
			throttleOutput = 0f;
			inputs.wheelBrake = 1f;
			currentRoutine.longitudinalFlightMode = LongitudinalFlightModes.IdleThrottle;

			return;
		}
		if (actionsManager.flightPlanFeed != null)
		if (actionsManager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.TakeOff) {


		}

		if (actionsManager.flightPlanFeed != null)
		if (actionsManager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.FlyFormation) {

			if(controller.isRGear && gearDown){
				inputs.SetLandingGear (false);
				gearDown = false;
			}
			if (inputs.flaps > 0)
				inputs.SetflapsUp (true);
			currentRoutine.longitudinalFlightMode = LongitudinalFlightModes.FormationFlight;
			currentRoutine.targetBnk = Mathf.Lerp (normalBankAngle * 0.15f, normalBankAngle, Mathf.Abs (deltaHdg) * 0.1f);
			if (actionsManager.formationPosition != null) {
				transform.position = Vector3.Lerp (transform.position, actionsManager.formationPosition.position, Time.deltaTime * 0.1f);
				aircraft.velocity = Vector3.Lerp (aircraft.velocity, actionsManager.flightPlanFeed.formationLeader.GetComponent<Rigidbody> ().velocity, Time.deltaTime * 1f);
				aircraft.rotation = Quaternion.Lerp (aircraft.rotation, actionsManager.flightPlanFeed.formationLeader.transform.rotation, Time.deltaTime * 1f);
			}
		}


		inputs.wheelBrake = 0f;
		if (actionsManager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.FlyRoute) {
			if (vehicle.vehicleType == UsimVehicle.VehicleTypes.Air) {
				bankFactor = 1f;
				if (controller.isRGear && gearDown) {
					inputs.SetLandingGear (false);
					gearDown = false;
				}
				if (inputs.flaps > 0)
					inputs.SetflapsUp (true);
			}
			
			if (actionsManager.waypointIndex < actionsManager.waypoints.Count)				
			currentRoutine.targetWp = actionsManager.waypoints [actionsManager.waypointIndex];
			currentRoutine.longitudinalFlightMode = LongitudinalFlightModes.HoldSpeed;
			currentRoutine.targetSpd = cruiseSpeed;
		}

		if(actionsManager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.Park) {

			actionsManager.currentWp = actionsManager.flightManager.parkingSpot;

			if (Vector3.Distance (actionsManager.flightManager.parkingSpot.position, transform.position) <= 15f) {
				actionsManager.flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.StandBy;
				foreach (Engine engine in engines.engines) {

					engine.fuelValveOpen = false;

				}
			}
		}

		if (actionsManager.flightPlanFeed.currentFlightState == AiFlightPlan.FlightStates.Taxi) {

			currentRoutine.longitudinalFlightMode = LongitudinalFlightModes.HoldSpeed;
						

			if (actionsManager.currentRunway != null) {				
				if (actionsManager.currentRunway.runwayState == Runway.RunwayStates.Traffic) {
					if(actionsManager.currentRunway.onAprouchAircraft = this.gameObject)
					actionsManager.currentRunway.runwayState = Runway.RunwayStates.Clear;
				}
			}
		}
	
		if (currentAction == null)
			return;
		
		if(currentAction.action.routines.Length > 0)
		currentRoutine = currentAction.action.routines[currentAction.action.routineIndex];
		if (verticalEnabled) {
			switch (currentRoutine.verticalFlightMode) {

			case  VerticalFlightModes.None:

				if (actionsManager.currentRunway == null)
					return;
				if (actionsManager.currentRunway.runwayState != Runway.RunwayStates.Clear && actionsManager.currentRunway.onAprouchAircraft == this.gameObject) {
					actionsManager.currentRunway.runwayState = Runway.RunwayStates.Clear;
					actionsManager.currentRunway = null;
				}

				break;

			case  VerticalFlightModes.ClimbOut:

				if (aircraftSpeed > v1Speed)			
				currentRoutine.targetPitch = bestClimbAngle;
				else
					currentRoutine.targetPitch = 1;
						
					HoldPitch ();

				break;

			case  VerticalFlightModes.ChangeFlightLevel:
					
				HoldAlt ();

				break;

			case  VerticalFlightModes.WaypointLevel:

				if (actionsManager.currentWp != null) {
					//currentRoutine.targetWp = actionsManager.currentWp;
					if(currentRoutine.targetWp != null)
					currentRoutine.targetAlt = currentRoutine.targetWp.transform.position.y;
					HoldAlt ();
				}

				break;
									

			case  VerticalFlightModes.Flare:

				float runwayAlt = actionsManager.currentRunway.transform.position.y;
				float flareDeltaAlt = aircraftAltitude - (runwayAlt + flareHeight);
				currentRoutine.targetVspd = 0f;
				HoldVspeed ();
				currentRoutine.targetSpd = stallSpeed * 0.5f; 
				//currentRoutine.longitudinalFlightMode = LongitudinalFlightModes.IdleThrottle;
				//currentRoutine.targetPitch = optimusAoa * 0.75f;
			//	HoldPitch ();
				currentRoutine.exitValue = stallSpeed * 0.75f;

				if (aircraftVerticalSpeed > 0f) {
					Vector3 aircraftVelocity = aircraft.velocity;
					aircraftVelocity.y = Mathf.Lerp (aircraftVelocity.y, 0f, Time.deltaTime * 3f);
					aircraft.velocity = aircraftVelocity;
				}

				break;

			case  VerticalFlightModes.HoldAngleAttack:

				HoldAngleOfAttack ();

				break;

			case  VerticalFlightModes.HoldVspeed:

				HoldVspeed ();

				break;

			case  VerticalFlightModes.HoldPitch:

				HoldPitch ();

				break;

			case  VerticalFlightModes.InvertFlight:



				break;

			case  VerticalFlightModes.LandingAprouch:

				if (actionsManager.currentRunway == null)
					actionsManager.currentRunway = actionsManager.flightPlanFeed.toRunway;

				if (actionsManager.currentRunway.runwayState != Runway.RunwayStates.Clear && actionsManager.currentRunway.onAprouchAircraft != this.gameObject) {

					GameObject newFlightPlan = (GameObject)Instantiate (actionsManager.currentRunway.waitCircuit.gameObject, transform) as GameObject;
					AiFlightPlan cloneFp = newFlightPlan.GetComponent<AiFlightPlan> ();

					cloneFp.manager = actionsManager;

					actionsManager.flightPlanFeed = cloneFp;
					actionsManager.waypoints = actionsManager.flightPlanFeed.waypoints;
					actionsManager.waypointIndex = 0;
					actionsManager.index = 0;
					//	actionsManager.startButton.flightPlan = actionsManager.flightPlanFeed;
					LoadActions ();

					return;
				}


				currentRoutine.exitValue = currentRoutine.targetWp.position.y + flareHeight;			
				actionsManager.flightPlanFeed.currentFlightState = AiFlightPlan.FlightStates.Aprouch;
				glideslopeAngle = actionsManager.currentRunway.glideslopeAngle;
				Vector3 comparePos = actionsManager.currentRunway.ilsVectorPointer.position;
				Vector3 dir = comparePos - transform.position;
				var localTarget = transform.InverseTransformPoint (comparePos);
				var targetAngle = Mathf.Atan2 (localTarget.y, localTarget.z) * Mathf.Rad2Deg;

				currentAngle = targetAngle;
				deltaAngle = glideslopeAngle + currentAngle;
				if (deltaAngle > 180)
					deltaAngle = 360 - currentAngle;
				

				currentRoutine.targetSpd = aprouchSpeed;
				HoldSpeed ();

				if (aircraftAltitude - actionsManager.currentRunway.ilsVectorPointer.position.y < flareHeight) {
					currentRoutine.targetVspd = 0f;
					HoldVspeed ();
				} else {
					currentRoutine.targetPitch = Mathf.Clamp (deltaAngle * 5f, -10f, 5f);
					HoldPitch ();	
				}

				if (actionsManager.currentRunway.runwayState == Runway.RunwayStates.Clear) {
					actionsManager.currentRunway.onAprouchAircraft = this.gameObject;
					actionsManager.currentRunway.runwayState = Runway.RunwayStates.Traffic;
					if (controller.isRGear && !gearDown) {
						inputs.SetLandingGear (true);				
						inputs.flaps = landingFlapIndex;
						gearDown = true;
					}
				}
				break;

			case  VerticalFlightModes.LevelFlight:



				break;

			case  VerticalFlightModes.Loop:

				currentRoutine.targetPitch += Time.deltaTime * 15f;
				HoldPitch ();

				break;

			case  VerticalFlightModes.StallOut:



				break;
			}
					
		}

		switch (currentRoutine.longitudinalFlightMode) {

		case  LongitudinalFlightModes.None:

			break;

		case  LongitudinalFlightModes.FullThrottle:

			FullThrottle ();

			break;

		case LongitudinalFlightModes.HoldSpeed:

			HoldSpeed();

			break;

		case LongitudinalFlightModes.IdleThrottle:

			currentRoutine.targetThr = 0f;
			HoldThrottle ();

			break;

		case LongitudinalFlightModes.FormationFlight:

			HoldFormationSpeed ();

			break;
		}


		switch (currentRoutine.horizontalFlightMode) {

		case  HorizontalFlightModes.None:

			break;

		case HorizontalFlightModes.LevelWings:

			currentRoutine.targetBnk = 0f;
			HoldBank ();

			break;

		case  HorizontalFlightModes.HoldHeding:

			HoldHdg ();

			break;

		case  HorizontalFlightModes.HoldBank:

			HoldBank ();

			break;

		case  HorizontalFlightModes.AlignToWp:


			if(!currentRoutine.useRoutineTarget)
			currentRoutine.targetWp = GetComponent<AutoPilotActionsManager> ().currentWp;

			if (currentRoutine.targetWp != null) {

				Vector3 pos = currentRoutine.targetWp.position;
				pos.y = transform.position.y;
				Vector3 dir = pos - transform.position;
				Vector3 pos2 = (pos + dir.normalized * 100f) - transform.position;
				Vector3 dir2 = pos2 - transform.position;
				currentRoutine.targetHdg = Quaternion.LookRotation (dir , Vector3.up).eulerAngles.y;
				HoldHdg ();
			}

			break;


		case  HorizontalFlightModes.AlignToRunway:

			if (actionsManager.currentRunway == null)
				actionsManager.currentRunway = actionsManager.flightPlanFeed.toRunway;

			actionsManager.currentWp = actionsManager.currentRunway.ilsVectorPointer;
			/*
			Vector3 comparePos = actionsManager.currentRunway.runwayEnd.transform.position;
			comparePos.y = transform.position.y;
			Vector3 runwayComparePos = actionsManager.currentRunway.transform.position;
			runwayComparePos.y = transform.position.y;
			Vector3 runwayVector = runwayComparePos - comparePos;
			Vector3 inboundVector = comparePos - transform.position;		
			runwayVector.y = inboundVector.y;
			deltaCourse = SignedAngle (inboundVector, runwayVector, Vector3.up);
		
			currentRoutine.targetHdg = actionsManager.currentRunway.transform.rotation.eulerAngles.y + Mathf.Clamp (-deltaCourse, -25f, 25);
			HoldHdg ();
*/

			Vector3 inversePos = actionsManager.currentWp.InverseTransformPoint (transform.position + (aircraft.velocity*10f));		
			inversePos.y = 0f;
			inversePos.z = 0f;
			t = ((1 + (inversePos.x * 0.01f)) * 0.5f) ;
			courseDev = Mathf.Lerp (-25f, 25f, t);
			currentRoutine.targetHdg = currentRoutine.targetWp.parent.eulerAngles.y - courseDev;
			HoldHdg ();
			bankFactor = 0.75f;
			transform.Translate ( actionsManager.currentWp.TransformDirection (-actionsManager.currentWp.right) * t * Time.deltaTime , Space.World);

			break;
		}

		switch (currentAction.action.actionExitTrigger) {


		case ExitTriggers.AboveVspeed:

			if (aircraftVerticalSpeed >= currentRoutine.exitValue) {

				ExitActionMode ();

			}


			break;

		case ExitTriggers.ArrivedWaypoint:


			if (distToWp <= currentRoutine.exitValue) {

				ExitActionMode ();

			}


			break;

		case ExitTriggers.ArrivedFinalWaypoint:


			if (finishRoute && !loadingActions) {

				ExitActionMode ();

			}


			break;

		case ExitTriggers.Altitude:


			if (aircraftAltitude >= currentRoutine.exitValue) {

				ExitActionMode ();

			}


			break;

		case ExitTriggers.BelowAltitude:


			if (aircraftAltitude <= currentRoutine.exitValue) {

				ExitActionMode ();

			}


			break;

		case ExitTriggers.AboveAirspeed:


			if (aircraftSpeed >= currentRoutine.exitValue) {

				ExitActionMode ();

			}


			break;

		

		case ExitTriggers.BelowAirspeed:


			if (aircraftSpeed <= currentRoutine.exitValue) {

				ExitActionMode ();

			}


			break;

		}


		switch (currentRoutine.exitTrigger) {


		case ExitTriggers.AboveVspeed:

			if (aircraftVerticalSpeed >= currentRoutine.exitValue) {

				ExitMode ();

			}				

			break;


		case ExitTriggers.AboveAirspeed:


			if (aircraftSpeed >= currentRoutine.exitValue) {

				ExitMode ();

			}


			break;

			case ExitTriggers.ArrivedWaypoint:


			if (distToWp <= currentRoutine.exitValue) {

				ExitMode ();

			}


			break;
					

		case ExitTriggers.Altitude:

			if (currentRoutine.verticalFlightMode == VerticalFlightModes.LandingAprouch) {

				if (distToWp > 20f)
					break;				

			}

			if (aircraftAltitude >= currentRoutine.exitValue) {

				ExitMode ();

			}


			break;

		case ExitTriggers.BelowAltitude:


			if (aircraftAltitude <= currentRoutine.exitValue) {

				ExitMode ();

			}


			break;


		case ExitTriggers.BelowAirspeed:


			if (aircraftSpeed <= currentRoutine.exitValue) {

				ExitMode ();

			}


			break;


		case ExitTriggers.BelowDistance:
			
			if (distToWp <= currentRoutine.exitValue) {

				ExitMode ();

			}

			break;

		case ExitTriggers.BelowDistanceToIls:

			if (Vector3.Distance(transform.position,actionsManager.currentRunway.ilsVectorPointer.position) <= currentRoutine.exitValue) {

				ExitMode ();

			}

			break;

		}


		distToWp = DistToWaypoint ();


		//if (currentAction.action.flightState == AiFlightPlan.FlightStates.FlyRoute || currentAction.action.flightState == AiFlightPlan.FlightStates.FlyFormation || currentAction.action.flightState == AiFlightPlan.FlightStates.Taxi) {

		if (currentRoutine.targetWp != null) {
			if (currentRoutine.targetWp.GetComponent<WaypointTrigger> () != null) {
				
				WaypointTrigger trigger = currentRoutine.targetWp.GetComponent<WaypointTrigger> ();
				if (!ignoreOutsideTriggers && Vector3.Distance (transform.position, currentRoutine.targetWp.position) <= trigger.triggerDistance) {
						print ("trigger");

						ignoreOutsideTriggers = true;

						switch (trigger.triggerMode) {

					case WaypointTrigger.WaypointTriggerModes.UseTriggerFlightPlan:

						GameObject newFlightPlan = (GameObject)Instantiate (trigger.triggerFlightplan.gameObject, transform) as GameObject;
						AiFlightPlan cloneFp = newFlightPlan.GetComponent<AiFlightPlan> ();
											
						cloneFp.waypoints = trigger.triggerFlightplan.waypoints;
						cloneFp.currentFlightState = trigger.triggerFlightplan.currentFlightState;
						cloneFp.manager = actionsManager;
						actionsManager.flightPlanFeed = cloneFp;
						actionsManager.actions = actionsManager.flightPlanFeed.actions;
						actionsManager.waypoints = actionsManager.flightPlanFeed.waypoints;
						actionsManager.currentRunway = trigger.runwayTarget;
						actionsManager.waypointIndex = 0;
						actionsManager.index = 0;

						//	actionsManager.startButton.flightPlan = actionsManager.flightPlanFeed;
						LoadActions ();

						print ("trigger flight plan");

							break;

						case WaypointTrigger.WaypointTriggerModes.GetRunwayAprouch:

							GameObject newAprouchFlightPlan = (GameObject)Instantiate (trigger.GetAprouch ().gameObject, transform) as GameObject;
							AiFlightPlan cloneFp2 = newAprouchFlightPlan.GetComponent<AiFlightPlan> ();

							cloneFp2.manager = actionsManager;
							actionsManager.flightPlanFeed = cloneFp2;
							actionsManager.waypoints = cloneFp2.waypoints;
							actionsManager.currentRunway = trigger.runwayTarget;
							//actionsManager.currentWp = actionsManager.currentRunway.ilsVectorPointer;
							actionsManager.waypointIndex = 0;
							actionsManager.index = 0;
							//actionsManager.autopilot.currentAction = actionsManager.actions [actionsManager.index];
							//actionsManager.currentWp = actionsManager.waypoints [actionsManager.waypointIndex];
						//	actionsManager.startButton.flightPlan = actionsManager.flightPlanFeed;
							
							LoadActions ();
							actionsManager.autopilot.currentRoutine.targetSpd = aprouchSpeed;

						print ("trigger get aprouch plan");

							break;

						case WaypointTrigger.WaypointTriggerModes.RunwayHold:

							StartCoroutine (actionsManager.flightManager.HoldForTraffic ());
							print ("hold runway");
							break;

						case WaypointTrigger.WaypointTriggerModes.RunwayTakeoff:

							StartCoroutine (actionsManager.flightManager.TakeOffFlight ());
							print ("takeoff runway");
							break;

					case WaypointTrigger.WaypointTriggerModes.Park:

						StartCoroutine (actionsManager.flightManager.Park ());
						print ("Parking");
						break;

						}



						if (GetComponentInChildren<Formation> () != null) {
							GetComponentInChildren<Formation> ().ToggleWingmans (false);
						}
					}
				}
			}
		//}
	}

	void LoadActions (){
		if (!loadingActions) {
			
			StartCoroutine (WaitAndLoadActions ());
		}
	}
	bool loadingActions;
	IEnumerator WaitAndLoadActions () {
		loadingActions = true;
		yield return new WaitForFixedUpdate ();

		finishRoute = false;
		actionsManager.actions = actionsManager.flightPlanFeed.actions;
		actionsManager.autopilot.currentAction = actionsManager.actions [actionsManager.index];
		actionsManager.currentWp = actionsManager.waypoints [actionsManager.waypointIndex];
		actionsManager.autopilot.currentAction = actionsManager.flightPlanFeed.actions [actionsManager.index];

		yield return new WaitForSeconds (3);

		ignoreOutsideTriggers = false;
		loadingActions = false;
	}

	void ExitMode (){

		if(currentRoutine.exitTrigger != ExitTriggers.None){

			switch (currentRoutine.exitMode) {

			case ExitModes.NextAction:

				int nextActionIndex = actionsManager.GetActionIdByName (currentAction.action.exitAction);
				actionsManager.index = nextActionIndex;
				currentAction = actionsManager.actions [nextActionIndex];
				if (actionsManager.flightPlanFeed != null)
					actionsManager.flightPlanFeed.currentFlightState = currentAction.action.flightState;
				
				break;

			case ExitModes.ExitAction:

				int actionIndex = actionsManager.GetActionIdByName (currentAction.action.exitAction);
				currentAction = actionsManager.actions [actionIndex];
				if (actionsManager.flightPlanFeed != null)
					actionsManager.flightPlanFeed.currentFlightState = currentAction.action.flightState;

				break;

			case ExitModes.NextRoutine:

				int nextRoutineIndex = currentAction.action.routineIndex + 1;
				if (nextRoutineIndex < currentAction.action.routines.Length)
					currentAction.action.routineIndex = nextRoutineIndex;
				
				break;

			case ExitModes.NextWaypoint:

				NextWaypoint ();

				break;

			}

		}


	}
	bool checkWaypoint = true;
	void NextWaypoint(){
		checkWaypoint = false;
		int nextWaypointIndex = actionsManager.waypointIndex + 1;		
		if (nextWaypointIndex == actionsManager.waypoints.Count )
			finishRoute = true;
		if (nextWaypointIndex < actionsManager.waypoints.Count) {
			actionsManager.waypointIndex = nextWaypointIndex;
			finishRoute = false;
		}
		if (nextWaypointIndex < actionsManager.waypoints.Count) 
			actionsManager.currentWp = actionsManager.flightPlanFeed.waypoints [nextWaypointIndex];
		
		
		if (actionsManager.flightPlanFeed != null)
			actionsManager.flightPlanFeed.currentFlightState = currentAction.action.flightState;
		//yield return new WaitForEndOfFrame ();
		distToWp = DistToWaypoint ();
		//yield return new WaitForEndOfFrame ();

	}

	void ExitActionMode (){

		if(currentAction.action.actionExitTrigger != ExitTriggers.None){

			switch (currentAction.action.actionExitMode) {

			case ExitModes.NextAction:

				int nextActionIndex = actionsManager.GetActionIdByName (currentAction.action.exitAction);
				actionsManager.index = nextActionIndex;
				currentAction = actionsManager.actions [nextActionIndex];
				if (actionsManager.flightPlanFeed != null)
					actionsManager.flightPlanFeed.currentFlightState = currentAction.action.flightState;

				break;

			case ExitModes.ExitAction:

				int actionIndex = actionsManager.GetActionIdByName (currentAction.action.exitAction);
				currentAction = actionsManager.actions [actionIndex];
				if (actionsManager.flightPlanFeed != null)
					actionsManager.flightPlanFeed.currentFlightState = currentAction.action.flightState;
				
				break;
		

			}

		}


	}

		float DistToWaypoint(){
		if (actionsManager.currentWp != null)
			return Vector3.Distance (transform.position, actionsManager.currentWp.position);
		else
			return 0f;
		}

	void HoldAngleOfAttack (){


		deltaAoa = aircraftAoa - currentRoutine.targetAoa;
		elevatorOutput = elevatorResponse.Evaluate (deltaAoa / 10f) ;
		inputs.SetElevator (elevatorOutput);

	}
	public float curPitch;
	void HoldPitch (){
		//pitch
		var pos = ProjectPointOnPlane(Vector3.up, Vector3.zero, transform.forward);
		curPitch = SignedAngle(transform.forward, pos, transform.right);
		deltaPitch = (curPitch - currentRoutine.targetPitch);
		elevatorOutput = elevatorResponse.Evaluate (deltaPitch );
		inputs.SetElevator (-elevatorOutput );
	}


	private void HoldAlt (){
		
		deltaAlt = currentRoutine.targetAlt - aircraftAltitude;
		currentRoutine.targetVspd = 5f * Mathf.Clamp (deltaAlt*0.05f, -1f, 1f);
		HoldVspeed ();


	}
	public float curHdg;
	float bankFactor;
	private void HoldHdg () {

		curHdg = transform.rotation.eulerAngles.y;	
			
		deltaHdg= currentRoutine.targetHdg -  curHdg;
		/*if (deltaHdg > 180)
			deltaHdg = deltaHdg - 360;*/

		Quaternion rotToHdg = Quaternion.Euler (new Vector3 (0f, currentRoutine.targetHdg, 0f));

		Vector3 localPos = transform.position + (rotToHdg * (Vector3.forward * 10f));
		/*if(currentRoutine.targetWp != null)
			localPos = currentRoutine.targetWp.position;*/
		localPos.y = transform.position.y;
		Vector3 vectorToTgt = localPos - transform.position;	
		float sign = transform.InverseTransformPoint (localPos).x;
		if (sign > 0f)
			sign = 1f;
		if (sign < 0f)
			sign = -1f;
		Vector3 fowd = transform.forward;
		fowd.y = vectorToTgt.y;

		deltaHdg = AngleSigned (transform.forward, vectorToTgt, Vector3.up);

		float bankAltFactor = 1f;
		if (transform.position.y < 300f)
			bankAltFactor = 0.35f;
		
		currentRoutine.targetBnk = Mathf.Clamp (deltaHdg, -normalBankAngle * bankAltFactor * bankFactor, normalBankAngle * bankAltFactor * bankFactor);
			HoldBank ();
		if (currentRoutine.useSteering) {

			inputs.rudder = inputs.steering = inputs.shipRudder = Mathf.SmoothStep (inputs.rudder, Mathf.Clamp (deltaHdg / 35f, -1f, 1f) * Mathf.Clamp01 (stallSpeed / aircraftSpeed), Time.deltaTime * 10f);

		}
	}


	public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
	{
		return Mathf.Atan2(
			Vector3.Dot(n, Vector3.Cross(v1, v2)),
			Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
	}

	private void HoldBank (){

		float curBank = 360 - transform.eulerAngles.z;
		if (curBank > 180)
			curBank = curBank - 360;


		deltaBank = curBank - currentRoutine.targetBnk;
		aileronOutput = aileronResponse.Evaluate (deltaBank / 5f) ;
		inputs.SetAileron (-aileronOutput);

	}

	private void HoldVspeed (){
		
		deltaVspd = currentRoutine.targetVspd - aircraftVerticalSpeed ;
		elevatorOutput = elevatorResponse.Evaluate (deltaVspd) ;
		SendMessageUpwards ("SetElevator", elevatorOutput);

	}

	private void HoldSpeed (){

		deltaSpd = aircraftSpeed - currentRoutine.targetSpd;

		currentRoutine.targetThr = Mathf.Clamp (-deltaSpd* 0.1f, 0.3f, 1f);

		HoldThrottle ();

	}

	void HoldFormationSpeed () {

		if (!actionsManager.flyFormation)
			return;
		if (actionsManager.formationPosition.position == null)
			return;
		
		Vector3 comparePos = transform.InverseTransformPoint (actionsManager.formationPosition.position);
		comparePos.y = transform.position.y;
		comparePos.x = transform.position.x;
		float distToFormation = Mathf.Clamp (comparePos.z ,-15f,25f);
		currentRoutine.targetSpd = actionsManager.flightPlanFeed.formationLeader.autopilot.aircraftSpeed + (distToFormation  -5);

	/*	deltaSpd = aircraftSpeed - currentRoutine.targetSpd;

		currentRoutine.targetThr = Mathf.Clamp (-deltaSpd*0.2f, 0.4f, 1f);

		if(!settingThrottle)
			StartCoroutine (HoldThrottle ());*/

		HoldSpeed ();

	}

	public float deltaThrotte;
	bool settingThrottle;
	void HoldThrottle (){
		settingThrottle = true;
		//yield return new WaitForFixedUpdate ();
		deltaThrotte =(currentRoutine.targetThr - inputs.enginesManager.throttle) * 1.5f;
		if (deltaThrotte < -1f)
			deltaThrotte = -1f;
		if (deltaThrotte > 1f)
			deltaThrotte = 1f;
		SetThrottle (deltaThrotte);
		settingThrottle = false;

	}

	private void FullThrottle () {

		inputs.SetThrottle (1f);
	}

	void SetThrottle (float value){

		inputs.SetThrottle (value);
	}

	private float GetSpeed (){

		Vector3 velocity = aircraft.GetPointVelocity (transform.position);

		return (transform.InverseTransformDirection (velocity).z * 1.94f);//Kts

	}


	private float GetAlt (){

		float altitudeSealvl = transform.position.y;

		return altitudeSealvl;//mts

	}

	private float GetVSpeed(){

		float vspeed = aircraft.velocity.y;

		return vspeed;
	}

	private void GetAoa () {

		Vector3 airVector = aircraft.GetPointVelocity (transform.position);
		float airVectorZ = transform.InverseTransformDirection (airVector).z;
		float airVectorY = transform.InverseTransformDirection (airVector).y;
		float airVectorX = transform.InverseTransformDirection (airVector).x;			

		aircraftAoa =  (Mathf.Atan2(-airVectorY, airVectorZ ) )* Mathf.Rad2Deg;

	}

	public float currentAngle;
	public float deltaAngle;
	public float glideslopeAngle;
				public float deltaCourse;

	Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3  planePoint,Vector3 point  ) {
		planeNormal.Normalize();
		float distance = -Vector3.Dot(planeNormal.normalized, (point - planePoint));
		return point + planeNormal * distance;
	}    

	float  SignedAngle(Vector3 v1,Vector3 v2,Vector3 normal) {
		var perp = Vector3.Cross(normal, v1);
		var angle = Vector3.Angle(v1, v2);
		angle *= Mathf.Sign(Vector3.Dot(perp, v2));
		return angle;
	}


	private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
	{
		Vector2 diference = vec2 - vec1;
		float sign = (vec2.x < vec1.x)? -1.0f : 1.0f;
		return Vector2.Angle(vec2, vec1) * sign;
	}
}
