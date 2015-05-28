// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UniRPG;

namespace UniRPGEditor {

public class UniRPGRatingPrompt : EditorWindow
{
	private const string URL = "content/9694";
	private const int DAYS_TO_WAIT = 14;
	private const string saveString = "UniRPG_Rated";

	private static bool exitedNormal = false;
	private static bool checkedThisSession = false;

	// call this to show the prompt
	public static void ShowRatingPrompt()
	{
		if (checkedThisSession) return;
		checkedThisSession = true;

		// first check if prompt should be shown or not
		int i = EditorPrefs.GetInt(saveString, 0);

		// -1: dont show again (either rated or chose not to see window again)
		//  0: mean, not set yet
		//  n: else a date number. check when last shown and prompt every 7 days

		if (i == -1) return;
		if (i > 0)
		{	// check if 7 days have passed yet
			System.TimeSpan t = (System.DateTime.Now - new System.DateTime(1, 1, 1));
			int days = ((int)t.TotalDays + 1 - i);
			if (days < DAYS_TO_WAIT) return;
		}

		UniRPGRatingPrompt win = EditorWindow.GetWindow<UniRPGRatingPrompt>(true, "UniRPG", true);
		win.inited = false;
		win.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "UniRPG";
		minSize = maxSize = new Vector2(300, 300);
	}

	void OnDestroy()
	{
		if (!exitedNormal)
		{
			exitedNormal = true;
			CallAgain();
		}
	}

	private void CallAgain()
	{	// will call again in 7 days
		System.TimeSpan t = (System.DateTime.Now - new System.DateTime(1, 1, 1));
		int days = (int)t.TotalDays + 1;
		EditorPrefs.SetInt(saveString, days);
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();

		GUILayout.Space(-10);
		EditorGUILayout.BeginHorizontal(UniRPGEdGui.AboutLogoAreaStyle, GUILayout.Height(120), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
		{
			GUILayout.FlexibleSpace();
			Rect r = GUILayoutUtility.GetRect(194, 100, GUILayout.Width(194), GUILayout.Height(100));
			GUILayout.FlexibleSpace();
			GUI.DrawTexture(r, UniRPGEdGui.Texture_Logo);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			GUILayout.Label("Please Review and Rate UniRPG", UniRPGEdGui.Head2Style);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		if (GUILayout.Button("Let me Rate it", GUILayout.MinHeight(30f)))
		{
			exitedNormal = true;
			UnityEditorInternal.AssetStore.Open(URL);
			EditorPrefs.SetInt(saveString, -1);
			Close();
			EditorGUIUtility.ExitGUI();
		}
		EditorGUILayout.Space();
		if (GUILayout.Button("I'll do it Later", GUILayout.MinHeight(30f)))
		{
			exitedNormal = true;
			CallAgain();
			Close();
			EditorGUIUtility.ExitGUI();
		}
		EditorGUILayout.Space();
		if (GUILayout.Button("Don't Ask me Again", GUILayout.MinHeight(30f)))
		{
			exitedNormal = true;
			EditorPrefs.SetInt(saveString, -1);
			Close();
			EditorGUIUtility.ExitGUI();
		}
	}

	// ================================================================================================================
} }