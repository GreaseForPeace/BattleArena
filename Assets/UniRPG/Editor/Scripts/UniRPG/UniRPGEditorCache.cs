// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

public class UniRPGEditorCache : ScriptableObject 
{
	// this cache is used for things that must be scanned for in the project and which 
	// would slow down the dev process if the designer have to wait for the scan
	// to run each time he wants to test or the code recompiles

	// cache of characters (player & npc)
	public List<Actor> actors = new List<Actor>();

	/// <summary>refresh all cached data</summary>
	public void RefreshAll()
	{
		RefreshActorCache(false);
		EditorApplication.SaveAssets();
	}

	/// <summary>
	/// Finds all the defined Actors (character - player, npc), and add 'em to the cache. Only adds those which are set as active.
	/// </summary>
	public void RefreshActorCache(bool callSave)
	{
		// clear/create the list
		actors = new List<Actor>();

		List<GUID> ids = new List<GUID>();
		List<Actor> acts = UniRPGEdUtil.FindPrefabsOfTypeAll<Actor>("Searching", "Finding all Actors.");
		foreach (Actor a in acts)
		{
			// only actors that are enabled are added
			if (a.gameObject.activeSelf)
			{
				if (a.Character == null)
				{
					Debug.LogError("Found a broken Actor. It has no Character component. ["+ a.gameObject.name+"]");
					continue;
				}

				// check that the character has unique id among characters
				if (a.Character.id.IsEmpty) { a.Character.id = GUID.Create(); EditorUtility.SetDirty(a.gameObject); }
				while (GUID.ListContains(ids, a.Character.id)) { a.Character.id = GUID.Create(); EditorUtility.SetDirty(a.gameObject); }
				ids.Add(a.Character.id);

				// add to found actors
				actors.Add(a);
			}
		}

		EditorUtility.SetDirty(this);
		if (callSave) EditorApplication.SaveAssets();
	}

	// ================================================================================================================
} }