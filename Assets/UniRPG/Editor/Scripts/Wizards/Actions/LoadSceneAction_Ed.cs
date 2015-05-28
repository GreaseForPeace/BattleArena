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

[ActionInfo(typeof(LoadSceneAction), "System/Load Scene", Description = "Load a scene/map")]
public class LoadSceneAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Game Map", "Main Menu", "by Name" };

	public override string ActionShortNfo(Object actionObj)
	{
		LoadSceneAction action = actionObj as LoadSceneAction;
		if (action == null) return "!ERROR!";
		if (action.loadBy == 0) return string.Format("Load Scene: {0}", action.gameSceneIdx >= UniRPGEditorGlobal.DB.gameSceneNames.Count ? "!ERROR!" : UniRPGEditorGlobal.DB.gameSceneNames[action.gameSceneIdx]);
		else if (action.loadBy==1) return "Exit to main menu";
		return string.Format("Load Scene: {0}", action.sceneName.GetValOrName());
	}

	public override void OnGUI(Object actionObj)
	{
		LoadSceneAction action = actionObj as LoadSceneAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("Load");
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			{
				action.loadBy = EditorGUILayout.Popup(action.loadBy, options);
				if (action.loadBy == 0)
				{
					action.gameSceneIdx = EditorGUILayout.Popup(action.gameSceneIdx, UniRPGEditorGlobal.DB.gameSceneNames.ToArray());
					EditorGUIUtility.LookLikeControls();
					action.num = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "SpawnPoint Ident", action.num, 120);
					EditorGUILayout.Space();
					EditorGUILayout.HelpBox("The 'SpawnPoint Ident' is the unique identifier number you entered for the SpawnPoint. Leave this at (-1) if you want UniRPG to use the first available Player Spawn point or use the world center.", MessageType.Info);
				}
				else if (action.loadBy == 1)
				{
					// no options
				}
				else if (action.loadBy == 2)
				{
					action.sceneName = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, null, action.sceneName);
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();
	}

	// ================================================================================================================
} }