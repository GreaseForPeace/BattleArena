using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Fury.Editor
{
	[UnityEditor.CustomEditor(typeof(Fury.Behaviors.Rotator))]
	public sealed class RotatorEditor : UnityEditor.Editor
	{
		private Fury.Behaviors.Rotator Target { get { return target as Fury.Behaviors.Rotator; } }
		
		public override void OnInspectorGUI()
		{
			Target.Speed = Math.Abs(EditorGUILayout.FloatField("Speed", Target.Speed));

			EditorGUILayout.BeginHorizontal();
			  
			if (GUILayout.Button("<"))
				Target.transform.RotateAroundLocal(Vector3.up, -0.2f);

			if (GUILayout.Button(">"))
				Target.transform.RotateAroundLocal(Vector3.up, +0.2f);

			EditorGUILayout.EndHorizontal();
		}
	}
}