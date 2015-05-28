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
public class UniRPGGameController : MonoBehaviour 
{
	// ================================================================================================================
	#region vars

	// all the Spawn Points that where found in the currently open game scene
	public SpawnPoint[] SpawnPoints { get; private set; }

	// all the Spawn Points that are set as player spawns
	public List<SpawnPoint> PlayerSpawnPoints { get; private set; }

	#endregion
	// ================================================================================================================
	#region Init/Start

	void Awake()
	{
		_instance = this;

		// get the player spawn points before they become inactive (spawnpoint sets itself diabled in Start() as needed)
		// also create a cache of ALL SpawnPoint while running through them

		SpawnPoints = new SpawnPoint[0];
		PlayerSpawnPoints = new List<SpawnPoint>();

		GameObject parent = GameObject.Find("SpawnPoints");
		if (parent)
		{
			SpawnPoints = parent.GetComponentsInChildren<SpawnPoint>();
			for (int i = 0; i < SpawnPoints.Length; i++)
			{
				if (SpawnPoints[i].isPlayerSpawn) PlayerSpawnPoints.Add(SpawnPoints[i]);
			}
		}

#if UNITY_EDITOR
		// check if UniRPGGLobal is loaded, if not then load it now. This is a hack which is only needed during development
		// time to allow developer to run the scene in unity editor but still load the needed global scene
		if (!UniRPGGlobal.Instance)
		{
			Application.LoadLevelAdditive("menudata");
			Application.LoadLevelAdditive("unirpg");
		}
#endif

		// Load the game gui
		Application.LoadLevelAdditive("gamegui");
	}

	void Start()
	{
		// find where to spawn the player
		Vector3 pos = new Vector3(0f, 0.1f, 0f);
		Quaternion rot = Quaternion.identity;

		bool found = false;
		if (UniRPGGlobal.Instance.playerSpawnPoint >= 0)
		{	// find the SpawnPoint with given ID
			foreach (SpawnPoint p in PlayerSpawnPoints)
			{
				if (p.ident == UniRPGGlobal.Instance.playerSpawnPoint) { pos = p.transform.position; rot = p.transform.rotation; found = true; break; }
			}
		}
		else
		{
			foreach (SpawnPoint p in PlayerSpawnPoints)
			{
				if (p.isNewGameLocation) { pos = p.transform.position; rot = p.transform.rotation; found = true; break; }
			}
		}

		if (!found && PlayerSpawnPoints.Count > 0)
		{	// default is to use the 1st avail playerSpawnPoint if nothing else worked
			pos = PlayerSpawnPoints[0].transform.position;
			rot = PlayerSpawnPoints[0].transform.rotation;
		}

		// spawn the player
#if UNITY_EDITOR
		// attemp to get a character to test with in case the developer is running a scene directly -for testing
		// otherwise the startCharacterDef should be set by now
		if (UniRPGGlobal.Instance.startCharacterDef == null)
		{
			Debug.LogWarning("The UniRPGGlobal.startCharacterDef is not set. The first player character from the db will be used.");
			if (UniRPGGlobal.MainMenuData.playerCharacters.Count > 0) UniRPGGlobal.Instance.startCharacterDef = UniRPGGlobal.MainMenuData.playerCharacters[0].gameObject;
		}
#endif

		if (UniRPGGlobal.Instance.startCharacterDef == null)
		{
			Debug.LogError("The UniRPGGlobal.startCharacterDef is not set. This must be fixed before you can properly playtest the scene. Please refer to the documentation to see how this can be setup.");
		}
		else
		{
			GameObject go = (GameObject)GameObject.Instantiate(UniRPGGlobal.Instance.startCharacterDef, pos, rot);

			Actor ac = go.GetComponent<Actor>();
			if (ac == null)
			{
				Debug.LogError("The Player GameObject do not have an Actor component on it. This must be fixed.");
			}
			else
			{
				if (!string.IsNullOrEmpty(UniRPGGlobal.Instance.startPlayerName)) ac.screenName = UniRPGGlobal.Instance.startPlayerName;
				UniRPGGlobal.Instance._player = go.GetComponent<CharacterBase>();
				if (UniRPGGlobal.Player == null)
				{
					Debug.LogError("The Player GameObject do not have a Character component on it. This must be fixed.");
				}
			}
		}

		//// wait a frame
		//yield return null;
		UniRPGGlobal.Instance.OnGameSceneLoaded();
	}

