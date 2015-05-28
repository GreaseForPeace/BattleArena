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

[DatabaseEditor("Loot", Priority = 8)]
public class RPGLootEditor : DatabaseEdBase
{
	// ================================================================================================================
	#region vars

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero };

	private RPGLoot curr = null;	// currently being edited
	private RPGLoot del = null;		// helper when deleting
	private int rewardId = 0;

	#endregion
	// ================================================================================================================
	#region pub

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
			if (GUILayout.Button(new GUIContent("Add Loot", UniRPGEdGui.Icon_Plus), EditorStyles.miniButtonLeft))
			{
				GUI.FocusControl("");
				curr = ScriptableObject.CreateInstance<RPGLoot>();
				curr.id = GUID.Create();
				ed.db.loot.Add(curr);
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_LOOT_FILE);
				curr.screenName = "Loot";
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
			}
			if (curr == null) GUI.enabled = false;
			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Copy, "Copy"), EditorStyles.miniButtonMid))
			{
				GUI.FocusControl("");
				RPGLoot loot = ScriptableObject.CreateInstance<RPGLoot>();
				curr.CopyTo(loot);
				loot.id = GUID.Create(); // needs to be diffent from copied loot's
				curr = loot;
				ed.db.loot.Add(curr);
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_LOOT_FILE);
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		{
			if (ed.db.loot.Count > 0)
			{
				foreach (RPGLoot loot in ed.db.loot)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(DatabaseEditor.LeftPanelWidth - 20), GUILayout.ExpandWidth(false));
					{
						if (UniRPGEdGui.ToggleButton(curr == loot, loot.screenName, UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(160), GUILayout.ExpandWidth(false)))
						{
							curr = loot;
							GUI.FocusControl("");
						}
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
						{
							del = loot;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Loot defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView(); // 0

		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();

		if (del != null)
		{
			if (curr == del) curr = null;
			ed.db.loot.Remove(del);
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

		GUILayout.Label("Loot Definition", UniRPGEdGui.Head1Style);
		if (curr == null) { EditorGUILayout.EndVertical(); return; }
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);

		BasicInfo();

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
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(600));
		{
			EditorGUI.BeginChangeCheck();
			curr.screenName = EditorGUILayout.TextField("Name", curr.screenName);
			if (EditorGUI.EndChangeCheck()) UniRPGEditorGlobal.DB.ForceUpdateCache(3);

			EditorGUILayout.Space();
			curr.dropAsBag = EditorGUILayout.Toggle("Drop as Bag", curr.dropAsBag);
			if (curr.dropAsBag == false)
			{
				GUI.enabled = false;
				curr.bagPrefab = null;
			}
			curr.bagPrefab = (GameObject)EditorGUILayout.ObjectField("Bag Prefab", curr.bagPrefab, typeof(GameObject), false);
			GUI.enabled = true;

			GUILayout.Space(20);
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Loot Table", UniRPGEdGui.Head3Style);
				GUILayout.Space(20);
				if (UniRPGEdGui.IconButton(" Item", UniRPGEdGui.Icon_Plus, EditorStyles.miniButtonLeft, GUILayout.Width(80)))
				{
					curr.rewards.Add( new RPGLoot.Reward() { type = RPGLoot.Reward.RewardType.Item });
					EditorUtility.SetDirty(curr);
				}
				if (UniRPGEdGui.IconButton(" Currency", UniRPGEdGui.Icon_Plus, EditorStyles.miniButtonMid, GUILayout.Width(100)))
				{
					curr.rewards.Add( new RPGLoot.Reward() { type = RPGLoot.Reward.RewardType.Currency });
					EditorUtility.SetDirty(curr);
				}
				if (UniRPGEdGui.IconButton(" Attribute", UniRPGEdGui.Icon_Plus, EditorStyles.miniButtonRight, GUILayout.Width(100)))
				{
					curr.rewards.Add( new RPGLoot.Reward() { type = RPGLoot.Reward.RewardType.Attribute });
					EditorUtility.SetDirty(curr);
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			scroll[2] = UniRPGEdGui.BeginScrollView(scroll[2], UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(350));
			{
				int delReward = -1;
				for (int i = 0; i < curr.rewards.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20))) delReward = i;						

						if (curr.rewards[i].type == RPGLoot.Reward.RewardType.Currency)
						{
							GUILayout.Label(UniRPGEditorGlobal.DB.currency, GUILayout.Width(100));
							curr.rewards[i].count = EditorGUILayout.FloatField(curr.rewards[i].count, GUILayout.Width(40));
						}

						if (curr.rewards[i].type == RPGLoot.Reward.RewardType.Item)
						{
							RPGItem item = UniRPGEditorGlobal.DB.GetItem(curr.rewards[i].guid);
							if (item == null)
							{
								if (curr.rewards[i].guid != null) curr.rewards[i].guid = null;
							}	
							if (GUILayout.Button(item == null? "-" : item.screenName, EditorStyles.miniButton, GUILayout.Width(100)))
							{
								rewardId = i;
								ItemSelectWiz.Show(false, OnItemSelect, null);
							}
							curr.rewards[i].count = (int)EditorGUILayout.IntField((int)curr.rewards[i].count, GUILayout.Width(40));
						}

						if (curr.rewards[i].type == RPGLoot.Reward.RewardType.Attribute)
						{
							RPGAttribute attr = RPGAttribute.GetAttribByGuid(UniRPGEditorGlobal.DB.attributes, curr.rewards[i].guid);
							if (attr == null)
							{
								if (curr.rewards[i].guid != null) curr.rewards[i].guid = null;
							}							
							if (GUILayout.Button(attr == null ? "-" : attr.screenName, EditorStyles.miniButton, GUILayout.Width(100)))
							{
								rewardId = i;
								AttributeSelectWiz.Show(OnAttribSelect, null);
							}
							curr.rewards[i].count = EditorGUILayout.FloatField(curr.rewards[i].count, GUILayout.Width(40));
						}

						GUILayout.Space(10);
						curr.rewards[i].chance = EditorGUILayout.Slider(curr.rewards[i].chance, 0, 100, GUILayout.Width(150));
						GUILayout.Label("%");
						GUILayout.Space(5);

						curr.rewards[i].lookatNpcLevel = GUILayout.Toggle(curr.rewards[i].lookatNpcLevel, " NPC level >=");
						GUI.enabled = curr.rewards[i].lookatNpcLevel;
						curr.rewards[i].npcMaxLevel = EditorGUILayout.IntField(curr.rewards[i].npcMaxLevel, GUILayout.Width(30));
						GUI.enabled = true;

						GUILayout.Label(" Gr");
						curr.rewards[i].group = EditorGUILayout.IntField(curr.rewards[i].group, GUILayout.Width(30));

						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}
				
				if (delReward >= 0)
				{
					curr.rewards.RemoveAt(delReward);
					EditorUtility.SetDirty(curr);
				}
			}
			UniRPGEdGui.EndScrollView();
		}
		EditorGUILayout.EndVertical();
	}

	private void OnAttribSelect(object sender, object[] args)
	{
		AttributeSelectWiz wiz = sender as AttributeSelectWiz;
		curr.rewards[rewardId].guid = new GUID(wiz.selectedAttrib.id.ToString());
		EditorUtility.SetDirty(curr);
		wiz.Close();
		ed.Repaint();
	}

	private void OnItemSelect(object sender, object[] args)
	{
		ItemSelectWiz wiz = sender as ItemSelectWiz;
		curr.rewards[rewardId].guid = new GUID(wiz.selectedItems[0].prefabId.ToString());
		EditorUtility.SetDirty(curr);
		wiz.Close();
		ed.Repaint();
	}

	#endregion
	// ================================================================================================================
} }
