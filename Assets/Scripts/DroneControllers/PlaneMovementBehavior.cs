using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DroneControllers;
using MovementBehaviors;
using DroneInterface;

namespace MovementBehaviors
{
	//public abstract class QuadMovementBehavior : MovementBehaviorBase<SimpleQuadController> {}
    public abstract class PlaneMovementBehavior : MovementBehaviorBase<IDroneController> { }
}