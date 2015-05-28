// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor
{

	[ActionInfo(typeof(Chara2_Type_act), "Actor/Character 2: Type", Description = "Action related to changing Character 2 Type.")]
	public class Chara2_Type_acted : ActionsEdBase
	{

		public override string ActionShortNfo(Object actionObj)
		{
			Chara2_Type_act a = actionObj as Chara2_Type_act;
			if (a == null) return "!ERROR!";
			return "Character 2 Type";
		}

		public override void OnGUI(Object actionObj)
		{
			Chara2_Type_act a = actionObj as Chara2_Type_act;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			UniRPGEdGui.TargetTypeField(this.ed, "Set Subject", a.subject, TargetTypeHelp);
			EditorGUILayout.Space();
			EditorGUIUtility.LookLikeControls(80);
			a.toType = (UniRPGGlobal.ActorType)EditorGUILayout.EnumPopup("to", a.toType);
			EditorGUIUtility.LookLikeControls();
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("This Action allows you to change the type of the Character. For example, setting a Neutral character to being Hostile.", MessageType.Info);
		}

		// ============================================================================================================
	}
}
