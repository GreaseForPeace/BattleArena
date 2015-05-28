// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Character 2/Movement/Simple")]
	[RequireComponent(typeof(Rigidbody))]
	public class Chara2_NaviSimple : Chara2_NaviBase
	{
		public LayerMask collisionLayers = -1;
		public float slopeLimit = 40f;
		public float stopDistance = 1f;
		public float stepHeight = 0.3f;
		public float gravity = 7f;

		// ============================================================================================================

		private bool grounded = false;
		private bool moving = false;
		private bool turning = false;
		private float moveSpeed;
		private float turnSpeed;
		private Transform _tr;
		private Rigidbody _rb;

		private Vector3 targetPosition;
		private Vector3 targetDirection;
		private Vector3 moveDirection;
		private Vector3 pos;

		// ============================================================================================================

		public void Awake()
		{
			_tr = transform;
			_rb = GetComponent<Rigidbody>();
			_rb.freezeRotation = true;
			_rb.constraints = RigidbodyConstraints.FreezeAll;
			_rb.useGravity = false;
			_rb.isKinematic = false;
		}

		void OnEnable()
		{
			moving = false;
			turning = false;
			targetPosition = _tr.position;
			_rb.constraints = RigidbodyConstraints.FreezeAll;
			_rb.WakeUp();
		}

		void OnDisable()
		{
			moving = false;
			turning = false;
			targetPosition = _tr.position;
			_rb.constraints = RigidbodyConstraints.FreezeAll;
			_rb.Sleep();
		}

		public void Update()
		{
			if (turning)
			{
				moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, 360f * Mathf.Deg2Rad * Time.deltaTime, 1000);
				moveDirection = moveDirection.normalized;
				if (Vector3.Dot(_tr.forward, targetDirection) == 0f) turning = false;
				else _tr.rotation = Quaternion.LookRotation(moveDirection);
			}

			if (!moving)
			{
				moveSpeed -= Time.deltaTime * 10f;
				if (moveSpeed < 0.0f) moveSpeed = 0.0f;
				if (!grounded && gravity != 0f)
				{
					RaycastHit hit2;
					if (Physics.Raycast(_tr.position + _tr.up, -_tr.up, out hit2, 100f, 1 << UniRPGGlobal.DB.floorLayerMask))
					{
						pos = _tr.position;
						if (Vector3.Distance(pos, hit2.point) < 0.01f) grounded = true;
						else
						{
							if (pos.y < hit2.point.y)
							{
								targetPosition.y = pos.y = hit2.point.y;
								grounded = true;
							}
							else
							{
								targetPosition.y = pos.y = Mathf.MoveTowards(pos.y, hit2.point.y, gravity * Time.deltaTime);
							}
							_tr.position = pos;
						}
					}
				}
				return;
			}

			// calculate direction to face
			targetDirection = targetPosition - _tr.position;
			moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
			moveDirection.y = 0f;
			moveDirection = moveDirection.normalized;
			_tr.rotation = Quaternion.LookRotation(moveDirection);

			// something I can't get over in front of me? Stop.
			RaycastHit hit;
			if (Physics.Raycast(_tr.position + _tr.up * stepHeight, _tr.forward, out hit, stopDistance, collisionLayers))
			{	// ignore triggers
				if (!hit.collider.isTrigger)
				{
					float angle = Vector3.Angle(hit.normal, Vector3.up);
					if (angle > slopeLimit) 
					{
						moving = false;
						_rb.constraints = RigidbodyConstraints.FreezeAll;
						return; 
					}
				}
			}

			// move it
			_tr.position = Vector3.MoveTowards(_tr.position, targetPosition, moveSpeed * Time.deltaTime);

			// gravity
			if (gravity != 0f)
			{
				grounded = false;
				if (Physics.Raycast(_tr.position + _tr.up, -_tr.up, out hit, 100f, 1 << UniRPGGlobal.DB.floorLayerMask))
				{
					pos = _tr.position;
					if (pos.y < hit.point.y)
					{
						targetPosition.y = pos.y = hit.point.y;
						grounded = true;
					}
					else targetPosition.y = pos.y = Mathf.MoveTowards(pos.y, hit.point.y, gravity * Time.deltaTime);
					_tr.position = pos;
				}
			}

			// near enough to target position? Stop.
			//if (targetDirection.sqrMagnitude < stopDistance * stopDistance)
			if (Vector3.Distance(targetPosition, _tr.position) < stopDistance)
			{
				moving = false;
				_rb.constraints = RigidbodyConstraints.FreezeAll;
				return;
			}
		}

		// ============================================================================================================

		public override void LookAt(Vector3 targetPosition)
		{
			if (moving) return;
			this.turning = true;
			targetPosition.y = _tr.position.y;
			this.targetDirection = targetPosition - _tr.position;
		}

		public override void MoveTo(Vector3 targetPosition, float moveSpeed, float turnSpeed)
		{
			_rb.constraints = RigidbodyConstraints.FreezeRotation;
			this.moving = true;
			this.turning = false;
			this.targetPosition = targetPosition;
			this.targetDirection = targetPosition - _tr.position;
			this.moveSpeed = moveSpeed;
			this.turnSpeed = turnSpeed;
		}

		public override void Stop()
		{
			_rb.constraints = RigidbodyConstraints.FreezeAll;
			this.moving = false;
			this.moveSpeed = 0f;
			this.targetPosition = transform.position;
		}

		public override Vector3 CurrentVelocity()
		{
			//if (!moving) return Vector3.zero;
			if (moveSpeed < 0.1f) return Vector3.zero;
			return (moveDirection * moveSpeed);
		}

		public override float CurrentSpeed()
		{
			//if (!moving) return 0f;
			if (moveSpeed < 0.1f) return 0f;
			return (moveDirection * moveSpeed).magnitude;
		}

		public override bool IsMovingOrPathing()
		{
			return moving;
		}

		// ============================================================================================================
	}
}