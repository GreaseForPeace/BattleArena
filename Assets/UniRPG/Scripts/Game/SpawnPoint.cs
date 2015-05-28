// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("UniRPG/Spawn Point")]
public class SpawnPoint : UniqueMonoBehaviour 
{
	public int ident = 0;					// a unique id for the spawn location (optional and used as needed)
	public bool isPlayerSpawn = false;		// true if spawn used with spawning player - like during map changes or when new-game
	public bool isNewGameLocation = false;	// true if it is where player spawns on new-game

	// options for NPC spawner
	public GameObject npcPrefab = null;		// the actor to spawn here if not a player
	public bool npcIsPersistent = false;	// save the state of the NPCs that where spawned?
	public float moveDistanceOnSpawn = 0f;	// 0 = stay where it spawn, else move forward a certain distance before AI takes over

	public bool spawnWhenDetectPlayer = true; // only start spawning when detected player, else it will start running as soon as it can
	public float detectionRadius = 20f;		// area to check for a player before activating the spawner

	// spawn rate and how many to spawn, etc
	public int maxSpawn = 1;				// 0 = there is no max, or the max actors that this spawner can ever spawn
	public float spawnInterval = 10f;		// how long to wait before spawning another "group" (in seconds)

	public int groupSize = 1;				// how big is a group
	public float groupSpawnDelay = 0.6f;	// how long to wait between spawning the actors in a group (only apply if groupSize > 1)
	public int spawnIfGroupSizeSmaller = 1;	// only spawn more when the existing group's size is smaller than this number (and if maxSpawn has not bene reached yet) 0 = ignore this and just keep spawning groups
	
	// things to set after spawning the NPC
	public UniRPGGlobal.AIBehaviour idleAction = UniRPGGlobal.AIBehaviour.Stay; // what should it do after being spawned or when not chacing after the player
	public bool wanderInCricle = true;		// else square area
	public Vector2 wanderWH = Vector2.one;	// when wanderInCricle = false
	public float wanderRadius = 10f;		// how far from spawn can it wander, used with NPC.IdleAction.Wander
	public float wanderDelayMin = 5f;		// how long the NPC can idle between nw wander destinations
	public float wanderDelayMax = 30f;
	public PatrolPath patrolPath;			// used with NPC.IdleAction.Patrol

	public bool useStartingXP = false;
	public int startingXP = 0;				// the amount of XP to init the NPC with to set its Level

	// ================================================================================================================

	private enum State { Idle, LookingForPlayer, Active, Spawning }
	private State state = State.Idle;

	private int totalSpawned = 0;
	private int groupSpawned = 0;

	private float spawnIntervalTimer = 0.0f;
	private float groupSpawnDelayTimer = 0.0f;

	private List<CharacterBase> spawned = new List<CharacterBase>(0); // keep track of the spawned NPCs

	// ================================================================================================================
	#region init/start/load/close

	void Start()
	{
		autoSaveLoadEnabled = false; // UniqueMonoBehaviour should not savbe/load the object/component's enabled state since funtions here will manage it

		if (isPlayerSpawn)
		{	// no need for this spawner to run if it is used with player
			IsPersistent = false;
			Deactivate();
			return;
		}

		if (!npcPrefab)
		{
			//Debug.LogWarning("No Actor Definition was specified for the Spawn Point.");
			IsPersistent = false;
			Deactivate();
			return;
		}

		// the spawn point must have a sphere collider to detect the player when "spawnWhenDetectPlayer = true"
		if (spawnWhenDetectPlayer)
		{
			SphereCollider collider = gameObject.GetComponent<SphereCollider>();
			if (!collider) collider = gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = detectionRadius;
		}

		spawnIntervalTimer = 0.0f; // don't wait to spawn the 1st one
		groupSpawnDelayTimer = 0.0f;

		if (spawnWhenDetectPlayer) state = State.LookingForPlayer;
		else state = State.Active;
	}

	private void Deactivate()
	{
		state = State.Idle;
		// it might happen that someone wants to use this as parent with Object Create actions, 
		// so rather not disable the actual GameObject, but only the component
		//gameObject.SetActive(false);
		enabled = false;
	}

	protected override void SaveState()
	{
		base.SaveState();

		totalSpawned -= spawned.Count; // cause these where allifed and can thus be spawned again when loading
		if (totalSpawned < 0) totalSpawned = 0;
		UniRPGGlobal.SaveInt(saveKey + "total", totalSpawned);
	}

	protected override void LoadState()
	{
		base.LoadState();
		if (UniRPGGlobal.Instance.DoNotLoad) return;

		totalSpawned = UniRPGGlobal.LoadInt(saveKey + "total", 0);
		if (totalSpawned >= maxSpawn && maxSpawn > 0 && totalSpawned > 0)
		{	// disable spawner since it can't spawn more critters
			this.Deactivate();
		}
	}

	#endregion
	// ================================================================================================================
	#region update/run

