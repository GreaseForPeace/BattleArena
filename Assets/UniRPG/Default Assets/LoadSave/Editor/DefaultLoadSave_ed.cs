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

[LoadSaveProvider("Default LoadSave", typeof(DefaultLoadSave))]
public class DefaultLoadSave_ed : LoadSaveProviderEdBase
{

	public override void OnGUI(DatabaseEditor ed, LoadSaveProviderBase loadSaveProvider)
	{
		//DefaultLoadSave provider = laodSaveProvider as DefaultLoadSave;
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(350));
		{
			GUILayout.Label("This is the Default UniRPG LoadSave Provider.\nIt makes use of Unity's PlayerPrefs to save game state.");
		}
		EditorGUILayout.EndVertical();
	}
	
	// ================================================================================================================
} }
