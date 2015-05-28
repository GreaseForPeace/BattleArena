// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

[ActionInfo(typeof(GUIAction), "System/Message to GUI", Description = "Send a message to the Game GUI")]
public class GUIAction_Ed : ActionsEdBase
{
	public override string ActionShortNfo(Object actionObj)
	{
		GUIAction action = actionObj as GUIAction;
		if (action == null) return "!ERROR!";
		if (action.opt1 != GUIAction.Opt1.FadeIn && action.opt1 != GUIAction.Opt1.FadeOut)
		{
			return string.Format("GUI: {0} {1}", action.opt1, action.opt2);
		}
		return string.Format("GUI: {0}", action.opt1);
	}

	public override void OnGUI(Object actionObj)
	{
		GUIAction action = actionObj as GUIAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUIUtility.LookLikeControls(110);
		action.opt1 = (GUIAction.Opt1)EditorGUILayout.EnumPopup("Send", action.opt1);

		if (action.opt1 != GUIAction.Opt1.FadeIn && action.opt1 != GUIAction.Opt1.FadeOut)
		{
			action.opt2 = (GUIAction.Opt2)EditorGUILayout.EnumPopup(" ", action.opt2);

			if (action.opt2 == GUIAction.Opt2.Custom)
			{
				action.optParam = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "With Param", action.optParam);
			}

			else if (action.opt2 == GUIAction.Opt2.Shop)
			{
				EditorGUILayout.Space();
				UniRPGEdGui.TargetTypeField(this.ed, "Shop keeper", action.subject, TargetTypeHelp);
			}
		}
	}

	// ================================================================================================================
} }