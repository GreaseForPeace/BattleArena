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

[ActionInfo(typeof(GlobalVarAction), "System/Set Global Variable", Description = "Set/Create a Global Variable")]
	public class GlobalVarAction_Ed : ActionsEdBase
{
	private int selAttrib = -1;
	private static readonly string[] VarTypeStrings = { "Number", "String", "Object" };
	private static readonly string[] partStrings = { "Value", "Min", "Max" };

	public override string ActionShortNfo(Object actionObj)
	{
		GlobalVarAction action = actionObj as GlobalVarAction;
		if (action == null) return "!ERROR!";
		return string.Format("Set Global Variable {0}", action.varName);
	}

	public override void OnGUI(Object actionObj)
	{
		GlobalVarAction action = actionObj as GlobalVarAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		GUILayout.Label("Set Variable", UniRPGEdGui.Head4Style);
		action.varType = EditorGUILayout.Popup("of Type", action.varType, VarTypeStrings);
		action.varName = EditorGUILayout.TextField("and Name", action.varName);

		EditorGUILayout.Space();
		GUILayout.Label("... to", UniRPGEdGui.Head4Style);

		if (action.varType == 2)
		{
			if (GUILayout.Toggle(action.objSet == 0, " Subject")) action.objSet = 0;
			if (action.objSet == 0)
			{
				UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
				EditorGUILayout.Space();
			}

			if (GUILayout.Toggle(action.objSet == 1, " Subject Child")) action.objSet = 1;
			if (action.objSet == 1)
			{
				UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
				action.str = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Name", action.str, 50);
				EditorGUILayout.Space();
			}

			if (GUILayout.Toggle(action.objSet == 2, " Object by Tag")) action.objSet = 2;
			if (action.objSet == 2)
			{
				action.str = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Tag", action.str, 50);
				EditorGUILayout.Space();
			}

			if (GUILayout.Toggle(action.objSet == 3, " Object by Name")) action.objSet = 3;
			if (action.objSet == 3)
			{
				action.str = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Name", action.str, 50);
				EditorGUILayout.Space();
			}
		}

		// ......
		else
		{
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(action.setFrom == 0, " String", GUILayout.Width(100))) action.setFrom = 0;
				if (action.setFrom == 0) action.str = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, action.str);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(action.setFrom == 1, " Number", GUILayout.Width(100))) action.setFrom = 1;
				if (action.setFrom == 1) action.num = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, action.num);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(action.setFrom == 2, " Attribute", GUILayout.Width(100))) action.setFrom = 2;
				if (action.setFrom == 2)
				{
					EditorGUILayout.BeginVertical();
					{
						UniRPGEdGui.TargetTypeField(this.ed, null, action.subject, TargetTypeHelp);

						EditorGUILayout.BeginHorizontal();
						{
							selAttrib = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, action.attId);
							EditorGUI.BeginChangeCheck();
							selAttrib = EditorGUILayout.Popup(selAttrib, UniRPGEditorGlobal.DB.AttributeNames);
							if (EditorGUI.EndChangeCheck()) action.attId.Value = UniRPGEditorGlobal.DB.attributes[selAttrib].id.Value;
							action.attWhat = EditorGUILayout.Popup(action.attWhat, partStrings, GUILayout.Width(60));
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(action.setFrom == 3, " Level", GUILayout.Width(100))) action.setFrom = 3;
				if (action.setFrom == 3) UniRPGEdGui.TargetTypeField(this.ed, null, action.subject, TargetTypeHelp);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(action.setFrom == 4, " Currency", GUILayout.Width(100))) action.setFrom = 4;
				if (action.setFrom == 4) UniRPGEdGui.TargetTypeField(this.ed, null, action.subject, TargetTypeHelp);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(action.setFrom == 5, " Custom var with name", GUILayout.Width(150))) action.setFrom = 5;
				if (action.setFrom == 5)
				{
					EditorGUILayout.BeginVertical();
					{
						action.str = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, action.str);
						UniRPGEdGui.TargetTypeField(this.ed, "from", action.subject, TargetTypeHelp);
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	// ================================================================================================================
} }