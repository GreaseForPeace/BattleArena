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

[CustomEditor(typeof(Trigger))]
public class TriggerInspector : UniqueMBInspector<Trigger>
{
	private Vector2 scroll = Vector2.zero;
	private int ruleArea = 0;
	private Action selected = null;
	private ActionWiz openWiz = null;

	void OnEnable()
	{
		// make sure the actions (which are components) are hidden in the inspector
		for (int i = 0; i < Target.onLoaded.Count; i++) if (Target.onLoaded[i] != null) Target.onLoaded[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onPlayerEnter.Count; i++) if (Target.onPlayerEnter[i] != null) Target.onPlayerEnter[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onPlayerLeave.Count; i++) if (Target.onPlayerLeave[i] != null) Target.onPlayerLeave[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onPlayerStay.Count; i++) if (Target.onPlayerStay[i] != null) Target.onPlayerStay[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onNPCEnter.Count; i++) if (Target.onNPCEnter[i] != null) Target.onNPCEnter[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onNPCLeave.Count; i++) if (Target.onNPCLeave[i] != null) Target.onNPCLeave[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onNPCStay.Count; i++) if (Target.onNPCStay[i] != null) Target.onNPCStay[i].hideFlags = HideFlags.HideInInspector;
	}

	public virtual void OnDisable()
	{
		if (openWiz != null) openWiz.Close();
	}

