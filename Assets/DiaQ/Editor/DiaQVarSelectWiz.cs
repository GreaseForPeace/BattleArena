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
	public class DiaQVarSelectWiz : EditorWindow
	{
		public DiaQVar selected = null;

		private bool accepted = false;
		private bool lostFocus = false;
		private Vector2 scroll = Vector2.zero;
		private DiaQAsset asset = null;
		private object[] args = null;

		public delegate void OnAcceptEvent(DiaQVarSelectWiz wiz, object[] args);
		private OnAcceptEvent onAccept = null;

		public delegate void OnUpdateValue(string val);
		private OnUpdateValue onUpdate = null;

		public static void ShowWiz(OnAcceptEvent onAccept, OnUpdateValue onUpdate, DiaQAsset asset, object[] args)
		{
			if (asset == null) return;
			DiaQVarSelectWiz window = EditorWindow.GetWindow<DiaQVarSelectWiz>(true);
			window.title = "Select Var";
			window.minSize = new Vector2(300, 350);
			window.maxSize = new Vector2(300, 350);
			window.onAccept = onAccept;
			window.onUpdate = onUpdate;
			window.asset = asset;
			window.args = args;
			window.ShowUtility();
		}

		void OnFocus() { lostFocus = false; }
		void OnLostFocus() { lostFocus = true; }

		void Update()
		{
			if (lostFocus) this.Close();
			if (accepted && onAccept != null) onAccept(this, args);
		}

		void OnGUI()
		{
			scroll = GUILayout.BeginScrollView(scroll, false, true);
			{
				if (asset.vars.Count > 0)
				{
					foreach (DiaQVar q in asset.vars)
					{
						if (GUILayout.Toggle(selected == q, q.name, GUI.skin.button, GUILayout.Width(275))) selected = q;
					}
				}
				else
				{
					GUILayout.Label("No DiaQ Variabled defined");
				}
			}
			GUILayout.EndScrollView();

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				if (selected == null) GUI.enabled = false;
				if (GUILayout.Button("Accept", GUILayout.Width(60), GUILayout.Height(20)))
				{
					if (onUpdate != null) onUpdate(selected.name);
					accepted = true;
				}
				GUI.enabled = true;

				if (GUILayout.Button("Cancel", GUILayout.Width(60), GUILayout.Height(20))) this.Close();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);
		}

		// ============================================================================================================
	}
}