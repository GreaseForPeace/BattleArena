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

public class AttributeSelectWiz : EditorWindow
{
	public RPGAttribute selectedAttrib = null;

	private bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;

	private Vector2 scroll = Vector2.zero;
	private UniRPGArgsEventHandler OnAccept = null;
	private object[] args = null;

	// ================================================================================================================

	public static void Show(UniRPGArgsEventHandler onAccept, object[] args)
	{
		if (!UniRPGEditorGlobal.LoadDatabase(true)) return;  // make sure DB is loaded
		AttributeSelectWiz window = EditorWindow.GetWindow<AttributeSelectWiz>(true, "Select Attribute",true);
		window.inited = false;
		window.OnAccept = onAccept;
		window.args = args;
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Attribute";
		minSize = new Vector2(200, 350);
		maxSize = new Vector2(200, 350);
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) this.Close();
		if (accepted && OnAccept != null) OnAccept(this, args);
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();

		scroll = UniRPGEdGui.BeginScrollView(scroll);
		{
			if (UniRPGEditorGlobal.DB.attributes.Count > 0)
			{
				foreach (RPGAttribute cl in UniRPGEditorGlobal.DB.attributes)
				{
					Rect r = EditorGUILayout.BeginHorizontal();
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (cl.icon[0] != null ? cl.icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(selectedAttrib == cl, cl.screenName, UniRPGEdGui.ButtonRightStyle, UniRPGEdGui.ButtonOnColor, GUILayout.Width(150)))
						{
							selectedAttrib = cl;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Attributes are defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();

			if (selectedAttrib == null) GUI.enabled = false;
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