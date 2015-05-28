// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.IO;

namespace DiaQEditor
{
	public class DiaQVersion : EditorWindow
	{
		private static string version = "(version file not found)";
		private static string copyright = "© 2013 by Leslie Young";
		private static string url = "http://www.plyoung.com/";

		// ================================================================================================================

		public static void ShowAbout()
		{
			DiaQVersion win = EditorWindow.GetWindow<DiaQVersion>(true);
			win.title = "DiaQ";
			win.minSize = win.maxSize = new Vector2(250, 240);
			win.InitVersion();
			win.ShowUtility();
		}

		private void InitVersion()
		{
			try
			{
				string fn = DiaQEdUtil.FullProjectPath + "/Assets/DiaQ/Documentation/version.txt";
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
			DiaQEdGUI.UseSkin();

			GUILayout.Space(-10);
			EditorGUILayout.BeginHorizontal(DiaQEdGUI.AboutLogoAreaStyle, GUILayout.Height(120), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			{
				GUILayout.FlexibleSpace();
				Rect r = GUILayoutUtility.GetRect(194, 100, GUILayout.Width(194), GUILayout.Height(100));
				GUILayout.FlexibleSpace();
				GUI.DrawTexture(r, DiaQEdGUI.Texture_Logo);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Label(version, EditorStyles.boldLabel);
			EditorGUILayout.Space();
			GUILayout.Label(copyright);
			if (GUILayout.Button(url, EditorStyles.label)) Application.OpenURL(url);
			GUILayout.Space(20);

			if (GUILayout.Button("Open Documentation"))
			{
				DiaQEditorWindow.ShowDiaQDocs();
			}
		}

		// ================================================================================================================
	}
}
