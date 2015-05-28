// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Character 2/AnimControl/Mecanim")]
	[RequireComponent(typeof(Animator))]
	public class Chara2_MecanimCtrl : Chara2_AnimCtrlBase
	{

		public string speedParam = "Speed";

		public string[] anticsAnimations = new string[0];
		public float anticsWaitTimeMin = 1f;
		public float anticsWaitTimeMax = 5f;

		// ============================================================================================================

		private Animator ani;
		private Chara2_Base chara;

		private int speedHash = 0;
		private int[] anticGUIDs = new int[0];
		private int resetAnticFlag = -1;
		private float speed = 0f;

		// ============================================================================================================

		protected void Start()
		{
			ani = GetComponent<Animator>();
			chara = GetComponent<Chara2_Base>();

			speedHash = Animator.StringToHash(speedParam);

			// init antics
			_anticsOn = (UniRPGGlobal.Instance.state != UniRPGGlobal.State.InMainMenu);
			anticsTimer = anticsWaitTimeMin;
			anticGUIDs = new int[anticsAnimations.Length];
			for (int i = 0; i < anticsAnimations.Length; i++)
			{
				if (!string.IsNullOrEmpty(anticsAnimations[i])) anticGUIDs[i] = Animator.StringToHash(anticsAnimations[i]);
				else anticGUIDs[i] = 0;
			}
		}

		protected void LateUpdate()
		{
			speed = chara.CurrentMovementSpeed();

			if (resetAnticFlag >= 0)
			{
				ani.SetBool(anticGUIDs[resetAnticFlag], false);
				resetAnticFlag = -1;
			}

			ani.SetFloat(speedHash, speed);

			if (speed == 0.0f && anticGUIDs.Length > 0 && AnticsOn && resetAnticFlag < 0)
			{
				anticsTimer -= Time.deltaTime;
				if (anticsTimer <= 0f)
				{
					anticsTimer = Random.Range(anticsWaitTimeMin, anticsWaitTimeMax);
					if (anticGUIDs.Length == 1) resetAnticFlag = 0;
					else resetAnticFlag = Random.Range(0, anticGUIDs.Length);
					if (anticGUIDs[resetAnticFlag] != 0)
					{
						ani.SetBool(anticGUIDs[resetAnticFlag], true);
					}
					else resetAnticFlag = -1;
				}
			}
		}

		//protected void OnAnimatorMove()
		//{
		//	_tr.position = ani.rootPosition;
		//	_tr.rotation = ani.rootRotation;
		//}

		public override void ForceIdle()
		{
			ani.SetFloat(speedHash, 0f);
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