// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using UniRPG;

namespace UniRPGEditor {

public class UniRPGAbout : EditorWindow
{
	private string version = "(version file not found)";
	private string copyright = "© 2013 by Leslie Young";
	private string url = "http://www.plyoung.com/";

	// ================================================================================================================

	public static void ShowAbout()
	{
		UniRPGAbout win = EditorWindow.GetWindow<UniRPGAbout>(true, "UniRPG", true);
		win.inited = false;
		win.InitVersion();
		win.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "UniRPG";
		minSize = maxSize = new Vector2(250, 200);
	}

	private void InitVersion()
	{
		try
		{
			string fn = UniRPGEdUtil.FullProjectPath + "/" + UniRPGEditorGlobal.PackagePath + "Documentation/version.txt";
			using (StreamReader s = File.OpenText(fn))
			{
				version = s.ReadLine();
				copyright = s.ReadLine();
				url = s.ReadLine();
				s.Close();
			}
		}
		catch { }
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

		GUILayout.Label(version, EditorStyles.boldLabel);
		EditorGUILayout.Space();
		GUILayout.Label(copyright);
		if (GUILayout.Button(url, EditorStyles.label)) Application.OpenURL(url);
		EditorGUILayout.Space();
	}

	// ================================================================================================================
} }