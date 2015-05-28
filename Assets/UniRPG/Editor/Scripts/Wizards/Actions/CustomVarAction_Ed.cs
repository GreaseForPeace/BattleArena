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

[ActionInfo(typeof(CustomVarAction), "System/Set Custom Variable", Description = "Set/Create a Custom Variable on Subject")]
	public class CustomVarAction_Ed : ActionsEdBase
{
	private int selAttrib = -1;
	private static readonly string[] partStrings = { "Value", "Min", "Max" };

	public override string ActionShortNfo(Object actionObj)
	{
		CustomVarAction action = actionObj as CustomVarAction;
		if (action == null) return "!ERROR!";
		return string.Format("Set Custom Variable {0} on {1}", action.varName, action.subject.type);

	}

	public override void OnGUI(Object actionObj)
	{
		CustomVarAction action = actionObj as CustomVarAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		GUILayout.Label("Set Custom Variable", UniRPGEdGui.Head4Style);

		UniRPGEdGui.TargetTypeField(this.ed, "of", action.customVariableOwner, TargetTypeHelp, 75);
		EditorGUIUtility.LookLikeControls(90);
		action.varName = EditorGUILayout.TextField("with Name", action.varName);

		EditorGUILayout.Space();
		GUILayout.Label("... to", UniRPGEdGui.Head4Style);

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

	/*public override string ActionShortNfo(Object actionObj)
	{
		CustomVarAction action = actionObj as CustomVarAction;
		if (action == null) return "!ERROR!";
		return string.Format("Set Custom Variable {0} on {1}", action.varName, action.subject.type);
	}

	public override void OnGUI(Object actionObj)
	{
		CustomVarAction action = actionObj as CustomVarAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		GUILayout.Space(20);

		GUILayout.Label("Set Variable", UniRPGEdGui.Head4Style);
		EditorGUIUtility.LookLikeControls(115);
		action.varName = EditorGUILayout.TextField("with Name", action.varName);
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Toggle(action.toVal == 0, " to string value", GUILayout.Width(130))) action.toVal = 0;
			if (action.toVal == 0) action.str = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, action.str);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Toggle(action.toVal == 1, " to number value", GUILayout.Width(130))) action.toVal = 1;
			if (action.toVal == 1) action.num = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, action.num);
		}
		EditorGUILayout.EndHorizontal();
		
	}*/

	// ================================================================================================================
} }