	void LateUpdate()
	{
		if (IsLoading) return;

		switch (state)
		{
			case State.Active:		// going through the spawnInterval countdown
			{
				if (spawnIfGroupSizeSmaller <= 0 || spawned.Count < spawnIfGroupSizeSmaller)
				{
					spawnIntervalTimer -= Time.deltaTime;
					if (spawnIntervalTimer <= 0.0f)
					{
						spawnIntervalTimer = spawnInterval;
						groupSpawnDelayTimer = 0.0f;
						state = State.Spawning;
					}
				}
			} break;

			case State.Spawning:	// bussy spawning a group
			{
				groupSpawnDelayTimer -= Time.deltaTime;
				if (groupSpawnDelayTimer <= 0.0f)
				{
					groupSpawnDelayTimer = groupSpawnDelay;
					SpawnNPC();

					totalSpawned++;
					groupSpawned++;

					if (groupSpawned >= groupSize)
					{	// spawned enough. go back to doing the spawnInterval countdown
						spawnIntervalTimer = spawnInterval;
						state = State.Active;
					}

					if (totalSpawned >= maxSpawn && maxSpawn > 0)
					{	// spawned max that this spawner can create, end it all
						this.Deactivate();
						return;
					}
				}				
			} break;
		}

	}

	private void SpawnNPC()
	{
		GameObject go = GameObject.Instantiate(npcPrefab.gameObject, transform.position, transform.rotation) as GameObject;
		CharacterBase npc = go.GetComponent<CharacterBase>();

		spawned.Add(npc);
		npc.SetSpawnPoint(this);

		// set level
		if (this.useStartingXP)
		{
			// just set the StartXP on the Actor instance. It will be calling its Start() when this function exits and do the correct initing.
			npc.Actor.startingXP = this.startingXP;
		}

		if (moveDistanceOnSpawn > 0) npc.RequestMoveTo(transform.position + (transform.forward * moveDistanceOnSpawn));
	}

	/// <summary>Remove a Character from the tracking list. should be called by Character when it is removed from the scene and was spawned by this SpawnPoint</summary>
	public void RemoveCharacter(CharacterBase chara)
	{
		spawned.Remove(chara);
	}

	#endregion
	// ================================================================================================================
	#region Events

	/// <summary>Will be called when the player enter's the spawnpoint's area. only usefull to those set to spawnWhenDetectPlayer = true </summary>
	public void OnPlayerEnterTrigger()
	{
		if (spawnWhenDetectPlayer && state == State.LookingForPlayer) state = State.Active;
	}

	/// <summary>Will be called when the player exit the spawnpoint's area. only usefull to those set to spawnWhenDetectPlayer = true </summary>
	public void OnPlayerExitTrigger()
	{
		if (spawnWhenDetectPlayer) state = State.LookingForPlayer;
	}

	#endregion
	// ================================================================================================================
	#region Gizmo/Editor related
#if UNITY_EDITOR
	public GameObject _preview { get; set; } // do not use this. it is used internally by the editor to tract the 3D preview
	void OnDrawGizmos()
	{
		UniRPGGlobal.ActorType type = (npcPrefab == null ? (isPlayerSpawn ? UniRPGGlobal.ActorType.Player : 0) : npcPrefab.GetComponent<Actor>().ActorType);

		if (type == UniRPGGlobal.ActorType.Player) Gizmos.color = Color.cyan;
		else if (type == UniRPGGlobal.ActorType.Friendly) Gizmos.color = Color.green;
		else if (type == UniRPGGlobal.ActorType.Neutral) Gizmos.color = Color.yellow;
		else if (type == UniRPGGlobal.ActorType.Hostile) Gizmos.color = Color.red;
		else Gizmos.color = Color.grey;

		// the main gizmo
		Gizmos.DrawLine(transform.position - Vector3.up * 0.1f, transform.position + Vector3.up * 2f);
		Gizmos.DrawCube(transform.position - Vector3.up * 0.1f, new Vector3(0.1f, 0.1f, 0.1f));
		Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.25f);

		// the direction which actor spawns and moves when spawned
		Vector3 end = transform.position + (transform.forward * (moveDistanceOnSpawn > 0f? moveDistanceOnSpawn : 0.3f));
		Gizmos.DrawLine(transform.position, end);
		Gizmos.DrawSphere(end, 0.05f);

		if (type == UniRPGGlobal.ActorType.Player) Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "UniRPG_spawn_blue.png");
		else if (type == UniRPGGlobal.ActorType.Friendly) Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "UniRPG_spawn_green.png");
		else if (type == UniRPGGlobal.ActorType.Neutral) Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "UniRPG_spawn_yellow.png");
		else if (type == UniRPGGlobal.ActorType.Hostile) Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "UniRPG_spawn_red.png");
		else Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "UniRPG_spawn_grey.png");		

		if (gameObject.activeSelf)
		{
			if (_preview == null && npcPrefab != null && !isPlayerSpawn && UniRPGSettings.autoLoad3DPreviews)
			{
				_preview = UniRPGUtil.InstantiatePreview(npcPrefab.gameObject, gameObject, transform.position, transform.rotation);
			}
		}
		else if (_preview != null)
		{	// dont want the preview if object is not active
			GameObject.DestroyImmediate(_preview);
			_preview = null;
		}

	}
#endif
	#endregion
	// ================================================================================================================
} }