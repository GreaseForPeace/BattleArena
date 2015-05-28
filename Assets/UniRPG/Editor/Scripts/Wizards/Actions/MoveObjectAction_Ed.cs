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

[ActionInfo(typeof(MoveObjectAction), "Object/Move", Description = "Move an object")]
public class MoveObjectAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Position", "Location of", "SpawnPoint" };

	public override string ActionShortNfo(Object actionObj)
	{
		MoveObjectAction action = actionObj as MoveObjectAction;
		if (action == null) return "!ERROR!";
		if (action.moveTo == 0) return string.Format("Move: {0} to {1}", action.subject.type, action.position);
		else if (action.moveTo == 1) return string.Format("Move: {0} to {1}", action.subject.type, action.positionTarget.type);
		else return string.Format("Move: {0} to SpawnPoint {1}", action.subject.type, action.num.GetValOrName());
	}

	public override void OnGUI(Object actionObj)
	{
		MoveObjectAction action = actionObj as MoveObjectAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Move Subject", action.subject, TargetTypeHelp, 85);
		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(100);
		action.moveTo = EditorGUILayout.Popup("to", action.moveTo, options);
		if (action.moveTo == 0)
		{
			action.position = EditorGUILayout.Vector3Field("", action.position);
		}
		else if (action.moveTo == 1)
		{
			EditorGUIUtility.LookLikeControls();
			UniRPGEdGui.TargetTypeField(this.ed, " ", action.positionTarget, TargetTypeHelp, 85);
			
			EditorGUILayout.BeginHorizontal();
			action.offset = GUILayout.Toggle(action.offset, " offset");
			if (action.offset) action.position = EditorGUILayout.Vector3Field("", action.position);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			action.doOrient = GUILayout.Toggle(action.doOrient, " orient to");
			if (action.doOrient)
			{
				action.orientTo = (SimpleMove.OrientTo) EditorGUILayout.EnumPopup(action.orientTo);
				action.orientOffset = EditorGUILayout.FloatField(action.orientOffset);
			}
			EditorGUILayout.EndHorizontal();

		}
		else
		{
			EditorGUIUtility.LookLikeControls();
			action.num = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "SpawnPoint Ident", action.num, 120);
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("The 'SpawnPoint Ident' is the unique identifier number you entered for the SpawnPoint.", MessageType.Info);

			EditorGUILayout.BeginHorizontal();
			action.offset = GUILayout.Toggle(action.offset, " offset");
			if (action.offset) action.position = EditorGUILayout.Vector3Field("", action.position);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(150);
		action.instant = EditorGUILayout.Toggle("Instant Movement", action.instant);
		if (!action.instant)
		{
			action.speed = EditorGUILayout.FloatField("Speed", action.speed);
			action.faceMoveDirection = EditorGUILayout.Toggle("Face in Move Direction", action.faceMoveDirection);
			action.autoDestroy = EditorGUILayout.Toggle("Destroy when reached", action.autoDestroy);
		}

	}

	// ================================================================================================================
} }