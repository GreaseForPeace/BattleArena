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

public class TextInputWiz : EditorWindow
{
	public string text = string.Empty;

	public bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;

	private UniRPGBasicEventHandler OnAccept = null;
	private UniRPGArgsEventHandler OnAccept2 = null;
	private string label = string.Empty;
	private object[] args = null;

	// ================================================================================================================

	public static void Show(string title, string label, string currText, UniRPGBasicEventHandler onAccept)
	{
		TextInputWiz window = EditorWindow.GetWindow<TextInputWiz>(true, title, true);
		window.inited = false;
		window.text = currText;
		window.label = label;
		window.OnAccept = onAccept;
		window.ShowUtility();
	}

	public static void Show(string title, string label, string currText, UniRPGArgsEventHandler onAccept, object[] args)
	{
		TextInputWiz window = EditorWindow.GetWindow<TextInputWiz>(true, title, true);
		window.inited = false;
		window.text = currText;
		window.label = label;
		window.OnAccept2 = onAccept;
		window.args = args;
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		minSize = new Vector2(250, 120);
		maxSize = new Vector2(250, 120);
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) this.Close();
		if (accepted && OnAccept != null) OnAccept(this);
		if (accepted && OnAccept2 != null) OnAccept2(this, args);
	}

	void OnGUI()
	{
		if (inited) Init();
		UniRPGEdGui.UseSkin();
		EditorGUILayout.Space();

		GUILayout.Label(label);
		text = EditorGUILayout.TextField(text);

		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 10, 10);

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