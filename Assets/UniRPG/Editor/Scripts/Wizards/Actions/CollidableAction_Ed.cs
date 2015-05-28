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

	[ActionInfo(typeof(CollidableAction), "Object/Collidable: Create", Description = "Create Collidable and set properties on it")]
	public class CollidableAction_Ed : ActionsEdBase
	{

		public override string ActionShortNfo(Object actionObj)
		{
			return "Create Collidable";
		}

		public override void OnGUI(Object actionObj)
		{
			CollidableAction action = actionObj as CollidableAction;
			if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

			action.prefab = UniRPGEdGui.GlobalObjectVarOrValueField(this.ed, "Prefab", action.prefab, typeof(GameObject), false, 40);
			EditorGUILayout.Space();
			UniRPGEdGui.TargetTypeField(this.ed, "Create at location of", action.spawnFrom, TargetTypeHelp);
			action.offsetFrom = EditorGUILayout.Vector3Field("Create offset", action.offsetFrom);
			EditorGUILayout.Space();
			UniRPGEdGui.TargetTypeField(this.ed, "Target", action.subject, TargetTypeHelp);
			action.offsetTargetPosition = EditorGUILayout.Vector3Field("Target Position Offset", action.offsetTargetPosition);
			EditorGUILayout.Space();
			EditorGUIUtility.LookLikeControls(160);
			action.setGlobalObjectVar = EditorGUILayout.TextField("Save to global object var", action.setGlobalObjectVar);
			action.speed = EditorGUILayout.FloatField("Movement Speed", action.speed);
			EditorGUIUtility.LookLikeControls();
			EditorGUILayout.Space();
			action.triggerWhenReachTarget = GUILayout.Toggle(action.triggerWhenReachTarget, " Trigger after reaching Target Position");
			action.selfDestructWhenNoTarget = GUILayout.Toggle(action.selfDestructWhenNoTarget, " Self Destruct if Target becomes invalid");
			//EditorGUILayout.HelpBox("You would normally want this enabled. The collidable will not trigger a hit when it collides with another potentially valid object while on its way to your specified target. It will also trigger immediately after reaching the target position, whether that target has a collider to collide against or not.", MessageType.Info);
		}

		// ================================================================================================================
	}
}