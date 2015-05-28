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
	[ActionInfo(typeof(DiaQVarAction), "DiaQ/Set Variable", Description = "Set DiaQ variable")]
	public class DiaQVarAction_Ed : ActionsEdBase
	{
		public override string ActionShortNfo(Object actionObj)
		{
			DiaQVarAction action = actionObj as DiaQVarAction;
			if (action == null) return "!ERROR!";
			return string.Format("Set DiaQ Variable: {0}", action.varName);
		}

		public override void OnGUI(Object actionObj)
		{
			if (DiaQEditorWindow.Asset == null)
			{
				GUILayout.Label("The DiaQ Asset could not be loaded.\nPlease make sure your DiaQ settings\nare valid before using this Action.");
				return;
			}

			DiaQVarAction action = actionObj as DiaQVarAction;
			if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

			GUILayout.Label("Set variable");
			GUILayout.BeginHorizontal();
			{
				action.varName = EditorGUILayout.TextField(action.varName);
				if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnSelectedVar, null, DiaQEditorWindow.Asset, new object[] { ed, action });
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			GUILayout.Label("To value");
			action.toVal = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, action.toVal);
		}

		private static void _OnSelectedVar(DiaQVarSelectWiz wiz, object[] args)
		{
			EditorWindow ed = args[0] as EditorWindow;
			DiaQVarAction action = args[1] as DiaQVarAction;
			action.varName = wiz.selected.name;
			wiz.Close();
			if (ed != null) ed.Repaint();
		}

		// ================================================================================================================
	}
}

#endif