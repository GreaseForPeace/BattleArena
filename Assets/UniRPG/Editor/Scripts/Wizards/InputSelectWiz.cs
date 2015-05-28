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

public class InputSelectWiz : EditorWindow
{
	private InputDefinition def;
	private bool forPrimary;

	private bool lostFocus = false;
	private Vector2 scroll = Vector2.zero;

	private string[] keys;
	private int sel = -1;
	private int prev = -1;
	
	// ================================================================================================================

	public static void Show(InputDefinition def, bool forPrimary)
	{
		InputSelectWiz window = EditorWindow.GetWindow<InputSelectWiz>(true, "Select Button",true);
		window.inited = false;
		window.def = def;
		window.forPrimary = forPrimary;

		window.keys = System.Enum.GetNames(typeof(KeyCode));
		string s = (forPrimary ? def.primaryButton.ToString() : def.secondaryButton.ToString());
		for (int i = 0; i < window.keys.Length; i++)
		{
			if (window.keys[i].Equals(s))
			{
				window.sel = window.prev = i;
				break;
			}
		}
		
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Button";
		minSize = new Vector2(800, 500);
		maxSize = new Vector2(800, 500);
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) this.Close();
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();
		scroll = GUILayout.BeginScrollView(scroll);
		{
			sel = GUILayout.SelectionGrid(sel, keys, 6);
			if (sel != prev)
			{
				lostFocus = true;
				System.Array arr = System.Enum.GetValues(typeof(KeyCode));
				foreach (KeyCode k in arr)
				{
					if (k.ToString().Equals(keys[sel]))
					{
						if (forPrimary) def.primaryButton = k;
						else def.secondaryButton = k;
						break;
					}
				}
			}
		}
		GUILayout.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", UniRPGEdGui.ButtonStyle)) this.Close();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);
	}

	// ================================================================================================================
} }