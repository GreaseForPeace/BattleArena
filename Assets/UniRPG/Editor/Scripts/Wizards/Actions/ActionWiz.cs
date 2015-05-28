// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using UniRPG;

namespace UniRPGEditor {

public class ActionWiz : EditorWindow
{
	public bool isNewAction = true;
	public Action Action { get { return action as Action; } }

	public bool accepted = false;	// if Select button was clicked

	private UniRPGBasicEventHandler OnAccept = null;	// callback to call when the accept button was clicked
	//private static bool showDesription = true;

	private int edIdx = -1;				// current ActionEditor's idx
	private Object action = null;		// the selected action (the one being edited)
	private Object newAction = null;	// used to keep tract of actions that must be destroyed when designer choses another
	private GameObject actionOwner;
	
	private int initialIdx = -1;
	private bool inited = false;

	// ================================================================================================================

	public static ActionWiz Show(UniRPGBasicEventHandler onAccept, GameObject actionOwner, Object action)
	{
		// create the actions editor main window
		ActionWiz window = EditorWindow.GetWindow<ActionWiz>(true, "Actions", true);
		window.OnAccept = onAccept;
		window.actionOwner = actionOwner;

		// check if editing existing action or addding new one
		if (action != null)
		{
			window.action = action;
			window.isNewAction = false;
			window.edIdx = -1;
			window.initialIdx = -1;
			for (int i = 0; i < UniRPGEditorGlobal.ActionEditors.Length; i++)
			{
				// if the action could be assigned from the given type then I assume it is of that type
				if (window.action.GetType().IsAssignableFrom(UniRPGEditorGlobal.ActionEditors[i].actionType))
				{
					window.edIdx = i;
					window.initialIdx = i;
					break;
				}
			}

			if (window.edIdx < 0)
			{	// still -1? then editor for selected action was not found
				window.action = null;
				window.isNewAction = true;
			}
		}

		// show window
		window.inited = false;
		window.ShowUtility();
		return window;
	}

	private void Init()
	{
		inited = true;
		title = "Actions";
		minSize = maxSize = new Vector2(310, 380);
	}

	// i cant use the auto close when lose focus feature cause some action editors might
	// require dragging obejcts from the hierarchy or project onto this wizard window
	//void OnFocus() { lostFocus = false; }
	//void OnLostFocus() { lostFocus = true; }

	void OnDestroy()
	{
		if (!accepted && newAction != null)
		{	// cleanup
			GameObject.DestroyImmediate(newAction, true);
			newAction = null;
		}
	}

	void Update()
	{
		//if (lostFocus)
		//{
		//	if (newAction != null)
		//	{	// cleanup if needed
		//		GameObject.DestroyImmediate(newAction, true);
		//		newAction = null;
		//	}
		//	this.Close(); // auto close on focus lost (same as cancel being clicked)
		//}

		if (accepted)
		{
			if (OnAccept != null)
			{
				OnAccept(this); // make callback to tell listener that accept button was clicked
			}
			else
			{	// else, just cleanup and close

				if (newAction != null)
				{	// cleanup if needed
					GameObject.DestroyImmediate(newAction, true);
					newAction = null;
				}
				this.Close();
			}
		}
	}

	void OnGUI()
	{
		if (!inited) Init();
		UniRPGEdGui.UseSkin();

		// show list of actions (action editors)
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label("Action", GUILayout.Width(60));
			int prev = edIdx;
			edIdx = EditorGUILayout.Popup(edIdx, UniRPGEditorGlobal.ActionEdNames, GUILayout.Width(150));
			if (prev != edIdx)
			{	// user selected another action from the list, init it
				if (newAction != null)
				{	// cleanup if needed
					GameObject.DestroyImmediate(newAction, true);
					newAction = null;
				}

				// create the new action 
				action = actionOwner.AddComponent(UniRPGEditorGlobal.ActionEditors[edIdx].actionType);
				action.hideFlags = HideFlags.HideInInspector;
				newAction = action;
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		if (edIdx >= 0)
		{
			//showDesription = UniRPGEdGui.FoldableLabel(showDesription, UniRPGEditorGlobal.ActionEditors[edIdx].descr, 290);
			UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 3, 8);

			// show the gui for the selected action's editor
			UniRPGEditorGlobal.ActionEditors[edIdx].editor.ed = this;
			UniRPGEditorGlobal.ActionEditors[edIdx].editor.OnGUI(action);
		}

		GUILayout.FlexibleSpace();
		UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 0, 10);
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();

			if (edIdx >= 0)
			{
				if (!UniRPGEditorGlobal.ActionEditors[edIdx].editor.showAcceptButton) GUI.enabled = false;
			}
			else GUI.enabled = false;
			if (GUILayout.Button("Accept", UniRPGEdGui.ButtonStyle)) accepted = true;
			GUI.enabled = true;

			EditorGUILayout.Space();
			GUI.enabled = (edIdx != initialIdx);
			if (GUILayout.Button("Cancel", UniRPGEdGui.ButtonStyle)) this.Close();
			GUI.enabled = true;
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(15);
	}

	// ================================================================================================================
} }