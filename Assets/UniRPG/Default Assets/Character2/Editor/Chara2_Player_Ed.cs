// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using UniRPG;

namespace UniRPGEditor
{
	[CustomEditor(typeof(Chara2_Player))]
	public class Chara2_Player_Ed : InteractableBaseEditor<Chara2_Player>
	{
		private static bool[] foldout2 = { false, false };

		// ============================================================================================================

		public override void OnInspectorGUI()
		{
			UniRPGEdGui.UseSkin();
			GUILayout.Space(10f);

			Settings();
			UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
			foldout2 = DrawInteractableInspector(foldout2, false, false, true);

			GUILayout.Space(10f);

			if (GUI.changed)
			{
				GUI.changed = false;
				EditorUtility.SetDirty(Target);
			}
		}

		private void Settings()
		{
			Target.minClickDistance = EditorGUILayout.FloatField("Min Click Distance", Target.minClickDistance);
			Target.moveSpeed = EditorGUILayout.FloatField("Move Speed", Target.moveSpeed);
			Target.turnSpeed = EditorGUILayout.FloatField("Turn Speed", Target.turnSpeed);
			EditorGUILayout.Space();
			Target.autoPickupItems = EditorGUILayout.Toggle("Can auto pick-up items", Target.autoPickupItems);
			EditorGUILayout.Space();
			Target.clickMarkerPrefab = (GameObject)EditorGUILayout.ObjectField("Click-marker prefab", Target.clickMarkerPrefab, typeof(GameObject), false);
		}

		// ============================================================================================================
	}
}