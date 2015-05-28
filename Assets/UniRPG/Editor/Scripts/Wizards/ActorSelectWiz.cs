// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UniRPG;

namespace UniRPGEditor {

public class ActorSelectWiz : EditorWindow
{
	public Actor selectedActor = null;

	public bool accepted = false;	// if Select button was clicked
	private bool lostFocus = false;

	private UniRPGGlobal.ActorType filter = 0;
	private Vector2 scroll = Vector2.zero;

	private UniRPGBasicEventHandler OnAccept = null;

	// ================================================================================================================

	public static void Show(UniRPGBasicEventHandler onAccept)
	{
		// make sure DB is loaded
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// create window
		ActorSelectWiz window = EditorWindow.GetWindow<ActorSelectWiz>(true, "Select Actor",true);
		window.inited = false;
		window.OnAccept = onAccept;

		// show window
		window.ShowUtility();
	}

	private bool inited = false;
	private void Init()
	{
		inited = true;
		title = "Select Actor";
		minSize = new Vector2(200, 350);
		maxSize = new Vector2(200, 350);
	}

	void OnFocus() { lostFocus = false; }
	void OnLostFocus() { lostFocus = true; }

	void Update()
	{
		if (lostFocus) this.Close();
		if (accepted && OnAccept != null) OnAccept(this);
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("filter", GUILayout.Width(60));
			filter = (UniRPGGlobal.ActorType)EditorGUILayout.EnumPopup(filter);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		scroll = UniRPGEdGui.BeginScrollView(scroll);
		{
			if (UniRPGEditorGlobal.Cache.actors.Count > 0)
			{
				foreach (Actor actor in UniRPGEditorGlobal.Cache.actors)
				{
					if (filter != 0 && (int)actor.GetComponent<Actor>().ActorType != (int)filter) continue;
					Rect r = EditorGUILayout.BeginHorizontal();
					{
						r.x = 3; r.width = 19; r.height = 19;
						GUI.DrawTexture(r, (actor.portrait[0] != null ? actor.portrait[0] : UniRPGEdGui.Texture_NoPreview));
						GUILayout.Space(21);
						if (UniRPGEdGui.ToggleButton(selectedActor == actor, actor.screenName, UniRPGEdGui.ButtonRightStyle, UniRPGEdGui.ButtonOnColor, GUILayout.Width(150)))
						{
							selectedActor = actor;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Actors are defined\n\nYou might need to Refresh the\ncache if you did define actors.\nDatatabase -> Main -> Actors", UniRPGEdGui.WarningLabelStyle);	
			}			
		}
		UniRPGEdGui.EndScrollView();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();

			if (selectedActor == null) GUI.enabled = false;
			if (GUILayout.Button("Accept", UniRPGEdGui.ButtonStyle)) accepted = true;
			GUI.enabled = true;

			if (GUILayout.Button("Cancel", UniRPGEdGui.ButtonStyle)) this.Close();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(10);

	}

	// ================================================================================================================
} }