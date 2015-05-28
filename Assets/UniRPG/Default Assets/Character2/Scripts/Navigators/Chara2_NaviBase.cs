// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Character 2/Movement/None")]
	public class Chara2_NaviBase : MonoBehaviour
	{

		public virtual void LookAt(Vector3 targetPosition)
		{
			targetPosition.y = transform.position.y;
			transform.LookAt(targetPosition);
		}

		public virtual void MoveTo(Vector3 targetPosition, float moveSpeed, float turnSpeed)
		{
		}

		public virtual void Stop()
		{
		}

		public virtual Vector3 CurrentVelocity()
		{
			return Vector3.zero;
		}

		public virtual float CurrentSpeed()
		{
			return 0f;
		}

		public virtual bool IsMovingOrPathing()
		{
			return false;
		}

		// ============================================================================================================
	}
}