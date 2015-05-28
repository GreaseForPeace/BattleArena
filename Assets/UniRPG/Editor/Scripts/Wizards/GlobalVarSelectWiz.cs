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

public class GlobalVarSelectWiz : EditorWindow
{
	private bool lostFocus = false;
	private Vector2 scroll = Vector2.zero;

	private NumericValue numVal = null;
	private StringValue stringVal = null;
	private ObjectValue objVal = null;

	private EditorWindow ed = null;

	// ================================================================================================================

	public static void Show(StringValue stringVal, EditorWindow ed)
	{
		if (stringVal == null)
		{
			Debug.LogError("Invalid value");
			return;
		}

		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		GlobalVarSelectWiz window = EditorWindow.GetWindow<GlobalVarSelectWiz>(true, "Select Variable", true);
		window.inited = false;
		window.ed = ed;
		window.stringVal = stringVal;

		// show window
		window.ShowUtility();
	}

	public static void Show(NumericValue numVal, EditorWindow ed)
	{
		if (numVal == null) 
		{
			Debug.LogError("Invalid value");
			return;
		}

		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		GlobalVarSelectWiz window = EditorWindow.GetWindow<GlobalVarSelectWiz>(true, "Select Variable", true);
		window.inited = false; 
		window.ed = ed;
		window.numVal = numVal;

		// show window
		window.ShowUtility();
	}

	public static void Show(ObjectValue objVal, EditorWindow ed)
	{
		if (objVal == null)
		{
			Debug.LogError("Invalid value");
			return;
		}

		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		GlobalVarSelectWiz window = EditorWindow.GetWindow<GlobalVarSelectWiz>(true, "Select Variable",true);
		window.inited = false;
		window.ed = ed;
		window.objVal = objVal;

		// show window
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Variable";
		minSize = new Vector2(200, 350);
		maxSize = new Vector2(200, 350);
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) this.Close();
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();

		scroll = UniRPGEdGui.BeginScrollView(scroll);
		{
			// *** Numeric Vars
			if (numVal != null)
			{
				if (GUILayout.Button("none"))
				{
					numVal.numericVarName = null;
					lostFocus = true; // so that this window closes
					if (ed != null) ed.Repaint();
				}

				if (UniRPGEditorGlobal.DB.numericVars.Count > 0)
				{
					foreach (NumericVar v in UniRPGEditorGlobal.DB.numericVars)
					{
						if (string.IsNullOrEmpty(v.name)) continue;
						if (GUILayout.Button(v.name))
						{
							numVal.numericVarName = v.name;
							lostFocus = true; // so that this window closes
							if (ed != null) ed.Repaint();
						}
					}
				}
				else
				{
					GUILayout.Label("No variables defined", UniRPGEdGui.WarningLabelStyle);
				}
			}

			// *** String Vars
			else if (stringVal != null)
			{
				if (GUILayout.Button("none"))
				{
					stringVal.stringVarName = null;
					lostFocus = true; // so that this window closes
					if (ed != null) ed.Repaint();
				}

				if (UniRPGEditorGlobal.DB.stringVars.Count > 0)
				{
					foreach (StringVar v in UniRPGEditorGlobal.DB.stringVars)
					{
						if (string.IsNullOrEmpty(v.name)) continue;
						if (GUILayout.Button(v.name))
						{
							stringVal.stringVarName = v.name;
							lostFocus = true; // so that this window closes
							if (ed != null) ed.Repaint();
						}
					}
				}
				else
				{
					GUILayout.Label("No variables defined", UniRPGEdGui.WarningLabelStyle);
				}
			}

			// *** Object Vars
			else if (objVal != null)
			{
				if (GUILayout.Button("none"))
				{
					objVal.objectVarName = null;
					lostFocus = true; // so that this window closes
					if (ed != null) ed.Repaint();
				}

				if (UniRPGEditorGlobal.DB.objectVars.Count > 0)
				{
					foreach (ObjectVar v in UniRPGEditorGlobal.DB.objectVars)
					{
						if (string.IsNullOrEmpty(v.name)) continue;
						if (GUILayout.Button(v.name))
						{
							objVal.objectVarName = v.name;
							lostFocus = true; // so that this window closes
							if (ed != null) ed.Repaint();
						}
					}
				}
				else
				{
					GUILayout.Label("No variables defined", UniRPGEdGui.WarningLabelStyle);
				}
			}

			else
			{
				Debug.LogError("Eish, an error here? o.O");
			}
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Cancel", UniRPGEdGui.ButtonStyle))
			{
				lostFocus = true; // so that this window closes
				if (ed != null) ed.Repaint();
			}
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);

	}

	// ================================================================================================================
} }