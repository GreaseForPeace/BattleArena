// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class LoadSceneAction : Action
{
	public int loadBy = 0; // 0:game scene from DB, 1: main menu, 2:specific name, 3:specific number
	public int gameSceneIdx = 0;
	public StringValue sceneName = new StringValue();
	public NumericValue num = new NumericValue(-1);

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		LoadSceneAction a = act as LoadSceneAction;
		a.loadBy = this.loadBy;
		a.gameSceneIdx = this.gameSceneIdx;
		a.sceneName = this.sceneName.Copy();
		a.num = this.num.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		UniRPGGlobal.Instance.playerSpawnPoint = -1; // reset it

		if (loadBy == 0)		// from DB
		{
			UniRPGGlobal.Instance.playerSpawnPoint = (int)num.Value;
			UniRPGGlobal.Instance.LoadGameScene(UniRPGGlobal.DB.gameSceneNames[gameSceneIdx]);
		}
		else if (loadBy == 1)	// main menu
		{
			UniRPGGlobal.Instance.LoadGameToMenu();
		}
		else if (loadBy == 2)	// using name
		{
			UniRPGGlobal.Instance.LoadGameScene(sceneName.Value);
		}
		return ReturnType.Done;
	}

	// ================================================================================================================
} }