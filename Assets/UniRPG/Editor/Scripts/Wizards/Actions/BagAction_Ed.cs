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

[ActionInfo(typeof(BagAction), "Actor/Bag: Add or Remove Item", Description = "Add or Remove Bag Item")]
public class BagAction_Ed : ActionsEdBase
{
	private static readonly string[] DoWhatOptions = { "Add Item from EquipSlot", "Add Specified Item", "Remove an Item from Slot" };
	private static readonly string[] EquipSlotOptions = { "Named Equip Slot", "Equip Slot Number" };

	public override string ActionShortNfo(Object actionObj)
	{
		BagAction action = actionObj as BagAction;
		if (action == null) return "!ERROR!";
		if (action.doWhat == 0)
		{
			int slot =(int)action.equipSlotId.GetValue(null);
			if (slot < 0 || slot >= UniRPGEditorGlobal.DB.equipSlots.Count) return "!ERROR!";
			else return string.Format("Bag, Add Item, from {0}", UniRPGEditorGlobal.DB.equipSlots[slot]);
		}
		else if (action.doWhat == 1) return string.Format("Bag, Add Item: {0}", action.specifiedItem == null ? "!ERROR!" : action.specifiedItem.screenName);
		else if (action.doWhat == 2) return string.Format("Bag, Remove Item, Slot {0}", action.equipSlotId.GetValOrName());
		return "!ERROR!";
	}

	public override void OnGUI(Object actionObj)
	{
		BagAction action = actionObj as BagAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(45);
		action.doWhat = EditorGUILayout.Popup(" ", action.doWhat, DoWhatOptions);
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(GUI.skin.box);
		if (action.doWhat == 0)
		{
			action.specifiedItem = null; // just to be sure one is not referenced when the option is not used
			EditorGUIUtility.LookLikeControls(100);
			action.equipSlotOption = EditorGUILayout.Popup("From", action.equipSlotOption, EquipSlotOptions);
			if (action.equipSlotOption == 0) action.equipSlotId.SetAsValue = EditorGUILayout.Popup(" ", (int)action.equipSlotId.GetValue(null), UniRPGEditorGlobal.DB.equipSlots.ToArray());
			else action.equipSlotId = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, " ", action.equipSlotId);
			EditorGUILayout.Space();
		}
		else if (action.doWhat == 1)
		{
			EditorGUIUtility.LookLikeControls();
			if (UniRPGEdGui.LabelButton("Item", action.specifiedItem == null ? "-select-" : action.specifiedItem.screenName, 120, 160)) ItemSelectWiz.Show(false, OnRPGItemSelected, new object[] {actionObj});
			EditorGUILayout.Space();
			action.numberOfItems = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Number of copies", action.numberOfItems, 120);
			EditorGUILayout.Space();
		}
		else if (action.doWhat == 2)
		{
			action.specifiedItem = null; // just to be sure one is not referenced when the option is not used
			EditorGUIUtility.LookLikeControls();
			action.bagSlotId = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "From Bag Slot", action.bagSlotId, 120);
			action.numberOfItems = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Number of copies", action.numberOfItems, 120);
			EditorGUILayout.Space();
		}
		
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();
		action.exitWhenFail = GUILayout.Toggle(action.exitWhenFail, " Exit Action Queue if failed to " + (action.doWhat == 2? "remove item":"add item"));
	}


	private void OnRPGItemSelected(object sender, object[] args)
	{
		ItemSelectWiz wiz = sender as ItemSelectWiz;
		BagAction action = args[0] as BagAction;
		if (action != null) action.specifiedItem = wiz.selectedItems[0];
		wiz.Close();
	}

	// ================================================================================================================
} }