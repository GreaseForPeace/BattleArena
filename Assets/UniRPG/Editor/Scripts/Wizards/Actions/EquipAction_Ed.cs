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

[ActionInfo(typeof(EquipAction), "Actor/Equip Slot", Description = "Add or Remove Item on Equip Slot")]
public class EquipAction_Ed : ActionsEdBase
{
	private static readonly string[] DoWhatOptions = { "Equip Item from Bag", "Equip Specified Item", "UnEquip Item from Slot" };
	private static readonly string[] EquipSlotOptions = { "Named Equip Slot", "Equip Slot Number" };

	public override string ActionShortNfo(Object actionObj)
	{
		EquipAction action = actionObj as EquipAction;
		if (action == null) return "!ERROR!";
		int slot = (int)action.equipSlotId.GetValue(null);
		if (slot < 0 || slot >= UniRPGEditorGlobal.DB.equipSlots.Count) return "!ERROR!";
		if (action.doWhat == 0) return string.Format("Add Item from bag to {0}", UniRPGEditorGlobal.DB.equipSlots[slot]);
		else if (action.doWhat == 1) return string.Format("Add {0} to {1}", action.specifiedItem == null ? "!ERROR!" : action.specifiedItem.screenName, UniRPGEditorGlobal.DB.equipSlots[slot]);
		else if (action.doWhat == 2) return string.Format("Remove item from {0}", UniRPGEditorGlobal.DB.equipSlots[slot]);
		return "!ERROR!";
	}

	public override void OnGUI(Object actionObj)
	{
		EquipAction action = actionObj as EquipAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(60);
		action.doWhat = EditorGUILayout.Popup(" ", action.doWhat, DoWhatOptions);
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(GUI.skin.box);
		action.equipSlotOption = EditorGUILayout.Popup(action.doWhat == 2 ? "From" : "To", action.equipSlotOption, EquipSlotOptions);
		if (action.equipSlotOption == 0) action.equipSlotId.SetAsValue = EditorGUILayout.Popup(" ", (int)action.equipSlotId.GetValue(null), UniRPGEditorGlobal.DB.equipSlots.ToArray());
		else action.equipSlotId = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, " ", action.equipSlotId);
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();

		if (action.doWhat == 0)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			action.specifiedItem = null; // just to be sure one is not referenced when the option is not used
			EditorGUIUtility.LookLikeControls();
			action.bagSlotId = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "From Bag Slot", action.bagSlotId, 120);
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
		}
		else if (action.doWhat == 1)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			EditorGUIUtility.LookLikeControls();
			if (UniRPGEdGui.LabelButton("Item", action.specifiedItem == null ? "-select-" : action.specifiedItem.screenName, 120, 160)) ItemSelectWiz.Show(false, OnRPGItemSelected, new object[] {actionObj});
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
		}
		else if (action.doWhat == 2)
		{
			action.specifiedItem = null; // just to be sure one is not referenced when the option is not used
		}

		EditorGUILayout.Space();
		action.exitWhenFail = GUILayout.Toggle(action.exitWhenFail, " Exit Action Queue if failed to " + (action.doWhat == 2 ? "remove item" : "add item"));
	}


	private void OnRPGItemSelected(object sender, object[] args)
	{
		ItemSelectWiz wiz = sender as ItemSelectWiz;
		EquipAction action = args[0] as EquipAction;
		if (action != null) action.specifiedItem = wiz.selectedItems[0];
		wiz.Close();
	}

	// ================================================================================================================
} }