// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor
{
	[CustomEditor(typeof(PlaceableObject))]
	public class PlaceableObjectInspector : UniqueMBInspector<PlaceableObject>
	{

		private bool[] foldout = { true, true };
		private Vector2 scroll = Vector2.zero;
		private Action selected = null;
		private ActionWiz openWiz = null;

		public virtual void OnEnable()
		{
			for (int i = 0; i < Target.actions.Count; i++) if (Target.actions[i] != null) Target.actions[i].hideFlags = HideFlags.HideInInspector;
		}

		public virtual void OnDisable()
		{
			if (openWiz != null) openWiz.Close();
		}

		public override void OnInspectorGUI()
		{
			UniRPGEdGui.UseSkin();
			GUILayout.Space(10f);

			Target.validTargetsMask = (UniRPGGlobal.Target)EditorGUILayout.EnumMaskField("Valid Targets", Target.validTargetsMask);
			Target.radius = EditorGUILayout.FloatField("Radius", Target.radius);
			Target.isTimed = EditorGUILayout.Toggle("Is timed trigger", Target.isTimed);
			
			if (Target.isTimed)
			{
				Target.timeout = EditorGUILayout.FloatField("Timeout", Target.timeout);
				Target.maxTargets = EditorGUILayout.IntField("Max targets affected", Target.maxTargets);
			}

			Target.autoDestroy = EditorGUILayout.Toggle("Auto-destroy", Target.autoDestroy);

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(100));
				{
					EditorGUILayout.Space();
					if (UniRPGEdGui.ToggleButton(true, new GUIContent("On Triggered", UniRPGEdGui.Icon_Accept), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { scroll = Vector2.zero; selected = null; }

					EditorGUILayout.Space();
					if (UniRPGEdGui.IconButton("Action", UniRPGEdGui.Icon_Plus))
					{
						if (openWiz != null) openWiz.Close();
						openWiz = ActionWiz.Show(OnActionSelected, Target.gameObject, null);
					}
				}
				EditorGUILayout.EndVertical();

				scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(175));
				ActionsList(Target.actions, Target);
				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndHorizontal();

			UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
			foldout[1] = DrawMBInspector(foldout[1]);

			if (GUI.changed)
			{
				GUI.changed = false;
				EditorUtility.SetDirty(Target);
			}
		}

		private void ActionsList(List<Action> actions, PlaceableObject obj)
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
									openWiz = ActionWiz.Show(OnActionSelected, obj.gameObject, act);
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
									openWiz = ActionWiz.Show(OnActionSelected, obj.gameObject, act);
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
			List<Action> list = Target.actions;;

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

		public virtual void OnSceneGUI()
		{
			Handles.color = new Color(0.9f, 0.1f, 0.1f, 0.1f);
			float r = Target.radius * Mathf.Max(new float[] {Target.transform.localScale.x,Target.transform.localScale.y,Target.transform.localScale.z});
			Handles.DrawSolidDisc(Target.transform.position, Vector3.up, r);
		}

		// ============================================================================================================
	}
}