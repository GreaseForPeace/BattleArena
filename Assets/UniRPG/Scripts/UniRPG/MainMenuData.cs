// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
/// <summary>
/// this is data that might be needed by the GUI in the main menu - like when choosing a character to play with
/// this data can not be included in the Database or Global classes since I want it to be unloaded
/// when the player goes in-game and auto-loaded when he goes back to the menu
/// </summary>
public class MainMenuData : MonoBehaviour 
{
	public List<GameObject> startupPlayerPrefabs=new List<GameObject>();
	public List<CharacterBase> playerCharacters { get; private set; }

	// ================================================================================================================

	void Awake()
	{
		playerCharacters = new List<CharacterBase>();
		foreach (GameObject go in startupPlayerPrefabs) playerCharacters.Add(go.GetComponent<CharacterBase>());
	}

	/// <summary>will find the first character actor set as "avail at start"</summary>
	public CharacterBase GetDefaultPlayerCharacter(bool playerCanSelect)
	{
		if (playerCanSelect)
		{
			// first try return one from this list since the menu might want to know about one that can be used in selection, else wotever is set
			if (playerCharacters.Count > 0) return playerCharacters[0];
			if (UniRPGGlobal.DB.defaultPlayerCharacterPrefab != null) return UniRPGGlobal.DB.defaultPlayerCharacterPrefab.GetComponent<CharacterBase>();
		}
		else
		{
			// first try the one that was set as default, else try any that can be found
			if (UniRPGGlobal.DB.defaultPlayerCharacterPrefab != null) return UniRPGGlobal.DB.defaultPlayerCharacterPrefab.GetComponent<CharacterBase>();
			if (playerCharacters.Count > 0) return playerCharacters[0];
		}
		return null;
	}

	/// <summary>return the 1st avail class that can be chosen by the player (only those marked as "avail at start")</summary>
	public RPGActorClass GetDefaultPlayerClass(Actor actor)
	{
		if (actor != null)
		{	// first check if the class currently on the actor can't be used
			if (actor.actorClassPrefab != null && UniRPGGlobal.DB.classes.Contains(actor.actorClassPrefab))
			{
				return actor.actorClassPrefab;
			}
		}

		foreach (RPGActorClass c in UniRPGGlobal.DB.classes)
		{
			if (c.availAtStart) return c;
		}
		return null;
	}

	// ================================================================================================================
} }