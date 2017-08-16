﻿/*********************************************************************
* Software License Agreement (BSD License)
*
*  Copyright (c) 2008, Willow Garage, Inc.
*  All rights reserved.
*
*  Redistribution and use in source and binary forms, with or without
*  modification, are permitted provided that the following conditions
*  are met:
*
*   * Redistributions of source code must retain the above copyright
*     notice, this list of conditions and the following disclaimer.
*   * Redistributions in binary form must reproduce the above
*     copyright notice, this list of conditions and the following
*     disclaimer in the documentation and/or other materials provided
*     with the distribution.
*   * Neither the name of the Willow Garage nor the names of its
*     contributors may be used to endorse or promote products derived
*     from this software without specific prior written permission.
*
*  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
*  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
*  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
*  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
*  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
*  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
*  BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
*  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
*  CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
*  LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
*  ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
*  POSSIBILITY OF SUCH DAMAGE.
*********************************************************************/

namespace actionlib
{
	/**
	* \brief Thin wrapper around an enum in order providing a simplified version of the
	* communication state, but with less states than CommState
	**/
	public class SimpleGoalState
	{
		//! \brief Defines the various states the SimpleGoalState can be in
		public enum StateEnum
		{
			PENDING,
			ACTIVE,
			DONE
		}

		public StateEnum state;

		public SimpleGoalState (StateEnum state)
		{
			this.state = state;
		}

		public static bool operator == (SimpleGoalState lhs, SimpleGoalState rhs)
		{
			if ( object.ReferenceEquals ( lhs, rhs ) )
				return true;

			return lhs.state == rhs.state;
		}

		public static bool operator != (SimpleGoalState lhs, SimpleGoalState rhs)
		{
			return !( lhs == rhs );
		}

		public static bool operator == (SimpleGoalState lhs, SimpleGoalState.StateEnum rhs)
		{
			return ( !object.ReferenceEquals ( lhs, null ) && lhs.state == rhs );
		}

		public static bool operator != (SimpleGoalState lhs, SimpleGoalState.StateEnum rhs)
		{
			return !( lhs == rhs );
		}

		public string toString()
		{
			return state.ToString ();
		}
	}
}