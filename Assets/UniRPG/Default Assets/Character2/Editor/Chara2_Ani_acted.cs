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

	[ActionInfo(typeof(Chara2_Ani_act), "Actor/Character 2: Animation", Description = "Action related to Character 2 Legacy Animation.")]
	public class Chara2_Ani_acted : ActionsEdBase
	{

		private readonly static string[] Opts = { "Go Idle", "Change Idle Clip", "Change MoveDef", "Set Move Definition Active", "Set Move Speed Detect", "Toggle Antics" };

		public override string ActionShortNfo(Object actionObj)
		{
			Chara2_Ani_act a = actionObj as Chara2_Ani_act;
			if (a == null) return "!ERROR!";
			return "Character 2 Animation";
		}

		public override void OnGUI(Object actionObj)
		{
			Chara2_Ani_act a = actionObj as Chara2_Ani_act;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			UniRPGEdGui.TargetTypeField(this.ed, "Subject", a.subject, TargetTypeHelp);
			EditorGUILayout.Space();
			a.act = EditorGUILayout.Popup(a.act, Opts);
			EditorGUILayout.Space();

			switch (a.act)
			{
				case 1:
				a.clipName = EditorGUILayout.TextField("Clip", a.clipName);
				a.aniSpeed = EditorGUILayout.FloatField("Play Speed", a.aniSpeed);
				a.b_opt = EditorGUILayout.Toggle("Go Idle now", a.b_opt);
				break;
				case 2:
				a.moveName = EditorGUILayout.TextField("Movement Name", a.moveName);
				a.clipName = EditorGUILayout.TextField("Clip", a.clipName);
				a.aniSpeed = EditorGUILayout.FloatField("Play Speed", a.aniSpeed);
				a.speedDetect = EditorGUILayout.FloatField("Max Speed Detect", a.speedDetect);
				a.b_opt = EditorGUILayout.Toggle("Detect is Active", a.b_opt);
				break;
				case 3:
				a.moveName = EditorGUILayout.TextField("Movement Name", a.moveName);
				a.b_opt = EditorGUILayout.Toggle("Detect is Active", a.b_opt);
				break;
				case 4:
				a.moveName = EditorGUILayout.TextField("Movement Name", a.moveName);
				a.speedDetect = EditorGUILayout.FloatField("Max Speed Detect", a.speedDetect);
				break;
				case 5:
				a.b_opt = EditorGUILayout.Toggle("Antics is On", a.b_opt);
				break;
			}
		}

		// ============================================================================================================
	}
}
