// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor
{

	[ActionInfo(typeof(MecanimParamAction), "Object/Animation: Mecanim Param", Description = "Set a mecanim param on subject's animator")]
	public class MecanimParamAction_Ed : ActionsEdBase
	{
		private static readonly string[] Opts = new string[] { "Bool", "Int", "Float", "Vector" };

		public override string ActionShortNfo(Object actionObj)
		{
			MecanimParamAction action = actionObj as MecanimParamAction;
			return string.Format("Set mecanim param [{0}]", action.paramName.GetValOrName());
		}

		public override void OnGUI(Object actionObj)
		{
			MecanimParamAction a = actionObj as MecanimParamAction;
			if (a == null) { GUILayout.Label("Error: Delete this Action."); return; }

			UniRPGEdGui.TargetTypeField(this.ed, "Subject", a.subject, TargetTypeHelp, 80);
			EditorGUILayout.Space();
			a.paramName = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Parameter", a.paramName, 80);
			EditorGUILayout.Space();
			EditorGUIUtility.LookLikeControls(95);
			a.paramType = EditorGUILayout.Popup("Type", a.paramType, Opts);
			EditorGUILayout.Space();

			switch (a.paramType)
			{
				case 0: 
				{
					a.paramBool = EditorGUILayout.Toggle("Value", a.paramBool);
					EditorGUILayout.LabelField(" ", a.paramBool ? "True" : "False");
				} break;

				case 1:
				case 2:
				{
					EditorGUIUtility.LookLikeControls();
					a.paramIntFloat = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Value", a.paramIntFloat, 80);
				} break;

				case 3:
				{
					a.paramVector = EditorGUILayout.Vector3Field("Value", a.paramVector);
				} break;
			}

			EditorGUILayout.Space();
			a.restoreOldValAfterCall = GUILayout.Toggle(a.restoreOldValAfterCall, " Restore old param value after call");
		}

		// ================================================================================================================
	}
}