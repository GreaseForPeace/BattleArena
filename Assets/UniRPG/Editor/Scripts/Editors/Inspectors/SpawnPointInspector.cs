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

[CanEditMultipleObjects, CustomEditor(typeof(SpawnPoint))]
public class SpawnPointInspector : UniqueMBInspector<SpawnPoint>
{
	private SerializedProperty p_id;
	private SerializedProperty p_isPlayerSpawn;
	private SerializedProperty p_isNewGameLocation;
	private SerializedProperty p_npcPrefab;

	private SerializedProperty p_moveDistanceOnSpawn;
	private SerializedProperty p_spawnWhenDetectPlayer;
	private SerializedProperty p_detectionRadius;

	private SerializedProperty p_maxSpawn;
	private SerializedProperty p_spawnInterval;

	private SerializedProperty p_groupSize;
	private SerializedProperty p_groupSpawnDelay;
	private SerializedProperty p_spawnIfGroupSizeSmaller;

	// things to set after spawning the NPC
	private SerializedProperty p_idleAction;
	private SerializedProperty p_wanderRadius;
	private SerializedProperty p_wanderIsCircle;
	private SerializedProperty p_wanderWH;
	public SerializedProperty p_wanderDelayMin;
	public SerializedProperty p_wanderDelayMax;
	private SerializedProperty p_patrolPath;
	private SerializedProperty p_useStartingXP;
	private SerializedProperty p_startingXP;

	// -------------------------

	private static readonly GUIContent l_id = new GUIContent("Identifier (optional)", "You can give this Spawn Point a unique identifier that can be used to identify the Spawn Point.");
	private static readonly GUIContent l_isPlayerSpawn = new GUIContent("Is player spawn", "Is this Spawn Point used to spawn the player character?");
	private static readonly GUIContent l_isNewGameLocation = new GUIContent("Is new game location", "Is this Spawn Point used to spawn a player character when a New Game is selected? Only usefull on the New Game map/ 1st game scene.");

	private static readonly GUIContent l_isPersistent = new GUIContent("Persistent (save)", "Save the state of this Spawn Point when changing map/ saving the game?");
	private static readonly GUIContent l_moveDistanceOnSpawn = new GUIContent("Move after Spawn", "How far the NPC must move forward (meters) after spawning and before AI takes over.");
	private static readonly GUIContent l_spawnWhenDetectPlayer = new GUIContent("Only when detect player", "Only spawn when detected the player, else spawner will start running as soon as it can.");
	private static readonly GUIContent l_detectionRadius = new GUIContent("Player detection radius", "Radius (in meters) to look for player before activating the spawner.");

	private static readonly GUIContent l_maxSpawn = new GUIContent("Max Spawn", "The max NPCs that can be spawned. If set to (0) then an unlimited number will be spawned.");
	private static readonly GUIContent l_spawnInterval = new GUIContent("Interval", "How long to wait (in seconds) before spawning the next group of NPCs. Use (0) to ignore interval and to spawn a new NPC as soon as the group size becomes smaller than the value set for 'Group Size'.");
	private static readonly GUIContent l_groupSize = new GUIContent("Group Size", "The size of a spawned group. This still falls within the 'Max Spawn' limit.");
	private static readonly GUIContent l_groupSpawnDelay = new GUIContent("Group spawn delay", "How long to wait (in seconds) between spawning NPCs of a group. Ignored if group is only 1 big.");
	private static readonly GUIContent l_spawnIfGroupSizeSmaller = new GUIContent("Spawn when group <", "Spawn when a group's size become smaller than this number. Use (0) to ignore this field and only work on 'Interval'.");
	private static readonly GUIContent l_idleAction = new GUIContent("Acion after spawned", "What should the NPC do after it was spawned? Note that a Hostile NPC will chace after the player if the player is in range. A neutral NPC will do this action until attacked by the player and then turn Hostile and chace after the player.");
	private static readonly GUIContent l_wanderRadius = new GUIContent("Wander Radius", "How far from the Spawn Point can the NPC wander?");
	private static readonly GUIContent l_wanderIsCircle = new GUIContent("Wander In Circle area", "NPC wander in circular or rectangular area?");
	private static readonly GUIContent l_wanderWH = new GUIContent("Wander area Size", "Width and Length of area to wander in");
	private static readonly GUIContent l_wanderDelayMin = new GUIContent("Wander IdleTime Min", "How long the NPC can idle before chosing a new destination for wandering.");
	private static readonly GUIContent l_wanderDelayMax = new GUIContent("Wander IdleTime Max", "How long the NPC can idle before chosing a new destination for wandering.");
	private static readonly GUIContent l_patrolPath = new GUIContent("Path", "The path to use with Patrol Action.");
	private static readonly GUIContent l_useStartingXP = new GUIContent("Starting XP", "Set NPC's starting XP?");
	private static readonly GUIContent l_startingXP = new GUIContent("", "The amount of XP to initialise the NPC with.");
	
