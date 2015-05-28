// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

[DatabaseEditor("Classes", Priority = 7)]
public class RPGActorClassEditor : DatabaseEdBase
{
	// ================================================================================================================
	#region vars

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero };
	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activeIcon = 0;

	private RPGActorClass curr = null;		// currently being edited
	private RPGActorClass del = null;		// helper when deleting
	private RPGAttributeData selAttrib = null;
	private RPGAttributeData delAttrib = null;

	private int selLevelAttribIdx = -1;
	private bool showLevelAttribError = false;

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
			if (GUILayout.Button(new GUIContent("Add Class", UniRPGEdGui.Icon_Plus), EditorStyles.miniButtonLeft))
			{
				GUI.FocusControl("");
				curr = ScriptableObject.CreateInstance<RPGActorClass>();
				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_CLASSES_FILE);
				curr.screenName = "ActorClass";
				ed.db.classes.Add(curr);
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
				selAttrib = null;
			}
			if (curr == null) GUI.enabled = false;
			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Copy, "Copy"), EditorStyles.miniButtonMid))
			{
				GUI.FocusControl("");
				RPGActorClass newAC = ScriptableObject.CreateInstance<RPGActorClass>();

				curr.CopyTo(newAC);
				curr = newAC;

				UniRPGEdUtil.AddObjectToAssetFile(curr, UniRPGEditorGlobal.DB_DEF_CLASSES_FILE);
				
				ed.db.classes.Add(curr);
				EditorUtility.SetDirty(curr);
				EditorUtility.SetDirty(ed.db);
				selAttrib = null;
			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		{
			if (ed.db.classes.Count > 0)
			{
				foreach (RPGActorClass charaClass in ed.db.classes)
				{
					Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(DatabaseEditor.LeftPanelWidth - 20), GUILayout.ExpandWidth(false));
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (charaClass.icon[0] != null ? charaClass.icon[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(curr == charaClass, charaClass.screenName, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(140), GUILayout.ExpandWidth(false)))
						{
							selAttrib = null;
							curr = charaClass;
							GUI.FocusControl("");
							CheckXPAttrib();
						}
						if (ed.db.classes.Count == 1) GUI.enabled = false; // can't allow deleting the class if there is only one left since runtime depends on at least one being present
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
						{
							del = charaClass;
						}
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Classes are defined", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView();

		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();
		// -------------------------------------------------------------

		if (del != null)
		{
			if (curr == del)
			{
				curr = null;
				selAttrib = null;
			}
			ed.db.classes.Remove(del);
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

		GUILayout.Label("Class Definition", UniRPGEdGui.Head1Style);
		if (curr == null) { EditorGUILayout.EndVertical(); return; }
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);

		BasicInfo();
		Attributes();

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
		EditorGUILayout.BeginHorizontal(UniRPGEdGui.BoxStyle, GUILayout.Width(650));
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(350));
			{
				// name
				EditorGUI.BeginChangeCheck();
				curr.screenName = EditorGUILayout.TextField("Screen Name", curr.screenName);
				if (EditorGUI.EndChangeCheck()) UniRPGEditorGlobal.DB.ForceUpdateCache(0);

				curr.availAtStart = EditorGUILayout.Toggle("Available at start", curr.availAtStart);
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
						if (toolbar1Sel == 0) curr.description = EditorGUILayout.TextArea(curr.description, GUILayout.Height(100), GUILayout.ExpandHeight(false));
						else curr.notes = EditorGUILayout.TextArea(curr.notes, GUILayout.Height(100), GUILayout.ExpandHeight(false));
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndVertical();
			UniRPGEdGui.DrawVerticalLine(1f, UniRPGEdGui.DividerColor, 5f, 10f, 200f);
			EditorGUILayout.BeginVertical(GUILayout.Width(100));
			{
				LevelingSettings();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void Attributes()
	{
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(650));
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Attributes", UniRPGEdGui.Head3Style);
				GUILayout.Space(20);
				if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButtonLeft, GUILayout.Width(80)))
				{
					AttributeSelectWiz.Show(OnAttributeSelected, null);
				}
				if (UniRPGEdGui.IconButton("All", UniRPGEdGui.Icon_Refresh, EditorStyles.miniButtonRight, GUILayout.Width(80)))
				{
					foreach (RPGAttribute a in UniRPGEditorGlobal.DB.attributes)
					{
						if (!ContainAttribType(curr.attribDataFabs, a.id))
						{
							//curr.attribDataFabs.Add(a.data.Copy());
							RPGAttributeData data = new RPGAttributeData();
							data.attribId = a.id;
							curr.attribDataFabs.Add(data);
						}
					}
					EditorUtility.SetDirty(curr);
					EditorUtility.SetDirty(ed.db);
					CheckXPAttrib();
					ResetLevelGraph(curr.maxLevel, curr.maxXP);
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();
			{
				// attrib list
				scroll[2] = UniRPGEdGui.BeginScrollView(scroll[2], UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Width(160), GUILayout.Height(230));
				AttributesList();
				UniRPGEdGui.EndScrollView();
				EditorGUILayout.Space();

				// details of selected attrib
				EditorGUILayout.BeginVertical();
				AttributeValues();
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

		}
		EditorGUILayout.EndHorizontal();

	}

	private void AttributesList()
	{
		if (curr.attribDataFabs.Count > 0)
		{
			GUIStyle back = UniRPGEdGui.ListItemBackDarkStyle;
			foreach (RPGAttributeData attrib in curr.attribDataFabs)
			{
				EditorGUILayout.BeginHorizontal();
				{
					RPGAttribute att = null;
					if (attrib != null) att = RPGAttribute.GetAttribByGuid(UniRPGEditorGlobal.DB.attributes, attrib.attribId);

					if (att == null) 
					{						
						GUILayout.Label("Error: NULL", back, GUILayout.ExpandWidth(true));
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(25))) delAttrib = attrib;
						GUILayout.Space(5);
					}
					else
					{
						if (UniRPGEdGui.ToggleButton(attrib == selAttrib, att.screenName, UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(110), GUILayout.ExpandWidth(false)))
						{
							selAttrib = attrib;
							GUI.FocusControl("");
						}
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
						{
							delAttrib = attrib;
						}
					}
					GUILayout.Space(10);
				}
				EditorGUILayout.EndHorizontal();
				back = (back == UniRPGEdGui.ListItemBackDarkStyle ? UniRPGEdGui.ListItemBackLightStyle : UniRPGEdGui.ListItemBackDarkStyle);
			}

			if (delAttrib != null)
			{
				if (selAttrib == delAttrib) selAttrib = null;
				curr.attribDataFabs.Remove(delAttrib);
				delAttrib = null;
				EditorUtility.SetDirty(curr);
				CheckXPAttrib();
			}
		}
	}

	private void AttributeValues()
	{
		if (selAttrib != null)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical();
				{
					EditorGUIUtility.LookLikeControls(135, 30);
					selAttrib.baseVal = EditorGUILayout.FloatField("Base Value", selAttrib.baseVal);
					selAttrib.baseMin = EditorGUILayout.FloatField("Min Base Value", selAttrib.baseMin);

					EditorGUI.BeginChangeCheck();
					selAttrib.baseMax = EditorGUILayout.FloatField("Max Base Value", selAttrib.baseMax);
					if (EditorGUI.EndChangeCheck())
					{
						if (selAttrib.attribId == curr.xpAttribId) ResetLevelGraph(curr.maxLevel, (int)selAttrib.baseMax);
					}

					EditorGUILayout.Space();
					EditorGUIUtility.LookLikeControls(150, 20);
					selAttrib.canRegen = EditorGUILayout.Toggle("Can Regenerate", selAttrib.canRegen);
					EditorGUILayout.Space();
					if (selAttrib.canRegen)
					{
						EditorGUIUtility.LookLikeControls(135, 30);
						selAttrib.regenRate = EditorGUILayout.FloatField("Regen Rate (per sec)", selAttrib.regenRate);
						selAttrib.regenAfterTimeout = EditorGUILayout.FloatField("Regen after Timeout", selAttrib.regenAfterTimeout);
						EditorGUILayout.Space();
					}
					EditorGUIUtility.LookLikeControls(150, 20);
					selAttrib.levelAffectVal = EditorGUILayout.Toggle("Level affects Base Value", selAttrib.levelAffectVal);
					selAttrib.levelAffectMax = EditorGUILayout.Toggle("Level affects Max Value", selAttrib.levelAffectMax);
					EditorGUIUtility.LookLikeControls();

				}
				EditorGUILayout.EndVertical();
				UniRPGEdGui.DrawVerticalLine(1f, UniRPGEdGui.DividerColor, 5f, 10f, 230f);
				EditorGUILayout.BeginVertical(GUILayout.Width(250));
				{
					GUILayout.Label("Execute on ...", UniRPGEdGui.Head3Style);

					RPGEvent[] s = { null, null, null };
					if (selAttrib.onValChangeEventPrefab) s[0] = selAttrib.onValChangeEventPrefab.GetComponent<RPGEvent>();
					if (selAttrib.onMinValueEventPrefab) s[1] = selAttrib.onMinValueEventPrefab.GetComponent<RPGEvent>();
					if (selAttrib.onMaxValueEventPrefab) s[2] = selAttrib.onMaxValueEventPrefab.GetComponent<RPGEvent>();

					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Value Change", GUILayout.Width(100));
					if (GUILayout.Button(s[0] == null ? "-" : s[0].screenName, UniRPGEdGui.ButtonLeftStyle)) EventSelectWiz.Show(OnEventSelected, null, 0);
					if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) { selAttrib.onValChangeEventPrefab = null; EditorUtility.SetDirty(curr); EditorUtility.SetDirty(ed.db); }
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Min Value", GUILayout.Width(100));
					if (GUILayout.Button(s[1] == null ? "-" : s[1].screenName, UniRPGEdGui.ButtonLeftStyle)) EventSelectWiz.Show(OnEventSelected, null, 1);
					if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) { selAttrib.onMinValueEventPrefab = null; EditorUtility.SetDirty(curr); EditorUtility.SetDirty(ed.db); }
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Max Value", GUILayout.Width(100));
					if (GUILayout.Button(s[2] == null ? "-" : s[2].screenName, UniRPGEdGui.ButtonLeftStyle)) EventSelectWiz.Show(OnEventSelected, null, 2);
					if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) { selAttrib.onMaxValueEventPrefab = null; EditorUtility.SetDirty(curr); EditorUtility.SetDirty(ed.db); }
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();
					LevelAffectAttrib();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	private void OnEventSelected(System.Object sender)
	{
		EventSelectWiz wiz = sender as EventSelectWiz;
		if (selAttrib != null)
		{
			if (wiz.helper == 0) selAttrib.onValChangeEventPrefab = wiz.selectedEvent.gameObject;
			else if (wiz.helper == 1) selAttrib.onMinValueEventPrefab = wiz.selectedEvent.gameObject;
			else if (wiz.helper == 2) selAttrib.onMaxValueEventPrefab = wiz.selectedEvent.gameObject;
		}
		EditorUtility.SetDirty(curr);
		EditorUtility.SetDirty(ed.db);
		wiz.Close();
		ed.Repaint();
	}

	private void OnAttributeSelected(object sender, object[] args)
	{
		AttributeSelectWiz wiz = sender as AttributeSelectWiz;
		if (!ContainAttribType(curr.attribDataFabs, wiz.selectedAttrib.id))
		{
			//selAttrib = wiz.selectedAttrib.data.Copy();
			//curr.attribDataFabs.Add(selAttrib);
			selAttrib = new RPGAttributeData();
			selAttrib.attribId = wiz.selectedAttrib.id;
			curr.attribDataFabs.Add(selAttrib);

			EditorUtility.SetDirty(curr);
			CheckXPAttrib();
			ResetLevelGraph(curr.maxLevel, curr.maxXP);
		}
		wiz.Close();
		ed.Repaint();
	}

	private bool ContainAttribType(List<RPGAttributeData> l, GUID id)
	{
		foreach (RPGAttributeData a in l)
		{
			if (a.attribId.Value == id.Value) return true;
		}
		return false;
	}

	private void LevelAffectAttrib()
	{
		if (selAttrib.levelAffectVal)
		{
			if (selAttrib.valAffectCurve == null) selAttrib.valAffectCurve = AnimationCurve.Linear(1, selAttrib.valAffectMin, curr.maxLevel, selAttrib.valAffectMax);
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.LookLikeControls(120, 30);
				selAttrib.valAffectMin = EditorGUILayout.FloatField("Base Value clamp", selAttrib.valAffectMin);
				EditorGUIUtility.LookLikeControls(20, 30);
				selAttrib.valAffectMax = EditorGUILayout.FloatField(" - ", selAttrib.valAffectMax);
				if (EditorGUI.EndChangeCheck()) selAttrib.valAffectCurve = AnimationCurve.Linear(1, selAttrib.valAffectMin, curr.maxLevel, selAttrib.valAffectMax);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.LookLikeControls();
			selAttrib.valAffectCurve = EditorGUILayout.CurveField(selAttrib.valAffectCurve, GUILayout.Height(40));
		}
		else if (selAttrib.valAffectCurve!=null) selAttrib.valAffectCurve = null;

		if (selAttrib.levelAffectMax)
		{
			if (selAttrib.maxAffectCurve == null) selAttrib.maxAffectCurve = AnimationCurve.Linear(1, selAttrib.maxAffectMin, curr.maxLevel, selAttrib.maxAffectMax);
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.LookLikeControls(120, 30);
				selAttrib.maxAffectMin = EditorGUILayout.FloatField("Max Value clamp", selAttrib.maxAffectMin);
				EditorGUIUtility.LookLikeControls(20, 30);
				selAttrib.maxAffectMax = EditorGUILayout.FloatField(" - ", selAttrib.maxAffectMax);
				if (EditorGUI.EndChangeCheck()) selAttrib.maxAffectCurve = AnimationCurve.Linear(1, selAttrib.maxAffectMin, curr.maxLevel, selAttrib.maxAffectMax);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.LookLikeControls();
			selAttrib.maxAffectCurve = EditorGUILayout.CurveField(selAttrib.maxAffectCurve, GUILayout.Height(40));
		}
		else if (selAttrib.maxAffectCurve != null) selAttrib.maxAffectCurve = null;
	}

	private void LevelingSettings()
	{
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("Leveling Settings", UniRPGEdGui.Head4Style);
			EditorGUILayout.Space();
			if (showLevelAttribError) GUILayout.Label("(this attribute must be in the class' list of attributes)", UniRPGEdGui.WarningLabelStyle);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUIUtility.LookLikeControls(135, 80);
			EditorGUI.BeginChangeCheck();
			selLevelAttribIdx = EditorGUILayout.Popup("Experience Attribute", selLevelAttribIdx, UniRPGEditorGlobal.DB.AttributeNames);
			if (EditorGUI.EndChangeCheck())
			{
				curr.xpAttribId.Value = UniRPGEditorGlobal.DB.attributes[selLevelAttribIdx].id.Value;
				CheckXPAttrib();
				ResetLevelGraph(curr.maxLevel, curr.maxXP);
			}
			if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
			{
				curr.xpAttribId = new GUID();
				CheckXPAttrib();
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUIUtility.LookLikeControls();
		if (!showLevelAttribError)
		{
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			int prevLv = curr.maxLevel;
			curr.maxLevel = EditorGUILayout.IntField("Max Level", curr.maxLevel);
			curr.maxXP = EditorGUILayout.IntField("Max XP", curr.maxXP);
			if (EditorGUI.EndChangeCheck())
			{
				ResetLevelGraph(curr.maxLevel, curr.maxXP);

				if (prevLv != curr.maxLevel)
				{	// level changed, update the graphs for level affected attribs
					foreach (RPGAttributeData att in curr.attribDataFabs)
					{
						if (att.levelAffectVal) att.valAffectCurve = AnimationCurve.Linear(1, att.valAffectMin, curr.maxLevel, att.valAffectMax);
						if (att.levelAffectMax) att.maxAffectCurve = AnimationCurve.Linear(1, att.maxAffectMin, curr.maxLevel, att.maxAffectMax);
					}
				}
			}
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical();
				{
					Rect r = GUILayoutUtility.GetRect(new GUIContent("A"), EditorStyles.label);
					Vector2 pivot = new Vector2(r.x, r.y);
					GUIUtility.RotateAroundPivot(-90, pivot);
					r.width = 100; r.x -= 40;
					GUI.Label(r, "Level");
					GUIUtility.RotateAroundPivot(90, pivot);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical();
				{
					curr.levelCurve = EditorGUILayout.CurveField(curr.levelCurve, GUILayout.Height(45));
					GUILayout.Label("Experience (xp)");
				}
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	private void CheckXPAttrib()
	{
		selLevelAttribIdx = RPGAttribute.GetAttribIdx(ed.db.attributes, curr.xpAttribId);
		showLevelAttribError = false;
		if (selLevelAttribIdx >= 0)
		{
			showLevelAttribError = true;
			foreach (RPGAttributeData att in curr.attribDataFabs)
			{
				if (att.attribId == curr.xpAttribId) { showLevelAttribError = false; break; }
			}
			ed.Repaint();
		}
	}

	private void ResetLevelGraph(int maxLevel, int maxXP)
	{
		curr.maxLevel = maxLevel;
		curr.maxXP = maxXP;
		curr.levelCurve = AnimationCurve.Linear(1, 1, curr.maxXP, curr.maxLevel);
		foreach (RPGAttributeData att in curr.attribDataFabs)
		{
			if (att.attribId == curr.xpAttribId)
			{
				att.baseMax = maxXP;
				break;
			}
		}
	}

	#endregion
	// ================================================================================================================
} }