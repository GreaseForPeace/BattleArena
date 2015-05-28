// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UnityEditor;
using UniRPG;
using UniRPGEditor;
using DiaQ;

namespace DiaQEditor
{
	[ActionInfo(typeof(DiaQAction), "DiaQ/Dialogue", Description = "Start a DiaQ conversation")]
	public class DiaQAction_Ed : ActionsEdBase
	{
		public override string ActionShortNfo(Object actionObj)
		{
			DiaQAction action = actionObj as DiaQAction;
			if (action == null) return "!ERROR!";
			return string.Format("DiaQ Dialogue: {0}", action.graphName);
		}

		public override void OnGUI(Object actionObj)
		{
			if (DiaQEditorWindow.Asset == null)
			{
				GUILayout.Label("The DiaQ Asset could not be loaded.\nPlease make sure your DiaQ settings\nare valid before using this Action.");
				return;
			}
			
			DiaQAction action = actionObj as DiaQAction;
			if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Dialogue", GUILayout.Width(60));
				if (GUILayout.Button(action.graphName))
				{
					DiaQGraphSelectWiz.ShowWiz(_OnGraphSelected, DiaQEditorWindow.Asset, new object[] { action, ed });
				}
			}
			GUILayout.EndHorizontal();
		}

		private void _OnGraphSelected(DiaQGraphSelectWiz wiz, object[] args)
		{
			DiaQAction action = args[0] as DiaQAction;
			EditorWindow ed = args[1] as EditorWindow;

			action.graphIdent = wiz.selected.IdentString;
			action.graphName = wiz.selected.name;

			wiz.Close();
			ed.Repaint();
		}

		// ================================================================================================================
	}
}

#endif