	private bool doRepaint = false;
	private bool doRereshPreviews = false;

	// ================================================================================================================

	void OnEnable()
	{
		// this is called when this inspector is enabled (not the gameobject being inspected)
		p_id = serializedObject.FindProperty("ident");
		p_isPlayerSpawn = serializedObject.FindProperty("isPlayerSpawn");
		p_isNewGameLocation = serializedObject.FindProperty("isNewGameLocation");
		p_npcPrefab = serializedObject.FindProperty("npcPrefab");

		p_moveDistanceOnSpawn = serializedObject.FindProperty("moveDistanceOnSpawn");
		p_spawnWhenDetectPlayer = serializedObject.FindProperty("spawnWhenDetectPlayer");
		p_detectionRadius = serializedObject.FindProperty("detectionRadius");

		p_maxSpawn = serializedObject.FindProperty("maxSpawn");
		p_spawnInterval = serializedObject.FindProperty("spawnInterval");

		p_groupSize = serializedObject.FindProperty("groupSize");
		p_groupSpawnDelay = serializedObject.FindProperty("groupSpawnDelay");
		p_spawnIfGroupSizeSmaller = serializedObject.FindProperty("spawnIfGroupSizeSmaller");

		p_idleAction = serializedObject.FindProperty("idleAction");
		p_wanderRadius = serializedObject.FindProperty("wanderRadius");
		p_wanderIsCircle = serializedObject.FindProperty("wanderInCricle");
		p_wanderWH = serializedObject.FindProperty("wanderWH");
		p_wanderDelayMin = serializedObject.FindProperty("wanderDelayMin");
		p_wanderDelayMax = serializedObject.FindProperty("wanderDelayMax");
		p_patrolPath = serializedObject.FindProperty("patrolPath");

		p_useStartingXP = serializedObject.FindProperty("useStartingXP");
		p_startingXP = serializedObject.FindProperty("startingXP");

		foreach (SpawnPoint p in targets)
		{
			if (p._preview == null && p.npcPrefab != null && !p.isPlayerSpawn && p.gameObject.activeSelf)
			{
				p._preview = UniRPGUtil.InstantiatePreview(p.npcPrefab.gameObject, p.gameObject, p.transform.position, p.transform.rotation);
			}
		}
	}

	void OnDisable()
	{
		// this is called when this inspector is not active (not the gameobject being inspected)
		if (!UniRPGSettings.autoLoad3DPreviews)
		{
			foreach (SpawnPoint p in targets)
			{
				if (p._preview != null)
				{
					GameObject.DestroyImmediate(p._preview);
					p._preview = null;
				}
			}
		}
	}

	void OnDestroy()
	{	// this is called when this inspector is destroyed (when closed/object unselected/not the gameobject being inspected is destroyed)
		if (!UniRPGSettings.autoLoad3DPreviews)
		{
			foreach (SpawnPoint p in targets)
			{
				if (p._preview != null)
				{
					GameObject.DestroyImmediate(p._preview);
					p._preview = null;
				}
			}
		}
	}

	// ================================================================================================================

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		UniRPGEdGui.UseSkin();
		GUILayout.Space(15);

