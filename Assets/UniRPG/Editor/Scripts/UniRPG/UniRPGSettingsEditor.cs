// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using UniRPG;

namespace UniRPGEditor {

public static class UniRPGSettingsEditor
{
	private static bool settingsLoaded = false;

	[PreferenceItem("UniRPG")]
	public static void UniRPGSettingsGUI()
	{
		// Load Settings
		LoadSettings();

		// Render Settings GUI
		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();
		UniRPGSettings.autoLoad3DPreviews = EditorGUILayout.Toggle("Auto-load 3D Previews", UniRPGSettings.autoLoad3DPreviews);
		if (EditorGUI.EndChangeCheck())
		{
			if (!UniRPGSettings.autoLoad3DPreviews) UniRPGEdUtil.DeleteAllEditorOnlyObjects();
			SceneView.lastActiveSceneView.Repaint();
		}

		UniRPGSettings.floorLayeMask = EditorGUILayout.LayerField("Floor Layer-Mask", UniRPGSettings.floorLayeMask);

		// Save Settings
		if (GUI.changed)
		{
			EditorPrefs.SetBool("UniRPG_autoLoad3DPreviews", UniRPGSettings.autoLoad3DPreviews);
			EditorPrefs.SetInt("UniRPG_floorLayeMask", UniRPGSettings.floorLayeMask);
		}
	}

	public static void LoadSettings()
	{
		if (!settingsLoaded)
		{
			settingsLoaded = true;
			UniRPGSettings.autoLoad3DPreviews = EditorPrefs.GetBool("UniRPG_autoLoad3DPreviews", true);
			UniRPGSettings.floorLayeMask = EditorPrefs.GetInt("UniRPG_floorLayeMask", 0);
		}
	}

	// ================================================================================================================
} }