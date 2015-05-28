// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(Actor))]
	public class Chara2_Base : CharacterBase
	{

		public float moveSpeed = 5f;
		public float turnSpeed = 800f;

		// ============================================================================================================

		protected Chara2_NaviBase navi;
		protected Chara2_AnimCtrlBase ani;
		public bool IsPerformingSkill { get; private set; }

		// ============================================================================================================

		public override void Awake()
		{
			base.Awake();
			IsPerformingSkill = false;
			navi = GetComponent<Chara2_NaviBase>();
			ani = GetComponent<Chara2_AnimCtrlBase>();
		}

		public override void Start()
		{
			base.Start();

			if (UniRPGGlobal.Instance.state == UniRPGGlobal.State.InMainMenu)
			{
				IsPersistent = false;
				if (navi) navi.enabled = false;
				if (ani) ani.enabled = false;
				enabled = false;
			}

			else
			{
				if (navi == null)
				{
					Debug.LogError(string.Format("The Character ({0}) does not have a Movement Control.", name));
					gameObject.SetActive(false);
					return;
				}

				if (ani == null)
				{
					Debug.LogError(string.Format("The Character ({0}) does not have an Animation Control.", name));
					gameObject.SetActive(false);
					return;
				}
			}
		}

		protected override void SaveState()
		{
			base.SaveState();

			UniRPGGlobal.SaveFloat(saveKey + "moveSpeed", moveSpeed);
			UniRPGGlobal.SaveFloat(saveKey + "turnSpeed", turnSpeed);
		}

		protected override void LoadState()
		{
			base.LoadState();
			if (UniRPGGlobal.Instance.DoNotLoad) return;

			moveSpeed = UniRPGGlobal.LoadFloat(saveKey + "moveSpeed", moveSpeed);
			turnSpeed = UniRPGGlobal.LoadFloat(saveKey + "turnSpeed", turnSpeed);
		}

		public void OnEnable()
		{
			if (UniRPGGlobal.Instance == null) return;
			if (UniRPGGlobal.Instance.state != UniRPGGlobal.State.InMainMenu)
			{
				if (navi) navi.enabled = true;
				if (ani) ani.enabled = true;
			}
		}

		public void OnDisable()
		{
			_actor.ClearQueuedSkill();
			ClearTarget();
			if (navi) navi.enabled = false;
			if (ani) ani.enabled = false;
		}

		public virtual float CurrentMovementSpeed()
		{
			if (navi == null) return 0f;
			return navi.CurrentSpeed();
		}

		// ============================================================================================================

		/// <summary>called by actor class when a skill is about to be executed</summary>
		public override void AboutToPerformSkill(RPGSkill skill)
		{
			navi.Stop();
			ani.ForceIdle();
			IsPerformingSkill = true;
		}

		/// <summary>called by actor class when a skill's execution has stopped</summary>
		public override void DonePerformingSkill(RPGSkill skill)
		{
			IsPerformingSkill = false;
		}

		/// <summary>
		/// called by actor to find out if it is ok to execute the skill?
		/// a plugin might want to return false here if it is still 
		/// turning the character to look at the skill's target
		/// </summary>
		public override bool CanPerformSkillNow(RPGSkill skill, Vector3 skillTargetLocation) { return true; }

		/// <summary>The character is requested to move to the target position. SpawnPoints might ask for this</summary>
		public override void RequestMoveTo(Vector3 position) 
		{			
			navi.MoveTo(position, moveSpeed, turnSpeed);
		}

		/// <summary>Called to find out if the character is idle (not moving)</summary>
		public override bool IsIdle() 
		{
			return (navi.CurrentSpeed() == 0f);
		}

		// ============================================================================================================
	}
}