		EditorGUILayout.PropertyField(p_id, l_id);
		
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(p_isPlayerSpawn, l_isPlayerSpawn);
		if (EditorGUI.EndChangeCheck())
		{
			// set npcPrefab to null if this is player spawn so that the prefab is not linked and unity thinks it must be loaded at runtime
			if (p_isPlayerSpawn.boolValue)
			{
				p_npcPrefab.objectReferenceValue = null;
				p_moveDistanceOnSpawn.floatValue = 0f;
				doRereshPreviews = true;
			}
		}

		if (!Target.isPlayerSpawn)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Actor Definition");
				EditorGUILayout.BeginVertical();
				{
					if (GUILayout.Button(targets.Length > 1 ? "-multiple selected-" : (Target.npcPrefab == null ? "none" : (string.IsNullOrEmpty(Target.npcPrefab.name) ? "undefined name" : Target.npcPrefab.name))))
					{
						ActorSelectWiz.Show(OnActorSelected);
					}

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField(p_npcPrefab, GUIContent.none);
					if (EditorGUI.EndChangeCheck()) doRereshPreviews = true;
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();


			//EditorGUILayout.PropertyField(p_isPersistent, l_isPersistent);
			Target.IsPersistent = EditorGUILayout.Toggle(l_isPersistent, Target.IsPersistent);
			foreach (SpawnPoint p in targets) p.IsPersistent = Target.IsPersistent;

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(p_spawnWhenDetectPlayer, l_spawnWhenDetectPlayer);
			if (Target.spawnWhenDetectPlayer) EditorGUILayout.PropertyField(p_detectionRadius, l_detectionRadius);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(p_maxSpawn, l_maxSpawn);
			EditorGUILayout.PropertyField(p_spawnInterval, l_spawnInterval);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(p_groupSize, l_groupSize);
			EditorGUILayout.PropertyField(p_groupSpawnDelay, l_groupSpawnDelay);
			EditorGUILayout.PropertyField(p_spawnIfGroupSizeSmaller, l_spawnIfGroupSizeSmaller);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(p_moveDistanceOnSpawn, l_moveDistanceOnSpawn);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(p_idleAction, l_idleAction);
			if (Target.idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				EditorGUILayout.PropertyField(p_wanderIsCircle, l_wanderIsCircle);
				if (Target.wanderInCricle)
				{
					EditorGUILayout.PropertyField(p_wanderRadius, l_wanderRadius);
				}
				else
				{
					EditorGUILayout.PropertyField(p_wanderWH, l_wanderWH);
				}

				EditorGUILayout.PropertyField(p_wanderDelayMin, l_wanderDelayMin);
				EditorGUILayout.PropertyField(p_wanderDelayMax, l_wanderDelayMax);
			}
			else if (Target.idleAction == UniRPGGlobal.AIBehaviour.Patrol)
			{
				EditorGUILayout.PropertyField(p_patrolPath, l_patrolPath);
			}

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PropertyField(p_useStartingXP, l_useStartingXP);
				EditorGUILayout.Space();
				if (p_useStartingXP.boolValue && Target.npcPrefab != null)
				{
					EditorGUILayout.PropertyField(p_startingXP, l_startingXP, GUILayout.Width(50));
					Actor actor = Target.npcPrefab.GetComponent<Actor>();
					if (actor)
					{
						GUILayout.Label(string.Format("(level: {0})", actor.actorClassPrefab.CalculateLevel(p_startingXP.intValue)));
						EditorGUILayout.Space();
					}
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
		}
		else
		{
			EditorGUILayout.PropertyField(p_isNewGameLocation, l_isNewGameLocation);
		}

		GUILayout.Space(15);

		// ------------------------------------------------------------------------------------------------------------

		serializedObject.ApplyModifiedProperties();

		foreach (SpawnPoint p in targets)
		{
			if (p._preview != null && (!p.gameObject.activeSelf || p.npcPrefab == null || p.isPlayerSpawn))
			{
				GameObject.DestroyImmediate(p._preview);
				p._preview = null;
				doRepaint = true;
			}
			else if (p._preview == null && p.gameObject.activeSelf && p.npcPrefab != null && !p.isPlayerSpawn)
			{
				doRereshPreviews = true;
			}
		}

		if (targets.Length == 1)
		{
			UniRPGEdGui.DrawHorizontalLine(1f, UniRPGEdGui.InspectorDividerColor, 1f, 10f);
			DrawMBInspector(true);
		}

		if (doRereshPreviews)
		{
			doRereshPreviews = false;
			RefreshActorPreview();
		}

		if (doRepaint)
		{	// refresh scene view
			doRepaint = false;
			HandleUtility.Repaint();
			if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.Repaint();
		}

	}

