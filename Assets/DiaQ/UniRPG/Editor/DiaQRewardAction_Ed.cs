// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UnityEditor;
using UniRPG;
using UniRPGEditor;
using DiaQ;

namespace DiaQEditor
{
	[ActionInfo(typeof(DiaQRewardAction), "DiaQ/Reward", Description = "Give Reward")]
	public class DiaQRewardAction_Ed : ActionsEdBase
	{
		public override string ActionShortNfo(Object actionObj)
		{
			DiaQRewardAction action = actionObj as DiaQRewardAction;
			if (action == null) return "!ERROR!";
			return string.Format("DiaQ Reward");
		}

		public override void OnGUI(Object actionObj)
		{
			if (DiaQEditorWindow.Asset == null)
			{
				GUILayout.Label("The DiaQ Asset could not be loaded.\nPlease make sure your DiaQ settings\nare valid before using this Action.");
				return;
			}

			DiaQRewardAction a = actionObj as DiaQRewardAction;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			GUILayout.Label("Give Rewards from Quest");
			if (GUILayout.Button(a.questName)) DiaQuestSelectWiz.ShowWiz(_OnQuestSelected, DiaQEditorWindow.Asset, new object[] { a, ed });
			EditorGUILayout.Space();

			a.checkIfAccepted = GUILayout.Toggle(a.checkIfAccepted, " quest must be Accepted");
			a.checkIfCompleted = GUILayout.Toggle(a.checkIfCompleted, " quest must be Completed");
			a.checkIfHandedIn = GUILayout.Toggle(a.checkIfHandedIn, " quest must not be Handed In previously");
		}

		private void _OnQuestSelected(DiaQuestSelectWiz wiz, object[] args)
		{
			DiaQRewardAction action = args[0] as DiaQRewardAction;
			EditorWindow ed = args[1] as EditorWindow;
			action.questIdent = wiz.selected.IdentString;
			action.questName = wiz.selected.name;
			wiz.Close();
			ed.Repaint();
		}

		// ================================================================================================================
	}
}

#endif