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

[InitializeOnLoad]
public class RegUniRPGDefaultButtons
{
	static RegUniRPGDefaultButtons()
	{
		UniRPGEditorGlobal.AddToolbarButtons(new System.Collections.Generic.List<UniRPGEditorToolbar.ToolbarButton>()
		{
			new UniRPGEditorToolbar.ToolbarButton() { order = 1, callback = UniRPGEditorGlobal.OpenDatabaseEditor, gui = new GUIContent(null, null, "UniRPG Editor"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/db" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },
			new UniRPGEditorToolbar.ToolbarButton() { order = 2, callback = UniRPGEditorGlobal.RunGame, gui = new GUIContent(null, null, "Play"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/play" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },
			new UniRPGEditorToolbar.ToolbarButton() { order = 3, callback = UniRPGEditorGlobal.RefreshAll, gui = new GUIContent(null, null, "Refresh Cache"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/refresh" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },
			new UniRPGEditorToolbar.ToolbarButton() { order = 100, callback = null, gui = null },
			new UniRPGEditorToolbar.ToolbarButton() { order = 101, callback = UniRPGEditorGlobal.Create_Scene, gui = new GUIContent(null, null, "Create UniRPG Scene"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/scene" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },
			new UniRPGEditorToolbar.ToolbarButton() { order = 102, callback = UniRPGEditorGlobal.Create_SpawnPoint, gui = new GUIContent(null, null, "Create SpawnPoint"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/spawn" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },
			new UniRPGEditorToolbar.ToolbarButton() { order = 103, callback = UniRPGEditorGlobal.Create_PatrolPath, gui = new GUIContent(null, null, "Create PatrolPath"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/patrol" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },
			new UniRPGEditorToolbar.ToolbarButton() { order = 104, callback = UniRPGEditorGlobal.Create_Trigger, gui = new GUIContent(null, null, "Create Trigger"), iconPath = UniRPGEdGui.EditorResourcePath + "Toolbar/trigger" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" },

		});
	}
}

public class UniRPGEditorToolbar : EditorWindow
{
	public class ToolbarButton
	{
		public int order = 0;
		public GUIContent gui;
		public Callback callback;
		public string iconPath = null;

		public delegate void Callback();
	}

	private static GUIStyle buttonStyle = null;
	private static GUIStyle foldStyleH = null;
	private static GUIStyle foldStyleV = null;

	private static bool[] foldout=null;
	private int option = -1;

	// ================================================================================================================

	[MenuItem("Window/UniRPG/Toolbar", false, 2)]
	public static void ShowToolbar()
	{
		UniRPGEditorToolbar win = EditorWindow.GetWindow<UniRPGEditorToolbar>(false, "UniRPG", true);
		win.inited = false;
		win.Show();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "UniRPG";
		minSize = new Vector2(41f, 41f);
	}

	private static void LoadResources()
	{
		if (foldStyleH != null) return;

		GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
		buttonStyle = new GUIStyle(skin.button)
		{
			name = "UniRPGEdToolbarButton",
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 0)
		};

		Texture2D foldinH = UniRPGEdGui.LoadEditorTexture(UniRPGEdGui.EditorResourcePath + "Toolbar/foldin_h" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture2D foldoutH = UniRPGEdGui.LoadEditorTexture(UniRPGEdGui.EditorResourcePath + "Toolbar/foldout_h" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture2D foldinV = UniRPGEdGui.LoadEditorTexture(UniRPGEdGui.EditorResourcePath + "Toolbar/foldin_v" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture2D foldoutV = UniRPGEdGui.LoadEditorTexture(UniRPGEdGui.EditorResourcePath + "Toolbar/foldout_v" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");

		foldStyleH = new GUIStyle(EditorStyles.foldout)
		{
			name = "UniRPGEdToolbarFoldoutH",
			padding = new RectOffset(7, 0, 40, 0),
			margin = new RectOffset(2, 4, 2, 4),
			border = new RectOffset(7, 0, 36, 0),
			fixedWidth = 7,
			fixedHeight = 35,
			normal = { background = foldinH },
			active = { background = foldinH },
			focused = { background = foldinH },
			onNormal = { background = foldoutH },
			onActive = { background = foldoutH },
			onFocused = { background = foldoutH }
		};
		foldStyleV = new GUIStyle(EditorStyles.foldout)
		{
			name = "UniRPGEdToolbarFoldoutV",
			padding = new RectOffset(40, 0, 7, 0),
			margin = new RectOffset(2, 4, 2, 6),
			border = new RectOffset(36, 0, 7, 0),
			fixedWidth = 35,
			fixedHeight = 7,
			normal = { background = foldinV },
			active = { background = foldinV },
			focused = { background = foldinV },
			onNormal = { background = foldoutV },
			onActive = { background = foldoutV },
			onFocused = { background = foldoutV }
		};

		// load the icons of the toolbuttons
		for (int i = 0; i < UniRPGEditorGlobal.ToolbarButtons.Count; i++)
		{
			if (UniRPGEditorGlobal.ToolbarButtons[i].gui != null && UniRPGEditorGlobal.ToolbarButtons[i].iconPath != null)
			{
				UniRPGEditorGlobal.ToolbarButtons[i].gui.image = UniRPGEdGui.LoadEditorTexture(UniRPGEditorGlobal.ToolbarButtons[i].iconPath);
			}
		}
	}

	void OnGUI()
	{
		if (!inited) Init();
		LoadResources();
		EditorGUILayout.Space();

		option = UniRPGEdGui.ButtonToolBar(this, UniRPGEditorGlobal.ToolbarButtons, ref foldout, buttonStyle, foldStyleH, foldStyleV, 40f);
		if (option >= 0)
		{
			if (UniRPGEditorGlobal.ToolbarButtons[option].callback != null)
			{
				UniRPGEditorGlobal.ToolbarButtons[option].callback();
				GUIUtility.ExitGUI();
			}
		}
	}

	// ================================================================================================================
} }