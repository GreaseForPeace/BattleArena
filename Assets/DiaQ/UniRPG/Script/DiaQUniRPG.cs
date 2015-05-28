// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UniRPG;

namespace DiaQ
{
	// this class is registered with UniRPG to be auto created when the player goes from menu to ganescene
	// it is just a typical monobehaviour and will be palced onto the UniRPGGlobal gameobject by UniRPG
	// This class will handle the saving/ loading of the DiaQ data

	[AddComponentMenu("")]
	public class DiaQUniRPG : AutoCallBase
	{
		private bool _loading = true;
		private bool regedForLoadEvent = false;

		public override void OnUniRPGAddedThis()
		{
			// Need to be notivied when game scenes are changed cause each time this happened the 
			// reged save events are wiped and I need to reg again for save calls. also use the
			// timing to do check to see if loading should take place

			if (!regedForLoadEvent)
			{
				regedForLoadEvent = true;
				UniRPGGlobal.RegisterLoadGameSceneListener(OnLoadGameScene);
			}
		}

		public void Update()
		{
			if (_loading)
			{
				_loading = false;
				UniRPGGlobal.RegisterForSaveEvent(SaveState);
				LoadState();
			}
		}

		private void OnLoadGameScene(bool started)
		{
			if (!started) _loading = true;
		}

		private void SaveState(System.Object sender)
		{
			string data = DiaQEngine.Instance.GetSaveData();
			if (string.IsNullOrEmpty(data)) return;
			UniRPGGlobal.SaveString("diaq", data);
		}

		private void LoadState()
		{
			// do not load if new game
			if (UniRPGGlobal.Instance.DoNotLoad) return;
			
			// do not load when moving between game scenes since DiaQ is global instance
			if (UniRPGGlobal.Instance.IsAutoLoadSave) return;
			
			string data = UniRPGGlobal.LoadString("diaq", null);
			DiaQEngine.Instance.RestoreViaSaveData(data);
		}

		// ================================================================================================================
	}
}

#endif