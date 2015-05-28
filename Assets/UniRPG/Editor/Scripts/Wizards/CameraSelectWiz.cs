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

public class CameraSelectWiz : EditorWindow
{
	public UniRPGCameraEdInfo selected = null;

	public bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;

	private Vector2 scroll = Vector2.zero;
	private UniRPGBasicEventHandler OnAccept = null;

	// ================================================================================================================

	public static void Show(UniRPGBasicEventHandler onAccept)
	{
		CameraSelectWiz window = EditorWindow.GetWindow<CameraSelectWiz>(true, "Select Camera",true);
		window.inited = false;
		window.OnAccept = onAccept;
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Camera";
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
			if (UniRPGEditorGlobal.CameraEditors.Length > 0)
			{
				foreach (UniRPGCameraEdInfo cam in UniRPGEditorGlobal.CameraEditors)
				{
					if (UniRPGEdGui.ToggleButton(selected == cam, cam.name, UniRPGEdGui.ButtonStyle, UniRPGEdGui.ButtonOnColor, GUILayout.Width(180)))
					{
						selected = cam;
					}
				}
			}
			else
			{
				GUILayout.Label("No Camera plugins found", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();

			if (selected == null) GUI.enabled = false;
			if (GUILayout.Button("Accept", UniRPGEdGui.ButtonStyle)) accepted = true;
			GUI.enabled = true;

			if (GUILayout.Button("Cancel", UniRPGEdGui.ButtonStyle)) this.Close();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);
	}

	// ================================================================================================================
} }