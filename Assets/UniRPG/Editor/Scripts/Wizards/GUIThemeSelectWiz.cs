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

public class GUIThemeSelectWiz : EditorWindow
{
	public int selIdx = 0;

	public bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;

	private Vector2 scroll = Vector2.zero;

	private UniRPGBasicEventHandler OnAccept = null;

	// ================================================================================================================

	public static void Show(UniRPGBasicEventHandler onAccept, string current)
	{
		if (UniRPGEditorGlobal.GUIEditors.Length == 0) return;
		GUIThemeSelectWiz window = EditorWindow.GetWindow<GUIThemeSelectWiz>(true, "Select GUI Theme",true);
		window.inited = false;
		window.OnAccept = onAccept;
		window.selIdx = 0;
		for (int i = 0; i < UniRPGEditorGlobal.GUIEditors.Length; i++)
		{
			if (UniRPGEditorGlobal.GUIEditors[i].name.Equals(current)) { window.selIdx = i; break; }
		}
		
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select GUI Theme";
		minSize = new Vector2(200, 350);
		maxSize = new Vector2(200, 350);
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) this.Close();
		if (accepted && OnAccept != null) OnAccept(this);
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();
		scroll = UniRPGEdGui.BeginScrollView(scroll);
		{
			for (int i= 0; i < UniRPGEditorGlobal.GUIEditors.Length; i++)
			{
				if (UniRPGEdGui.ToggleButton(selIdx == i, UniRPGEditorGlobal.GUIEditors[i].name, UniRPGEdGui.ButtonStyle, UniRPGEdGui.ButtonOnColor, GUILayout.Width(170))) selIdx = i;
			}
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Accept", UniRPGEdGui.ButtonStyle)) accepted = true;
			if (GUILayout.Button("Cancel", UniRPGEdGui.ButtonStyle)) this.Close();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);
	}

	// ================================================================================================================
} }