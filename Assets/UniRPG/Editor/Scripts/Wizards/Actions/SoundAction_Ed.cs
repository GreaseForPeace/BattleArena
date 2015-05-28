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

[ActionInfo(typeof(SoundAction), "System/Sound or Music", Description = "Stop sound or music")]
	public class SoundAction_Ed : ActionsEdBase
{
	private static readonly string[] options = { "Start Sound or Music", "Play Target Sound", "Stop Target Sound" };
	private static readonly string[] volOptions = { "Custom", "Effect", "Environment", "GUI", "Music" };
	private Vector2 scroll = Vector2.zero;

	public override string ActionShortNfo(Object actionObj)
	{
		SoundAction action = actionObj as SoundAction;
		if (action.doWhat == 0) return "Start Sound or Music";
		if (action.doWhat == 1) return "Play Target Sound";
		return "Stop Target Sound";
	}

	public override void OnGUI(Object actionObj)
	{
		SoundAction action = actionObj as SoundAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		action.doWhat = EditorGUILayout.Popup(action.doWhat, options);
		EditorGUILayout.Space();

		// play new sound or music
		if (action.doWhat == 0)
		{
			if (GUILayout.Button("Add Clip", EditorStyles.miniButton)) action.clips.Add(null);
			scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(50));
			{
				int del = -1;
				for (int i = 0; i < action.clips.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					action.clips[i] = (AudioClip)EditorGUILayout.ObjectField(action.clips[i], typeof(AudioClip), false);
					if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(25))) del = i;
					GUILayout.Space(20);
					EditorGUILayout.EndHorizontal();
				}
				if (del >= 0) action.clips.RemoveAt(del);
			}
			UniRPGEdGui.EndScrollView();
		}

		// play/stop an existing sound
		else
		{
			UniRPGEdGui.TargetTypeField(this.ed, "Target Sound Object", action.subject, TargetTypeHelp);
		}

		if (action.doWhat == 0 || action.doWhat == 1)
		{
			EditorGUIUtility.LookLikeControls(100);
			
			EditorGUILayout.BeginHorizontal();
			action.loop = GUILayout.Toggle(action.loop, " looped");
			EditorGUILayout.Space();
			if (action.loop == false) action.destroyWhenDone = GUILayout.Toggle(action.destroyWhenDone, " destroy when done");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			action.useVolume = EditorGUILayout.Popup("Volume", action.useVolume, volOptions);
			if (action.useVolume == 0) action.customVolume = EditorGUILayout.Slider(action.customVolume, 0f, 1f); //EditorGUILayout.FloatField("... (0-1)", action.customVolume);
			if (action.doWhat == 0)
			{
				action.position = EditorGUILayout.Vector3Field("Position", action.position);
				EditorGUILayout.BeginHorizontal();
				action.useParent = GUILayout.Toggle(action.useParent, " make child of");
				EditorGUILayout.Space();
				if (action.useParent) UniRPGEdGui.TargetTypeField(this.ed, null, action.subject, TargetTypeHelp);
				EditorGUILayout.EndHorizontal();
			}
		}

		if (action.loop || action.destroyWhenDone == false)
		{
			EditorGUIUtility.LookLikeControls(150);
			GUILayout.Space(5);
			action.setGlobalObjectVar = EditorGUILayout.TextField("Save to global object var", action.setGlobalObjectVar);
		}
		else action.setGlobalObjectVar = null;
	}

	// ================================================================================================================
} }