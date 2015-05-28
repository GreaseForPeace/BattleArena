// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Character 2/Non-Player (NPC)")]
	public class Chara2_NPC : Chara2_Base
	{

		[SerializeField] private UniRPGGlobal.ActorType type = UniRPGGlobal.ActorType.Friendly;
		public UniRPGGlobal.ActorType actorType
		{
			get { return type; }
			set
			{
				type = value;
				if (eyes != null)
				{
					eyes.SetActive(type == UniRPGGlobal.ActorType.Hostile);

					if (type == UniRPGGlobal.ActorType.Hostile)
					{	// if eyes just went active then the NPC won't detect the player 
						// if he was already within the trigger area and not moving
						if (UniRPGGlobal.InstanceExist)
						{
							float d = (UniRPGGlobal.Player.transform.position - transform.position).magnitude;
							if (d <= detectionRadius) OnPlayerEnterTrigger();
						}
					}

				}
			}
		}

		public UniRPGGlobal.AIBehaviour idleAction = UniRPGGlobal.AIBehaviour.Stay;

		public float chaseSpeed = 4f;
		public PatrolPath patrolPath;					// used with NPC.IdleAction.Patrol
		public bool wanderInCricle = true;				// else square area
		public Vector2 wanderWH = Vector2.one;			// when wanderInCricle = false
		public float wanderRadius = 10f;				// how far from spawn can it wander, used with NPC.IdleAction.Wander
		public float wanderDelayMin = 0.5f;				// how long the NPC can idle between wander destinations
		public float wanderDelayMax = 2.0f;
		public float chaseTimeout = 10f;
		public float detectionRadius = 3f;				// from how far the NPC can detect the player

		public float chooseSkill2DistanceMod = 0.5f;	// NPC will use skill2 when player is in detection radius but not closer than radius *  chooseSkill2DistanceMod

		// ============================================================================================================

		public Vector3 SpawnLocation { get; set; }		// where spawn point is, needed for wander in radius around it
		private float wanderIdleTimer = 0.1f;			// helper to decide when to choose new wander destination
		private int currPatrolPoint = 0;				// used with tracking what patrol point the unit is at. used with NPC.IdleAction.Patrol
		private GameObject eyes;						// object used to detect player
		private float scanTimer = 0f;					// a helper which will scan every second to see if player changed position while chasing player
		private PatrolPath _startupPathHelper = null;	// a helper for LoadSave system
		private RPGSkill skill = null;					// skill chosen to be used
		private bool regHitCheck = true;
		private float chaseTimer = 0f;

		// ============================================================================================================

		public override void Awake()
		{
			base.Awake();
		}

		public override void Start()
		{
			base.Start();

			wanderIdleTimer = 0.1f;
			currPatrolPoint = 0;

			_startupPathHelper = patrolPath;
			if (patrolPath == null && idleAction == UniRPGGlobal.AIBehaviour.Patrol)
			{
				Debug.LogError("IdleAction.Patrol selected but no Patrol Path specified.");
				enabled = false;
				IsPersistent = false;
				return;
			}

			if (SpawnPoint == null) SpawnLocation = _tr.position;

			// create detection trigger (eyes)
			eyes = new GameObject("_DETECT_PLAYER_");
			eyes.layer = gameObject.layer;
			eyes.transform.position = transform.position;
			eyes.transform.rotation = transform.rotation;
			eyes.transform.parent = transform;
			SphereCollider col = eyes.AddComponent<SphereCollider>();
			col.isTrigger = true;
			col.radius = detectionRadius;

			if (actorType != UniRPGGlobal.ActorType.Hostile)
			{	// no need for eyes to be active if not hostile
				eyes.SetActive(false);
			}

			if (rigidbody) rigidbody.isKinematic = true;
		}

		protected override void SaveState()
		{
			base.SaveState();

			UniRPGGlobal.SaveFloat(saveKey + "chaseSpeed", chaseSpeed);
			UniRPGGlobal.SaveVector3(saveKey + "pos", _tr.position);
			UniRPGGlobal.SaveInt(saveKey + "type", (int)actorType);
			UniRPGGlobal.SaveInt(saveKey + "idleAct", (int)idleAction);
			if (idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				UniRPGGlobal.SaveBool(saveKey + "wndr_is_c", wanderInCricle);
				UniRPGGlobal.SaveFloat(saveKey + "wndr_r", wanderRadius);
				UniRPGGlobal.SaveVector3(saveKey + "wndr_rec", wanderWH);
				UniRPGGlobal.SaveFloat(saveKey + "wnrd_dmin", wanderDelayMin);
				UniRPGGlobal.SaveFloat(saveKey + "wndr_dmax", wanderDelayMax);
			}
			else
			{
				UniRPGGlobal.DeleteSaveKey(saveKey + "wndr_is_c");
				UniRPGGlobal.DeleteSaveKey(saveKey + "wndr_r");
				UniRPGGlobal.DeleteSaveKey(saveKey + "wndr_rec");
				UniRPGGlobal.DeleteSaveKey(saveKey + "wnrd_dmin");
				UniRPGGlobal.DeleteSaveKey(saveKey + "wndr_dmax");
			}

			if (idleAction == UniRPGGlobal.AIBehaviour.Patrol && _startupPathHelper != patrolPath) // only save if path was changed during runtime
			{
				if (patrolPath != null)
				{
					UniRPGGlobal.SaveString(saveKey + "patrol", patrolPath.id.ToString());
				}
				else UniRPGGlobal.DeleteSaveKey(saveKey + "patrol");
			}
			else UniRPGGlobal.DeleteSaveKey(saveKey + "patrol");
		}

		protected override void LoadState()
		{
			base.LoadState();
			if (UniRPGGlobal.Instance.DoNotLoad) return;

			chaseSpeed = UniRPGGlobal.LoadFloat(saveKey + "chaseSpeed", chaseSpeed);
			_tr.position = UniRPGGlobal.LoadVector3(saveKey + "pos", _tr.position);
			_tr.position += new Vector3(0f, 0.1f, 0f); // for sanity sake cause sometimes it falls through floor after spawned
			actorType = (UniRPGGlobal.ActorType)UniRPGGlobal.LoadInt(saveKey + "type", (int)actorType);
			idleAction = (UniRPGGlobal.AIBehaviour)UniRPGGlobal.LoadInt(saveKey + "idleAct", (int)idleAction);
			if (idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				wanderInCricle = UniRPGGlobal.LoadBool(saveKey + "wndr_is_c", wanderInCricle);
				wanderWH = UniRPGGlobal.LoadVector3(saveKey + "wndr_rec", wanderWH);
				wanderRadius = UniRPGGlobal.LoadFloat(saveKey + "wndr_r", wanderRadius);
				wanderDelayMin = UniRPGGlobal.LoadFloat(saveKey + "wnrd_dmin", wanderDelayMin);
				wanderDelayMax = UniRPGGlobal.LoadFloat(saveKey + "wndr_dmax", wanderDelayMax);
			}
			if (idleAction == UniRPGGlobal.AIBehaviour.Patrol)
			{
				string s = UniRPGGlobal.LoadString(saveKey + "patrol", null);
				if (!string.IsNullOrEmpty(s))
				{	// find the path in the scene
					GameObject parent = GameObject.Find("PatrolPaths");
					if (parent)
					{
						GUID id = new GUID(s);
						if (!id.IsEmpty)
						{
							PatrolPath[] paths = parent.GetComponentsInChildren<PatrolPath>();
							for (int i = 0; i < paths.Length; i++)
							{
								if (paths[i].id == id)
								{
									patrolPath = paths[i];
								}
							}
						}
					}
				}
			}

		}

		public override void SetSpawnPoint(SpawnPoint sp)
		{
			base.SetSpawnPoint(sp);

			// init AI
			idleAction = sp.idleAction;
			wanderInCricle = sp.wanderInCricle;
			wanderWH = sp.wanderWH;
			wanderRadius = sp.wanderRadius;
			SpawnLocation = sp.transform.position;
			wanderDelayMin = sp.wanderDelayMin;
			wanderDelayMax = sp.wanderDelayMax;
			patrolPath = sp.patrolPath;

			wanderIdleTimer = 0.1f;
			currPatrolPoint = 0;

			if (patrolPath == null && idleAction == UniRPGGlobal.AIBehaviour.Patrol)
			{
				Debug.LogError("IdleAction.Patrol selected but no Patrol Path specified.");
				enabled = false;
				IsPersistent = false;
				return;
			}
		}

		public override UniRPGGlobal.ActorType ActorType()
		{
			return actorType;
		}

		// ============================================================================================================

		public override void Update()
		{
			if (UniRPGGlobal.Instance.state == UniRPGGlobal.State.InMainMenu) return;

			base.Update();
			if (IsLoading) return;

			if (regHitCheck)
			{
				regHitCheck = false;
				if (hitAttrib != null)
				{
					RPGAttribute att = ActorClass.GetAttribute(hitAttrib);
					if (att != null)
					{
						att.onValueChange += WasHit;
					}
				}
			}

			if (actorType == UniRPGGlobal.ActorType.Hostile && TargetedCharacter != null)
			{	// go after the player if hostile and detected him
				UpdateHostileAI();
				return;
			}

			if (idleAction == UniRPGGlobal.AIBehaviour.Stay) return; // nothing more to do

			if (navi.CurrentSpeed() > 0.0f) return;

			// Wander
			if (idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				wanderIdleTimer -= Time.deltaTime;
				if (wanderIdleTimer <= 0.0f)
				{
					wanderIdleTimer = Random.Range(wanderDelayMin, wanderDelayMax); // this is how long the NPC might idle before choosing to move again next time
					if (wanderInCricle)
					{
						Vector3 p = SpawnLocation + UniRPGUtil.PickPointInCircle(wanderRadius);
						navi.MoveTo(p, moveSpeed, turnSpeed);
					}
					else
					{
						Vector3 p = SpawnLocation + UniRPGUtil.PickPointInRectanle(wanderWH);
						navi.MoveTo(p, moveSpeed, turnSpeed);
					}
				}
			}

			// Patrol
			else if (idleAction == UniRPGGlobal.AIBehaviour.Patrol)
			{
				Vector3 p = patrolPath.GetPatrolPointPosition(currPatrolPoint);
				navi.MoveTo(p, moveSpeed, turnSpeed);
				currPatrolPoint = (currPatrolPoint + 1 < patrolPath.Length ? currPatrolPoint + 1 : 0);
			}
		}

		private void UpdateHostileAI()
		{
			// try to use the primary skill on the player (it is most likely an attack skill)
			if (_actor.skills.Count == 0 || TargetedCharacter == null) return;
			if (_actor.IsPerformingSkill) return;

			if (skill != null && false == UniRPGGlobal.Player.canBeTargeted || actorType != UniRPGGlobal.ActorType.Hostile)
			{	// player became non-targetable (or the NPC is not hostile any longer)
				skill = null;
				ClearTarget();
				_actor.ClearQueuedSkill();
				return;
			}

			chaseTimer -= Time.deltaTime;
			if (chaseTimer <= 0.0f)
			{
				skill = null;
				ClearTarget();
				_actor.ClearQueuedSkill();
				return;
			}

			// if it has a skill 2 when 1st check if that skill can't rather be used
			skill = null;
			if (_actor.skills.Count > 1)
			{
				if (_actor.skills[1])
				{
					float distance = Vector3.Distance(UniRPGGlobal.Player.transform.position, _tr.position);
					if (distance >= detectionRadius * chooseSkill2DistanceMod) skill = _actor.skills[1];
				}
			}

			if (skill == null) skill = _actor.skills[0];
			if (skill)
			{
				if (false == UniRPGGlobal.Player.canBeTargeted || actorType != UniRPGGlobal.ActorType.Hostile)
				{	// player became non-targetable (or the NPC is not hostile any longer)
					ClearTarget();
					_actor.ClearQueuedSkill();
					return;
				}

				if (skill.IsReady)
				{
					int r = CheckIfCanPerform(skill, TargetedCharacter.transform.position);
					if (r == 0)
					{
						chaseTimer = chaseTimeout;
						navi.Stop();
						_actor.UseSkill(skill, TargetedCharacter.gameObject, false);
						navi.LookAt(TargetedCharacter.transform.position);
					}
					else
					{
						scanTimer -= Time.deltaTime;
						if (navi.IsMovingOrPathing() == false || scanTimer <= 0f)
						{
							scanTimer = 1f;
							// these updates can only happen in intervals. Do not want MoveTo() called continuously
							if (r == 1)
							{	// move closer to target
								navi.MoveTo(TargetedCharacter.transform.position, chaseSpeed, turnSpeed);
							}
							else
							{	// at least look in correct direction
								navi.LookAt(TargetedCharacter.transform.position);
							}
						}
					}
				}
			}
		}

		// 0: yes, 1:too far, 2:turn
		private int CheckIfCanPerform(RPGSkill skill, Vector3 skillTargetLocation) 
		{
			Vector3 d = (skillTargetLocation - _tr.position);
			float m = d.magnitude;
			if (m > skill.onUseMaxTargetDistance) return 1; // too far
			d = d.normalized; d.y = _tr.forward.y;
			if (Vector3.Dot(_tr.forward, d) > 0.8f) return 0; // all good
			return 2; // need to turn
		}

		// ============================================================================================================

		public void WasHit(RPGAttribute att)
		{
			// Force it to chase player even if not in detection radius
			OnPlayerEnterTrigger();
		}

		/// <summary>
		/// When the player enters the NPC's "detection collider/trigger" it will 
		/// send a OnPlayerEnterTrigger message to the NPC's GameObject
		/// </summary>
		public void OnPlayerEnterTrigger()
		{
			chaseTimer = chaseTimeout;
			if (!enabled) return;
			if (actorType != UniRPGGlobal.ActorType.Hostile) return;	// don't bother if not hostile
			if (TargetInteract != null) return;							// don't bother if already targeted player
			if (_actor.skills.Count == 0) return;						// don't bother if got no skills
			if (_actor.skills[0] == null) return;						// don't bother if got no skill-0 to use
			if (UniRPGGlobal.Player.canBeTargeted == false) return;		// don't bother if the player cant be targeted

			navi.Stop();
			TargetInteract = UniRPGGlobal.Player;
			UniRPGGameController.ExecuteActions(TargetInteract.onTargetedActions, TargetInteract.gameObject, null, gameObject, null, null, false);
		}

		/// <summary>
		/// When the player leaves the NPC's "detection collider/trigger" he will 
		/// send a OnPlayerExitTrigger message to the NPC's GameObject
		/// </summary>
		public void OnPlayerExitTrigger()
		{
			// give up chasing the player. un-target and clear any skills that where queued
			ClearTarget();
			_actor.ClearQueuedSkill();
		}

		/// <summary>
		/// Called while player remains inside the detection area/ trigger
		/// </summary>
		public void OnPlayerStayTrigger()
		{
			// simply call onEnter since I want the same things to happen
			OnPlayerEnterTrigger();
		}

		// ============================================================================================================

#if UNITY_EDITOR
		void OnDrawGizmosSelected()
		{
			if (actorType == UniRPGGlobal.ActorType.Hostile)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position, detectionRadius);
			}
		}
#endif
		// ============================================================================================================
	}
}
