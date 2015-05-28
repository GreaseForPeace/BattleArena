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

[CustomEditor(typeof(RPGObject))]
public class RPGObjectInspector : InteractableBaseEditor<RPGObject>
{
	private static readonly string[] toolbar1 = { "Description", "Notes" };
	private int toolbar1Sel = 0;
	private int activeIcon = 0;
	private static bool[] foldout = { true };
	private static bool[] foldout2 = { false, false };
	
	// ================================================================================================================

	public override void OnInspectorGUI()
	{
		UniRPGEdGui.UseSkin();
		GUILayout.Space(10f);

		BasicInfo();
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
		foldout2= DrawInteractableInspector(foldout2, false, false, false);

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
			// name
			EditorGUILayout.Space();
			Target.screenName = EditorGUILayout.TextField("Screen Name", Target.screenName);
			EditorGUILayout.Space();

			// icon, description and notes
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(100));
				{
					GUILayout.Label("Icon");
					EditorGUILayout.BeginHorizontal();
					{
						Target.icon[activeIcon] = (Texture2D)EditorGUILayout.ObjectField(Target.icon[activeIcon], typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
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
					if (toolbar1Sel == 0) Target.description = EditorGUILayout.TextArea(Target.description, GUILayout.Height(60), GUILayout.ExpandHeight(false));
					else Target.notes = EditorGUILayout.TextArea(Target.notes, GUILayout.Height(60), GUILayout.ExpandHeight(false));
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	public override void OnSceneGUI()
	{
		base.OnSceneGUI();
	}

	// ================================================================================================================
} }