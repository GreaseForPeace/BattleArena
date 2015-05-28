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

[DatabaseEditor("GUI", Priority = 1)]
public class GUIEditor : DatabaseEdBase
{

	// ================================================================================================================

	public static bool CheckSelectedTheme(Database db)
	{
		if (UniRPGEditorGlobal.GUIEditors.Length == 0) return false;
		if (UniRPGEditorGlobal.activeGUIThemeIdx < 0 || UniRPGEditorGlobal.activeGUIThemeIdx >= UniRPGEditorGlobal.GUIEditors.Length)
		{
			UniRPGEditorGlobal.activeGUIThemeIdx = -1;
			for (int i = 0; i < UniRPGEditorGlobal.GUIEditors.Length; i++)
			{
				if (UniRPGEditorGlobal.GUIEditors[i].name.Equals(db.activeGUITheme))
				{
					UniRPGEditorGlobal.activeGUIThemeIdx = i;
					UniRPGEditorGlobal.LoadInputsFromBinder(UniRPGEditorGlobal.GUIEditors[UniRPGEditorGlobal.activeGUIThemeIdx].editor.GetInputBinder(db.menuGUIData, db.gameGUIData));
					break;
				}
			}

			if (UniRPGEditorGlobal.activeGUIThemeIdx < 0)
			{
				UniRPGEditorGlobal.activeGUIThemeIdx = 0;
				db.activeGUITheme = UniRPGEditorGlobal.GUIEditors[UniRPGEditorGlobal.activeGUIThemeIdx].name;
				UniRPGEditorGlobal.InitGUIThemeData(db, UniRPGEditorGlobal.activeGUIThemeIdx);
				UniRPGEditorGlobal.LoadInputsFromBinder(UniRPGEditorGlobal.GUIEditors[UniRPGEditorGlobal.activeGUIThemeIdx].editor.GetInputBinder(db.menuGUIData, db.gameGUIData));
				EditorUtility.SetDirty(db);
			}
		}
		return true;
	}

	public override void OnEnable(DatabaseEditor ed)
	{
		base.OnEnable(ed);
		if (!CheckSelectedTheme(ed.db)) return;
		UniRPGEditorGlobal.GUIEditors[UniRPGEditorGlobal.activeGUIThemeIdx].editor.OnEnable(ed.db.menuGUIData, ed.db.gameGUIData);
	}

	public override void Update(DatabaseEditor ed)
	{
		base.Update(ed);
		if (!CheckSelectedTheme(ed.db)) return;
		UniRPGEditorGlobal.GUIEditors[UniRPGEditorGlobal.activeGUIThemeIdx].editor.Update(ed.db.menuGUIData, ed.db.gameGUIData);
	}

	public override void OnGUI(DatabaseEditor ed)
	{
		base.OnGUI(ed);
		if (!CheckSelectedTheme(ed.db)) return;
		UniRPGEditorGlobal.GUIEditors[UniRPGEditorGlobal.activeGUIThemeIdx].editor.OnGUI(ed, ed.db.menuGUIData, ed.db.gameGUIData);
	}

	// ================================================================================================================
} }