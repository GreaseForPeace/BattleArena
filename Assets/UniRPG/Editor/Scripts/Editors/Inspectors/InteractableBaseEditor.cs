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

public class InteractableBaseEditor<T> : UniqueMBInspector<T> where T : Interactable
{
	private Vector2 intr_scroll = Vector2.zero;
	private int ruleArea = 0;
	private Action selected = null;
	private ActionWiz openWiz = null;

	// ================================================================================================================

	public virtual void OnEnable()
	{
		// make sure the actions (which are components) are hidden in the inspector
		for (int i = 0; i < Target.onTargetedActions.Count; i++) if (Target.onTargetedActions[i] != null) Target.onTargetedActions[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onClearTargetActions.Count; i++) if (Target.onClearTargetActions[i] != null) Target.onClearTargetActions[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onInteractActions.Count; i++) if (Target.onInteractActions[i] != null) Target.onInteractActions[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onUseActions.Count; i++) if (Target.onUseActions[i] != null) Target.onUseActions[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onEquipActions.Count; i++) if (Target.onEquipActions[i] != null) Target.onEquipActions[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onUnEquipActions.Count; i++) if (Target.onUnEquipActions[i] != null) Target.onUnEquipActions[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onGetHit.Count; i++) if (Target.onGetHit[i] != null) Target.onGetHit[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onDeath.Count; i++) if (Target.onDeath[i] != null) Target.onDeath[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onAddedToBag.Count; i++) if (Target.onAddedToBag[i] != null) Target.onAddedToBag[i].hideFlags = HideFlags.HideInInspector;
		for (int i = 0; i < Target.onRemovedFromBag.Count; i++) if (Target.onRemovedFromBag[i] != null) Target.onRemovedFromBag[i].hideFlags = HideFlags.HideInInspector;
	}

	public virtual void OnDisable()
	{
		if (openWiz != null) openWiz.Close();
	}

