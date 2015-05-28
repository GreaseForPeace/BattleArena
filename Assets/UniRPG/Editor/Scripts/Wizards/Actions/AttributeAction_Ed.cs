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

[ActionInfo(typeof(AttributeAction), "Actor/Attribute", Description = "Change the value of an attribute")]
public class ChangeAttribValueAction_Ed : ActionsEdBase
{
	private int selAttrib = -1;
	private static readonly string[] partStrings = { "Value", "Bonus", "Min", "Max" };
	private static readonly string[] doWhatStrings = { "Subtract from", "Add to", "Set to" };
	private static readonly string[] percentStrings = { "Current Value", "Max Value" };
	private Vector2 scroll = Vector2.zero;

	private static readonly string[] paramStrings = { "None", "Numeric", "The Value", "Level (1)", "Attribute (1)", "CustomVar (1)", "Level (2)", "Attribute (2)", "CustomVar (2)" };
	private static readonly string[] valDoWhatStrings = { "plus", "min", "div", "mul", "mod" };

	public override string ActionShortNfo(Object actionObj)
	{
		AttributeAction action = actionObj as AttributeAction;
		if (action == null) return "!ERROR!";
		RPGAttribute a = RPGAttribute.GetAttribByGuid(UniRPGEditorGlobal.DB.attributes, action.attribIdent);
		return string.Format("Change {0} on {1}", (a == null ? "!ERROR!" : a.screenName), action.subject.type);
	}

	public override void OnGUI(Object actionObj)
	{
		AttributeAction action = actionObj as AttributeAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }
		scroll = UniRPGEdGui.BeginScrollView(scroll, GUILayout.Height(280));

		selAttrib = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, action.attribIdent);

		UniRPGEdGui.TargetTypeField(this.ed, "(1) Subject", action.subject, TargetTypeHelp, 85);
		EditorGUILayout.Space();
		UniRPGEdGui.TargetTypeField(this.ed, "(2) Aggressor", action.aggressor, TargetTypeHelp, 85);
		EditorGUILayout.Space();

		EditorGUIUtility.LookLikeControls(100);
		action.doWhat = EditorGUILayout.Popup("Main Action", action.doWhat, doWhatStrings, GUILayout.Width(220));
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("Attribute of (1)", GUILayout.Width(95));
			EditorGUI.BeginChangeCheck();
			selAttrib = EditorGUILayout.Popup(selAttrib, UniRPGEditorGlobal.DB.AttributeNames, EditorStyles.miniButtonLeft, GUILayout.Width(120));
			if (EditorGUI.EndChangeCheck()) action.attribIdent.Value = UniRPGEditorGlobal.DB.attributes[selAttrib].id.Value;
			action.affectedPart = EditorGUILayout.Popup(action.affectedPart, partStrings, EditorStyles.miniButtonRight, GUILayout.Width(60));
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(GUI.skin.box);
		{
			EditorGUILayout.BeginHorizontal();
			{
				action.val = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, action.val, 0, 80);
				GUILayout.Label(" - ");
				action.useRandomRange = EditorGUILayout.Toggle(action.useRandomRange, GUILayout.Width(20));
				if (!action.useRandomRange) GUILayout.Label("use random range", GUILayout.Width(120));
				else action.val2 = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, action.val2, 0, 85);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(5);
				action.usePercentage = EditorGUILayout.Toggle(action.usePercentage, GUILayout.Width(20));
				GUILayout.Label("values are %percent of", UniRPGEdGui.BoldLabelStyle, GUILayout.Width(160));
				action.percentBase = EditorGUILayout.Popup(action.percentBase, percentStrings, GUILayout.Width(90));
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical(GUI.skin.box);
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Modifiers");
				EditorGUILayout.Space();
				if (GUILayout.Button("add", EditorStyles.miniButton)) action.valueMod.Add(new ValueModifier());
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

			ValueModifier del = null;
			foreach (ValueModifier vm in action.valueMod)
			{
				EditorGUILayout.BeginHorizontal();
				{
					vm.resultToValue = EditorGUILayout.Popup(vm.resultToValue, valDoWhatStrings, EditorStyles.miniButtonLeft, GUILayout.Width(40));
					EditorGUILayout.BeginVertical();
					{
						vm.param[0] = EditorGUILayout.Popup(vm.param[0], paramStrings, EditorStyles.miniButtonMid, GUILayout.Width(90));
						if (vm.param[0] == 1) // variable
						{
							vm.numVal[0] = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, vm.numVal[0], 0, 50);
						}
						else if (vm.param[0] == 4 || vm.param[0] == 7) // attribute
						{
							int attIdx = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, vm.attribId[0]);
							if (attIdx >= 0) vm.attribId[0].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
							EditorGUI.BeginChangeCheck();
							attIdx = EditorGUILayout.Popup(attIdx, UniRPGEditorGlobal.DB.AttributeNames, GUILayout.Width(80));
							if (EditorGUI.EndChangeCheck()) vm.attribId[0].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
						}
						else if (vm.param[0] == 5 || vm.param[0] == 8) // custom var
						{
							vm.customVar[0] = EditorGUILayout.TextField(vm.customVar[0], GUILayout.Width(80));
						}
					}
					EditorGUILayout.EndVertical();
					vm.doWhatToParams = EditorGUILayout.Popup(vm.doWhatToParams, valDoWhatStrings, EditorStyles.miniButtonMid, GUILayout.Width(40));
					EditorGUILayout.BeginVertical();
					{
						vm.param[1] = EditorGUILayout.Popup(vm.param[1], paramStrings, EditorStyles.miniButtonMid, GUILayout.Width(90));
						if (vm.param[1] == 1) // variable
						{
							vm.numVal[1] = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, null, vm.numVal[1], 0, 50);
						}
						else if (vm.param[1] == 4 || vm.param[1] == 7) // attribute
						{
							int attIdx = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, vm.attribId[1]);
							if (attIdx >= 0) vm.attribId[1].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
							EditorGUI.BeginChangeCheck();
							attIdx = EditorGUILayout.Popup(attIdx, UniRPGEditorGlobal.DB.AttributeNames, GUILayout.Width(80));
							if (EditorGUI.EndChangeCheck()) vm.attribId[1].Value = UniRPGEditorGlobal.DB.attributes[attIdx].id.Value;
						}
						else if (vm.param[1] == 5 || vm.param[1] == 8) // custom var
						{
							vm.customVar[1] = EditorGUILayout.TextField(vm.customVar[1], GUILayout.Width(80));
						}
					}
					EditorGUILayout.EndVertical();

					if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(18))) del = vm;
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}
			if (del != null) action.valueMod.Remove(del);

		}
		EditorGUILayout.EndVertical();

		UniRPGEdGui.EndScrollView();
	}

	// ================================================================================================================
} }