	private void OnActorSelected(System.Object sender)
	{
		doRepaint = true; // will force refresh of the scene view since the preview model changed
		ActorSelectWiz wiz = sender as ActorSelectWiz;
		foreach (SpawnPoint p in targets) p.npcPrefab = wiz.selectedActor.gameObject;
		wiz.Close();
		RefreshActorPreview();
	}

	private void RefreshActorPreview()
	{
		doRepaint = true; // will force refresh of the scene view since the preview model changed

		// delete the old preview and create new preview
		foreach (SpawnPoint p in targets)
		{
			if (p._preview != null) GameObject.DestroyImmediate(p._preview);
			if (p.npcPrefab != null && !p.isPlayerSpawn && p.gameObject.activeSelf)
			{
				p._preview = UniRPGUtil.InstantiatePreview(p.npcPrefab.gameObject, p.gameObject, p.transform.position, p.transform.rotation);
			}
		}
	}

	// ================================================================================================================

	void OnSceneGUI()
	{
		if (Target._preview != null)
		{
			Target._preview.transform.position = Target.transform.position;
			Target._preview.transform.rotation = Target.transform.rotation;
		}

		// draw spawn direction arrow
		UniRPGGlobal.ActorType type = (Target.npcPrefab == null ? (Target.isPlayerSpawn ? UniRPGGlobal.ActorType.Player : 0) : Target.npcPrefab.GetComponent<Actor>().ActorType);
		if (type == UniRPGGlobal.ActorType.Player) Handles.color = Color.green;
		else if (type == UniRPGGlobal.ActorType.Friendly) Handles.color = Color.green;
		else if (type == UniRPGGlobal.ActorType.Neutral) Handles.color = Color.yellow;
		else if (type == UniRPGGlobal.ActorType.Hostile) Handles.color = Color.red;
		else Handles.color = Color.grey;

		Handles.ArrowCap(0, Target.transform.position, Target.transform.rotation, HandleUtility.GetHandleSize(Target.transform.position));

		// npc related info
		if (!Target.isPlayerSpawn)
		{
			if (Target.spawnWhenDetectPlayer)
			{
				Handles.DrawWireDisc(Target.transform.position, Vector3.up, Target.detectionRadius);
				Handles.DrawWireDisc(Target.transform.position, Vector3.up, Target.detectionRadius - 0.1f);
				Handles.DrawWireDisc(Target.transform.position, Vector3.up, Target.detectionRadius - 0.2f);
			}

			if (Target.idleAction == UniRPGGlobal.AIBehaviour.Wander)
			{
				if (Target.wanderInCricle)
				{
					Handles.color = new Color(0.4f, 1f, 0.9f, 0.1f);
					Handles.DrawSolidDisc(Target.transform.position, Vector3.up, Target.wanderRadius);
				}
				else
				{
					Vector3 p = Target.transform.position;
					Vector3[] verts = new Vector3[]{
									  new Vector3(p.x-Target.wanderWH.x, p.y, p.z+Target.wanderWH.y),
									  new Vector3(p.x+Target.wanderWH.x, p.y, p.z+Target.wanderWH.y),
									  new Vector3(p.x+Target.wanderWH.x, p.y, p.z-Target.wanderWH.y),
									  new Vector3(p.x-Target.wanderWH.x, p.y, p.z-Target.wanderWH.y),
								  };
					Handles.color = Color.white;
					Handles.DrawSolidRectangleWithOutline(verts, new Color(0.4f, 1f, 0.9f, 0.1f), new Color(0.4f, 1f, 0.9f, 0.1f));
				}
			}
		}
	}

	// ================================================================================================================
} }