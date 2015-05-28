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

public class DatabaseEditor : EditorWindow 
{
	// ================================================================================================================
	#region vars

	public Database db = null; // reference to db being edited
	public static float LeftPanelWidth = 200f;
	private int currDbArea = 0;

	#endregion
	// ================================================================================================================
	#region pub

	public static void ShowEditor()
	{
		DatabaseEditor ed = EditorWindow.GetWindow<DatabaseEditor>();
		ed.inited = false;
		ed.db = UniRPGEditorGlobal.DB;
		ed.Show();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "UniRPG";
		minSize = new Vector2(900, 600);
		wantsMouseMove = true; // for some mouse hover effects (like the rules lists on items for example)

		// use this opportunity to show the ratings prompt
		UniRPGRatingPrompt.ShowRatingPrompt();
	}

	public void ShowErrorMessage(string msg)
	{
		Debug.LogError(msg);
		this.ShowNotification(new GUIContent(msg));
	}

	#endregion
	// ================================================================================================================
	#region update & ongui

	public void Update()
	{
		if (UniRPGEditorGlobal.DBEditors.Length == 0) return;
		if (currDbArea >= UniRPGEditorGlobal.DBEditors.Length) currDbArea = 0;
		UniRPGEditorGlobal.DBEditors[currDbArea].Update(this);
	}

	void OnGUI()
	{
		if (!inited) Init();

		if (db == null)
		{
			Close();
			return;
		}

		if (UniRPGEditorGlobal.DBEditors.Length == 0) return;
		if (currDbArea >= UniRPGEditorGlobal.DBEditors.Length) currDbArea = 0;
		UniRPGEdGui.UseSkin();

		// draw the logo icon in bottom corner
		GUI.DrawTexture(new Rect(this.position.width - 300, this.position.height - 300, 400, 400), UniRPGEdGui.Texture_LogoIcon);

		// show the main toolbar
		int prev = currDbArea;
		currDbArea = GUILayout.Toolbar(currDbArea, UniRPGEditorGlobal.DBEdNames, UniRPGEdGui.ToolbarStyle);
		if (currDbArea != prev)
		{
			UniRPGEditorGlobal.DBEditors[prev].OnDisable(this);
			UniRPGEditorGlobal.DBEditors[currDbArea].OnEnable(this);
			GUI.FocusControl(""); // i need to do this to clear the focus on text fields which are bugging if they stay focused
		}

		UniRPGEdGui.DrawHorizontalLine(2f, UniRPGEdGui.ToolbarDividerColor, -3);

		// show the active editor area
		UniRPGEditorGlobal.DBEditors[currDbArea].OnGUI(this);

		if (Event.current.type == EventType.MouseMove)
		{
			Repaint();
		}
	}

	#endregion
	// ================================================================================================================
} }