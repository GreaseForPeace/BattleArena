// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class AnimationAction : Action
{
	[System.Serializable]
	public class AniInfo
	{
		public StringValue clipName = new StringValue();
		public NumericValue speed = new NumericValue(1f);
		public WrapMode wrapMode = WrapMode.Default;
		public bool crossFade = false;
		public bool reversed = false;
	}

	public StringValue clipName = new StringValue();
	public NumericValue speed = new NumericValue(1f);
	public WrapMode wrapMode = WrapMode.Default;
	public bool crossFade = false;
	public bool reversed = false;

	public bool useRandomClips = false;
	public List<AniInfo> aniList = new List<AniInfo>(0);

	public enum DoWhat { Stop, Play }
	public DoWhat doWhat = DoWhat.Play;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		AnimationAction a = act as AnimationAction;
		a.clipName = this.clipName.Copy();
		a.speed = this.speed.Copy();
		a.wrapMode = this.wrapMode;
		a.crossFade = this.crossFade;
		a.reversed = this.reversed;
		a.doWhat = this.doWhat;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj == null)
		{
			Debug.LogError("Animation Action Error: The subject did not exist.");
			return ReturnType.Done;
		}

		if (obj.animation)
		{
			if (doWhat == DoWhat.Stop)
			{
				obj.animation.Stop();
			}
			else
			{
				if (useRandomClips)
				{
					if (aniList.Count > 0)
					{
						int i = 0;
						if (aniList.Count > 1) i = Random.Range(0, aniList.Count);
						AnimationState ani = obj.animation[aniList[i].clipName.Value];
						if (ani)
						{
							if (aniList[i].reversed)
							{
								ani.time = ani.length;
								ani.speed = -1 * aniList[i].speed.Value;
							}
							else
							{
								ani.time = 0;
								ani.speed = aniList[i].speed.Value;
							}
							ani.wrapMode = aniList[i].wrapMode;
							if (aniList[i].crossFade) obj.animation.CrossFade(aniList[i].clipName.Value);
							else obj.animation.Play(aniList[i].clipName.Value);
						}
						else Debug.LogError(string.Format("Animation Action Error: The subject did not have a clip named [{0}]", clipName.Value));
					}
					else Debug.LogError("Animation Action Error: There are no clips defined");
				}

				else
				{
					AnimationState ani = obj.animation[clipName.Value];
					if (ani)
					{
						if (reversed)
						{
							ani.time = ani.length;
							ani.speed = -1 * speed.Value;
						}
						else
						{
							ani.time = 0;
							ani.speed = speed.Value;
						}
						ani.wrapMode = wrapMode;
						if (crossFade) obj.animation.CrossFade(clipName.Value);
						else obj.animation.Play(clipName.Value);
					}
					else Debug.LogError(string.Format("Animation Action Error: The subject did not have a clip named [{0}]", clipName.Value));
				}
			}
		}
		else Debug.LogError("Animation Action Error: The subject did not have any Animation Component.");

		return ReturnType.Done; // this action is done
	}

	// ================================================================================================================
} }