// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class RPGEvent : MonoBehaviour
{
	public GUID id;

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField] private string nm = string.Empty;

	public string description = string.Empty;
	public string notes = string.Empty;			// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] icon = new Texture2D[3]; // up to 3 icons. what is used depends on game gui. editor uses the 1st if avail

	public string guiHelper;					// this can be used to help the gui determine what to do with this attrib (what you use here depends on the gui theme)

	public List<Action> onUseActions = new List<Action>(0);

	// ================================================================================================================

	public void CopyTo(RPGEvent e)
	{
		e.id = this.id.Copy();

		e.screenName = this.screenName;
		e.description = this.description;
		e.notes = this.notes;
		e.icon = new Texture2D[3] { this.icon[0], this.icon[1], this.icon[2] };
		e.guiHelper = this.guiHelper;

		e.onUseActions = new List<Action>(0);
		GameObject g = e.gameObject;
		foreach (Action act in this.onUseActions)
		{
			Action newAct = (Action)g.AddComponent(act.GetType());
			newAct.hideFlags = HideFlags.HideInInspector;
			act.CopyTo(newAct);
			e.onUseActions.Add(newAct);
		}
	}

	void Reset()
	{	// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time
		id = GUID.Create(id);
	}

	void Awake()
	{
		// disable the action components since they will be manually called as needed
		for (int i = 0; i < onUseActions.Count; i++) onUseActions[i].enabled = false;
	}

	/// <summary>Call this to use execute the actions defined in this event</summary>
	public void Execute(GameObject owner)
	{
		UniRPGGameController.ExecuteActions(onUseActions, owner, null, null, null, null, false);
	}

	// ================================================================================================================
} }