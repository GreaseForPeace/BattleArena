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
	[ActionInfo(typeof(DiaQuestAction), "DiaQ/Quest", Description = "Give Quest")]
	public class DiaQuestAction_Ed : ActionsEdBase
	{
		public override string ActionShortNfo(Object actionObj)
		{
			DiaQuestAction action = actionObj as DiaQuestAction;
			if (action == null) return "!ERROR!";
			return string.Format("DiaQ Quest: {0}", action.questName);
		}

		public override void OnGUI(Object actionObj)
		{
			if (DiaQEditorWindow.Asset == null)
			{
				GUILayout.Label("The DiaQ Asset could not be loaded.\nPlease make sure your DiaQ settings\nare valid before using this Action.");
				return;
			}

			DiaQuestAction a = actionObj as DiaQuestAction;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Quest", GUILayout.Width(80));
				if (GUILayout.Button(a.questName)) DiaQuestSelectWiz.ShowWiz(_OnQuestSelected, DiaQEditorWindow.Asset, new object[] { a, ed });
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			a.give = EditorGUILayout.Toggle("Give player the Quest", a.give);
			a.setCompleted = EditorGUILayout.Toggle("Set Quest as Completed", a.setCompleted);
			a.setHadedIn = EditorGUILayout.Toggle("Set Quest as Handed-in", a.setHadedIn);
		}

		private void _OnQuestSelected(DiaQuestSelectWiz wiz, object[] args)
		{
			DiaQuestAction action = args[0] as DiaQuestAction;
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