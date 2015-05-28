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
	[ActionInfo(typeof(DiaQDecisionAction), "DiaQ/Decision (IfThen)", Description = "DiaQ specific test to decide what to do next")]
	public class DiaQDecisionAction_Ed : ActionsEdBase 
	{
		private static readonly string[] DoOptionStrings = { "Skip the Next Action", "Go to this Action Number", "Exit Action Queue now" };
		private Vector2 scroll = Vector2.zero;

		public override string ActionShortNfo(Object actionObj)
		{
			DiaQDecisionAction action = actionObj as DiaQDecisionAction;
			if (action == null) return "!ERROR!";
			return "DiaQ Decision (IfThen)";
		}

		public override void OnGUI(Object actionObj)
		{
			if (DiaQEditorWindow.Asset == null)
			{
				GUILayout.Label("The DiaQ Asset could not be loaded.\nPlease make sure your DiaQ settings\nare valid before using this Action.");
				return;
			}

			DiaQDecisionAction a = actionObj as DiaQDecisionAction;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			GUILayout.Label("If these result in True", UniRPGEdGui.Head4Style);
			scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoMarginPaddingStyle, GUILayout.Height(120), GUILayout.Width(ed.position.width));
			{
				DiaQDecisionEd.OnGUI(ed, a.decision, DiaQEditorWindow.Asset, ed.position.width);
			}
			UniRPGEdGui.EndScrollView();

			GUILayout.Space(20);
			GUILayout.Label("Then do this", UniRPGEdGui.Head4Style);

			a.doOption = EditorGUILayout.Popup(a.doOption, DoOptionStrings);
			if (a.doOption == 1)
			{
				a.gotoAction = EditorGUILayout.IntField("Action number: ", a.gotoAction);
				EditorGUILayout.HelpBox("This is the Action number to go to next, counted from the top of the list, starting at (1).", MessageType.Info);
			}
			else
			{
				GUILayout.Space(20);
			}

			GUILayout.Label("Else: Simply execute the Next Action, if any");

		}

		// ================================================================================================================
	}
}

#endif