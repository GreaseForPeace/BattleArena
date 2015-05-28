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

[GameCamera("Default Cam 1", typeof(DefaultCam1))]
public class DefaultCam1Editor : GameCameraEditorBase
{

	public override void OnGUI(DatabaseEditor ed, GameCameraBase gameCam)
	{
		DefaultCam1 cam = gameCam as DefaultCam1;

		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(400));
		{
			EditorGUIUtility.LookLikeControls(160);
			cam.autoRotateToPlayer = EditorGUILayout.Toggle("Auto-rotate to player", cam.autoRotateToPlayer);
			cam.allowRotation = EditorGUILayout.Toggle("Allow rotation", cam.allowRotation);
			cam.allowZooming = EditorGUILayout.Toggle("Allow zooming", cam.allowZooming);
			EditorGUILayout.Space();

			cam.backDistance = EditorGUILayout.FloatField("Back Distance", cam.backDistance);
			cam.angle = EditorGUILayout.FloatField("Start Angle", cam.angle);
			EditorGUILayout.Space();

			cam.targetCenterOffset = EditorGUILayout.Vector3Field("Player Center Offset", cam.targetCenterOffset);
			EditorGUILayout.Space();

			cam.zoomSpeed = EditorGUILayout.FloatField("Zoom Speed", cam.zoomSpeed);
			cam.distance = EditorGUILayout.FloatField("Start Distance", cam.distance);
			cam.minZoomDistance = EditorGUILayout.FloatField("Min Zoom Distance", cam.minZoomDistance);
			cam.maxZoomDistance = EditorGUILayout.FloatField("Max Zoom Distance", cam.maxZoomDistance);
			EditorGUILayout.Space();

			cam.rotationStepping = EditorGUILayout.FloatField("Rotation Stepping", cam.rotationStepping);
			cam.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", cam.rotationSpeed);
			cam.autoRotationSpeed = EditorGUILayout.FloatField("Auto-rotation Speed", cam.autoRotationSpeed);

		}
		EditorGUILayout.EndVertical();
	}

	// ================================================================================================================
} }