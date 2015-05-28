// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

[ActionInfo(typeof(CreateLootAction), "System/Loot: Create", Description = "Spawn loot")]
public class CreateLootAction_Ed : ActionsEdBase
{
	private string cachedName = null;
	private CreateLootAction action = null;

	public override string ActionShortNfo(Object actionObj)
	{
		return "Spawn Loot";
	}

	public override void OnGUI(Object actionObj)
	{
		action = actionObj as CreateLootAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		if (cachedName == null) GetLootName(action);

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Loot");
		if (GUILayout.Button(cachedName, GUILayout.Width(240))) LootSelectWiz.Show(OnLootSelected);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		EditorGUILayout.HelpBox("If the Subject is a character (Actor) then the Level of that Actor will be send to the Loot Table. For any other object the Level send to the Loot Table will be set as (0). This only matters if your Loot Table checks 'NPC Level'.", MessageType.Info);
	}

	private void OnLootSelected(System.Object sender)
	{
		LootSelectWiz wiz = sender as LootSelectWiz;
		if (action != null)
		{
			action.lootId = wiz.loot.id;
			cachedName = wiz.loot.screenName;
		}
		wiz.Close();
		ed.Repaint();
	}

	private void GetLootName(CreateLootAction action)
	{
		cachedName = "-";
		RPGLoot loot = UniRPGEditorGlobal.DB.GetLoot(action.lootId);
		if (loot != null) cachedName = loot.screenName;
	}

	// ================================================================================================================
} }
