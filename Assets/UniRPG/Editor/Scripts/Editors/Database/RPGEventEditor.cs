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

[DatabaseEditor("Events", Priority = 6)]
public class RPGEventEditor : DatabaseEdBase
{
	// ================================================================================================================
	#region vars

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero };
	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activeIcon = 0;

	private RPGEvent curr = null;	// currently being edited
	private RPGEvent del = null;	// helper when deleting
	private Action selectedAct = null;
	private ActionWiz openWiz = null;

	#endregion
	// ================================================================================================================
	#region editor

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

	// ================================================================================================================

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
			if (GUILayout.Button(new GUIContent("Add Event", UniRPGEdGui.Icon_Plus), EditorStyles.miniButtonLeft))
			{
				GUI.FocusControl("");
				TextInputWiz.Show("New RPGEvent", "Enter name for new Event", "", CreateNewEvent);
			}
			if (curr == null) GUI.enabled = false;
			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Copy, "Copy"), EditorStyles.miniButtonMid))
			{
				GUI.FocusControl("");
				CreateEventCopy();
			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		{
			if (ed.db.RPGEvents.Length > 0)
			{
				foreach (RPGEvent rpgEvent in ed.db.RPGEvents)
				{
					if (rpgEvent == null) continue;

					Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(DatabaseEditor.LeftPanelWidth - 20), GUILayout.ExpandWidth(false));
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (rpgEvent.icon[0] != null ? rpgEvent.icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(curr == rpgEvent, rpgEvent.screenName, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(140), GUILayout.ExpandWidth(false)))
						{
							curr = rpgEvent;
							GUI.FocusControl("");
						}
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
						{
							del = rpgEvent;
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

		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();

		if (del != null)
		{
			if (curr == del) curr = null;
			ed.db.rpgEventPrefabs.Remove(del.gameObject);
			EditorUtility.SetDirty(ed.db);
			AssetDatabase.SaveAssets();

			string path = AssetDatabase.GetAssetPath(del.gameObject);
			AssetDatabase.DeleteAsset(path);
			AssetDatabase.Refresh();
			del = null;
		}
	}

	private void CreateNewEvent(System.Object sender)
	{
		TextInputWiz wiz = sender as TextInputWiz;
		string name = wiz.text;
		wiz.Close();

		if (string.IsNullOrEmpty(name)) name = "Event";

		UniRPGEditorGlobal.CheckDatabasePath(UniRPGEditorGlobal.DB_DATA_PATH, UniRPGEditorGlobal.DB_EVENTS_PATH);
		string fn = UniRPGEditorGlobal.DB_EVENTS_PATH + name + ".prefab";
		if (UniRPGEdUtil.RelativeFileExist(fn)) fn = AssetDatabase.GenerateUniqueAssetPath(fn);

		Object prefab = PrefabUtility.CreateEmptyPrefab(fn);
		GameObject go = new GameObject(name);							// create temp object in scene 
		go.AddComponent<RPGEvent>();
		GameObject toRef = PrefabUtility.ReplacePrefab(go, prefab);		// save prefab
		GameObject.DestroyImmediate(go);								// clear temp object from scene

		curr = toRef.GetComponent<RPGEvent>();
		curr.screenName = name;
		ed.db.rpgEventPrefabs.Add(toRef);

		EditorUtility.SetDirty(curr);
		EditorUtility.SetDirty(ed.db);
		AssetDatabase.SaveAssets();

		ed.Repaint();
	}

	private void CreateEventCopy()
	{
		string name = curr.name;
		if (string.IsNullOrEmpty(name)) name = "Event";

		UniRPGEditorGlobal.CheckDatabasePath(UniRPGEditorGlobal.DB_DATA_PATH, UniRPGEditorGlobal.DB_EVENTS_PATH);
		string fn = UniRPGEditorGlobal.DB_EVENTS_PATH + name + ".prefab";
		if (UniRPGEdUtil.RelativeFileExist(fn)) fn = AssetDatabase.GenerateUniqueAssetPath(fn);

		Object prefab = PrefabUtility.CreateEmptyPrefab(fn);
		GameObject go = new GameObject(name);							// create temp object in scene 
		go.AddComponent<RPGEvent>();
		GameObject toRef = PrefabUtility.ReplacePrefab(go, prefab);		// save prefab
		GameObject.DestroyImmediate(go);								// clear temp object from scene

		RPGEvent e = toRef.GetComponent<RPGEvent>();
		e.screenName = name;
		curr.CopyTo(e);
		e.id = GUID.Create();
		curr = e;

		ed.db.rpgEventPrefabs.Add(toRef);

		EditorUtility.SetDirty(curr);
		EditorUtility.SetDirty(ed.db);
		AssetDatabase.SaveAssets();

		ed.Repaint();
	}

	private void RightPanel()
	{
		EditorGUILayout.BeginVertical();
		GUILayout.Space(5);
		// -------------------------------------------------------------

		GUILayout.Label("Event Definition", UniRPGEdGui.Head1Style);
		if (curr == null) { EditorGUILayout.EndVertical(); return; }
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);

		BasicInfo();
		ActionsEditor();

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
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical();
	}

	private void ActionsEditor()
	{
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(450));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Actions", UniRPGEdGui.Head3Style);
				GUILayout.Space(20);
				if (UniRPGEdGui.IconButton("Action", UniRPGEdGui.Icon_Plus, GUILayout.Width(100)))
				{
					if (openWiz != null) openWiz.Close();
					openWiz = ActionWiz.Show(OnActionSelected, curr.gameObject, null);
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20);
			scroll[2] = UniRPGEdGui.BeginScrollView(scroll[2], UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(175));
			ActionsList(curr.onUseActions, 450);
			UniRPGEdGui.EndScrollView();
		}
		EditorGUILayout.EndVertical();
	}

	private void ActionsList(List<Action> actions, float w)
	{
		if (actions.Count > 0)
		{
			GUIStyle back = UniRPGEdGui.ListItemBackDarkStyle;
			Action delAct = null;
			int move = 0;
			int count = 0;
			foreach (Action act in actions)
			{
				count++;
				EditorGUILayout.BeginHorizontal();
				{
					if (act == null)
					{
						if (GUILayout.Button("FIX", UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(25)))
						{
							actions = UniRPGUtil.CleanupList<Action>(actions);
							GUIUtility.ExitGUI();
							break;
						}
						GUILayout.Label(count + ": Error => NULL value Action found.", back, GUILayout.ExpandWidth(true));
					}
					else
					{
						if (selectedAct == act)
						{
							if (GUILayout.Button(count + ": " + UniRPGEditorGlobal.GetActionShortNfoString(act), UniRPGEdGui.ListItemSelectedStyle, GUILayout.Width(w - 125)))
							{
								if (openWiz != null) openWiz.Close();
								openWiz = ActionWiz.Show(OnActionSelected, curr.gameObject, act);
							}
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Minus, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(25))) delAct = act;
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Arrow2_Up, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(25))) move = -1;
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Arrow2_Down, UniRPGEdGui.ButtonRightStyle, GUILayout.Width(25))) move = +1;
						}
						else
						{
							if (GUILayout.Button(count + ": " + UniRPGEditorGlobal.GetActionShortNfoString(act), back, GUILayout.Width(w-55)))
							{
								selectedAct = act;
								if (openWiz != null) openWiz.Close();
								openWiz = ActionWiz.Show(OnActionSelected, curr.gameObject, act);
							}
							GUILayout.Space(5);
						}
					}
					GUILayout.Space(10);
				}
				EditorGUILayout.EndHorizontal();
				back = (back == UniRPGEdGui.ListItemBackDarkStyle ? UniRPGEdGui.ListItemBackLightStyle : UniRPGEdGui.ListItemBackDarkStyle);
			}

			if (move != 0 && selectedAct != null)
			{
				if (openWiz != null) { openWiz.Close(); openWiz = null; }
				int idx = actions.IndexOf(selectedAct);
				actions.Remove(selectedAct);
				idx = idx + move;
				if (idx > actions.Count) idx = actions.Count;
				if (idx < 0) idx = 0;
				actions.Insert(idx, selectedAct);
				EditorUtility.SetDirty(curr);
			}

			if (delAct != null)
			{
				if (openWiz != null) { openWiz.Close(); openWiz = null; }
				if (selectedAct == delAct) selectedAct = null;
				actions.Remove(delAct);
				GameObject.DestroyImmediate(delAct, true); // remove the component from prefab
				delAct = null;
				EditorUtility.SetDirty(curr);
			}
		}
	}

	private void OnActionSelected(System.Object sender)
	{
		ActionWiz wiz = sender as ActionWiz;
		if (wiz.accepted)
		{
			if (wiz.isNewAction)
			{
				curr.onUseActions.Add(wiz.Action);
				selectedAct = wiz.Action;
			}
			else if (wiz.Action != selectedAct)
			{
				int idx = curr.onUseActions.IndexOf(selectedAct);
				curr.onUseActions.Remove(selectedAct);
				GameObject.DestroyImmediate(selectedAct, true); // remove the asset from file too
				curr.onUseActions.Insert(idx, wiz.Action);
				selectedAct = wiz.Action;
			}
			EditorUtility.SetDirty(curr);
		}
		wiz.Close();
		ed.Repaint();
	}

	#endregion
	// ================================================================================================================
} }