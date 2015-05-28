// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using UniRPG;

namespace UniRPGEditor {

[DatabaseEditor("Attributes", Priority = 3)]
public class RPGAttributeEditor : DatabaseEdBase
{
	// ================================================================================================================
	#region vars

	private Vector2[] scroll = { Vector2.zero, Vector2.zero };
	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activeIcon = 0;

	private RPGAttribute curr = null;	// currently being edited
	private RPGAttribute del = null;	// helper when deleting

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
			if (GUILayout.Button(new GUIContent("Add Attribute", UniRPGEdGui.Icon_Plus), EditorStyles.miniButtonLeft))
			{
				GUI.FocusControl("");
				curr = ScriptableObject.CreateInstance<RPGAttribute>();
				curr.id = GUID.Create();
				ed.db.attributes.Add(curr);
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_ATTRIBS_FILE);
				curr.screenName = "Attribute";
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
			}
			if (curr == null) GUI.enabled = false;
			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Copy, "Copy"), EditorStyles.miniButtonMid))
			{
				GUI.FocusControl("");
				RPGAttribute att = ScriptableObject.CreateInstance<RPGAttribute>();
				curr.CopyTo(att, 1);
				att.id = GUID.Create();
				curr = att;
				ed.db.attributes.Add(curr);
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_ATTRIBS_FILE);
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);

			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		{
			if (ed.db.attributes.Count > 0)
			{
				foreach (RPGAttribute attrib in ed.db.attributes)
				{
					Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(DatabaseEditor.LeftPanelWidth - 20), GUILayout.ExpandWidth(false));
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (attrib.icon[0] != null ? attrib.icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(curr == attrib, attrib.screenName, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(140), GUILayout.ExpandWidth(false)))
						{
							curr = attrib;
							GUI.FocusControl("");
						}
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
						{
							del = attrib;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Attributes are defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView(); // 0

		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();

		if (del != null)
		{
			if (curr == del) curr = null;
			ed.db.attributes.Remove(del);
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

		GUILayout.Label("Attribute Definition", UniRPGEdGui.Head1Style);
		if (curr == null) { EditorGUILayout.EndVertical(); return; }
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);

		BasicInfo();
		//Rules();

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
			EditorGUI.BeginChangeCheck();
			curr.screenName = EditorGUILayout.TextField("Screen Name", curr.screenName);
			if (EditorGUI.EndChangeCheck()) UniRPGEditorGlobal.DB.ForceUpdateCache(2);

			curr.shortName = EditorGUILayout.TextField("Short Name", curr.shortName);
			curr.guiHelper = EditorGUILayout.TextField("GUI Helper", curr.guiHelper);
			curr.lootDropPrefab = (GameObject)EditorGUILayout.ObjectField("LootDrop Prefab", curr.lootDropPrefab, typeof(GameObject), false);
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
		}
		EditorGUILayout.EndVertical();
	}
	
	#endregion
	// ================================================================================================================
} }