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

[CustomEditor(typeof(Actor))]
public class ActorInspector : InspectorBase<Actor>
{
	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activePortrait = 0;
	private bool initItemsShowBag = false;
	private bool initSkillsEquipped = true;
	private int selectingForSlot = -1; // -1 = bag, else an equip slot

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero };
	private static bool[] foldout = { true, false, false, false, false };

	private List<RPGSkill> skillz1 = null;
	private List<RPGSkill> skillz2 = null;

	// ================================================================================================================

	void OnEnable()
	{
		// use 1st avail class if none set
		if (Target.actorClassPrefab == null) Target.actorClassPrefab = UniRPGEditorGlobal.DB.classes[0];

		// cache the skills
		Cache1();
		Cache2();
	}

	public override void OnInspectorGUI()
	{
		UniRPGEdGui.UseSkin();
		GUILayout.Space(10f);

		BasicInfo();
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		InitialItems();
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		EquippedSkills();
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		States();
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);

		GUILayout.Space(10f);

		if (GUI.changed)
		{
			GUI.changed = false;
			EditorUtility.SetDirty(Target);
		}
	}

	private void BasicInfo()
	{
		foldout[0] = UniRPGEdGui.Foldout(foldout[0], "Basic Info", UniRPGEdGui.InspectorHeadFoldStyle);

		if (foldout[0])
		{
			EditorGUILayout.Space();

			// name
			Target.screenName = EditorGUILayout.TextField("Screen Name", Target.screenName);
			if (Target.ActorType == UniRPGGlobal.ActorType.Player)
			{
				Target.availAtStart = EditorGUILayout.Toggle("Available at start", Target.availAtStart);
			}
			EditorGUILayout.Space();

			// portrait, description and notes
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(100));
				{
					GUILayout.Label("Portrait");
					EditorGUILayout.BeginHorizontal();
					{
						Target.portrait[activePortrait] = (Texture2D)EditorGUILayout.ObjectField(Target.portrait[activePortrait], typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
						EditorGUILayout.BeginVertical();
						{
							if (UniRPGEdGui.ToggleButton(activePortrait == 0, "1", EditorStyles.miniButton)) activePortrait = 0;
							if (UniRPGEdGui.ToggleButton(activePortrait == 1, "2", EditorStyles.miniButton)) activePortrait = 1;
							if (UniRPGEdGui.ToggleButton(activePortrait == 2, "3", EditorStyles.miniButton)) activePortrait = 2;
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
					if (toolbar1Sel == 0) Target.description = EditorGUILayout.TextArea(Target.description, GUILayout.Height(60), GUILayout.ExpandHeight(false));
					else Target.notes = EditorGUILayout.TextArea(Target.notes, GUILayout.Height(60), GUILayout.ExpandHeight(false));
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();

			// class, level and max level
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Class");
				EditorGUILayout.Space();
				if (GUILayout.Button((Target.actorClassPrefab == null ? "-select-" : Target.actorClassPrefab.screenName), GUILayout.MinWidth(120)))
				{
					ActorClassSelectWiz.Show(OnActorClassSelected);
				}
				EditorGUILayout.Space();
				GUILayout.Label("Start XP");
				Target.startingXP = EditorGUILayout.IntField(Target.startingXP, GUILayout.Width(50));
				GUILayout.Label(string.Format("(level: {0})", Target.actorClassPrefab.CalculateLevel(Target.startingXP)));
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
	}

	private void InitialItems()
	{
		EditorGUILayout.BeginHorizontal();
		{
			foldout[1] = UniRPGEdGui.Foldout(foldout[1], "Start Items", UniRPGEdGui.InspectorHeadFoldStyle);
			if (foldout[1])
			{
				EditorGUILayout.Space();
				if (UniRPGEdGui.ToggleButton(!initItemsShowBag, "Equipped", UniRPGEdGui.Icon_User, EditorStyles.miniButtonLeft, GUILayout.Width(75))) initItemsShowBag = false;
				if (UniRPGEdGui.ToggleButton(initItemsShowBag, "Bag", UniRPGEdGui.Icon_Bag, EditorStyles.miniButtonMid, GUILayout.Width(65))) initItemsShowBag = true;
				GUI.enabled = initItemsShowBag;
				if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Plus, EditorStyles.miniButtonRight, GUILayout.Width(25)))
				{
					selectingForSlot = -1; // -1 = for bag
					ItemSelectWiz.Show(true, OnRPGItemSelected, null);
				}
				GUI.enabled = true;
				GUILayout.FlexibleSpace();
			}
		}
		EditorGUILayout.EndHorizontal();

		if (foldout[1])
		{
			GUILayout.Space(5);
			scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], UniRPGEdGui.ScrollViewStyle, GUILayout.Height(150));
			{
				// show bag
				if (initItemsShowBag)
				{
					Target.currency = EditorGUILayout.IntField("Currency ("+UniRPGEditorGlobal.DB.currency+")", Target.currency);
					Target.bagSize = EditorGUILayout.IntField("Bag Size", Target.bagSize);
					if (Target.bagSize < 0) Target.bagSize = 0;
					EditorGUILayout.Space();
					float w = Screen.width - 190;
					for (int i = 0; i < Target.bagSize; i++)
					{
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Space(10);
							GUILayout.Label((i + 1).ToString(), GUILayout.Width(50), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(false));
							GUILayout.Space(10);
							if (Target.GetBagStackSize(i) <= 0) GUILayout.Label("-", GUILayout.Width(w)/*, GUILayout.MaxWidth(120), GUILayout.ExpandWidth(false)*/);
							else
							{
								if (GUILayout.Button(Target.GetBagItemName(i, true) + " (" + Target.GetBagStackSize(i) + ")", EditorStyles.label, GUILayout.Width(w)/*, GUILayout.MaxWidth(120), GUILayout.ExpandWidth(false)*/))
								{
									RPGItem item = Target.GetBagItem(i);
									if (item != null) EditorGUIUtility.PingObject(item.gameObject);
								}
							}
							GUILayout.Space(10);
							if (GUILayout.Button("+", UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(20)))
							{
								if (Target.GetBagStackSize(i) == 0)
								{
									selectingForSlot = -1; // -1 = for bag
									ItemSelectWiz.Show(true, OnRPGItemSelected, null);
								}
								else
								{
									Target.AddToBag(Target.GetBagItem(i), 1);
									EditorUtility.SetDirty(Target);
								}
							}
							if (GUILayout.Button("-", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
							{
								Target.RemoveFromBag(i, 1);
								EditorUtility.SetDirty(Target);
							}
							GUILayout.Space(10);
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				// show equipped
				else
				{
					if (UniRPGEditorGlobal.DB != null)
					{
						float w = Screen.width - 180;
						for (int i = 0; i < UniRPGEditorGlobal.DB.equipSlots.Count; i++)
						{
							EditorGUILayout.BeginHorizontal();
							{
								GUILayout.Label(UniRPGEditorGlobal.DB.equipSlots[i], UniRPGEdGui.LabelRightStyle, GUILayout.Width(80));
								GUILayout.Space(10);
								if (GUILayout.Button(Target.GetEquippedName(i, true), UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(w)))
								{
									selectingForSlot = i;
									ItemSelectWiz.Show(false, (UniRPGGlobal.Target)Target.ActorType, i, OnRPGItemSelected, null);
								}
								if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
								{
									UnEquipTarget(i);
									EditorUtility.SetDirty(Target);
								}
								GUILayout.Space(10);
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					else
					{
						GUILayout.Label("No database defined.", UniRPGEdGui.WarningLabelStyle);
					}
				}
			}
			UniRPGEdGui.EndScrollView();
		}
	}

	private void EquippedSkills()
	{
		EditorGUILayout.BeginHorizontal();
		{
			foldout[4] = UniRPGEdGui.Foldout(foldout[4], "Start Skills", UniRPGEdGui.InspectorHeadFoldStyle);
			if (foldout[4])
			{
				EditorGUILayout.Space();
				if (UniRPGEdGui.ToggleButton(initSkillsEquipped, "Equipped", UniRPGEdGui.Icon_User, EditorStyles.miniButtonLeft, GUILayout.Width(80))) initSkillsEquipped = true;
				if (UniRPGEdGui.ToggleButton(!initSkillsEquipped, "Available", UniRPGEdGui.Icon_Bag, EditorStyles.miniButtonMid, GUILayout.Width(80))) initSkillsEquipped = false;
				GUI.enabled = !initSkillsEquipped;
				if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Plus, EditorStyles.miniButtonRight, GUILayout.Width(25)))
				{
					Target.startingSkills.Add(null);
					skillz2.Add(null);
				}
				GUI.enabled = true;
				GUILayout.FlexibleSpace();
			}
		}
		EditorGUILayout.EndHorizontal();

		if (foldout[4])
		{
			GUILayout.Space(5);
			scroll[2] = UniRPGEdGui.BeginScrollView(scroll[2], UniRPGEdGui.ScrollViewStyle, GUILayout.Height(150));
			{
				// show equipped
				if (initSkillsEquipped)
				{
					EditorGUI.BeginChangeCheck();
					Target.actionSlotCount = EditorGUILayout.IntField("Slots Count", Target.actionSlotCount);
					if (Target.actionSlotCount < 0) Target.actionSlotCount = 0;
					if (EditorGUI.EndChangeCheck()) CheckStartingSkillSlotsSize();

					EditorGUILayout.Space();
					for (int i = 0; i < Target.actionSlotCount; i++)
					{
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Label("Slot " + (i + 1).ToString(), GUILayout.Width(120));
							if (GUILayout.Button(GetStartingSkillSlotName1(i), UniRPGEdGui.ButtonLeftStyle)) SkillSelectWiz.Show(OnSkillSelected1, GetStartingSkillFromSlot1(i), i);
							if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) ClearStartingSkillSlot(i);
							GUILayout.Space(10);
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				// show available
				else
				{
					int del = -1;
					for (int i = 0; i < Target.startingSkills.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						{
							if (GUILayout.Button(GetStartingSkillSlotName2(i), UniRPGEdGui.ButtonLeftStyle)) SkillSelectWiz.Show(OnSkillSelected2, GetStartingSkillFromSlot2(i), i);
							if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) del = i;
							GUILayout.Space(10);
						}
						EditorGUILayout.EndHorizontal();
					}
					if (del >= 0)
					{
						Target.startingSkills.RemoveAt(del);
						skillz2.RemoveAt(del);
					}
				}
			}
			UniRPGEdGui.EndScrollView();
		}

	}

	private void States()
	{
		EditorGUILayout.BeginHorizontal();
		{
			foldout[2] = UniRPGEdGui.Foldout(foldout[2], "Start States", UniRPGEdGui.InspectorHeadFoldStyle);
			if (foldout[2])
			{
				EditorGUILayout.Space();
				if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
				{
					StateSelectWiz.Show(OnStateSelected);
				}
				GUILayout.FlexibleSpace();				
			}
		}
		EditorGUILayout.EndHorizontal();		

		if (foldout[2])
		{
			EditorGUILayout.Space();
			scroll[1] = UniRPGEdGui.BeginScrollView(scroll[1], UniRPGEdGui.ScrollViewStyle, GUILayout.Height(100));
			{
				float w = Screen.width - 80;
				GUIStyle back = UniRPGEdGui.ListItemBackDarkStyle;
				int del = -1;
				for (int i = 0; i < Target.startingStates.Count; i++ )
				{
					EditorGUILayout.BeginHorizontal();
					{
						// it can happen if the designer deleted the state from the database (so check for that and print error if needed)
						// add to list to remove from Target't states after this list has run through
						GUILayout.Label(Target.startingStates[i] != null ? Target.startingStates[i].screenName : "!ERROR! State Definition Missing", back, GUILayout.Width(w));
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) del = i;
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndHorizontal();

					back = (back == UniRPGEdGui.ListItemBackDarkStyle ? UniRPGEdGui.ListItemBackLightStyle : UniRPGEdGui.ListItemBackDarkStyle);
				}
				if (del >= 0) Target.startingStates.RemoveAt(del);
			}
			UniRPGEdGui.EndScrollView();
		}
	}

	// ================================================================================================================

	private void OnSkillSelected1(System.Object sender)
	{
		SkillSelectWiz wiz = sender as SkillSelectWiz;
		SetStartingSkillSlot1(wiz.helper, wiz.selectedSkill.gameObject);
		EditorUtility.SetDirty(Target);
		wiz.Close();
		Repaint();
	}

	private void OnSkillSelected2(System.Object sender)
	{
		SkillSelectWiz wiz = sender as SkillSelectWiz;
		SetStartingSkillSlot2(wiz.helper, wiz.selectedSkill.gameObject);
		EditorUtility.SetDirty(Target);
		wiz.Close();
		Repaint();
	}

	private void OnActorClassSelected(System.Object sender)
	{
		ActorClassSelectWiz wiz = sender as ActorClassSelectWiz;
		Target.actorClassPrefab = wiz.selectedClass;
		EditorUtility.SetDirty(Target);
		wiz.Close();
	}

	private void OnStateSelected(System.Object sender)
	{
		StateSelectWiz wiz = sender as StateSelectWiz;
		Target.startingStates.Add(wiz.selected);
		EditorUtility.SetDirty(Target);
		wiz.Close();
	}

	private void OnRPGItemSelected(object sender, object[] args)
	{
		ItemSelectWiz wiz = sender as ItemSelectWiz;

		if (selectingForSlot >= 0)
		{	// selected for equip slot
			EquipTarget(selectingForSlot, wiz.selectedItems[0]);
		}
		else
		{	// selected for bag
			foreach (RPGItem item in wiz.selectedItems) Target.AddToBag(item, 1);
		}

		wiz.Close();

		EditorUtility.SetDirty(Target);
		selectingForSlot = -1; // done
		this.Repaint();
	}

	// ================================================================================================================

	private void Cache1()
	{
		skillz1 = new List<RPGSkill>(Target.actionSlotCount);
		for (int i = 0; i < Target.actionSlotCount; i++)
		{
			skillz1.Add(null);
			if (i >= Target.startingSkillSlots.Count) break;
			if (Target.startingSkillSlots[i]) skillz1[i] = Target.startingSkillSlots[i].GetComponent<RPGSkill>();
		}
	}

	private void Cache2()
	{
		skillz2 = new List<RPGSkill>(Target.startingSkills.Count);
		for (int i = 0; i < Target.startingSkills.Count; i++)
		{
			skillz2.Add(null);
			if (Target.startingSkills[i]) skillz2[i] = Target.startingSkills[i].GetComponent<RPGSkill>();
		}
	}

	private void CheckStartingSkillSlotsSize()
	{
		if (Target.startingSkillSlots.Count != Target.actionSlotCount)
		{
			Target.startingSkillSlots = new List<GameObject>(Target.actionSlotCount);
			skillz1 = new List<RPGSkill>(Target.actionSlotCount);
			for (int i = 0; i < Target.actionSlotCount; i++)
			{
				Target.startingSkillSlots.Add(null);
				skillz1.Add(null);
			}
		}
	}

	public string GetStartingSkillSlotName1(int slot)
	{
		if (slot >= 0 && slot < skillz1.Count)
		{
			if (slot >= Target.startingSkillSlots.Count) return "-";
			if (Target.startingSkillSlots[slot] == null) return "-";
			if (skillz1[slot] != null) return skillz1[slot].screenName;
		}
		return "-";
	}

	public string GetStartingSkillSlotName2(int slot)
	{
		if (slot >= 0 && slot < skillz2.Count)
		{
			if (skillz2[slot] != null) return skillz2[slot].screenName;
		}
		return "-";
	}

	public RPGSkill GetStartingSkillFromSlot1(int slot)
	{
		if (slot >= 0 && slot < skillz1.Count) return skillz1[slot];
		return null;
	}

	public RPGSkill GetStartingSkillFromSlot2(int slot)
	{
		if (slot >= 0 && slot < skillz2.Count) return skillz2[slot];
		return null;
	}

	public void SetStartingSkillSlot1(int slot, GameObject skillPrefab)
	{
		CheckStartingSkillSlotsSize();
		if (slot >= 0 && slot < Target.startingSkillSlots.Count)
		{
			// 1st "clear" whatever might be in the slot (do it like this so that rules can run if needed)
			if (Target.startingSkillSlots[slot] != null) ClearStartingSkillSlot(slot);

			// clear might have made the list size = 0, fix that (only happens in editor mode)
			CheckStartingSkillSlotsSize();
			Target.startingSkillSlots[slot] = skillPrefab;
			skillz1[slot] = skillPrefab.GetComponent<RPGSkill>();
		}
	}

	public void SetStartingSkillSlot2(int slot, GameObject skillPrefab)
	{
		if (slot >= 0 && slot < Target.startingSkills.Count)
		{
			Target.startingSkills[slot] = skillPrefab;
			skillz2[slot] = skillPrefab.GetComponent<RPGSkill>();
		}
	}

	public void ClearStartingSkillSlot(int slot)
	{
		if (slot >= 0 && slot < Target.startingSkillSlots.Count)
		{
			if (Target.startingSkillSlots[slot] == null) return;

			Target.startingSkillSlots[slot] = null;
			skillz1[slot] = null;

			// now check if there is at least one slot that is not null, else set whole list as empty (only during edit/design time)
			bool foundOne = false;
			foreach (GameObject s in Target.startingSkillSlots)
			{
				if (s != null) { foundOne = true; break; }
			}
			if (!foundOne)
			{
				Target.startingSkillSlots = new List<GameObject>(0);
				skillz1 = new List<RPGSkill>(0);
			}
		}
	}

	public bool EquipTarget(int slot, RPGItem item)
	{
		if (!Target.CanEquip(slot, item)) return false;

		// attempt equip
		Target.CheckEquipSlotsSize(UniRPGEditorGlobal.DB);
		if (slot >= 0 && slot < Target.equipped.Count)
		{
			// 1st "unequip" whatever might be in the slot
			if (Target.equipped[slot] != null) UnEquipTarget(slot);

			// unequip might have made the list size = 0, fix that (only happens in editor mode)
			Target.CheckEquipSlotsSize(UniRPGEditorGlobal.DB);

			// now equip
			Target.equipped[slot] = item;

			// run the equip action of the item
			// !!!! Actions cannot be run in edit mode
			//GameController.ExecuteActions(item.onEquipActions, item.gameObject, null, null, Target.gameObject);

			return true;
		}
		return false;
	}

	public void UnEquipTarget(int slot)
	{
		if (slot >= 0 && slot < Target.equipped.Count)
		{
			if (Target.equipped[slot] == null) return;

			// run the unequip rules of the item
			// !!!! Actions cannot be run in edit mode
			//GameController.ExecuteActions(Target.equipped[slot].onUnEquipActions, Target.equipped[slot].gameObject, null, null, Target.gameObject);

			// unequip
			Target.equipped[slot] = null;

			// now check if there is at least one slot that is not null, else set whole list as empty (only during edit/design time)
			bool foundOne = false;
			foreach (RPGItem i in Target.equipped)
			{
				if (i != null) { foundOne = true; break; }
			}
			if (!foundOne) Target.equipped = new List<RPGItem>(0);
		}
	}

	// ================================================================================================================
} }