	protected bool[] DrawInteractableInspector(bool[] foldout, bool isItem, bool isEquipable, bool isCharacter)
	{
		foldout[0] = UniRPGEdGui.Foldout(foldout[0], "Events & Actions", UniRPGEdGui.InspectorHeadFoldStyle);
		if (foldout[0])
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(100));
				{
					EditorGUILayout.Space();
					if (UniRPGEdGui.ToggleButton(ruleArea == 0, new GUIContent("On Targeted", UniRPGEdGui.Icon_Accept, "Actions executed when this object is targeted"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 0; intr_scroll = Vector2.zero; selected = null; }
					if (UniRPGEdGui.ToggleButton(ruleArea == 1, new GUIContent("On UnTarget", UniRPGEdGui.Icon_Cancel, "Actions executed when this object is untargeted"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 1; intr_scroll = Vector2.zero; selected = null; }

					if (isItem)
					{
						if (UniRPGEdGui.ToggleButton(ruleArea == 2, new GUIContent("On Pickup", UniRPGEdGui.Icon_Arrow_Up, "Actions executed after the item was placed in the bag"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 2; intr_scroll = Vector2.zero; selected = null; }
						if (isEquipable)
						{
							if (UniRPGEdGui.ToggleButton(ruleArea == 4, new GUIContent("On Equip", UniRPGEdGui.Icon_UserPlus, "Actions executed when the object is equipped on a target"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 4; intr_scroll = Vector2.zero; selected = null; }
							if (UniRPGEdGui.ToggleButton(ruleArea == 5, new GUIContent("On UnEquip", UniRPGEdGui.Icon_UserMinus, "Actions executed when the object is unequipped from target"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 5; intr_scroll = Vector2.zero; selected = null; }
						}
						else
						{	// note, equipable items do not have Use action since they should use Skills for that (A sword for example should add an attack skil lto the player when it is equipped)
							if (UniRPGEdGui.ToggleButton(ruleArea == 3, new GUIContent("On Use", UniRPGEdGui.Icon_Star, "Actions executed when the object is used"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 3; intr_scroll = Vector2.zero; selected = null; }
						}
						if (UniRPGEdGui.ToggleButton(ruleArea == 20, new GUIContent("Added to Bag", "Actions executed when the object is added to a bag"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 20; intr_scroll = Vector2.zero; selected = null; }
						if (UniRPGEdGui.ToggleButton(ruleArea == 21, new GUIContent("Rem from Bag", "Actions executed when the object is removed from a bag"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 21; intr_scroll = Vector2.zero; selected = null; }
					}
					else
					{
						if (UniRPGEdGui.ToggleButton(ruleArea == 2, new GUIContent("On Interact", UniRPGEdGui.Icon_Star, "Actions executed when interaction with this object is started"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 2; intr_scroll = Vector2.zero; selected = null; }
					}

					if (isCharacter)
					{
						if (UniRPGEdGui.ToggleButton(ruleArea == 10, new GUIContent("On GetHit", UniRPGEdGui.Icon_Weapon, "Actions executed when character is hit/attacked"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 10; intr_scroll = Vector2.zero; selected = null; }
						if (UniRPGEdGui.ToggleButton(ruleArea == 11, new GUIContent("On Death", UniRPGEdGui.Icon_Skull, "Actions executed when character should die"), UniRPGEdGui.LeftTabStyle, GUILayout.Width(100))) { ruleArea = 11; intr_scroll = Vector2.zero; selected = null; }
					}

					EditorGUILayout.Space();
					if (UniRPGEdGui.IconButton("Action", UniRPGEdGui.Icon_Plus))
					{
						if (openWiz != null) openWiz.Close();
						openWiz = ActionWiz.Show(OnActionSelected, Target.gameObject, null);
					}
				}
				EditorGUILayout.EndVertical();
				intr_scroll = UniRPGEdGui.BeginScrollView(intr_scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(250));
				switch (ruleArea)
				{
					case 0: ActionsList(Target.onTargetedActions, Target); break;
					case 1: ActionsList(Target.onClearTargetActions, Target); break;
					case 2:
					{
						Target.interactDistance = EditorGUILayout.FloatField(new GUIContent("Max Interact Distance", "Distance that an object must be to interact with this."), Target.interactDistance);
						ActionsList(Target.onInteractActions, Target);
					} break;
					case 3: ActionsList(Target.onUseActions, Target); break;
					case 4: ActionsList(Target.onEquipActions, Target); break;
					case 5: ActionsList(Target.onUnEquipActions, Target); break;
					case 10:
					{
						EditorGUILayout.BeginHorizontal();
						{
							int selAttrib = -1;
							if (Target.hitAttrib != null) selAttrib = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, Target.hitAttrib);
							EditorGUI.BeginChangeCheck();
							selAttrib = EditorGUILayout.Popup("If Attribute Decrease", selAttrib, UniRPGEditorGlobal.DB.AttributeNames);
							if (EditorGUI.EndChangeCheck()) Target.hitAttrib = UniRPGEditorGlobal.DB.attributes[selAttrib].id.Copy();
							if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(25))) Target.hitAttrib = null;
							EditorGUILayout.Space();
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space();
						ActionsList(Target.onGetHit, Target);
					} break;
					case 11:
					{
						EditorGUILayout.BeginHorizontal();
						{
						int selAttrib = -1;
						if (Target.hitAttrib != null) selAttrib = RPGAttribute.GetAttribIdx(UniRPGEditorGlobal.DB.attributes, Target.deathAttrib);
						EditorGUI.BeginChangeCheck();
						selAttrib = EditorGUILayout.Popup("If Attribute reach Zero", selAttrib, UniRPGEditorGlobal.DB.AttributeNames);
						if (EditorGUI.EndChangeCheck()) Target.deathAttrib = UniRPGEditorGlobal.DB.attributes[selAttrib].id.Copy();
						if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(25))) Target.deathAttrib = null;
						EditorGUILayout.Space();
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space(); ActionsList(Target.onDeath, Target);
					} break;
					case 20: ActionsList(Target.onAddedToBag, Target); break;
					case 21: ActionsList(Target.onRemovedFromBag, Target); break;
				}
				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndHorizontal();
		}

		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		foldout[1] = DrawMBInspector(foldout[1]);

		return foldout;
	}

	private void ActionsList(List<Action> actions, Interactable obj)
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
			case 0: list = Target.onTargetedActions; break;
			case 1: list = Target.onClearTargetActions; break;
			case 2: list = Target.onInteractActions; break;
			case 3: list = Target.onUseActions; break;
			case 4: list = Target.onEquipActions; break;
			case 5: list = Target.onUnEquipActions; break;
			case 10: list = Target.onGetHit; break;
			case 11: list = Target.onDeath; break;
			case 20: list = Target.onAddedToBag; break;
			case 21: list = Target.onRemovedFromBag; break;
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

	public virtual void OnSceneGUI()
	{
		if (ruleArea == 2)
		{	// * Interact
			Handles.color = new Color(0.4f, 1f, 0.9f, 0.1f);
			Handles.DrawSolidDisc(Target.transform.position, Vector3.up, Target.interactDistance);
		}
	}

	// ================================================================================================================
} }