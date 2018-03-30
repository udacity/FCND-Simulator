using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class uSimVehicleEditor : EditorWindow {

	public enum TabModes {Vehicle, Engines, Undercarriage, Transmision, Aerodynamics, Systems, Compartments, Panel};
	public static UsimVehicle uSimVehicle;
	TabModes tabMode;
	public int selGridInt = 0;
	public string[] selStrings = new string[] {"Vehicle Main", "Engines", "Undercarriage", "Transmision", "Aerodynamics", "Systems", "Compartments", "Panel"};

	public int engineSelInt = 0;
	public List<string> engineTitles;
	//Undercarriage
	Vector2 ucScrollPos;


	Editor gameObjectEditor;
		// Add menu named "My Window" to the Window menu
		[MenuItem ("Window/Usim/Edit vehicle")]
		static void Init () {

		uSimVehicle = Selection.activeGameObject.GetComponent<UsimVehicle> ();

			// Get existing open window or if none, make a new one:
		uSimVehicleEditor window = (uSimVehicleEditor)EditorWindow.GetWindow (typeof (uSimVehicleEditor));
		window.minSize = new Vector2 (700f, 700f);
		window.Show();
		}

		void OnGUI () {

		if (uSimVehicle != null) {

			EditorUtility.SetDirty (uSimVehicle);

			GUILayout.BeginArea (new Rect (0f, 0f, 250f, 100f));
			GUILayout.Label ("uSim Vehicle Editor Window", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal ();

			GUILayout.Label ("Name: ");
			uSimVehicle.name = EditorGUILayout.TextField ( uSimVehicle.name);

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();

			GUILayout.Label ("Description: ");
			uSimVehicle.description = EditorGUILayout.TextField (uSimVehicle.description);

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();

			GUILayout.Label ("Vehicle type: " + uSimVehicle.vehicleType.ToString());


			GUILayout.EndHorizontal ();


			/*groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
			EditorGUILayout.EndToggleGroup ();*/
		
			GUILayout.EndArea ();

			GUILayout.BeginArea (new Rect (270f, 10f, 250f, 100f));
			if(gameObjectEditor == null)
			gameObjectEditor = Editor.CreateEditor (uSimVehicle.gameObject);
		
			gameObjectEditor.OnPreviewGUI (new Rect(0f,0f,120f,120f),GUIStyle.none);

			GUILayout.EndArea ();

		} else {

			GUILayout.Label ("Select a valid uSim vehicle to edit", EditorStyles.boldLabel);

			}

		DrawButtons ();
		DrawTab ();
		}


			void DrawButtons (){


			GUILayout.BeginArea (new Rect (10f, 120f, 700f, 50f));

			GUILayout.BeginHorizontal ();

			selGridInt = GUILayout.SelectionGrid(selGridInt, selStrings, 4);

			GUILayout.EndHorizontal ();

			GUILayout.EndArea ();

			}

	void DrawTab (){

		if (uSimVehicle == null)
			return;
		
		Rigidbody rg = null;
		if (uSimVehicle.GetComponent<Rigidbody> () != null)
			rg = uSimVehicle.GetComponent<Rigidbody> ();
		
		GUILayout.BeginArea (new Rect (10f, 170f, 700f, 700f));

		switch (tabMode) {

		case TabModes.Vehicle:

			if (uSimVehicle.vehicleType == UsimVehicle.VehicleTypes.Air) {

				if (rg != null) {

					rg.mass =  EditorGUILayout.FloatField ("Vehicle mass", rg.mass);
					rg.drag =  EditorGUILayout.FloatField ("Vehicle drag", rg.drag);
					rg.angularDrag =  EditorGUILayout.FloatField ("Angular drag", rg.angularDrag);
				}

				if (uSimVehicle.aircraftController.cog != null)
					uSimVehicle.aircraftController.cog.localPosition = EditorGUILayout.Vector3Field ("CoG Position", uSimVehicle.aircraftController.cog.localPosition);
				else
					uSimVehicle.aircraftController.cog = uSimVehicle.transform.Find ("Cog");

				uSimVehicle.aircraftController.inertiaTensors = EditorGUILayout.Vector3Field ("Inertia tensors", uSimVehicle.aircraftController.inertiaTensors);

				GUILayout.BeginHorizontal ();

				uSimVehicle.aircraftController.steerWheel = EditorGUILayout.ObjectField ("Steeering Wheel", uSimVehicle.aircraftController.steerWheel, typeof(Transform), true) as Transform;
				uSimVehicle.aircraftController.maxSteerAngle = EditorGUILayout.FloatField ("Max steer angle", uSimVehicle.aircraftController.maxSteerAngle);


				GUILayout.EndHorizontal ();

				uSimVehicle.aircraftController.isTailWheel = EditorGUILayout.Toggle ("is tail wheel?", uSimVehicle.aircraftController.isTailWheel);
				uSimVehicle.isSeaPlane = EditorGUILayout.Toggle ("is sea plane?", uSimVehicle.isSeaPlane);

				ControlAnimator animator = uSimVehicle.GetComponent<ControlAnimator> ();
			
				if (animator != null) {
					
					GUILayout.Label ("Main Control surfaces", EditorStyles.boldLabel);
					GUILayout.Label ("-----------------------------------------------------------------------");

					GUILayout.BeginHorizontal ();								

					GUILayout.BeginVertical ();

					ControlAnimator.ControlSurface racs = animator.rightAilerons [0]; 
					EditorGUIUtility.labelWidth = 90f;
					racs.obj = EditorGUILayout.ObjectField ("Right aileron", racs.obj, typeof(Transform), true) as Transform;
					racs.maxDeflection = EditorGUILayout.FloatField ("Max deflection", racs.maxDeflection);

					GUILayout.EndVertical ();

					GUILayout.BeginVertical ();

					ControlAnimator.ControlSurface lacs = animator.leftAilerons [0]; 
					EditorGUIUtility.labelWidth = 90f;
					lacs.obj = EditorGUILayout.ObjectField ("Left aileron", lacs.obj, typeof(Transform), true) as Transform;
					lacs.maxDeflection = EditorGUILayout.FloatField ("Max deflection", lacs.maxDeflection);

					GUILayout.EndVertical ();
				
					GUILayout.EndHorizontal ();

					//***
					GUILayout.Label ("-----------------------------------------------------------------------");

					GUILayout.BeginHorizontal ();								

					GUILayout.BeginVertical ();

					ControlAnimator.ControlSurface recs = animator.elevators [0]; 
					EditorGUIUtility.labelWidth = 90f;
					recs.obj = EditorGUILayout.ObjectField ("Elevator 0", recs.obj, typeof(Transform), true) as Transform;
					recs.maxDeflection = EditorGUILayout.FloatField ("Max deflection", recs.maxDeflection);

					GUILayout.EndVertical ();

					if (animator.elevators.Length > 1) {

						GUILayout.BeginVertical ();

						ControlAnimator.ControlSurface lecs = animator.elevators [1]; 
						EditorGUIUtility.labelWidth = 90f;
						lecs.obj = EditorGUILayout.ObjectField ("Elevator 1", lecs.obj, typeof(Transform), true) as Transform;
						lecs.maxDeflection = EditorGUILayout.FloatField ("Max deflection", lecs.maxDeflection);

						GUILayout.EndVertical ();

					}

					GUILayout.EndHorizontal ();


					GUILayout.Label ("-----------------------------------------------------------------------");

					GUILayout.BeginHorizontal ();								

					GUILayout.BeginVertical ();

					ControlAnimator.ControlSurface rrcs = animator.rudders [0]; 
					EditorGUIUtility.labelWidth = 90f;
					rrcs.obj = EditorGUILayout.ObjectField ("Rudder 0", rrcs.obj, typeof(Transform), true) as Transform;
					rrcs.maxDeflection = EditorGUILayout.FloatField ("Max deflection", rrcs.maxDeflection);

					GUILayout.EndVertical ();

					if (animator.rudders.Length > 1) {

						GUILayout.BeginVertical ();

						ControlAnimator.ControlSurface lrcs = animator.rudders [1]; 
						EditorGUIUtility.labelWidth = 90f;
						lrcs.obj = EditorGUILayout.ObjectField ("Rudder 1", lrcs.obj, typeof(Transform), true) as Transform;
						lrcs.maxDeflection = EditorGUILayout.FloatField ("Max deflection", lrcs.maxDeflection);

						GUILayout.EndVertical ();

					}

					GUILayout.EndHorizontal ();


				}
			}
			if (uSimVehicle.vehicleType == UsimVehicle.VehicleTypes.Land) {

				if (rg != null) {

					rg.mass =  EditorGUILayout.FloatField ("Vehicle mass", rg.mass);
					rg.drag =  EditorGUILayout.FloatField ("Vehicle drag", rg.drag);
					rg.angularDrag =  EditorGUILayout.FloatField ("Angular drag", rg.angularDrag);
				}

				if (uSimVehicle.vehicleController.cog != null)
					uSimVehicle.vehicleController.cog.localPosition = EditorGUILayout.Vector3Field ("CoG Position", uSimVehicle.vehicleController.cog.localPosition);
				else
					uSimVehicle.vehicleController.cog = uSimVehicle.transform.Find ("CoG");

				uSimVehicle.vehicleController.inertiaTensors = EditorGUILayout.Vector3Field ("Inertia tensors", uSimVehicle.vehicleController.inertiaTensors);

				GUILayout.BeginVertical ();

				for (int i = 0; i < uSimVehicle.vehicleController.steeringWheels.Length; i++){
					Transform steeringWheel = uSimVehicle.vehicleController.steeringWheels [i];

					steeringWheel = EditorGUILayout.ObjectField ("Steeering Wheel", steeringWheel, typeof(Transform), true) as Transform;

				}

				GUILayout.EndVertical ();


			}
			if (uSimVehicle.vehicleType == UsimVehicle.VehicleTypes.Sea) {

				if (rg != null) {

					rg.mass =  EditorGUILayout.FloatField ("Vehicle mass", rg.mass);
					rg.drag =  EditorGUILayout.FloatField ("Vehicle drag", rg.drag);
					rg.angularDrag =  EditorGUILayout.FloatField ("Angular drag", rg.angularDrag);
				}

				GUILayout.Label ("Ship rudders");
				GUILayout.Label ("-----------------------------------------------------------------------");
				for (int i = 0; i < uSimVehicle.shipController.shipRudders.Length; i++){
					Transform rudder = uSimVehicle.shipController.shipRudders [i];

					rudder = EditorGUILayout.ObjectField ("Rudder", rudder, typeof(Transform), true) as Transform;

				}


			}

			break;


		case TabModes.Engines:


		

			GUILayout.BeginArea (new Rect (10f, 0f, 680f, 50f));

			engineTitles = new List<string> ();

			for (int i = 0; i < uSimVehicle.engines.engines.Length; i++) {

				engineTitles.Add ("Engine " + (1 + i).ToString ());

			}
			string[] enginesTitles = engineTitles.ToArray ();

			GUILayout.BeginHorizontal ();

			engineSelInt = GUILayout.SelectionGrid (engineSelInt, enginesTitles, 4);

			GUILayout.EndHorizontal ();

			GUILayout.EndArea ();

		
				
			GUILayout.BeginVertical ();
			
			GUILayout.Space (20f);
			GUILayout.Label ("Engine " + (1 + engineSelInt).ToString ());
			GUILayout.Label ("-----------------------------------------------------------------------");


			Engine engine = uSimVehicle.engines.engines [engineSelInt];

			GUILayout.BeginHorizontal ();

			engine.torqueCurve = EditorGUILayout.CurveField ("Torque :", engine.torqueCurve);
			GUILayout.Label (" -> in function of RPM");

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();

			engine.frictionCurve = EditorGUILayout.CurveField ("Friction :", engine.frictionCurve);
			GUILayout.Label (" -> in function of RPM");

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			engine.stallThreshold = EditorGUILayout.FloatField ("Stall threshold RPM :", engine.stallThreshold);
			engine.idleThrottle = EditorGUILayout.FloatField ("Idle throttle :", engine.idleThrottle);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			engine.rpmLimit = EditorGUILayout.FloatField ("RPM limit :", engine.rpmLimit);
			engine.inertia = EditorGUILayout.FloatField ("spool up factor :", engine.inertia);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();

			engine.maxFuelConsumption = EditorGUILayout.FloatField ("Max fuel consuption: ", engine.maxFuelConsumption);
			engine.flyWheelRadius = EditorGUILayout.FloatField ("Flywheel factor: ", engine.flyWheelRadius);
			engine.totalMass = EditorGUILayout.FloatField ("Total mass: ", engine.totalMass);
			engine.useClutch = EditorGUILayout.Toggle ("clutch? ", engine.useClutch);
			GUILayout.EndHorizontal ();
							
			if (engine.GetComponentInChildren<Prop> () != null) {
			
				Prop attachedProp = engine.GetComponentInChildren<Prop> ();
				GUILayout.Label ("-----------------------------------------------------------------------");
				GUILayout.Label ("Prop attached");

				attachedProp.propRadius = EditorGUILayout.FloatField ("Prop radius in meters: ", attachedProp.propRadius);
				attachedProp.propMass = EditorGUILayout.FloatField ("Prop mass: ", attachedProp.propMass);
				GUILayout.BeginHorizontal ();
				attachedProp.outputCurve =  EditorGUILayout.CurveField ("Output coefficient curve :", attachedProp.outputCurve);
				GUILayout.Label (" -> in function of prop RPM");
				GUILayout.EndHorizontal ();
				attachedProp.friction = EditorGUILayout.FloatField ("Base friction : ", attachedProp.friction);
				GUILayout.BeginHorizontal ();
				attachedProp.frictionCurve =  EditorGUILayout.CurveField ("Friction coefficient curve :", attachedProp.frictionCurve);
				GUILayout.Label (" -> in function of prop RPM");
				GUILayout.EndHorizontal ();
				attachedProp.reductionGear =  EditorGUILayout.FloatField ("Transmision reduction rate : ", attachedProp.reductionGear);
			}
				GUILayout.EndVertical ();
		




			break;

		case TabModes.Undercarriage:

		
		

			GUILayout.BeginVertical ();

			LandingGearAnimation gearAnimation = null;
			if (uSimVehicle.GetComponentInChildren<LandingGearAnimation> () != null){
				gearAnimation = uSimVehicle.GetComponentInChildren<LandingGearAnimation> ();			
			gearAnimation.extendSpeed = EditorGUILayout.FloatField ("Extend Speed: ", gearAnimation.extendSpeed);
			gearAnimation.retractSpeed = EditorGUILayout.FloatField ("Retract Speed: ", gearAnimation.retractSpeed);
			GUILayout.Label ("-----------------------------------------------------------------------");
			}


			VehicleSuspension[] suspensionArms = uSimVehicle.GetComponentsInChildren<VehicleSuspension> () as VehicleSuspension[];

			for (int i = 0; i < suspensionArms.Length; i++) {

				VehicleSuspension suspension =  suspensionArms[i];

				GUILayout.BeginVertical ();

				GUILayout.Label ("-----------------------------------------------------------------------");

				GUILayout.BeginHorizontal ();

				suspension.compressionCurve =  EditorGUILayout.CurveField ("Compression curve: ", suspension.compressionCurve);
				GUILayout.Label (" -> x axis is compression (0 - 1)");

				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				suspension.runLength =  EditorGUILayout.FloatField ("Run Length: ", suspension.runLength);
				suspension.springForce =  EditorGUILayout.FloatField ("Spring Force: ", suspension.springForce);
				suspension.springDamping =  EditorGUILayout.FloatField ("Spring Damp: ", suspension.springDamping);
				GUILayout.EndHorizontal ();

				suspension.attachedWheel = EditorGUILayout.ObjectField ("Attached Wheel: ",suspension.attachedWheel, typeof(VehicleWheel), true) as VehicleWheel;
				GUILayout.BeginHorizontal ();
				suspension.attachedWheel.radius = EditorGUILayout.FloatField ("radius: ",suspension.attachedWheel.radius);
				suspension.attachedWheel.brakeForce = EditorGUILayout.FloatField ("Brake: ",suspension.attachedWheel.brakeForce);
				suspension.attachedWheel.maxDeltaRPM = EditorGUILayout.FloatField ("Max delta RPM (slip): ",suspension.attachedWheel.maxDeltaRPM);
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("latitudinal ");
				suspension.attachedWheel.latitudinalGripCurve =  EditorGUILayout.CurveField (suspension.attachedWheel.latitudinalGripCurve);
				GUILayout.Label ("Base value: ");
				suspension.attachedWheel.maxGripForce = EditorGUILayout.FloatField (suspension.attachedWheel.maxGripForce);
				GUILayout.Label ("longitudinal");
				suspension.attachedWheel.longitudinalGripCurve =  EditorGUILayout.CurveField (suspension.attachedWheel.longitudinalGripCurve);
				GUILayout.Label ("Base value: ");
				suspension.attachedWheel.maxLongForce = EditorGUILayout.FloatField (suspension.attachedWheel.maxLongForce);
				GUILayout.EndHorizontal ();



				GUILayout.Label ("-----------------------------------------------------------------------");


				GUILayout.EndVertical ();


			}

			GUILayout.EndVertical ();

			break;


		case TabModes.Aerodynamics:

			if (uSimVehicle.vehicleType == UsimVehicle.VehicleTypes.Air) {

				CurvesManager curvesManager = null;

				if (uSimVehicle.GetComponentInChildren<CurvesManager> () != null)
					curvesManager = uSimVehicle.GetComponentInChildren<CurvesManager> ();

				for (int i = 0; i < curvesManager.curves.Length; i++) {

					GUILayout.BeginVertical ();

					CurvesManager.Curves curve = curvesManager.curves [i];

					GUILayout.BeginHorizontal ();
					curve.curve = EditorGUILayout.CurveField ("Curve: ", curve.curve);
					curve.name = EditorGUILayout.TextField ("name: ", curve.name);
					GUILayout.EndHorizontal ();

					GUILayout.Label ("-----------------------------------------------------------------------");

					GUILayout.EndVertical ();
				}
			}
			break;


		case TabModes.Transmision:

			if (uSimVehicle.vehicleType == UsimVehicle.VehicleTypes.Land) {
				VehicleGearBox gearBox = null;
				Differential diferential = null;

				if (uSimVehicle.GetComponentInChildren<VehicleGearBox> () != null)
					gearBox = uSimVehicle.GetComponentInChildren<VehicleGearBox> ();

				if (uSimVehicle.GetComponentInChildren<Differential> () != null)
					diferential = uSimVehicle.GetComponentInChildren<Differential> ();
			
				if (gearBox != null) {
					for (int i = 0; i < gearBox.gears.Length; i++) {

						GUILayout.BeginVertical ();

						VehicleGearBox.Gear gear = gearBox.gears [i];

						GUILayout.BeginHorizontal ();
						gear.displayName = EditorGUILayout.TextField ("Display Name: ", gear.displayName);
						gear.ratio = EditorGUILayout.FloatField ("gear ratio: ", gear.ratio);
						GUILayout.EndHorizontal ();

						GUILayout.Label ("-----------------------------------------------------------------------");

						GUILayout.EndVertical ();
					}
				}

				if (diferential != null)
					diferential.diffRatio = EditorGUILayout.FloatField ("diferential ratio: ", diferential.diffRatio);
			}

			break;


		case TabModes.Systems:

			FuelManager fuelManager = null;

			if (uSimVehicle.GetComponentInChildren<FuelManager> () != null)
				fuelManager = uSimVehicle.GetComponentInChildren<FuelManager> ();

			GUILayout.Label ("Fuel system");
			GUILayout.Label ("-----------------------------------------------------------------------");
			float fuelUnitWeight = 1f;
			fuelUnitWeight = EditorGUILayout.FloatField ("Fuel unit weight", fuelUnitWeight);

			GUILayout.Label ("-----------------------------------------------------------------------");

			GUILayout.BeginHorizontal ();
		
			GUILayout.BeginVertical ();

			for (int i = 0; i < fuelManager.fuelCompartments.Length; i++) {
				
			
				GUILayout.Label ("Fuel compartment " + (1+i).ToString());
			
				FuelCompartment fuelCompartment = fuelManager.fuelCompartments [i];

				GUILayout.BeginHorizontal ();
				fuelCompartment.emptyWeight = EditorGUILayout.FloatField ("Empty weight: ", fuelCompartment.emptyWeight);
				fuelCompartment.capacity = EditorGUILayout.FloatField ("Compartment Capacity: ", fuelCompartment.capacity);
				fuelCompartment.fuelQuantity = EditorGUILayout.FloatField ("Current quantity: ", Mathf.Clamp (fuelCompartment.fuelQuantity,0f,fuelCompartment.capacity));
				fuelCompartment.unitWeight = fuelUnitWeight;

				GUILayout.EndHorizontal ();

				GUILayout.Label ("-----------------------------------------------------------------------");

			}
			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();

			break;

		case TabModes.Panel:

			GUILayout.Label ("2D panel");
			GUILayout.Label ("-----------------------------------------------------------------------");
			uSimVehicle.panelPrefab = EditorGUILayout.ObjectField ("2D Panel prefab", uSimVehicle.panelPrefab, typeof(GameObject), true) as GameObject;
			uSimVehicle.spawnPanelOnStart = EditorGUILayout.Toggle ("spawn panel on start?", uSimVehicle.spawnPanelOnStart);
			uSimVehicle.panelPosition = EditorGUILayout.Vector3Field ("panel screen position", uSimVehicle.panelPosition);

			break;

		}
		GUILayout.EndArea ();

	}

	void Update (){
		
		if (selGridInt == 0)
			tabMode = TabModes.Vehicle;
		if (selGridInt == 1)
			tabMode = TabModes.Engines;
		if (selGridInt == 2)
			tabMode = TabModes.Undercarriage;
		if (selGridInt == 3)
			tabMode = TabModes.Transmision;
		if (selGridInt == 4)
			tabMode = TabModes.Aerodynamics;
		if (selGridInt == 5)
			tabMode = TabModes.Systems;
		if (selGridInt == 6)
			tabMode = TabModes.Compartments;
		if (selGridInt == 7)
			tabMode = TabModes.Panel;

	}

}
