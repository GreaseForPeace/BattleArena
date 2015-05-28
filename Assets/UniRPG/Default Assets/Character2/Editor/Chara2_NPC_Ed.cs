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
	[CustomEditor(typeof(Chara2_NPC))]
	public class Chara2_NPC_Ed : InteractableBaseEditor<Chara2_NPC>
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
			Target.moveSpeed = EditorGUILayout.FloatField("Move Speed", Target.moveSpeed);
			Target.chaseSpeed = EditorGUILayout.FloatField("Chase Speed", Target.chaseSpeed);
			Target.turnSpeed = EditorGUILayout.FloatField("Turn Speed", Target.turnSpeed);
			EditorGUILayout.Space();

			Target.actorType = (UniRPGGlobal.ActorType)EditorGUILayout.EnumPopup("Type", Target.actorType);
			if (Target.actorType == UniRPGGlobal.ActorType.Player) Target.actorType = UniRPGGlobal.ActorType.Friendly;
			Target.detectionRadius = EditorGUILayout.FloatField("Detection Radius", Target.detectionRadius);
			Target.chaseTimeout = EditorGUILayout.FloatField("Chase Timeout", Target.chaseTimeout);			
			Target.idleAction = (UniRPGGlobal.AIBehaviour)EditorGUILayout.EnumPopup("Action", Target.idleAction);
			if (Target.idleAction == UniRPGGlobal.AIBehaviour.Patrol)
			{
				Target.patrolPath = (PatrolPath)EditorGUILayout.ObjectField("Patrol Path", Target.patrolPath, typeof(PatrolPath), true);
			}
			else if (Target.idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				Target.wanderInCricle = EditorGUILayout.Toggle("Wander area is Circular", Target.wanderInCricle);
				if (Target.wanderInCricle)
				{
					Target.wanderRadius = EditorGUILayout.FloatField("Wander Radius", Target.wanderRadius);
				}
				else
				{
					Target.wanderWH = EditorGUILayout.Vector2Field("Area Size", Target.wanderWH);
				}
				Target.wanderDelayMin = EditorGUILayout.FloatField("Idle Time Min", Target.wanderDelayMin);
				Target.wanderDelayMax = EditorGUILayout.FloatField("Idle Time Max", Target.wanderDelayMax);
			}

			EditorGUILayout.Space();
			Target.chooseSkill2DistanceMod = EditorGUILayout.FloatField("Skill 2 Distance mod", Target.chooseSkill2DistanceMod);

		}

		public override void OnSceneGUI()
		{
			base.OnSceneGUI();
			if (Target.idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				if (Target.wanderInCricle)
				{
					Handles.color = new Color(0.4f, 1f, 0.9f, 0.1f);
					Handles.DrawSolidDisc(Target.transform.position, Vector3.up, Target.wanderRadius);
				}
				else
				{
					Vector3 p = Target.transform.position;
					Vector3[] verts = new Vector3[]{
									  new Vector3(p.x-Target.wanderWH.x, p.y, p.z+Target.wanderWH.y),
									  new Vector3(p.x+Target.wanderWH.x, p.y, p.z+Target.wanderWH.y),
									  new Vector3(p.x+Target.wanderWH.x, p.y, p.z-Target.wanderWH.y),
									  new Vector3(p.x-Target.wanderWH.x, p.y, p.z-Target.wanderWH.y),
								  };
					Handles.color = Color.white;
					Handles.DrawSolidRectangleWithOutline(verts, new Color(0.4f, 1f, 0.9f, 0.1f), new Color(0.4f, 1f, 0.9f, 0.11f));
				}
			}
		}

		// ============================================================================================================
	}
}