	public override void OnInspectorGUI()
	{
		UniRPGEdGui.UseSkin();
		GUILayout.Space(15f);

		EditorGUIUtility.LookLikeControls(120);
		Target.areaKind = (Trigger.AreaKind)EditorGUILayout.EnumPopup("Trigger Kind", Target.areaKind);

		EditorGUILayout.Space();

		GUILayout.Label("Events & Actions", UniRPGEdGui.InspectorHeadFoldStyle);
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(100));
			{
				EditorGUILayout.Space();
				if (UniRPGEdGui.ToggleButton(ruleArea == 0, new GUIContent("Loaded", UniRPGEdGui.Icon_Accept, "Actions executed when this Trigger is loaded"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 0; scroll = Vector2.zero; selected = null; }
				EditorGUILayout.Space();
				if (Target.areaKind != Trigger.AreaKind.None)
				{
					if (UniRPGEdGui.ToggleButton(ruleArea == 1, new GUIContent("Player Enter", UniRPGEdGui.Icon_User, "Actions executed when a Player enters the trigger area"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 1; scroll = Vector2.zero; selected = null; }
					if (UniRPGEdGui.ToggleButton(ruleArea == 2, new GUIContent("Player Leave", UniRPGEdGui.Icon_User, "Actions executed when a Player leaves the trigger area"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 2; scroll = Vector2.zero; selected = null; }
					if (UniRPGEdGui.ToggleButton(ruleArea == 3, new GUIContent("Player Stay", UniRPGEdGui.Icon_User, "Actions executed while a Player stays in the trigger area"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 3; scroll = Vector2.zero; selected = null; }
					EditorGUILayout.Space();
					if (UniRPGEdGui.ToggleButton(ruleArea == 4, new GUIContent("NPC Enter", UniRPGEdGui.Icon_Users, "Actions executed when an NPC enters the trigger area"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 4; scroll = Vector2.zero; selected = null; }
					if (UniRPGEdGui.ToggleButton(ruleArea == 5, new GUIContent("NPC Leave", UniRPGEdGui.Icon_Users, "Actions executed when an NPC leaves the trigger area"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 5; scroll = Vector2.zero; selected = null; }
					if (UniRPGEdGui.ToggleButton(ruleArea == 6, new GUIContent("NPC Stay", UniRPGEdGui.Icon_Users, "Actions executed while an NPC stays in the trigger area"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 6; scroll = Vector2.zero; selected = null; }
				}

				EditorGUILayout.Space();
				if (UniRPGEdGui.IconButton("Action", UniRPGEdGui.Icon_Plus))
				{
					if (openWiz != null) openWiz.Close();
					openWiz = ActionWiz.Show(OnActionSelected, Target.gameObject, null);
				}
			}
			EditorGUILayout.EndVertical();
			scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(250));
			
			switch (ruleArea)
			{
				case 0: ActionsList(Target.onLoaded); break;
				case 1: ActionsList(Target.onPlayerEnter); break;
				case 2: ActionsList(Target.onPlayerLeave); break;
				case 3: ActionsList(Target.onPlayerStay); break;
				case 4: ActionsList(Target.onNPCEnter); break;
				case 5: ActionsList(Target.onNPCLeave); break;
				case 6: ActionsList(Target.onNPCStay); break;
			}
			UniRPGEdGui.EndScrollView();
		}
		EditorGUILayout.EndHorizontal();

		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		DrawMBInspector(true);

		EditorGUILayout.Space();
		if (GUI.changed)
		{
			HandleUtility.Repaint();
			if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.Repaint();
		}
	}

	private void ActionsList(List<Action> actions)
	{
		if (actions.Count > 0)
		{
			float w = Screen.width - 150;
			GUIStyle back = UniRPGEdGui.ListItemBackDarkStyle;
			Action del = null;
			int move = 0;
			int count = 0;
			foreach (Action act in actions)
			{
				count++;
				EditorGUILayout.BeginHorizontal();
				{
					if (act == null)
					{
						GUILayout.Label(count + ": Error => NULL value Action found.", back, GUILayout.Width(w - 25));
						if (GUILayout.Button("FIX", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(25)))
						{
							actions = UniRPGUtil.CleanupList<Action>(actions);
							GUIUtility.ExitGUI();
							break;
						}
						GUILayout.FlexibleSpace();
					}
					else
					{
						if (selected == act)
						{
							if (GUILayout.Button(count + ": " + UniRPGEditorGlobal.GetActionShortNfoString(act), UniRPGEdGui.ListItemSelectedStyle, GUILayout.Width(w - 70)))
							{
								if (openWiz != null) openWiz.Close();
								openWiz = ActionWiz.Show(OnActionSelected, Target.gameObject, act);
							}
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Minus, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(25))) del = act;
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Arrow2_Up, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(25))) move = -1;
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Arrow2_Down, UniRPGEdGui.ButtonRightStyle, GUILayout.Width(25))) move = +1;
							GUILayout.FlexibleSpace();
						}
						else
						{
							if (GUILayout.Button(count + ": " + UniRPGEditorGlobal.GetActionShortNfoString(act), back, GUILayout.Width(w)))
							{
								selected = act;
								if (openWiz != null) openWiz.Close();
								openWiz = ActionWiz.Show(OnActionSelected, Target.gameObject, act);
							}
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				back = (back == UniRPGEdGui.ListItemBackDarkStyle ? UniRPGEdGui.ListItemBackLightStyle : UniRPGEdGui.ListItemBackDarkStyle);
			}

			if (move != 0 && selected != null)
			{
				if (openWiz != null) { openWiz.Close(); openWiz = null; }
				int idx = actions.IndexOf(selected);
				actions.Remove(selected);
				idx = idx + move;
				if (idx > actions.Count) idx = actions.Count;
				if (idx < 0) idx = 0;
				actions.Insert(idx, selected);
				EditorUtility.SetDirty(Target);
			}

			if (del != null)
			{
				if (openWiz != null) { openWiz.Close(); openWiz = null; }
				if (selected == del) selected = null;
				actions.Remove(del);
				GameObject.DestroyImmediate(del, true); // remove the asset from file too
				del = null;
				EditorUtility.SetDirty(Target);
			}
		}
	}

	private void OnActionSelected(System.Object sender)
	{
		ActionWiz wiz = sender as ActionWiz;
		List<Action> list = null;
		switch (ruleArea)
		{
			case 0: list = Target.onLoaded; break;
			case 1: list = Target.onPlayerEnter; break;
			case 2: list = Target.onPlayerLeave; break;
			case 3: list = Target.onPlayerStay; break;
			case 4: list = Target.onNPCEnter; break;
			case 5: list = Target.onNPCLeave; break;
			case 6: list = Target.onNPCStay; break;
		}

		if (list != null)
		{
			if (wiz.isNewAction)
			{
				list.Add(wiz.Action);
				selected = wiz.Action;
			}
			else if (wiz.Action != selected)
			{
				int idx = list.IndexOf(selected);
				list.Remove(selected);
				GameObject.DestroyImmediate(selected, true); // remove the asset from file too
				list.Insert(idx, wiz.Action);
				selected = wiz.Action;
			}
		}
		EditorUtility.SetDirty(Target);
		wiz.Close();
		Repaint();
	}

	// ================================================================================================================
} }