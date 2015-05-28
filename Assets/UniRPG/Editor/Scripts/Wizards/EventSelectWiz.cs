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

public class EventSelectWiz : EditorWindow
{
	public RPGEvent selectedEvent = null;
	public int helper = 0; // used as needed by whatever called .Show()

	public bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;
	private Vector2 scroll = Vector2.zero;
	private UniRPGBasicEventHandler OnAccept = null;
	private UniRPGArgsEventHandler OnAccept2 = null;
	private object[] args = null;

	// ================================================================================================================

	public static void Show(UniRPGBasicEventHandler onAccept, RPGEvent selectedEvent, int helper)
	{
		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		EventSelectWiz window = EditorWindow.GetWindow<EventSelectWiz>(true, "Select Event",true);
		window.inited = false;
		window.selectedEvent = selectedEvent;
		window.helper = helper;
		window.OnAccept = onAccept;

		// show window
		window.ShowUtility();
	}

	public static void Show(UniRPGArgsEventHandler onAccept, RPGEvent selectedEvent, object[] args)
	{
		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		EventSelectWiz window = EditorWindow.GetWindow<EventSelectWiz>(true, "Select Event", true);
		window.inited = false;
		window.selectedEvent = selectedEvent;
		window.args = args;
		window.OnAccept2 = onAccept;

		// show window
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Event";
		minSize = new Vector2(200, 350);
		maxSize = new Vector2(200, 350);
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
		if (!inited) Init();
		UniRPGEdGui.UseSkin();
		scroll = UniRPGEdGui.BeginScrollView(scroll);
		{
			if (UniRPGEditorGlobal.DB.RPGEvents.Length > 0)
			{
				for (int i = 0; i < UniRPGEditorGlobal.DB.RPGEvents.Length; i++)
				{
					Rect r = EditorGUILayout.BeginHorizontal();
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (UniRPGEditorGlobal.DB.RPGEvents[i].icon[0] != null ? UniRPGEditorGlobal.DB.RPGEvents[i].icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(selectedEvent == UniRPGEditorGlobal.DB.RPGEvents[i], UniRPGEditorGlobal.DB.RPGEvents[i].screenName, UniRPGEdGui.ButtonRightStyle, UniRPGEdGui.ButtonOnColor, GUILayout.Width(150)))
						{
							selectedEvent = UniRPGEditorGlobal.DB.RPGEvents[i];
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Events are defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();

			if (selectedEvent == null) GUI.enabled = false;
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