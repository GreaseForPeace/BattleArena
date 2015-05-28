// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("")]
	public class Chara2_Ani_act : Action
	{

		public int act = 0; // 0:go idle, 1:set idle clip, 2:set move clip, 3:set move clip active, 4:set move clip speed detect
		public string moveName;
		public string clipName;
		public float aniSpeed = 1f;
		public float speedDetect = 0f;
		public bool b_opt = false; // if act=1,2,3

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			Chara2_Ani_act a = act as Chara2_Ani_act;
			a.act = this.act;
			a.moveName = this.moveName;
			a.clipName = this.clipName;
			a.aniSpeed = this.aniSpeed;
			a.speedDetect = this.speedDetect;
			a.b_opt = this.b_opt;
		}

		public override bool ExecuteWhenRestoringState()
		{
			return true;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj == null)
			{
				Debug.LogError("Character2 Animation Action Error: The subject did not exist.");
				return ReturnType.Done;
			}

			Chara2_AnimCtrl ani = obj.GetComponent<Chara2_AnimCtrl>();
			if (ani != null)
			{
				if (act == 0) // 0:go idle
				{
					Chara2_NaviBase nav = obj.GetComponent<Chara2_NaviBase>();
					if (nav != null) nav.Stop();
					ani.ForceIdle();
				}

				else if (act == 1) // 1:set idle clip
				{
					Animation animation = obj.GetComponent<Animation>();
					if (animation)
					{
						animation[clipName].speed = aniSpeed;
						animation[clipName].wrapMode = WrapMode.Loop;
					}

					if (b_opt)
					{
						Chara2_NaviBase nav = obj.GetComponent<Chara2_NaviBase>();
						if (nav != null) nav.Stop();

						ani.ForceIdle();

						if (animation)
						{
							animation.Stop();
							animation.Play(clipName);
						}
					}

					ani.idleClip = clipName;
					ani.idlePlaySpeed = aniSpeed;
				}

				else if (act == 2) // 2:set move clip
				{
					Chara2_AnimCtrl.AnimationDefition def = ani.FindMovementDef(moveName);
					if (def != null)
					{
						def.clipName = clipName;
						def.playSpeed = aniSpeed;
						def.maxSpeedDetect = speedDetect;
						def.detectIsActive = b_opt;
						Animation animation = obj.GetComponent<Animation>();
						if (animation) animation[clipName].wrapMode = WrapMode.Loop;
					}
					else Debug.LogError("Character2 Animation Action Error: The movement definition could not be found: " + moveName);
				}

				else if (act == 3) // 3:set move clip active
				{
					Chara2_AnimCtrl.AnimationDefition def = ani.FindMovementDef(moveName);
					if (def != null)
					{
						def.detectIsActive = b_opt;
					}
					else Debug.LogError("Character2 Animation Action Error: The movement definition could not be found: " + moveName);
				}

				else if (act == 4) // 4:set move clip speed detect
				{
					Chara2_AnimCtrl.AnimationDefition def = ani.FindMovementDef(moveName);
					if (def != null)
					{
						def.maxSpeedDetect = speedDetect;
					}
					else Debug.LogError("Character2 Animation Action Error: The movement definition could not be found: " + moveName);
				}

				else if (act == 5) // 5:antics on/off
				{
					ani.AnticsOn = b_opt;
				}

			}
			else Debug.LogError("Character2 Animation Action Error: The subject must be a Character 2 type, using the Legacy Animation controller.");

			return ReturnType.Done;
		}

		// ============================================================================================================
	}
}
