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

public class ItemSelectWiz : EditorWindow
{
	public List<RPGItem> selectedItems = new List<RPGItem>();

	private bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;

	private bool multiSelect = false;
	private int limitToSlot = -1;
	private int filter = 0;			// filter to view only certain category of defined items
	private Vector2 scroll = Vector2.zero;
	private int target = -1;

	private UniRPGArgsEventHandler OnAccept = null;
	private object[] args = null;

	// ================================================================================================================

	public static void Show(bool multiSelect, UniRPGArgsEventHandler onAccept, object[] args)
	{
		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		ItemSelectWiz window = EditorWindow.GetWindow<ItemSelectWiz>(true, "Select Item(s)",true);
		window.inited = false;
		window.multiSelect = multiSelect;
		window.limitToSlot = -1;
		window.target = -1;
		window.OnAccept = onAccept;
		window.args = args;

		// show window
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Item(s)";
		minSize = new Vector2(200, 350);
		maxSize = new Vector2(200, 350);
	}

	public static void Show(bool multiSelect, UniRPGGlobal.Target target, int limitToSlot, UniRPGArgsEventHandler onAccept, object[] args)
	{
		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		ItemSelectWiz window = EditorWindow.GetWindow<ItemSelectWiz>(true);
		window.inited = false;
		window.multiSelect = multiSelect;
		window.limitToSlot = limitToSlot;
		window.target = (int)target;
		window.OnAccept = onAccept;
		window.args = args;

		// show window
		window.ShowUtility();
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
		if (filter >= UniRPGEditorGlobal.DB.itemCategories.Count) filter = 0;
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("filter");
			filter = EditorGUILayout.Popup(filter, UniRPGEditorGlobal.DB.ItemCategoryNames);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		scroll = UniRPGEdGui.BeginScrollView(scroll);
		{
			if (UniRPGEditorGlobal.DB.RPGItems.Count > 0)
			{
				foreach (RPGItem item in UniRPGEditorGlobal.DB.RPGItems.Values)
				{
					if (filter > 0 && item.categoryId != filter) continue;
					if (target >= 0)
					{
						if (((UniRPGGlobal.Target)target & item.equipTargetMask) == 0) continue;
					}
					if (limitToSlot >= 0 && !item.validEquipSlots.Contains(limitToSlot)) continue;

					Rect r = EditorGUILayout.BeginHorizontal();
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (item.icon[0] != null ? item.icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(selectedItems.Contains(item), item.screenName, UniRPGEdGui.ButtonRightStyle, UniRPGEdGui.ButtonOnColor, GUILayout.Width(150)))
						{
							if (multiSelect)
							{
								if (selectedItems.Contains(item)) selectedItems.Remove(item);
								else selectedItems.Add(item);
							}
							else if (!selectedItems.Contains(item))
							{
								selectedItems.Clear();
								selectedItems.Add(item);
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Items are defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();

			if (selectedItems.Count == 0) GUI.enabled = false;
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