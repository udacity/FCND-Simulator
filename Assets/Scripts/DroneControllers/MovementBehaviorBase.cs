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

		/// <summary>
		/// NOTE: This is meant to be implemented in conjuction with a remote controller.
		/// If that's not the case implement one of the other Update methods.
		/// </summary>
		public virtual void RemoteUpdate(float thrust, float pitchRate, float yawRate, float rollRate) {}
		public virtual void OnUpdate () {}
		public virtual void OnLateUpdate () {}
		public virtual void OnFixedUpdate () {}
	}
}