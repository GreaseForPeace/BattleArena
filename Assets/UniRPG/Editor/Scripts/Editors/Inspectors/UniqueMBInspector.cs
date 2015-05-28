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

public class UniqueMBInspector<T> : InspectorBase<T> where T : UniqueMonoBehaviour
{
	private Vector2 mb_scroll = Vector2.zero;

	protected bool DrawMBInspector(bool foldout)
	{
		EditorGUILayout.BeginHorizontal();
		{
			foldout = UniRPGEdGui.Foldout(foldout, "Custom Variables", UniRPGEdGui.InspectorHeadFoldStyle);
			if (foldout)
			{
				EditorGUILayout.Space();
				if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
				{
					Target.customVars.Add(new StringVar() { name = "var" + Target.customVars.Count, val = string.Empty });
					EditorUtility.SetDirty(Target);
				}
				GUILayout.FlexibleSpace();
			}
		}
		EditorGUILayout.EndHorizontal();		

		if (foldout)
		{
			EditorGUILayout.Space();
			mb_scroll = UniRPGEdGui.BeginScrollView(mb_scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(100));
			{
				StringVar delStrVar = null;
				foreach (StringVar v in Target.customVars)
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUI.BeginChangeCheck();
						v.name = EditorGUILayout.TextField(v.name, GUILayout.Width(120));
						if (EditorGUI.EndChangeCheck())
						{	// make sure te name dont contain an invalid character like "|" which is used by save system
							if (v.name.Contains("|")) v.name = v.name.Replace("|", "");
						}
						GUILayout.Space(5);
						v.val = EditorGUILayout.TextField(v.val);
						GUILayout.Space(5);
						if (GUILayout.Button("X", GUILayout.Width(20))) delStrVar = v;
						GUILayout.Space(20);
					}
					EditorGUILayout.EndHorizontal();
				}

				if (delStrVar != null)
				{
					Target.customVars.Remove(delStrVar);
					delStrVar = null;
					EditorUtility.SetDirty(Target);
				}
			}
			UniRPGEdGui.EndScrollView();
		}

		return foldout;
	}

	// ================================================================================================================
} }
