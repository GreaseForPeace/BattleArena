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
	[ActionInfo(typeof(DiaQCondAction), "DiaQ/Quest Condition", Description = "Update a quest condition")]
	public class DiaQCondAction_Ed : ActionsEdBase
	{
		private static readonly string[] WhatOpts = { "For All Quests", "For Selected Quest" };

		public override string ActionShortNfo(Object actionObj)
		{
			DiaQCondAction action = actionObj as DiaQCondAction;
			if (action == null) return "!ERROR!";
			return string.Format("DiaQ Condition: {0} => {1}", action.conditionKey.GetValOrName(), action.value.GetValOrName());
		}

		public override void OnGUI(Object actionObj)
		{
			if (DiaQEditorWindow.Asset == null)
			{
				GUILayout.Label("The DiaQ Asset could not be loaded.\nPlease make sure your DiaQ settings\nare valid before using this Action.");
				return;
			}

			DiaQCondAction a = actionObj as DiaQCondAction;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			a.setWhat = EditorGUILayout.Popup(a.setWhat, WhatOpts);
			EditorGUILayout.Space();
			if (a.setWhat == 1)
			{
				if (GUILayout.Button(a.questName)) DiaQuestSelectWiz.ShowWiz(_OnQuestSelected, DiaQEditorWindow.Asset, new object[] { a, ed });
				EditorGUILayout.Space();
			}

			a.conditionKey = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "update Condition", a.conditionKey);
			EditorGUILayout.Space();
			a.value = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "adding Value", a.value);
		}

		private void _OnQuestSelected(DiaQuestSelectWiz wiz, object[] args)
		{
			DiaQCondAction action = args[0] as DiaQCondAction;
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