// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Character 2/AnimControl/Legacy")]
	[RequireComponent(typeof(Animation))]
	public class Chara2_AnimCtrl : Chara2_AnimCtrlBase
	{
		[System.Serializable]
		public class AnimationDefition
		{
			public string name = "";
			public string clipName = "";
			public float maxSpeedDetect = 1f;
			public bool detectIsActive = true;
			public float playSpeed = 1f;
		}

		public string idleClip;
		public float idlePlaySpeed = 1f;

		public string[] anticsAnimations = new string[0];
		public float anticsWaitTimeMin = 1f;
		public float anticsWaitTimeMax = 5f;

		public List<AnimationDefition> movementAnimations = new List<AnimationDefition>();

		// ============================================================================================================

		private Animation ani;
		private Chara2_Base chara;

		private int currAni = -1; // -2: was performing skill, -1=Idle, else index into movementAnimations
		private float speed = 0f;
		private int resetAnticFlag = -1;

		// ============================================================================================================

		public void Start()
		{
			ani = GetComponent<Animation>();
			chara = GetComponent<Chara2_Base>();

			// init idle ani
			if (string.IsNullOrEmpty(idleClip)) idleClip = null; // set to null for faster if() checks
			if (idleClip != null)
			{
				ani[idleClip].speed = idlePlaySpeed;
				ani[idleClip].wrapMode = WrapMode.Loop;
				ani.Play(idleClip);
			}

			// sorted so that lower speedDetect is checked first
			movementAnimations.Sort((a, b) => { return a.maxSpeedDetect.CompareTo(b.maxSpeedDetect); });

			// init movement anis
			for (int i = 0; i < movementAnimations.Count; i++)
			{
				if (!string.IsNullOrEmpty(movementAnimations[i].clipName))
				{
					ani[movementAnimations[i].clipName].speed = movementAnimations[i].playSpeed;
					ani[movementAnimations[i].clipName].wrapMode = WrapMode.Loop;
				}
				else movementAnimations[i] = null; // set to null for faster if() checks
			}

			// init antics
			_anticsOn = (UniRPGGlobal.Instance.state != UniRPGGlobal.State.InMainMenu);
			anticsTimer = anticsWaitTimeMin;
		}

		public void LateUpdate()
		{
			if (chara.IsPerformingSkill)
			{
				currAni = -2; // set flag to indicate that idle/other should be checked for when action done
				return;
			}

			speed = chara.CurrentMovementSpeed();

			// Speed too low, go to idle
			if (speed <= 0.001f)
			{
				if (currAni != -1)
				{
					currAni = -1;
					if (idleClip != null) ani.CrossFade(idleClip);
				}
				else
				{	// play an antic?
					if (ani != null && anticsAnimations.Length > 0 && AnticsOn && resetAnticFlag < 0)
					{
						anticsTimer -= Time.deltaTime;
						if (anticsTimer <= 0f)
						{
							anticsTimer = Random.Range(anticsWaitTimeMin, anticsWaitTimeMax);
							string s = null;
							if (anticsAnimations.Length == 1)s = anticsAnimations[0];
							else s = anticsAnimations[Random.Range(0, anticsAnimations.Length)];
							if (!string.IsNullOrEmpty(s))
							{
								ani.CrossFade(s);
								if (idleClip != null) ani.PlayQueued(idleClip, QueueMode.CompleteOthers);
							}
						}
					}
				}
				return;
			}

			// else check what movement ani should be playing
			for (int i = 0; i < movementAnimations.Count; i++)
			{
				if (null == movementAnimations[i]) continue;
				if (false == movementAnimations[i].detectIsActive) continue;
				if (speed <= movementAnimations[i].maxSpeedDetect)
				{
					if (currAni != i || !ani.IsPlaying(movementAnimations[i].clipName))
					{
						currAni = i;
						ani.CrossFade(movementAnimations[i].clipName);
					}
					break;
				}
			}
		}

		public override void ForceIdle() 
		{
			currAni = -1;
			if (idleClip != null) ani.Play(idleClip);
		}

		public AnimationDefition FindMovementDef(string name)
		{
			for (int i = 0; i < movementAnimations.Count; i++)
			{
				if (movementAnimations[i].name.Equals(name)) return movementAnimations[i];
			}
			return null;
		}

		private float anticsTimer = 0f;
		private bool _anticsOn = false;
		public bool AnticsOn
		{
			get { return _anticsOn; }
			set { _anticsOn = value; }
		}

		// ============================================================================================================
	}
}