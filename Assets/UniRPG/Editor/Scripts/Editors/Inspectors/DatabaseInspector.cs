// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using UniRPG;

namespace UniRPGEditor {

[CustomEditor(typeof(Database))]
public class DatabaseInspector : InspectorBase<Database>
{
	public override void OnInspectorGUI()
	{
		UniRPGEdGui.UseSkin();
		GUILayout.Space(15f);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Open Database", GUILayout.MinHeight(30f), GUILayout.MaxWidth(230f))) DatabaseEditor.ShowEditor();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(15f);
	}

	// ================================================================================================================
} }