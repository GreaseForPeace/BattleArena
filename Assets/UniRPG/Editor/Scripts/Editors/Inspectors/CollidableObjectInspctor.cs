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
	[CustomEditor(typeof(CollidableObject))]
	public class CollidableObjectInspctor : UniqueMBInspector<CollidableObject>
	{
		private bool[] foldout = { true, true };
		private Vector2 scroll = Vector2.zero;
		private int ruleArea = 0;
		private Action selected = null;
		private ActionWiz openWiz = null;

		public virtual void OnEnable()
		{
			for (int i = 0; i < Target.onTriggerEnterActions.Count; i++) if (Target.onTriggerEnterActions[i] != null) Target.onTriggerEnterActions[i].hideFlags = HideFlags.HideInInspector;
			for (int i = 0; i < Target.onTriggerExitAction.Count; i++) if (Target.onTriggerExitAction[i] != null) Target.onTriggerExitAction[i].hideFlags = HideFlags.HideInInspector;
			for (int i = 0; i < Target.onTriggerStayAction.Count; i++) if (Target.onTriggerStayAction[i] != null) Target.onTriggerStayAction[i].hideFlags = HideFlags.HideInInspector;
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
			Target.stayCallRate = EditorGUILayout.FloatField("Stay call Rate (sec)", Target.stayCallRate);

			GUILayout.Space(10f);
			foldout[0] = UniRPGEdGui.Foldout(foldout[0], "Events & Actions", UniRPGEdGui.InspectorHeadFoldStyle);
			if (foldout[0])
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(100));
					{
						EditorGUILayout.Space();
						if (UniRPGEdGui.ToggleButton(ruleArea == 0, new GUIContent("On Enter", UniRPGEdGui.Icon_Accept, "Enter into a collision"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 0; scroll = Vector2.zero; selected = null; }
						if (UniRPGEdGui.ToggleButton(ruleArea == 1, new GUIContent("On Exit", UniRPGEdGui.Icon_Cancel, "Exit a collision"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 1; scroll = Vector2.zero; selected = null; }
						if (UniRPGEdGui.ToggleButton(ruleArea == 2, new GUIContent("On Stay", UniRPGEdGui.Icon_Refresh, "Stay within a collision"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 2; scroll = Vector2.zero; selected = null; }

						EditorGUILayout.Space();
						if (UniRPGEdGui.IconButton("Action", UniRPGEdGui.Icon_Plus))
						{
							if (openWiz != null) openWiz.Close();
							openWiz = ActionWiz.Show(OnActionSelected, Target.gameObject, null);
						}
					}
					EditorGUILayout.EndVertical();

					scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(175));
					switch (ruleArea)
					{
						case 0: ActionsList(Target.onTriggerEnterActions, Target); break;
						case 1: ActionsList(Target.onTriggerExitAction, Target); break;
						case 2: ActionsList(Target.onTriggerStayAction, Target); break;
					}
					UniRPGEdGui.EndScrollView();
				}
				EditorGUILayout.EndHorizontal();
			}

			UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
			foldout[1] = DrawMBInspector(foldout[1]);

			if (GUI.changed)
			{
				GUI.changed = false;
				EditorUtility.SetDirty(Target);
			}
		}

		private void ActionsList(List<Action> actions, CollidableObject obj)
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
			List<Action> list = null;
			switch (ruleArea)
			{
				case 0: list = Target.onTriggerEnterActions; break;
				case 1: list = Target.onTriggerExitAction; break;
				case 2: list = Target.onTriggerStayAction; break;
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

		// ============================================================================================================
	}
}