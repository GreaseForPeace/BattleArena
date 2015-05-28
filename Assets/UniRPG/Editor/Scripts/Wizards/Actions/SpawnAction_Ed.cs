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

[ActionInfo(typeof(SpawnAction), "Object/Spawn (create)", Description = "Instantiate a prefab")]
public class SpawnAction_Ed : ActionsEdBase
{
	private static readonly string[] Ops = { "No", "Yes", "No, Offset Only" };

	public override string ActionShortNfo(Object actionObj)
	{
		SpawnAction action = actionObj as SpawnAction;
		if (action == null) return "!ERROR!";
		return string.Format("Spawn: {0}", (string.IsNullOrEmpty(action.prefab.GetValOrName())?"!ERROR!":action.prefab.GetValOrName()));
	}

	public override void OnGUI(Object actionObj) 
	{
		SpawnAction action = actionObj as SpawnAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		//action.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", action.prefab, typeof(GameObject), false);
		action.prefab = UniRPGEdGui.GlobalObjectVarOrValueField(this.ed, "Prefab", action.prefab, typeof(GameObject), false);

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		{
			action.position = EditorGUILayout.Vector3Field("Position", action.position);
			if (action.doParent == 1)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical();
				{
					GUILayout.Space(20);
					action.localPos = GUILayout.Toggle(action.localPos, " local");
				}
				EditorGUILayout.EndVertical();
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		{
			action.rotation = EditorGUILayout.Vector3Field("Rotation", action.rotation);
			if (action.doParent == 1)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical();
				{
					GUILayout.Space(20);
					action.localRot = GUILayout.Toggle(action.localRot, " local");
				}
				EditorGUILayout.EndVertical();
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(95);
		action.doParent = EditorGUILayout.Popup("Make as child", action.doParent, Ops);
		if (action.doParent != 0) UniRPGEdGui.TargetTypeField(this.ed, "Parent Object", action.subject, TargetTypeHelp);


		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(160);
		action.setGlobalObjectVar = EditorGUILayout.TextField("Save to global object var", action.setGlobalObjectVar);

		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(240);
		action.autoDestroyTimeout = EditorGUILayout.FloatField("Auto Destroy after (seconds, 0=never)", action.autoDestroyTimeout);

		EditorGUILayout.Space();
		action.persistItem = GUILayout.Toggle(action.persistItem, " Persist Item, only valid if Prefab is an RPGItem");

	}

	// ================================================================================================================
} }