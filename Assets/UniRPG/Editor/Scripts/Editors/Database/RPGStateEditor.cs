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

[DatabaseEditor("States", Priority = 4)]
public class RPGStateEditor: DatabaseEdBase
{
	private Vector2[] scroll = { Vector2.zero, Vector2.zero };

	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activeIcon = 0;

	private RPGState curr = null;	// currently being edited
	private RPGState del = null;	// helper when deleting

	// ================================================================================================================

	public override void OnGUI(DatabaseEditor ed)
	{
		base.OnGUI(ed);
		EditorGUILayout.BeginHorizontal();
		{
			LeftPanel();
			UniRPGEdGui.DrawVerticalLine(2f, UniRPGEdGui.DividerColor, 0f, 10f);
			RightPanel();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void LeftPanel()
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		GUILayout.Space(5);
		// -------------------------------------------------------------

		// the add button
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(new GUIContent("Add State", UniRPGEdGui.Icon_Plus), EditorStyles.miniButtonLeft))
			{
				GUI.FocusControl("");
				curr = ScriptableObject.CreateInstance<RPGState>();
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_STATES_FILE);
				curr.screenName = "State";
				ed.db.states.Add(curr);
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
			}
			if (curr == null) GUI.enabled = false;
			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Copy, "Copy"), EditorStyles.miniButtonMid))
			{
				GUI.FocusControl("");
				RPGState st = ScriptableObject.CreateInstance<RPGState>();
				curr.CopyTo(st);
				st.id = GUID.Create();
				curr = st;
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_STATES_FILE);
				ed.db.states.Add(curr);
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		{
			if (ed.db.states.Count > 0)
			{
				foreach (RPGState state in ed.db.states)
				{
					Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(DatabaseEditor.LeftPanelWidth - 20), GUILayout.ExpandWidth(false));
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (state.icon[0] != null ? state.icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(curr == state, state.screenName, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(140), GUILayout.ExpandWidth(false)))
						{
							curr = state;
							GUI.FocusControl("");
						}
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
						{
							del = state;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No States are defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView();

		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();

		if (del != null)
		{
			if (curr == del) curr = null;
			ed.db.states.Remove(del);
			Object.DestroyImmediate(del, true);
			del = null;
			EditorUtility.SetDirty(ed.db);
			AssetDatabase.SaveAssets();
		}
	}

	private void RightPanel()
	{
		EditorGUILayout.BeginVertical();
		GUILayout.Space(5);
		// -------------------------------------------------------------

		GUILayout.Label("State Definition", UniRPGEdGui.Head1Style);
		if (curr == null) { EditorGUILayout.EndVertical(); return; }
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);

		BasicInfo();
		StateRules();

		// check if data changed and should be saved
		if (GUI.changed && curr != null)
		{
			EditorUtility.SetDirty(curr);
		}

		EditorGUILayout.EndScrollView();//1
		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();
	}

	private void BasicInfo()
	{
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(450));
		{
			// name
			curr.screenName = EditorGUILayout.TextField("Screen Name", curr.screenName);
			curr.guiHelper = EditorGUILayout.TextField("GUI Helper", curr.guiHelper);
			EditorGUILayout.Space();

			// icon, description and notes
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(100));
				{
					GUILayout.Label("Icon");
					EditorGUILayout.BeginHorizontal();
					{
						curr.icon[activeIcon] = (Texture2D)EditorGUILayout.ObjectField(curr.icon[activeIcon], typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
						EditorGUILayout.BeginVertical();
						{
							if (UniRPGEdGui.ToggleButton(activeIcon == 0, "1", EditorStyles.miniButton)) activeIcon = 0;
							if (UniRPGEdGui.ToggleButton(activeIcon == 1, "2", EditorStyles.miniButton)) activeIcon = 1;
							if (UniRPGEdGui.ToggleButton(activeIcon == 2, "3", EditorStyles.miniButton)) activeIcon = 2;
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical();
				{
					EditorGUI.BeginChangeCheck();
					toolbar1Sel = GUILayout.Toolbar(toolbar1Sel, toolbar1);
					if (EditorGUI.EndChangeCheck()) GUI.FocusControl(""); // i need to do this to clear the focus on text fields which are bugging if they stay focused
					if (toolbar1Sel == 0) curr.description = EditorGUILayout.TextArea(curr.description, GUILayout.Height(60), GUILayout.ExpandHeight(false));
					else curr.notes = EditorGUILayout.TextArea(curr.notes, GUILayout.Height(60), GUILayout.ExpandHeight(false));
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUIUtility.LookLikeControls(300);
			curr.maxInstances = EditorGUILayout.IntField("Max instances of State on an Actor (0=unlimted)", curr.maxInstances);
			EditorGUIUtility.LookLikeControls();
		}
		EditorGUILayout.EndVertical();
	}

	private void StateRules()
	{
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(450));
		{
			EditorGUI.BeginChangeCheck();
			curr.effect = (RPGState.Effect)EditorGUILayout.EnumPopup("Effect", curr.effect);
			if (EditorGUI.EndChangeCheck())
			{	// reset opts
				curr.slot = 0;
				curr.timeoutSettings = 0f;
				curr.eventPrefab = null;
			}
			EditorGUILayout.Space();

			switch (curr.effect)
			{
				case RPGState.Effect.RunEventInIntervals: EventRule(); break;
				case RPGState.Effect.PreventEquipSlot: EquipSlotRule(); break;
			}

			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical();
	}

	private void EquipSlotRule()
	{
		curr.slot = EditorGUILayout.Popup("Prevent Equiping on Slot", curr.slot, UniRPGEditorGlobal.DB.equipSlots.ToArray());
	}

	private void EventRule()
	{
		RPGEvent e = null;
		if (curr.eventPrefab) e = curr.eventPrefab.GetComponent<RPGEvent>();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Call Event", GUILayout.Width(150));
		if (GUILayout.Button(e == null ? "-" : e.screenName, UniRPGEdGui.ButtonLeftStyle)) EventSelectWiz.Show(OnEventSelected, null, 0);
		if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) { curr.eventPrefab = null; EditorUtility.SetDirty(curr); EditorUtility.SetDirty(ed.db); }
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUIUtility.LookLikeControls(250);
		curr.timeoutSettings = EditorGUILayout.FloatField("Every (seconds)", curr.timeoutSettings);
		curr.autoRemoveTimeoutSettings = EditorGUILayout.FloatField("Auto-remove After (seconds, 0=never)", curr.autoRemoveTimeoutSettings);
	}

	private void OnEventSelected(System.Object sender)
	{
		EventSelectWiz wiz = sender as EventSelectWiz;
		if (curr != null) curr.eventPrefab = wiz.selectedEvent.gameObject;
		EditorUtility.SetDirty(curr);
		EditorUtility.SetDirty(ed.db);
		wiz.Close();
		ed.Repaint();
	}

	// ================================================================================================================
} }