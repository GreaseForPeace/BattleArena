// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================
	
using UnityEngine;
using UnityEditor;
using System.Collections;
using DiaQ;

namespace DiaQEditor
{
	public class DiaQDecisionEd
	{
		private static readonly string[] ComBineOptions = { "and", "or" };

		private static readonly string[] ValTypeNames1 = { "True", "False" };
		private static readonly string[] ValTypeNames2 = { "Empty", "Value", "DiaQVar" };
		
		private static readonly string[] TestTypeNames1 = { "==", "!=" };
		//private static readonly string[] TestTypeNames2 = { "==", "!=", ">", "<", ">=", "<=" };

		public static void OnGUI(EditorWindow ed, DiaQDecision d, DiaQAsset asset, float areaWidth)
		{
			float w = (areaWidth - 104) * 0.5f;

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Evaluation");
				if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) d.tests.Add(new DiaQDecisionTest());
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			DiaQDecisionTest del = null;
			bool first = true;
			foreach (DiaQDecisionTest t in d.tests)
			{
				GUILayout.BeginHorizontal();
				{
					// delete button
					if (GUILayout.Button("x", EditorStyles.miniButtonLeft, GUILayout.Width(20))) del = t;
					
					// how to combine with previous test
					if (!first) t.combineWithPrev = EditorGUILayout.Popup(t.combineWithPrev, ComBineOptions, EditorStyles.miniButtonMid, GUILayout.Width(30));
					else GUILayout.Button(" ", EditorStyles.miniButtonMid, GUILayout.Width(30));
					first = false;
					
					ShowVariableField(ed, t, asset, w);
					ShowValueField(ed, t, asset, w);

					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			if (del != null) d.tests.Remove(del);
		}

		// ============================================================================================================

		private static void ShowVariableField(EditorWindow ed, DiaQDecisionTest t, DiaQAsset asset, float w)
		{
			GUILayout.BeginVertical();
			{
				// show main variable choise
				t.varType = (DiaQDecisionTest.VarType)EditorGUILayout.EnumPopup(t.varType, EditorStyles.miniButtonMid, GUILayout.Width(w));

				// show additional field for quest related var
				if (t.varType == DiaQDecisionTest.VarType.HasSeenPlayer)
				{
					if (t.valType != DiaQDecisionTest.ValType.True && t.valType != DiaQDecisionTest.ValType.False) t.valType = DiaQDecisionTest.ValType.True;
				}

				else if (t.varType == DiaQDecisionTest.VarType.AcceptedQuest || t.varType == DiaQDecisionTest.VarType.CompletedQuest || t.varType == DiaQDecisionTest.VarType.HandedInQuest)
				{
					if (t.valType != DiaQDecisionTest.ValType.True && t.valType != DiaQDecisionTest.ValType.False) t.valType = DiaQDecisionTest.ValType.True;
					if (t.s_opt1.Length < 2) t.s_opt1 = new string[] { "", "-" };
					if (GUILayout.Button(t.s_opt1[1], EditorStyles.miniButton, GUILayout.Width(w - 16))) DiaQuestSelectWiz.ShowWiz(_OnSelectedQuest, asset, new object[] { t, ed, asset });
				}

				else if (t.varType == DiaQDecisionTest.VarType.DiaQVariable)
				{
					if (t.valType < DiaQDecisionTest.ValType.Empty || t.valType > DiaQDecisionTest.ValType.DiaQVar) t.valType = DiaQDecisionTest.ValType.Empty;
					if (t.s_opt1.Length < 1) t.s_opt1 = new string[] { "" };
					GUILayout.BeginHorizontal();
					t.s_opt1[0] = EditorGUILayout.TextField(t.s_opt1[0], GUILayout.Width(w - 46));
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnSelectedVar, (s) => { t.s_opt1[0] = s; }, asset, new object[] { ed, asset });
					GUILayout.EndHorizontal();
				}

			}
			GUILayout.EndVertical();

			// --------------------------------------------------------------------------------------------------------
			// show test types that can be performed on this type of variable
			if (t.varType >= DiaQDecisionTest.VarType.HasSeenPlayer && t.varType <= DiaQDecisionTest.VarType.DiaQVariable)
			{
				t.testType = (DiaQDecisionTest.TestType)EditorGUILayout.Popup((int)t.testType, TestTypeNames1, EditorStyles.miniButtonMid, GUILayout.Width(30));
			}

		}

		// ============================================================================================================

		private static void ShowValueField(EditorWindow ed, DiaQDecisionTest t, DiaQAsset asset, float w)
		{
			GUILayout.BeginVertical();
			{
				if (t.varType >= DiaQDecisionTest.VarType.HasSeenPlayer && t.varType <= DiaQDecisionTest.VarType.HandedInQuest)
				{
					t.valType = (DiaQDecisionTest.ValType)EditorGUILayout.Popup((int)t.valType, ValTypeNames1, EditorStyles.miniButtonRight, GUILayout.Width(w));
				}

				else if (t.varType == DiaQDecisionTest.VarType.DiaQVariable)
				{
					int sel = ((int)t.valType) - 2;
					if (sel < 0) sel = 0;
					EditorGUI.BeginChangeCheck();
					sel = EditorGUILayout.Popup(sel, ValTypeNames2, EditorStyles.miniButtonRight, GUILayout.Width(w));
					if (EditorGUI.EndChangeCheck())
					{
						t.valType = (DiaQDecisionTest.ValType)(sel + 2);
						t.s_opt2 = (t.valType == DiaQDecisionTest.ValType.Empty ? null : new string[] { "" });
					}

					if (t.valType == DiaQDecisionTest.ValType.String)
					{
						t.s_opt2[0] = EditorGUILayout.TextField(t.s_opt2[0], GUILayout.Width(w - 16));
					}
					else if (t.valType == DiaQDecisionTest.ValType.DiaQVar)
					{
						GUILayout.BeginHorizontal();
						t.s_opt2[0] = EditorGUILayout.TextField(t.s_opt2[0], GUILayout.Width(w-46)); 
						if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnSelectedVar, (s) => { t.s_opt2[0] = s; }, asset, new object[] { ed, asset });
						GUILayout.EndHorizontal();
					}

				}

			}
			GUILayout.EndVertical();
		}

		// ============================================================================================================

		private static void _OnSelectedQuest(DiaQuestSelectWiz wiz, object[] args)
		{
			DiaQDecisionTest t = args[0] as DiaQDecisionTest;
			EditorWindow ed = args[1] as EditorWindow;
			DiaQAsset asset = args[2] as DiaQAsset;
			if (t != null)
			{
				t.s_opt1 = new string[2];
				t.s_opt1[0] = wiz.selected.IdentString;
				t.s_opt1[1] = wiz.selected.name;
			}
			wiz.Close();
			if (ed != null) ed.Repaint();
			if (asset != null) EditorUtility.SetDirty(asset);
		}

		private static void _OnSelectedVar(DiaQVarSelectWiz wiz, object[] args)
		{
			EditorWindow ed = args[0] as EditorWindow;
			DiaQAsset asset = args[1] as DiaQAsset;
			// note, actual variable setting happened in lambda
			wiz.Close();
			if (ed != null) ed.Repaint();
			if (asset != null) EditorUtility.SetDirty(asset);
		}

		// ============================================================================================================
	}
}