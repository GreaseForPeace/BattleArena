// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using UniRPG;

namespace UniRPGEditor {

[CustomEditor(typeof(PatrolPath))]
public class PatrolPathInspector : UniqueMBInspector<PatrolPath>
{
	private bool doRepaint = false;
	private bool refreshInspector = false;

	private PatrolPoint curr = null;
	private PatrolPoint del = null;
	private static bool fold = false;

	// ================================================================================================================

	public override void OnInspectorGUI()
	{
		UniRPGEdGui.UseSkin();
		GUILayout.Space(15f);

		fold = DrawMBInspector(fold);
		UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Add Patrol Point", GUILayout.MinHeight(30f), GUILayout.MaxWidth(230f)))
			{
				PatrolPoint p = new PatrolPoint();
				
				if (curr != null)
				{
					int pos = Target.patrolpoints.IndexOf(curr);
					p.position = curr.position + new Vector3(0.5f, 0f, 0f);
					Target.patrolpoints.Insert(pos+1, p);
				}
				else
				{
					if (Target.patrolpoints.Count == 0) p.position = new Vector3(-1f, 0.1f, 0f);
					else p.position = new Vector3(0.5f, 0f, 0f) + Target.patrolpoints[Target.patrolpoints.Count-1].position;
					Target.patrolpoints.Add(p);
				}
				curr = p;
				doRepaint = true;
			}
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Label("Points in Path", UniRPGEdGui.InspectorHeadBoxStyle);
		for (int i = 0; i < Target.patrolpoints.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(10f);
				if (UniRPGEdGui.ToggleButton(curr == Target.patrolpoints[i], "Point " + i, UniRPGEdGui.ButtonLeftStyle))
				{
					curr = Target.patrolpoints[i];
					doRepaint = true;
				}
				if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) del = Target.patrolpoints[i];
				GUILayout.Space(10f);
			}
			EditorGUILayout.EndHorizontal();

			if (curr == Target.patrolpoints[i])
			{
				curr.position = EditorGUILayout.Vector3Field("Position", curr.position);
				curr.rotation.eulerAngles = EditorGUILayout.Vector3Field("Rotation", curr.rotation.eulerAngles);
				EditorGUILayout.Space();
			}
		}

		GUILayout.Space(15f);

		// ------------------------------------------------

		if (del != null)
		{
			if (curr == del) curr = null;
			Target.patrolpoints.Remove(del);
			del = null;
			doRepaint = true;
		}

		if (doRepaint)
		{
			doRepaint = false;
			HandleUtility.Repaint();
			if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.Repaint();
		}

	}

	// ================================================================================================================

	void OnSceneGUI()
	{
		Vector3 offset = Target.transform.position;
		Vector3 lineOffset = offset + new Vector3(0f, 0.1f, 0f);

		for (int i = 0; i < Target.patrolpoints.Count; i++)
		{
			if (Target.patrolpoints[i] == curr)
			{
				Handles.color = Color.red;
				Handles.ArrowCap(0, Target.patrolpoints[i].position + offset, Target.patrolpoints[i].rotation, HandleUtility.GetHandleSize(Target.patrolpoints[i].position + offset));
				if (UnityEditor.Tools.current == Tool.Rotate) 
				{
					Undo.SetSnapshotTarget(Target, "Rotate PatrolPoint");
					Target.patrolpoints[i].rotation = Handles.RotationHandle(Target.patrolpoints[i].rotation, Target.patrolpoints[i].position + offset);
				}
				else if (UnityEditor.Tools.current == Tool.Move)  
				{
					Undo.SetSnapshotTarget(Target, "Move PatrolPoint");
					Target.patrolpoints[i].position = Handles.PositionHandle(Target.patrolpoints[i].position + offset, Target.patrolpoints[i].rotation) - offset;
				}
			}
			else
			{
				Handles.color = Color.green;
				if (Handles.Button(Target.patrolpoints[i].position + offset, Target.patrolpoints[i].rotation, HandleUtility.GetHandleSize(Target.patrolpoints[i].position + offset) * 0.2f, HandleUtility.GetHandleSize(Target.patrolpoints[i].position + offset) * 0.1f, DrawPatrolPointButton))
				{
					curr = Target.patrolpoints[i];
					doRepaint = true;
					refreshInspector = true;
				}
				Handles.ArrowCap(0, Target.patrolpoints[i].position + offset, Target.patrolpoints[i].rotation, HandleUtility.GetHandleSize(Target.patrolpoints[i].position + offset));
			}

			Handles.DrawLine(Target.patrolpoints[i].position + offset, Target.patrolpoints[i].position + lineOffset);
			if (i < Target.patrolpoints.Count - 1) Handles.DrawLine(Target.patrolpoints[i].position + lineOffset, Target.patrolpoints[i + 1].position + lineOffset);
			else Handles.DrawLine(Target.patrolpoints[i].position + lineOffset, Target.patrolpoints[0].position + lineOffset);
		}

		Undo.ClearSnapshotTarget();
		// ------------------------------------------------

		if (refreshInspector)
		{
			refreshInspector = false;
			this.Repaint();
		}
	}

	private void DrawPatrolPointButton(int controlId, Vector3 position, Quaternion rotation, float size)
	{
		Handles.CubeCap(controlId, position, rotation, size);
	}

	// ================================================================================================================
} }