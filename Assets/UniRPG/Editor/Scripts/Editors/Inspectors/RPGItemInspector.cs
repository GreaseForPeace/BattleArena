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

[CustomEditor(typeof(RPGItem))]
public class RPGItemInspector : InteractableBaseEditor<RPGItem>
{
	private static readonly string[] useFromBag = { "Trigger OnUse", "Equip" };
	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activeIcon = 0;
	private static bool[] foldout = { true, true };
	private static bool[] foldout2 = { false, false };
	private Vector2 equipScroll = Vector2.zero;

	// ================================================================================================================

	public override void OnInspectorGUI()
	{
		UniRPGEdGui.UseSkin();
		GUILayout.Space(10f);

		BasicInfo();
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		foldout2 = DrawInteractableInspector(foldout2, true, Target.equipWhenUseFromBag, false);

		GUILayout.Space(10f);

		if (GUI.changed)
		{
			GUI.changed = false;
			EditorUtility.SetDirty(Target);
		}
	}

	private void BasicInfo()
	{
		foldout[0] = UniRPGEdGui.Foldout(foldout[0], "Basic Info", UniRPGEdGui.InspectorHeadFoldStyle);
		if (foldout[0])
		{
			// name
			EditorGUILayout.Space();
			Target.screenName = EditorGUILayout.TextField("Screen Name", Target.screenName);
			Target.lootDropPrefab = (GameObject)EditorGUILayout.ObjectField("LootDrop Prefab", Target.lootDropPrefab, typeof(GameObject), false);
			EditorGUILayout.Space();

			// icon, description and notes
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(100));
				{
					GUILayout.Label("Icon");
					EditorGUILayout.BeginHorizontal();
					{
						Target.icon[activeIcon] = (Texture2D)EditorGUILayout.ObjectField(Target.icon[activeIcon], typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
						EditorGUILayout.BeginVertical();
						{
							if (UniRPGEdGui.ToggleButton(activeIcon == 0, "1", EditorStyles.miniButton)) activeIcon = 0;
							if (UniRPGEdGui.ToggleButton(activeIcon == 1, "2", EditorStyles.miniButton)) activeIcon = 1;
							if (UniRPGEdGui.ToggleButton(activeIcon == 2, "3", EditorStyles.miniButton)) activeIcon = 2;
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical();
				{
					EditorGUI.BeginChangeCheck();
					toolbar1Sel = GUILayout.Toolbar(toolbar1Sel, toolbar1);
					if (EditorGUI.EndChangeCheck()) GUI.FocusControl(""); // i need to do this to clear the focus on text fields which are bugging if they stay focused
					if (toolbar1Sel == 0) Target.description = EditorGUILayout.TextArea(Target.description, GUILayout.Height(60), GUILayout.ExpandHeight(false));
					else Target.notes = EditorGUILayout.TextArea(Target.notes, GUILayout.Height(60), GUILayout.ExpandHeight(false));
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();

			// price and stacking
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Price" + (string.IsNullOrEmpty(UniRPGEditorGlobal.DB.currency) ? "" : " (" + UniRPGEditorGlobal.DB.currency + ")"), GUILayout.Width(100));
				GUILayout.Label("Max Stack", GUILayout.Width(100));
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				Target.price = EditorGUILayout.IntField(Target.price, GUILayout.Width(100));
				Target.maxStack = EditorGUILayout.IntField(Target.maxStack, GUILayout.Width(100));
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			Target.autoPickup = EditorGUILayout.Toggle("Allow auto-pickup", Target.autoPickup);

			// Category and Sub-type
			GUILayout.Space(5);
			if (Target.categoryId >= UniRPGEditorGlobal.DB.itemCategories.Count) Target.categoryId = 0;
			if (Target.subTypeId >= UniRPGEditorGlobal.DB.itemCategories[Target.categoryId].types.Count) Target.subTypeId = 0;
			Target.categoryId = EditorGUILayout.Popup("Category", Target.categoryId, UniRPGEditorGlobal.DB.ItemCategoryNames);
			if (UniRPGEditorGlobal.DB.itemCategories[Target.categoryId].types.Count == 0) GUI.enabled = false;
			Target.subTypeId = EditorGUILayout.Popup("Sub-Type", Target.subTypeId, UniRPGEditorGlobal.DB.itemCategories[Target.categoryId].types.ToArray());
			GUI.enabled = true;

			// On use-from-bag
			Target.equipWhenUseFromBag = (1 == EditorGUILayout.Popup("On use-from-bag", (Target.equipWhenUseFromBag ? 1 : 0), useFromBag));
		}

		if (Target.equipWhenUseFromBag)
		{
			EditorGUILayout.Space();
			Target.consumable = false;
			UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
			foldout[1] = UniRPGEdGui.Foldout(foldout[1], "Equip", UniRPGEdGui.InspectorHeadFoldStyle);
			if (foldout[1])
			{
				EditorGUILayout.Space();
				Target.equipTargetMask = (UniRPGGlobal.Target)EditorGUILayout.EnumMaskField("Equip Target(s)", Target.equipTargetMask);
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Target Slots");
					equipScroll = UniRPGEdGui.BeginScrollView(equipScroll, GUILayout.Height(120));
					for (int i = 0; i < UniRPGEditorGlobal.DB.equipSlots.Count; i++)
					{
						if (UniRPGEdGui.ToggleButton(Target.validEquipSlots.Contains(i), UniRPGEditorGlobal.DB.equipSlots[i], EditorStyles.miniButton, UniRPGEdGui.ButtonOnColor))
						{
							if (Target.validEquipSlots.Contains(i)) Target.validEquipSlots.Remove(i);
							else Target.validEquipSlots.Add(i);
						}
					}
					UniRPGEdGui.EndScrollView();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}
		}
		else
		{
			Target.consumable = EditorGUILayout.Toggle("Consumable", Target.consumable);
		}
	}

	// ================================================================================================================
} }