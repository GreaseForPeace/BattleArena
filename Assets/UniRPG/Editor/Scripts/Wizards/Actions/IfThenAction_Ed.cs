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

[ActionInfo(typeof(IfThenAction), "System/Execution: Branch (IfThen)", Description = "Do a test to decide what to do next")]
public class IfThenAction_Ed : ActionsEdBase
{
	private static readonly string[] DoOptionStrings = { "Skip the Next Action", "Go to this Action Number", "Exit Action Queue now" };
	private static readonly string[] CombineStrings = { "AND", "OR" };
	private static readonly string[] TestTypeStrings1 = { "==", "!=" };
	private static readonly string[] TestTypeStrings2 = { "==", "!=", ">", "<", ">=", "<=" };
	private static readonly string[] VarTypeStrings1 = { "Numeric", "Text", "Object", "Attribute", "AttribMin", "AttribMax", "Level", "Currency", "CustomVar", "Subject" };
	private static readonly string[] VarTypeStrings2 = { "Numeric", "Text", "Object", "Attribute", "AttribMin", "AttribMax", "Level", "Currency", "CustomVar", "Subject", "Empty (null)", "IsActor", "IsPlayer", "Enabled", "Friendly", "Neutral", "Hostile" };

	private Vector2 scroll = Vector2.zero;

	public override string ActionShortNfo(Object actionObj)
	{
		IfThenAction action = actionObj as IfThenAction;
		if (action == null) return "!ERROR!";
		return "IfThen Action";
	}

