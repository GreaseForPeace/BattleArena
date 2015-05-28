// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#define UNIRPG_MENU_ENABLED
#if UNIRPG_MENU_ENABLED

using UnityEngine;
using UnityEditor;
using UniRPG;

namespace UniRPGEditor {

public class UniRPGEditorMenu
{
	// order all UniRPG related content into its own menu for quick access by the designer/developer

	[MenuItem("UniRPG/Open Database", false, 1)]
	public static void OpenDatabaseEditor()
	{
		UniRPGEditorGlobal.OpenDatabaseEditor();
	}

	[MenuItem("UniRPG/Run Game", false, 2)]
	public static void RunGame()
	{
		UniRPGEditorGlobal.RunGame();
	}

	[MenuItem("UniRPG/Show Toolbar", false, 3)]
	public static void ShowUniRPGToolbar()
	{
		UniRPGEditorToolbar.ShowToolbar();
	}

	[MenuItem("UniRPG/Create/UniRPG Scene", false, 15)]
	public static void Create_Scene()
	{
		UniRPGEditorGlobal.Create_Scene();
	}

	[MenuItem("UniRPG/Create/Spawn Point", false, 16)]
	public static void Create_SpawnPoint()
	{
		UniRPGEditorGlobal.Create_SpawnPoint();
	}

	[MenuItem("UniRPG/Create/Patrol Path", false, 16)]
	public static void Create_PatrolPath()
	{
		UniRPGEditorGlobal.Create_PatrolPath();
	}

	[MenuItem("UniRPG/Create/Trigger", false, 16)]
	public static void Create_Trigger()
	{
		UniRPGEditorGlobal.Create_Trigger();
	}

	[MenuItem("UniRPG/Help/Documentation", false, 500)]
	public static void UniRPG_Help()
	{
		UniRPGEditorGlobal.ShowUniRPGDocs();
	}

	[MenuItem("UniRPG/Help/About", false, 501)]
	public static void ShowUniRPGAbout()
	{
		UniRPGAbout.ShowAbout();
	}

	[MenuItem("UniRPG/Misc/Insert NewLine &\r")]
	public static void InsertNewLine()
	{
		EditorGUIUtility.systemCopyBuffer = System.Environment.NewLine;
		EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Paste"));
	}

	// ================================================================================================================
} }
#endif
