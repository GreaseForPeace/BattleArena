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

[ActionInfo(typeof(SendMessageAction), "System/Send Message", Description = "Calls a method on every behaviour/script of the game object")]
public class SendMessageAction_Ed : ActionsEdBase
{
	private static readonly string[] ParamTypes = { "None", "Text", "Numeric", "Vector4", "Scene Object or Prefab" };

	public override string ActionShortNfo(Object actionObj)
	{
		SendMessageAction action = actionObj as SendMessageAction;
		if (action == null) return "!ERROR!";
		if (action.sendToTaggedObjects) return string.Format("SendMessage ({0}) to all with tag: {1}", action.functionName, action.tagToUse);
		else return string.Format("SendMessage ({0}) to: {1}", action.functionName, action.subject);
	}

	public override void OnGUI(Object actionObj)
	{
		SendMessageAction action = actionObj as SendMessageAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		action.sendToTaggedObjects = EditorGUILayout.Toggle("To all objects with Tag", action.sendToTaggedObjects);
		if (action.sendToTaggedObjects)
		{
			action.tagToUse = EditorGUILayout.TextField("Tag", action.tagToUse);
		}
		else
		{
			UniRPGEdGui.TargetTypeField(this.ed, "To Object", action.subject, TargetTypeHelp, 80);
		}
		GUILayout.Space(20);
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();
			{
				GUILayout.Label("With Function");
				GUILayout.Label("and param");
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			{
				action.functionName = EditorGUILayout.TextField(action.functionName);
				action.paramType = EditorGUILayout.Popup(action.paramType, ParamTypes);
				switch (action.paramType)
				{
					case 1: //Text:
					{
						action.param1 = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, action.param1);
						action.param4 = null; // clear any unintended links with prefabs and other assets
					} break;
					case 2: //Numeric:
					{
						action.param2 = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, action.param2);
						action.param4 = null; // clear any unintended links with prefabs and other assets
					}
					break;
					case 3: //Vector4:
					{
						action.param3 = EditorGUILayout.Vector4Field(null, action.param3);
						action.param4 = null; // clear any unintended links with prefabs and other assets
					} break;
					case 4: //GameObject:
					{
						action.param4 = (GameObject)EditorGUILayout.ObjectField(action.param4, typeof(GameObject), true);
					} break;
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();
	}

	// ================================================================================================================
} }