	public override void OnGUI(Object actionObj)
	{
		IfThenAction action = actionObj as IfThenAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("If these result in True", UniRPGEdGui.Head4Style);
			EditorGUILayout.Space();
			if (GUILayout.Button("add test", EditorStyles.miniButton)) action.tests.Add(new IfThenActionTest());
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoMarginPaddingStyle, GUILayout.Height(120));
		{
			IfThenActionTest del = null;
			bool first = true;
			foreach (IfThenActionTest t in action.tests)
			{
				EditorGUILayout.BeginHorizontal();
				{
					if (first)
					{
						first = false;
						GUI.enabled = false;
						EditorGUILayout.Popup(-1, CombineStrings, GUILayout.Width(45));
						GUI.enabled = true;
					}
					else t.combineWithPrev = EditorGUILayout.Popup(t.combineWithPrev, CombineStrings, GUILayout.Width(45));

					EditorGUILayout.BeginVertical();
					{
						t.varType[0] = (IfThenActionTest.VarType)EditorGUILayout.Popup((int)t.varType[0], VarTypeStrings1, GUILayout.Width(85));
						
						// num, str, obj
						if (t.varType[0] == IfThenActionTest.VarType.Number) t.num[0] = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, t.num[0], 0, 55);
						else if (t.varType[0] == IfThenActionTest.VarType.String) t.str[0] = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, t.str[0], 0, 55);
						else if (t.varType[0] == IfThenActionTest.VarType.Object) t.obj[0] = UniRPGEdGui.GlobalObjectVarOrValueField(this.ed, null, t.obj[0], typeof(GameObject), false, 0, 55);
						
						// attribute, level, currency, subject
						else if (	t.varType[0] == IfThenActionTest.VarType.AttributeVal ||
									t.varType[0] == IfThenActionTest.VarType.AttributeMin ||
									t.varType[0] == IfThenActionTest.VarType.AttributeMax ||
									t.varType[0] == IfThenActionTest.VarType.Level ||
									t.varType[0] == IfThenActionTest.VarType.Currency ||
									t.varType[0] == IfThenActionTest.VarType.CustomVar ||
									t.varType[0] == IfThenActionTest.VarType.Subject)
						{
							UniRPGEdGui.TargetTypeField(this.ed, null, t.target[0], null);
							if (t.varType[0] == IfThenActionTest.VarType.AttributeVal || t.varType[0] == IfThenActionTest.VarType.AttributeMin || t.varType[0] == IfThenActionTest.VarType.AttributeMax)
							{	// attribute
								int attIdx = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, t.attribId[0]);
								if (attIdx >= 0) t.attribId[0].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
								EditorGUI.BeginChangeCheck();
								attIdx = EditorGUILayout.Popup(attIdx, UniRPGEditorGlobal.DB.AttributeNames);
								if (EditorGUI.EndChangeCheck()) t.attribId[0].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
							}
							if (t.varType[0] == IfThenActionTest.VarType.CustomVar) t.str[0] = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, t.str[0], 0, 55);
						}
					}
					EditorGUILayout.EndVertical();

					if (t.varType[0] == IfThenActionTest.VarType.Number ||
						t.varType[0] == IfThenActionTest.VarType.AttributeVal ||
						t.varType[0] == IfThenActionTest.VarType.AttributeMin ||
						t.varType[0] == IfThenActionTest.VarType.AttributeMax ||
						t.varType[0] == IfThenActionTest.VarType.Level ||
						t.varType[0] == IfThenActionTest.VarType.Currency)
					{
						t.testType = (IfThenActionTest.TestType)EditorGUILayout.Popup((int)t.testType, TestTypeStrings2, GUILayout.Width(40));
					}
					else
					{
						if ((int)t.testType > 1) t.testType = IfThenActionTest.TestType.Equal;
						t.testType = (IfThenActionTest.TestType)EditorGUILayout.Popup((int)t.testType, TestTypeStrings1, GUILayout.Width(40));
					}

					EditorGUILayout.BeginVertical();
					{
						t.varType[1] = (IfThenActionTest.VarType)EditorGUILayout.Popup((int)t.varType[1], VarTypeStrings2, GUILayout.Width(85));

						// num, str, obj
						if (t.varType[1] == IfThenActionTest.VarType.Number) t.num[1] = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, t.num[1], 0, 55);
						else if (t.varType[1] == IfThenActionTest.VarType.String) t.str[1] = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, t.str[1], 0, 55);
						else if (t.varType[1] == IfThenActionTest.VarType.Object) t.obj[1] = UniRPGEdGui.GlobalObjectVarOrValueField(this.ed, null, t.obj[1], typeof(GameObject), false, 0, 55);

						// attribute, level, currency
						else if (	t.varType[1] == IfThenActionTest.VarType.AttributeVal ||
									t.varType[1] == IfThenActionTest.VarType.AttributeMin ||
									t.varType[1] == IfThenActionTest.VarType.AttributeMax ||
									t.varType[1] == IfThenActionTest.VarType.Level ||
									t.varType[1] == IfThenActionTest.VarType.Currency ||
									t.varType[1] == IfThenActionTest.VarType.CustomVar ||
									t.varType[1] == IfThenActionTest.VarType.Subject)						
						{
							UniRPGEdGui.TargetTypeField(this.ed, null, t.target[1], null);
							if (t.varType[1] == IfThenActionTest.VarType.AttributeVal || t.varType[1] == IfThenActionTest.VarType.AttributeMin || t.varType[1] == IfThenActionTest.VarType.AttributeMax)
							{	// attribute
								int attIdx = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, t.attribId[1]);
								if (attIdx >= 0) t.attribId[1].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
								EditorGUI.BeginChangeCheck();
								attIdx = EditorGUILayout.Popup(attIdx, UniRPGEditorGlobal.DB.AttributeNames);
								if (EditorGUI.EndChangeCheck()) t.attribId[1].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
							}
							if (t.varType[1] == IfThenActionTest.VarType.CustomVar) t.str[1] = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, t.str[1], 0, 55);
						}
					}
					EditorGUILayout.EndVertical();

					if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(18))) del = t;
					GUILayout.Space(16);
				}
				EditorGUILayout.EndHorizontal();
				//GUILayout.Space(5);

				// make sure to be unlinked with Objects as needed
				if (t.varType[0] != IfThenActionTest.VarType.Object) t.obj[0].SetValue(null, null);
				if (t.varType[1] != IfThenActionTest.VarType.Object) t.obj[1].SetValue(null, null);

			}
			if (del != null) action.tests.Remove(del);
		}
		UniRPGEdGui.EndScrollView();

		GUILayout.Space(20);
		GUILayout.Label("Then do this", UniRPGEdGui.Head4Style);

		action.doOption = EditorGUILayout.Popup(action.doOption, DoOptionStrings);
		if (action.doOption == 1)
		{
			action.gotoAction = EditorGUILayout.IntField("Action number: ", action.gotoAction);
			EditorGUILayout.HelpBox("This is the Action number to go to next, counted from the top of the list, starting at (1).", MessageType.Info);
		}
		else
		{
			GUILayout.Space(20);
		}

		GUILayout.Label("Else: Simply execute the Next Action, if any");
	}

	// ================================================================================================================
} }