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

	[ActionInfo(typeof(Chara2_Move_act), "Actor/Character 2: Movement", Description = "Action related to Character 2 Movement.")]
	public class Chara2_Move_acted : ActionsEdBase
	{

		public override string ActionShortNfo(Object actionObj)
		{
			Chara2_Move_act a = actionObj as Chara2_Move_act;
			if (a == null) return "!ERROR!";
			return "Character 2 Movement";
		}

		public override void OnGUI(Object actionObj)
		{
			Chara2_Move_act a = actionObj as Chara2_Move_act;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			UniRPGEdGui.TargetTypeField(this.ed, "Subject", a.subject, TargetTypeHelp);
			EditorGUILayout.Space();
			a.moveSpeed = EditorGUILayout.FloatField("Move Speed", a.moveSpeed);
			a.turnSpeed = EditorGUILayout.FloatField("Turn Speed", a.turnSpeed);
			EditorGUILayout.Space();
			a.chaseSpeed = EditorGUILayout.FloatField("Chase Speed", a.chaseSpeed);
			GUILayout.Label("Chase speed apply to NPC only.");
		}

		// ============================================================================================================
	}
}