	#endregion
	// ================================================================================================================
	#region Action Execution

	// Note to self, Do not change this!!!
	// I am doing it this way cause Items (and prolly other Interactables) might not have a real instance for yield
	// to hook onto since some actions need to run when the object might just be a prefab. Only Skills are different
	// and will run their own Actions inside Update()

	/// <summary>
	/// NOTE: Actions are not executed in Edit mode and this function should not be called by editor scripts
	/// This will start processing actions. see Action.TargetType for description of what params mean. Not all the params will be set.
	/// for example, equipTarget will be null in any other situation that equip/unquip actions being called
	/// </summary>
	public static void ExecuteActions(List<Action> actions, GameObject self, GameObject target, GameObject targetedBy, GameObject equipTarget, GameObject helper, bool isLoadingProcess)
	{
		if (actions.Count > 0)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Actions can't be executed in edit mode.");
				return; // cant run actions in edit mode
			}
			if (_instance != null)
			{
				_instance.StartCoroutine(ExecuteActionsCoroutine(actions, self, target, targetedBy, equipTarget, helper, isLoadingProcess));
			}
		}
	}

	private static IEnumerator ExecuteActionsCoroutine(List<Action> actions, GameObject self, GameObject target, GameObject targetedBy, GameObject equipTarget, GameObject helper, bool isLoadingProcess)
	{
		Action.ReturnType ret = Action.ReturnType.Done;
		for (int i = 0; i < actions.Count; i++)
		{
			if (isLoadingProcess)
			{	// do not execute an action that does not want to run when this is an event fired by the LoadSave System is restoring something
				if (false == actions[i].ExecuteWhenRestoringState()) continue;
			}

			actions[i].Init(self, target, targetedBy, equipTarget, helper);
			do
			{
				ret = actions[i].Execute(self, target, targetedBy, equipTarget, helper);
				if (ret == Action.ReturnType.CallAgain) yield return null; // wait a frame before going on
			}
			while (ret == Action.ReturnType.CallAgain);

			if (ret == Action.ReturnType.SkipNext)
			{
				i++; // so that next one is skipped
				ret = Action.ReturnType.Done;
				if (i >= actions.Count) ret = Action.ReturnType.Exit;
			}

			// ---------- note that the following should be last else if tests

			else if (ret <= Action.ReturnType.ExecuteSpecificNext && ret != Action.ReturnType.Done && ret != Action.ReturnType.Exit)
			{
				Debug.LogError("There is no Action (0) or smaller. Please specify a number higher than (0).");
				ret = Action.ReturnType.Exit;
			}

			else if (ret > Action.ReturnType.ExecuteSpecificNext)
			{
				i = (int)ret - (int)Action.ReturnType.ExecuteSpecificNext - 2; // note, (-2) cause this is in for i++ loop; while RPGSkill queue executer was (-1)
				ret = Action.ReturnType.Done;
				if (i >= actions.Count) ret = Action.ReturnType.Exit;
			}

			// ---------- this should not be in else-if since the above code depends on something checking for Exit

			if (ret == Action.ReturnType.Exit) break; // do not process actions to follow
		}
	}

	#endregion
	// ================================================================================================================

	private static UniRPGGameController _instance = null;
	public static UniRPGGameController Instance
	{
		get
		{
			if (_instance == null) 
			{
				Object[] objs = FindObjectsOfType(typeof(UniRPGGameController));
				if (objs.Length > 0) _instance = objs[0] as UniRPGGameController;
			}
			return _instance;
		}
	}

	// ================================================================================================================
} }