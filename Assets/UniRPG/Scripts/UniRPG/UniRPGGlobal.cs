// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class UniRPGGlobal : MonoBehaviour
{
	// ================================================================================================================
	#region enums and static

	// these enums uses the values: 1,2,4,8,16,32,64,128,256,512,1024,etc so that they can be used as masks where needed
	// it is important to use 2 to power values for the mask system to work

	public enum Target
	{						// possible target types
		Player = 1,			// the player character
		Friendly = 2,		// Friendly NPC (like a shop keeper or quest giver)
		Neutral = 4,		// Neutral NPC (like a montser that will not attack first)
		Hostile = 8,		// Hostile NPC (like a monster)
		Self = 16,			// a special helper for things that can only be used on the user of the item or skill
		RPGItem = 32,		// an item (further rules might be required, for example identify scroll can only be used on items that are unknown)
		RPGObject = 64,		// an object
	}

	public enum ActorType
	{						// types of actors (these should follow the same values as Target)
		Player = 1,			// player character
		Friendly = 2,		// a friendly NPC, like quest giver or shop-keeper
		Neutral = 4,		// a neutral NPC, like wildlife or monster that will not attack first (turns Hostile when attacked)
		Hostile = 8,		// a hostile NPC, like a monster or wildlife that will attack as soon as it detects the player
	}

	public enum AIBehaviour 
	{ 
		Stay = 1,			// The AI will stay in spot
		Wander = 2,			// The AI will wander around
		Patrol = 3			// The AI will follow a Patrol Path
	}

	// for now i dont really see a need for these
	//// These are all the Tags that are used by UniRPG. These will be defined when a 
	//// new UniRPG DB iscreated or when "update build settings" button is pressed
	//public static class Tag
	//{
	//	public const string Character = "Character";
	//	public const string Object = "Object";
	//	public const string Item = "Item";
	//	public const string LootBag = "LootBag";
	//	public const string Trigger = "Trigger";
	//	public const string SpawnPoint = "SpawnPoint";
	//	public const string PatrolPath = "PatrolPath";
	//}

	#endregion
	// ================================================================================================================
	#region Inspector Properties

	public GameObject dbPrefab;				// the prefab that contains the Database component
	public GameObject loadingCamObj;		// the cam used on loading screens


	public static Database DB				// the database! UniRPGGlobal.DB
	{
		get
		{
			if (Instance._db == null) Debug.LogError("Error: UniRPG Database not loaded!");
			return _instance._db;
		}
	}
	private Database _db = null;

	#endregion
	// ================================================================================================================
	#region vars

	/// <summary>This is the Player running round in the game scene</summary>
	public static CharacterBase Player { get { return Instance._player; } } 
	public CharacterBase _player { get; set; }

	public enum State { 
		None = 0, LoadingMainMenu_Step1, LoadingMainMenu_Step2, LoadingGameScene, 
		InMainMenu,		// the playe is in the Main Menu
		InGame			// the player is in-game (running around in a game scene)
	}
	public State state { get; private set; }

	// the currently active camera, only one can be active at a time. Camera.main can also be used to access the active Camera
	public static GameCameraBase ActiveCamera { get { return Instance._activeCam; } }
	private GameCameraBase _activeCam = null;

	public List<GameCameraBase> gameCameras { get; private set; } // runtime instances of all defined game cameras

	private LoadSaveProviderBase loadSaveProvider = null;
	private string activeSaveSlot = "0";				// name of load/save slot
	//private string lastLoadedGameScene = "";			// referred to when having to save the game state

	public bool IsAutoLoadSave { get; private set; }	// true if this is an auto load/save (like switching between game scenes, not a PROPER Load/Save. You would for example not bother to restore the player's position when this is set)
	public bool DoNotLoad { get; private set; }			// this can be checked before deciding to Load. LoadString/Int/Float will just return default passed if this is set to true
	public Dictionary<string, string> SaveSlots { get; private set; }// the key is the slot and the value is the last datetime that a save was done

	private bool doNotSave = false;						// set to true when app quits. do not allow saving when app is quitting.
	private event UniRPGBasicEventHandler saveEventListener = null;

	public QuestListProviderBase QuestListProvider { get; private set; } // How you get the quest list. Will never be NULL as NoQUest will be used if no other provider selected.

	private bool gameAutoCallsMade = false;

	#endregion
	// ================================================================================================================
	#region gui related vars

	// used to find out if input was handled by a GUI. Set this each frame when your GUI handled input. It is automatic for OnGUI() type GUIs
	public static bool GUIConsumedInput {
		get 
		{ 
			if (GUIUtility.hotControl != 0) Instance._guiConsumedInput = true;
			return Instance._guiConsumedInput; 
		}
		set { Instance._guiConsumedInput = value; } }
	private bool _guiConsumedInput = false;

	// reference to the Main Menu Object
	// GameObject must be called "MainMenu" (this should not be called when the player is NOT in the Main Menu else it will return null)
	public static GameObject MainMenuObject 
	{
		get
		{
			if (_mainMenuObject == null) _mainMenuObject = GameObject.Find("MainMenu");
			return _mainMenuObject;
		}
	}

	// reference to the In-Game GUI/HUD Object
	// GameObject must be called "GameGUI" (this should not be called when the player is in the Main Menu, will return null)
	public static GameObject GameGUIObject
	{ 	
		get
		{
			if (_gameGUIObject == null) _gameGUIObject = GameObject.Find("GameGUI");
			return _gameGUIObject;
		}
	}

	private static GameObject _mainMenuObject = null;
	private static GameObject _gameGUIObject = null;

	#endregion
	// ================================================================================================================
	#region misc

	// startup and transition related-----
	public string startPlayerName { get; set; }				// name that the player selected to use
	public GameObject startCharacterDef { get; set; }		// this tells what character prefab to spawn as player (or default chosen at design time)
	public RPGActorClass startCharacterClass { get; set; }	// what class the player selected (or auto set by system)
	public int playerSpawnPoint { get; set; }				// -1: use 1st new-game spawn in scene or center of world, else use the specified ID
	private string playerCharaIdent = string.Empty;			// used by the LoadSave system to identify what chracter was chosen

	// ....
	// contains data that cant be carried by Database since it is only used in main menu (note, only available for MainMenu, not in-game)
	public static MainMenuData MainMenuData
	{
		get
		{
			if (Instance._mainMenuData == null)
			{
				Instance._mainMenuData = (MainMenuData)GameObject.FindObjectOfType(typeof(MainMenuData));
			}
			return Instance._mainMenuData;
		}
	}
	private MainMenuData _mainMenuData = null;

	private LoadingGUI loadGUI;								// I keep this seperate from Global data so that it can be enabled and disabled as needed

	public delegate void OnLoadGameScene(bool started);		// started = true when loading started, else false when it is completed
	public event OnLoadGameScene sceneLoadListeners = null;	// listeners to call when loading a game scene (when showing loading screen)

	private List<GameObject> regedGlobalObjects = new List<GameObject>(); // these are gameobjects that must be destroyed when the game is exited (when player moves back to the menu), since they have been set to not auto destroy

	#endregion
	// ================================================================================================================
	#region awake/start

	void Awake()
	{
		_instance = this;
		state = State.None;

		IsAutoLoadSave = false;
		DoNotLoad = true;
		SaveSlots = new Dictionary<string, string>(0);

		if (dbPrefab)
		{
			GameObject go = (GameObject)GameObject.Instantiate(dbPrefab);
			go.name = "Database";
			go.transform.parent = gameObject.transform;
			_db = go.GetComponent<Database>();
		}

		if (_db == null)
		{
			Debug.LogError("The Database prefab is not set. The main UniRPG scene is invalid. This issue MUST be fixed!");
			return;
		}

		_db.Initialise();

		// add the Quest List Provider component, if any
		if (!string.IsNullOrEmpty(_db.questListProviderType)) QuestListProvider = (QuestListProviderBase)gameObject.AddComponent(_db.questListProviderType);
		if (QuestListProvider == null) QuestListProvider = (QuestListProviderBase)gameObject.AddComponent<NoQuest>();

		// set some defaults
		startPlayerName = null;
		if (_db.defaultPlayerCharacterPrefab != null) startCharacterDef = _db.defaultPlayerCharacterPrefab;

		// this object is global and should exist until the game exit
		GameObject.DontDestroyOnLoad(this.gameObject);

		// setup loading-gui
		LoadGUISettings lgui = _db.loadGUISettings.GetComponent<LoadGUISettings>();
		loadGUI = gameObject.GetComponent<LoadingGUI>();
		loadGUI.skin = lgui.guiSkin;
		loadGUI.text = lgui.loadText;
		loadGUI.image = lgui.loadBackground;
		if (loadGUI.image) loadGUI.imgRect = GUIElementTransform.CalcRect(lgui.designW, lgui.designH, lgui.trLoadBackground);
		loadGUI.designW = lgui.designW;
		loadGUI.designH = lgui.designH;
		loadGUI.enabled = false;
		
		// disable the cam for now, only used when showing loading screen
		loadingCamObj.SetActive(false);

		_guiConsumedInput = false;
	}

	void Start()
	{
#if UNITY_EDITOR
		// During development a designer might Play another scene which would load this one.
		// In that case I do not want to force load the MENUGUI except if dev played the "unirpg" scene directly
		if (Application.loadedLevelName.Equals("unirpg"))
		{
			ShowLoading(State.LoadingMainMenu_Step1);
			//Application.LoadLevelAdditiveAsync("menudata");
			Application.LoadLevelAdditive("menudata");
		}
#else
		// load the main menu scene now
		ShowLoading(State.LoadingMainMenu_Step1);
		//Application.LoadLevelAdditiveAsync("menudata");		
		Application.LoadLevelAdditive("menudata");		
#endif

		// create the LoadSave Provider instance
		if (_db.loadSaveProviderPrefab)
		{
			GameObject go = (GameObject)GameObject.Instantiate(_db.loadSaveProviderPrefab);
			go.name = "LoadSave Provider";
			go.transform.parent = transform;
			loadSaveProvider = go.GetComponent<LoadSaveProviderBase>();
		}

		if (loadSaveProvider == null)
		{
			Debug.LogError("No LoadSave Provider found. This will cause errors.");
		}
		else
		{
			// load info on the saved data (save slots)
			int count = 0;
			count = loadSaveProvider.GetInt("saveslots", 0);
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					string s = loadSaveProvider.GetString("saveslot" + i, null);
					if (!string.IsNullOrEmpty(s))
					{
						string[] vs = s.Split('|');
						SaveSlots.Add(vs[0], vs[1]);
					}
				}
			}
		}

		LoadGameSettings();
	}

	public void OnApplicationQuit()
	{
		doNotSave = true;
	}

	#endregion
	// ================================================================================================================
	#region moving between scenes (game-game and menu) (loading)

	/*public void ShowLoading()
	{
		ShowLoading(State.LoadingGameScene);
	}*/

	/// <summary>
	/// Call this to load a new game. startPlayerName, startCharacterDef and startCharacterClass
	/// should be set before this is called if the player had an option to choose these
	/// </summary>
	public void LoadNewGame()
	{
		//loadSaveProvider.DeleteAll();

		ShowLoading(State.LoadingGameScene);

		DoNotLoad = true;		// this is New game. Do not load from saved data (not that there would be any after the next call)
		ClearActiveSaveSlot();	// make sure the active slot is cleared of old save data since this is a new game on the same slot

		// make sure there is a player character prefab set for spawning. this must be done here before MainMenuData is unloaded
		// since it is the only thing that carries data about characters than can be chosen from as player characters at start
		if (UniRPGGlobal.Instance.startCharacterDef == null)
		{
			Debug.LogWarning("The UniRPGGlobal.startCharacterDef is not set. The first player character from the db will be used.");
			if (UniRPGGlobal.MainMenuData.playerCharacters.Count > 0) UniRPGGlobal.Instance.startCharacterDef = UniRPGGlobal.MainMenuData.playerCharacters[0].gameObject;
		}

		// make record of the ident of selected player character
		CharacterBase c =  UniRPGGlobal.Instance.startCharacterDef.GetComponent<CharacterBase>();
		if (c) playerCharaIdent = c.id.ToString();
		
		// load the very 1st game scene as that is the "new game" scene
		if (DB.gameSceneNames.Count > 0)
		{
			//lastLoadedGameScene = DB.gameSceneNames[0];
			//Application.LoadLevelAsync(DB.gameSceneNames[0]);
			Application.LoadLevel(DB.gameSceneNames[0]);
		}
		else
		{
			Debug.LogError("You did not set any Game Map/Scenes yet. Do this under the Main tab -> Project Scenes option of the Database.");
		}
	}

	/// <summary>Exit from in-game to the Main Menu</summary>
	public void LoadGameToMenu()
	{
		DestroyCameras();
		ShowLoading(State.LoadingMainMenu_Step1);

		gameAutoCallsMade = false;
		IsAutoLoadSave = false;
		PerformSave();
		saveEventListener = null;	// clear all old listeners

		// destroy the reged globals
		for (int i = 0; i < regedGlobalObjects.Count; i++)
		{
			GameObject.Destroy(regedGlobalObjects[i]);
		}
		regedGlobalObjects.Clear();

		// destroy the DB and load a new copy so that everything in it is as during deisgn time
		GameObject.Destroy(_db);
		GameObject go = (GameObject)GameObject.Instantiate(dbPrefab);
		go.name = "Database";
		go.transform.parent = gameObject.transform;
		_db = go.GetComponent<Database>();

		// load menu
		//Application.LoadLevelAsync("menudata");
		Application.LoadLevel("menudata");
	}

	/// <summary>
	/// called to load a new "in-game" scene. will show the loading screen while loading the new scene which 
	/// will also cause the curent game map/scene scene to be unloaded
	/// </summary>
	public void LoadGameScene(string name)
	{
		ClearActiveCamera();
		ShowLoading(State.LoadingGameScene);
		if (sceneLoadListeners != null) sceneLoadListeners(true);

		DoNotLoad = false;
		IsAutoLoadSave = true;
		PerformSave();
		saveEventListener = null;	// clear all old listeners

		//lastLoadedGameScene = name;
		//Application.LoadLevelAsync(name);
		Application.LoadLevel(name);
	}

	/// <summary>
	/// register a listener to notify when LoadGameScene() is called (true is passed to callback) 
	/// and also when the scene is done loading (false is passed to callback/listener)
	/// </summary>
	public static void RegisterLoadGameSceneListener(OnLoadGameScene callback)
	{
		_instance.sceneLoadListeners += callback;
	}

	public static void RemoveLoadGameSceneListener(OnLoadGameScene callback)
	{
		if (_instance == null) return; // could be called during game exit
		_instance.sceneLoadListeners -= callback;
	}

	private void ShowLoading(State setState)
	{
		state = setState;
		loadGUI.enabled = true;
		loadingCamObj.SetActive(true);
	}

	public void OnMenusLoaded()
	{
		state = State.InMainMenu;

		gameAutoCallsMade = false;
		RunAutoCalls(_db.menuAutoCalls);

		loadGUI.enabled = false;
		loadingCamObj.SetActive(false);
	}

	/// <summary>Internal use. UniRPGGameController calls this when it has loaded, which is good indicator that the game scene has loaded</summary>
	public void OnGameSceneLoaded()
	{
		RestoreAutoItems(Application.loadedLevelName);

		state = State.InGame;

		if (!gameAutoCallsMade)
		{
			gameAutoCallsMade = true;
			RunAutoCalls(_db.gameAutoCalls);
		}

		loadGUI.enabled = false;
		loadingCamObj.SetActive(false);

		// Check the Game Controller
		if (UniRPGGameController.Instance == null)
		{
			Debug.LogError("GameController not found. The game scene is not setup correctly.");
		}

		// Create the Cameras
		CreateCameras();

		// set the active camera
		if (gameCameras.Count > 0)
		{
			SetActiveCamera(gameCameras[0]);
		}
		else
		{
			Camera cam = Camera.main;
			if (cam == null) cam = (Camera)GameObject.FindObjectOfType(typeof(Camera));
			if (cam != null)
			{
				GameCameraBase gc = cam.gameObject.GetComponent<GameCameraBase>();
				if (gc == null) gc = cam.gameObject.AddComponent<GameCameraBase>();
				SetActiveCamera(gc);
			}
		}

		// check enviro sounds volume
		if (DB.affectEnviroAudioVolume)
		{
			Object[] aus = GameObject.FindObjectsOfType(typeof(AudioSource));
			for (int i = 0; i < aus.Length; i++)
			{
				((AudioSource)aus[i]).volume = DB.enviroAudioVolume;
			}
		}

		// notify listeners
		if (sceneLoadListeners != null) sceneLoadListeners(false);
	}

	private void RunAutoCalls(List<string> classNames)
	{
		for (int i=0; i < classNames.Count; i++)
		{
			// first check if it is not already on the gameObject
			Component c = gameObject.GetComponent(classNames[i]);
			
			// if not found, then add it now
			if (c == null) c = gameObject.AddComponent(classNames[i]);

			// cast to AutoCallBase which is MUST be based on
			AutoCallBase b = c as AutoCallBase;
			if (b == null)
			{	// something wrong here
				Debug.LogError("Error: The class registered for Auto Call was not derived from AutoCallBase. ("+classNames[i]+")");
				Destroy(c);
				continue;
			}

			// enable it just in case it was disabled previously (when found, not applicable if just added)
			b.enabled = true;
			b.OnUniRPGAddedThis();
		}
	}

	#endregion
	// ================================================================================================================
	#region cameras

	private void CreateCameras()
	{
		if (gameCameras == null)
		{
			gameCameras = new List<GameCameraBase>();
			for (int i = 0; i < _db.Cameras.Length; i++)
			{
				GameObject go;
				if (_db.Cameras[i].cameraPrefab)
				{
					go = (GameObject)GameObject.Instantiate(_db.Cameras[i].cameraPrefab);
				}
				else
				{
					go = new GameObject();
					go.AddComponent<Camera>();
					if (_db.Cameras[i].includeAudioListener) go.AddComponent<AudioListener>();
					if (_db.Cameras[i].includeFlareLayer) go.AddComponent("FlareLayer");
					if (_db.Cameras[i].includeGuiLayer) go.AddComponent<GUILayer>();
				}
				go.name = _db.Cameras[i].name;

				// first try and get the component from the object, in case it was there on the prefab, else add it
				GameCameraBase cam = (GameCameraBase)go.GetComponent(_db.Cameras[i].GetType());
				if (!cam) cam = (GameCameraBase)go.AddComponent(_db.Cameras[i].GetType());

				GameCameraBase.DontDestroyOnLoad(go);	// dont want it auto destroyed
				_db.Cameras[i].CopyTo(cam);				// copy properties
				cam.gameObject.SetActive(false);		// it starts off inactive
				gameCameras.Add(cam);					// add to list of avail cams
				cam.CreatedByUniRPG();					// tell it that it was just created
			}
		}
	}

	private void DestroyCameras()
	{
		ClearActiveCamera();
		for (int i = gameCameras.Count - 1; i >= 0; i--)
		{
			if (gameCameras[i] == null)
			{
				Debug.LogWarning("Trying to Destroy a camera that did not exist. This should not happen and the camera being destroyed could have caused bugs in other areas.");
				continue;
			}
			GameObject.Destroy(gameCameras[i].gameObject);
		}
		gameCameras.Clear();
		gameCameras = null;
	}

	public void ClearActiveCamera()
	{
		if (_activeCam != null)
		{
			_activeCam.gameObject.SetActive(false);
			_activeCam.DeactivatedByUniRPG();
		}
		_activeCam = null;
	}

	public bool SetActiveCamera(GUID id)
	{
		if (id == null) return false;

		for (int i = 0; i < gameCameras.Count; i++)
		{
			if (gameCameras[i].id == id)
			{
				// deactivate prev cam
				if (_activeCam != null)
				{
					_activeCam.gameObject.SetActive(false);
					_activeCam.DeactivatedByUniRPG();
				}
				_activeCam = null;

				// activate selected cam
				SetActiveCamera(gameCameras[i]);
				return true;
			}
		}

		return false;
	}

	public void SetActiveCamera(GameCameraBase cam)
	{
		if (_activeCam != null)
		{
			_activeCam.gameObject.SetActive(false);
			_activeCam.DeactivatedByUniRPG();
		}
		_activeCam = null;
		if (cam == null) return;

		_activeCam = cam;
		_activeCam.gameObject.SetActive(true);
		_activeCam.ActivatedByUniRPG();

		AudioListener.volume = UniRPGGlobal.DB.audioMainVolume;
	}

	#endregion
	// ================================================================================================================
	#region Load/Save

	private static List<RPGItem> itemsRegedForAutoSaving = new List<RPGItem>();

	/// <summary> 
	/// Tell UniRPG that you want it to recreate this item in the world when 
	/// the player loads or re-enters the same scene. UniRPG will not
	/// save the item if it becomes null before saving is requested
	/// </summary>
	public static void RegisterItemForAutoSaving(RPGItem item)
	{
		if (!itemsRegedForAutoSaving.Contains(item))
		{
			item.IsPersistent = false;
			itemsRegedForAutoSaving.Add(item);
		}
	}

	/// <summary>
	/// Tell UniRPG that you do not want this item to be restored when loading happens.
	/// Only works for items that where registered with RegisterItemForAutoSaving.
	/// </summary>
	public static void RemoveItemFromAutoSaving(RPGItem item)
	{
		itemsRegedForAutoSaving.Remove(item);
	}

	private void SaveAutoItems(string scene)
	{
		if (loadSaveProvider == null) return;
		string key = activeSaveSlot + scene + "_items";
		string s = "";
		for (int i = 0; i < itemsRegedForAutoSaving.Count; i++)
		{
			if (itemsRegedForAutoSaving[i] != null)
			{
				s += itemsRegedForAutoSaving[i].prefabId.ToString() + "," +
					itemsRegedForAutoSaving[i].transform.position.x + "," +
					itemsRegedForAutoSaving[i].transform.position.y + "," +
					itemsRegedForAutoSaving[i].transform.position.z + ";";
			}
		}

		loadSaveProvider.SetString(key, s);
	}

	private void RestoreAutoItems(string scene)
	{
		if (loadSaveProvider == null) return;

		itemsRegedForAutoSaving = new List<RPGItem>();

		string key = activeSaveSlot + scene + "_items";
		string s = loadSaveProvider.GetString(key, null);

		if (!string.IsNullOrEmpty(s))
		{
			string[] its = s.Split(';');
			for (int i = 0; i < its.Length; i++)
			{
				string[] v = its[i].Split(',');
				Vector3 pos = Vector3.zero;
				RPGItem prefab = UniRPGGlobal.DB.GetItem(new GUID(v[0]));
				if (prefab != null)
				{
					float.TryParse(v[1], out pos.x);
					float.TryParse(v[2], out pos.y);
					float.TryParse(v[3], out pos.z);
					GameObject go = (GameObject)GameObject.Instantiate(prefab.gameObject);
					go.transform.position = pos;
					RPGItem item = go.GetComponent<RPGItem>();
					item.IsPersistent = false;
					RegisterItemForAutoSaving(item);
				}
			}
		}
	}

	// ================================================================================================================

	public static void RegisterForSaveEvent(UniRPGBasicEventHandler callback)
	{
		_instance.saveEventListener += callback;
	}

	public static void RemoveFromSaveEvent(UniRPGBasicEventHandler callback)
	{
		if (_instance == null) return; // might have been a call on destroy of object while app was exiting
		_instance.saveEventListener -= callback;
	}

	public static bool SetActiveSaveSlot(string slot)
	{
		if (slot.Contains("|"))
		{
			Debug.LogError("Error: Slot name can't contain the '|' character");
			return false;
		}

		_instance.activeSaveSlot = slot;

		return true;
	}

	public static void SaveGameState()
	{
		_instance.IsAutoLoadSave = false;
		_instance.PerformSave();
	}

	private void PerformSave()
	{
		// *** set the datetime of this slot's save
		string save = System.DateTime.Now.ToString("MMM d, HH:mm");
		if (SaveSlots.ContainsKey(activeSaveSlot))
		{
			SaveSlots[activeSaveSlot] = save;
			for (int i = 0; i < SaveSlots.Count; i++)
			{
				string s = loadSaveProvider.GetString("saveslot" + i, null);
				if (!string.IsNullOrEmpty(s))
				{
					string[] vs = s.Split('|');
					if (vs[0].Equals(activeSaveSlot))
					{
						loadSaveProvider.SetString("saveslot" + i, activeSaveSlot + "|" + save);
						break;
					}
				}
			}
		}
		else
		{
			SaveSlots.Add(activeSaveSlot, save);
			loadSaveProvider.SetString("saveslot" + (SaveSlots.Count - 1).ToString(), activeSaveSlot + "|" + save);
			loadSaveProvider.SetInt("saveslots", SaveSlots.Count);
		}

		// *** save which scene was open

		SaveString("scene", Application.loadedLevelName);

		// *** save player's selected character and class info

		if (UniRPGGlobal.DB.playerCanChooseName)
		{
			SaveString("plr_nm", startPlayerName);
		}

		if (UniRPGGlobal.DB.playerCanSelectCharacter)
		{
			if (string.IsNullOrEmpty(playerCharaIdent) && _player != null) playerCharaIdent = _player.id.ToString();
			if (string.IsNullOrEmpty(playerCharaIdent))
			{
				DeleteSaveKey("plr_ch");
				Debug.LogError("Error while saving selected player character. The ident was not set.");
			}
			else SaveString("plr_ch", playerCharaIdent);
		}

		if (UniRPGGlobal.DB.playerCanSelectClass)
		{
			if (startCharacterClass == null && _player != null)
			{
				if (_player.Actor != null) startCharacterClass = _player.Actor.ActorClass;
			}

			int idx = -1;
			for (int i = 0; i < UniRPGGlobal.DB.classes.Count; i++)
			{
				if (UniRPGGlobal.DB.classes[i].id == startCharacterClass.id) { idx = i; break; }
			}

			if (idx >= 0)
			{
				SaveInt("plr_cl", idx);
			}
			else
			{
				DeleteSaveKey("plr_cl");
				Debug.LogError("Error while saving selected player character class. The class could not be found in the list of defined character classes.");
			}
		}

		SaveAutoItems(Application.loadedLevelName);

		// *** save global vars

		SaveInt("global_nums", DB.numericVars.Count);
		SaveInt("global_strs", DB.stringVars.Count);
		for (int i = 0; i < DB.numericVars.Count; i++) SaveString("gnum" + i, DB.numericVars[i].name + "|" + DB.numericVars[i].val);
		for (int i = 0; i < DB.stringVars.Count; i++) SaveString("gstr" + i, DB.stringVars[i].name + "|" + DB.stringVars[i].val);

		// DO NOT SUPPORT SAVING GLOBAL VARS FOR OBJECT CAUSE IT DOES NOT MAKE REAL SENSE AND THERE
		// WILL BE BETTER WAYS TO DO WOTEVER THE DESIGNER WANTED TO DO WITHOUT USING OBJECT VAR

		// *** tell listeners to save
		if (saveEventListener != null) saveEventListener(null);
		loadSaveProvider.Save();
	}

	public static bool LoadGameState()
	{
		_instance.DoNotLoad = false; // whatever wants to load should load
		_instance.loadSaveProvider.Load();

		// *** load some needed info from save slot

		string scene = LoadString("scene", null);
		if (string.IsNullOrEmpty(scene))
		{
			Debug.LogError("Error: Could not find the scene to load");
			return false;
		}

		// *** load player's selected character and class info

		if (UniRPGGlobal.DB.playerCanChooseName)
		{
			_instance.startPlayerName = LoadString("plr_nm", _instance.startPlayerName);
		}

		if (UniRPGGlobal.DB.playerCanSelectCharacter)
		{
			_instance.playerCharaIdent = LoadString("plr_ch", string.Empty);
			GUID id = new GUID(_instance.playerCharaIdent);
			if (!string.IsNullOrEmpty(_instance.playerCharaIdent) && !id.IsEmpty)
			{
				bool found = false;
				for (int i = 0; i < UniRPGGlobal.MainMenuData.playerCharacters.Count; i++)
				{
					if (UniRPGGlobal.MainMenuData.playerCharacters[i].id == id)
					{
						_instance.startCharacterDef = UniRPGGlobal.MainMenuData.playerCharacters[i].gameObject;
						found = true; break;
					}
				}

				if (!found)
				{
					CharacterBase c = UniRPGGlobal.Instance.startCharacterDef.GetComponent<CharacterBase>();
					if (c) _instance.playerCharaIdent = c.id.ToString(); else _instance.playerCharaIdent = string.Empty;
					Debug.LogError("Error while restoring player character. The character prefab could not be found.");
				}
			}
			else
			{
				CharacterBase c = UniRPGGlobal.Instance.startCharacterDef.GetComponent<CharacterBase>();
				if (c) _instance.playerCharaIdent = c.id.ToString(); else _instance.playerCharaIdent = string.Empty;
				Debug.LogError("Error while restoring player character. The ident was not set.");
			}
		}

		if (UniRPGGlobal.DB.playerCanSelectClass)
		{
			int idx = LoadInt("plr_cl", -1);
			if (idx >= 0 && idx < UniRPGGlobal.DB.classes.Count)
			{
				_instance.startCharacterClass = UniRPGGlobal.DB.classes[idx];
			}
			else
			{
				Debug.LogError("Error while restoring player character class. The class idx was not saved or do not match with any defined classes. ("+idx+")");
			}
		}

		// *** Load global vars

		int count = LoadInt("global_nums", DB.numericVars.Count);
		for (int i = 0; i < count; i++)
		{
			string s =  LoadString("gnum" + i, null);
			if (!string.IsNullOrEmpty(s))
			{
				string[] vs = s.Split('|');
				if (vs.Length >= 2)
				{
					float n = 0f;
					if (float.TryParse(vs[1], out n)) DB.SetGlobalNumber(vs[0], n);
				}
			}
		}

		count = LoadInt("global_strs", DB.stringVars.Count);
		for (int i = 0; i < count; i++)
		{
			string s = LoadString("gstr" + i, null);
			if (!string.IsNullOrEmpty(s))
			{
				//string[] vs = s.Split('|');
				//if (vs.Length >= 2) DB.SetGlobalString(vs[0], vs[1]);
				int p = s.IndexOf('|');
				if (p > 0)
				{
					string key = s.Substring(0, p);
					DB.SetGlobalString(key, s.Substring(p+1));
					//Debug.Log("Load: (" + key + ") (" + s.Substring(p + 1) + ")");
				}
			}
		}

		// *** ready to load

		_instance.ShowLoading(State.LoadingGameScene);

		// make sure there is a player character prefab set for spawning. this must be done here before MainMenuData is unloaded
		// since it is the only thing that carries data about characters than can be chosen from as player characters at start
		if (UniRPGGlobal.Instance.startCharacterDef == null)
		{
			Debug.LogWarning("The UniRPGGlobal.startCharacterDef is not set. The first player character from the db will be used.");
			if (UniRPGGlobal.MainMenuData.playerCharacters.Count > 0) UniRPGGlobal.Instance.startCharacterDef = UniRPGGlobal.MainMenuData.playerCharacters[0].gameObject;
		}

		// load the game scene
		//_instance.lastLoadedGameScene = scene;
		//Application.LoadLevelAsync(DB.gameSceneNames[0]);
		Application.LoadLevel(scene);

		return true;
	}

	public void SaveGameSettings()
	{
		loadSaveProvider.SetFloat("sett_vol_main", DB.audioMainVolume);
		loadSaveProvider.SetFloat("sett_vol_gui", DB.guiAudioVolume);
		loadSaveProvider.SetFloat("sett_vol_mus", DB.musicVolume);
		loadSaveProvider.SetFloat("sett_vol_sfx", DB.fxAudioVolume);
		loadSaveProvider.SetFloat("sett_vol_env", DB.enviroAudioVolume);
		loadSaveProvider.SetInt("sett_gfx_q", DB.gfxQuality);
		loadSaveProvider.SetInt("sett_gfx_r", DB.gfxResolution);
		loadSaveProvider.SetBool("sett_gfx_f", DB.gfxFullscreen);
		loadSaveProvider.Save();
	}

	public void LoadGameSettings()
	{
		loadSaveProvider.Load();
		DB.audioMainVolume = loadSaveProvider.GetFloat("sett_vol_main", DB.audioMainVolume);
		DB.guiAudioVolume = loadSaveProvider.GetFloat("sett_vol_gui", DB.guiAudioVolume);
		DB.musicVolume = loadSaveProvider.GetFloat("sett_vol_mus", DB.musicVolume);
		DB.fxAudioVolume = loadSaveProvider.GetFloat("sett_vol_sfx", DB.fxAudioVolume);
		DB.enviroAudioVolume = loadSaveProvider.GetFloat("sett_vol_env", DB.enviroAudioVolume);
		DB.gfxQuality = loadSaveProvider.GetInt("sett_gfx_q", QualitySettings.GetQualityLevel());
		DB.gfxResolution = loadSaveProvider.GetInt("sett_gfx_r", GetRes());
		DB.gfxFullscreen = loadSaveProvider.GetBool("sett_gfx_f", Screen.fullScreen);
		UpdateSoundVol();
		UpdateGFX();		
	}

	public void UpdateSoundVol()
	{
		AudioListener.volume = UniRPGGlobal.DB.audioMainVolume;
	}

	private int GetRes()
	{
		int r = 0;
		int w = Screen.width;
		int h = Screen.height;

		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (Screen.resolutions[i].width == w && Screen.resolutions[i].height == h)
			{
				r = i;
				break;
			}
		}

		return r;
	}

	public void UpdateGFX()
	{
		int w = Screen.width;
		int h = Screen.height;

		if (DB.gfxResolution >= 0 && DB.gfxResolution < Screen.resolutions.Length)
		{
			w = Screen.resolutions[DB.gfxResolution].width;
			h = Screen.resolutions[DB.gfxResolution].height;
		}
		else
		{
			DB.gfxResolution = GetRes();
		}

		Screen.SetResolution(w, h, DB.gfxFullscreen);
		QualitySettings.SetQualityLevel(DB.gfxQuality);
	}

	public static void SaveString(string key, string value)
	{
		if (_instance == null) return;		// might have been save call on destroy of object while app was exiting
		if (_instance.doNotSave) return;	// app is quitting, too late to save now
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		_instance.RememberKey(key);
		_instance.loadSaveProvider.SetString(key, value);
	}

	public static void SaveInt(string key, int value)
	{
		if (_instance == null) return;		// might have been save call on destroy of object while app was exiting
		if (_instance.doNotSave) return;	// app is quiting, too late to save now
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		_instance.RememberKey(key);
		_instance.loadSaveProvider.SetInt(key, value);
	}

	public static void SaveFloat(string key, float value)
	{
		if (_instance == null) return;		// might have been save call on destroy of object while app was exiting
		if (_instance.doNotSave) return;	// app is quiting, too late to save now
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		_instance.RememberKey(key);
		_instance.loadSaveProvider.SetFloat(key, value);
	}

	public static void SaveBool(string key, bool value)
	{
		if (_instance == null) return;		// might have been save call on destroy of object while app was exiting
		if (_instance.doNotSave) return;	// app is quiting, too late to save now
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		_instance.RememberKey(key);
		_instance.loadSaveProvider.SetBool(key, value);
	}

	public static void SaveVector3(string key, Vector3 value)
	{
		if (_instance == null) return;		// might have been save call on destroy of object while app was exiting
		if (_instance.doNotSave) return;	// app is quiting, too late to save now
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		_instance.RememberKey(key);
		_instance.loadSaveProvider.SetVector3(key, value);
	}

	public static string LoadString(string key, string defaultValue)
	{
		if (_instance.DoNotLoad) return defaultValue;
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		return _instance.loadSaveProvider.GetString(key, defaultValue);
	}

	public static int LoadInt(string key, int defaultValue)
	{
		if (_instance.DoNotLoad) return defaultValue;
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		return _instance.loadSaveProvider.GetInt(key, defaultValue);
	}

	public static float LoadFloat(string key, float defaultValue)
	{
		if (_instance.DoNotLoad) return defaultValue;
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		return _instance.loadSaveProvider.GetFloat(key, defaultValue);
	}

	public static bool LoadBool(string key, bool defaultValue)
	{
		if (_instance.DoNotLoad) return defaultValue;
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		return _instance.loadSaveProvider.GetBool(key, defaultValue);
	}

	public static Vector3 LoadVector3(string key, Vector3 defaultValue)
	{
		if (_instance.DoNotLoad) return defaultValue;
		key = string.Format("{0}_{1}", _instance.activeSaveSlot, key);
		return _instance.loadSaveProvider.GetVector3(key, defaultValue);
	}

	public static void DeleteSaveKey(string key)
	{
		_instance.loadSaveProvider.DeleteKey(key);
	}

	public static void DeleteAllSaveSlots()
	{
		if (DB.loadSaveProviderPrefab)
		{
			LoadSaveProviderBase loadSaveProvider = DB.loadSaveProviderPrefab.GetComponent<LoadSaveProviderBase>();
			if (loadSaveProvider != null)
			{
				loadSaveProvider.DeleteAll();
			}
		}
	}

	public static void ClearActiveSaveSlot()
	{
		string s = _instance.loadSaveProvider.GetString(_instance.activeSaveSlot + "keys", null);
		if (!string.IsNullOrEmpty(s))
		{
			_instance.loadSaveProvider.DeleteKey(_instance.activeSaveSlot + "keys"); // don't need it any longer, so delete this entry too
			string[] keys = s.Split('|');
			s = null;
			for (int i = 0; i < keys.Length; i++)
			{
				DeleteSaveKey(keys[i]);
			}
		}
	}

	private void RememberKey(string key)
	{
		// first check if the key is not already saved before adding it
		string s = loadSaveProvider.GetString(activeSaveSlot + "keys", null);
		if (!string.IsNullOrEmpty(s))
		{
			if (s.Contains(key)) return;
		}

		s += "|" + key;
		loadSaveProvider.SetString(activeSaveSlot + "keys", s);
	}

	#endregion
	// ================================================================================================================
	#region pub - misc

	/// <summary>
	/// register a new global object with UniRPG. A Global object is one which is set to not be destroyed when 
	/// game scenes are changed but it will be destroyed when the player choses to go back to the main menu.
	/// </summary>
	public static void RegisterGlobalObject(GameObject obj)
	{
		if (!_instance.regedGlobalObjects.Contains(obj))
		{
			GameObject.DontDestroyOnLoad(obj);
			_instance.regedGlobalObjects.Add(obj);
		}
	}

	#endregion
	// ================================================================================================================
	#region update

	void Update()
	{
		if (state == State.LoadingMainMenu_Step1)
		{
			if (!Application.isLoadingLevel)
			{	// done loading the Menu Data, now load the actual main menu scene
				_mainMenuData = (MainMenuData)GameObject.FindObjectOfType(typeof(MainMenuData));
				state = State.LoadingMainMenu_Step2;
				//Application.LoadLevelAdditiveAsync("menugui");
				Application.LoadLevelAdditive("menugui");
			}
		}

		else if (state == State.LoadingMainMenu_Step2)
		{
			if (!Application.isLoadingLevel)
			{	// done loading main menu
				OnMenusLoaded();
			}
		}
	}

	#endregion
	// ================================================================================================================
	#region instance

	private static UniRPGGlobal _instance=null;
	public static UniRPGGlobal Instance
	{
		get
		{
			if (_instance == null)
			{	// if it can't be found like this then do not create it manually since the unirpg scene MUST be loaded
				_instance = GameObject.FindObjectOfType(typeof(UniRPGGlobal)) as UniRPGGlobal;
			}
			return _instance;
		}
	}

	public static bool InstanceExist { get { return _instance != null; } }

	#endregion
	// ================================================================================================================
} }