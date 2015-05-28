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

[ActionInfo(typeof(CurrencyAction), "Actor/Currency: Change", Description = "Set, Add, or Remove Currency")]
public class CurrencyAction_Ed : ActionsEdBase
{
	private static readonly string[] DoWhatOptions = { "Set Currency to", "Add to Currency", "Subtract from Currency" };

	public override string ActionShortNfo(Object actionObj)
	{
		CurrencyAction action = actionObj as CurrencyAction;
		if (action == null) return "!ERROR!";
		return string.Format("{0} {1}, on {2}", DoWhatOptions[action.doWhat], action.amount.GetValOrName(), action.subject.type);
	}
	
	public override void OnGUI(Object actionObj)
	{
		CurrencyAction action = actionObj as CurrencyAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();
		action.doWhat = EditorGUILayout.Popup(action.doWhat, DoWhatOptions);
		EditorGUILayout.Space();
		action.amount = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, " ", action.amount, 100);
	}

	// ================================================================================================================
} }
