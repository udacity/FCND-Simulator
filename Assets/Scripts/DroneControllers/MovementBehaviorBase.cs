using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovementBehaviors
{
	public abstract class MovementBehaviorBase<ControllerType> : ScriptableObject
	{
		[System.NonSerialized]
		public ControllerType controller;
		
		public virtual void OnSelect (ControllerType _controller)
		{
			controller = _controller;
		}

		public virtual void OverrideUpdate(float throttle, float pitchRate, float yawRate, float rollRate) {}
		public virtual void OnUpdate () {}
		public virtual void OnLateUpdate () {}
		public virtual void OnFixedUpdate () {}
	}
}