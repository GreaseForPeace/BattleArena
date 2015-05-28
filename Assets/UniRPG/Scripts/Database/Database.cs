// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class Database : MonoBehaviour 
{
	// ================================================================================================================
	#region system/unirpg settings

	public string activeGUITheme = "Fantasy Default";	// the GUI theme to use for game
	public GameObject menuGUIData;						// active theme's data/settings - for main menu related gui
	public GameObject gameGUIData;						// active theme's data/settings - for ingame related gui

	public GameObject startGUISettings;					// settings and images used when game is loaded for first time
	public GameObject loadGUISettings;					// settings and images used when showing load screen

	public GameObject loadSaveProviderPrefab;			// the prefab of the selected provider type
	public string questListProviderType = "";			// the System.Type name of provider

	// 0: Startup scene; 1: The UniRPG main scene; 2: MainMenuData; 3: The Menu GUI; 4: The In-Game GUI
	public string[] mainScenePaths = { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty }; // relative paths

	// These layers could be used to check things as needed. The systems needing these to be set should indicate such in the documentation
	public LayerMask floorLayerMask = 0;		// for example movement input might need to know what the floor/terrain is that can be clicked on
	public LayerMask playerLayerMask = 0;		// the player
	public LayerMask npcLayerMask = 0;			// NPCs
	public LayerMask rpgItemLayerMask = 0;		// Sword, Potions, etc
	public LayerMask rpgObjectLayerMask = 0;	// Chest, Trap, etc

	public float shopGlobalBuyMod = 0.5f;		// default for shops - at what modified price shops will buy (1 = no modification to price)
	public float shopGlobalSellMod = 1f;		// default for shops - at what modified price shops will sell (1 = no modification to price)
	public bool shopGlobalUsesCurrency = false;	// default for shops - if shops can only buy when they have currency
	public bool shopGlobalUnlimited = false;	// default for shops - if shop has unlimited stock of items

	#endregion
	// ================================================================================================================
	#region game settings

	public string currency = "Coin(s)";			// name of the currency used in the game
	public string currencyShort = "c";
	public GameObject currencyDropPrefab;		// the prefab to show when dropping currency loot

	public bool skillDoubleTapSystemOn = true;	// used to determine if player can double tap to auto-queue skill
	public float doubleTapTimeout = 0.3f;

	// these prefabs can be instantiated and used by the Player Character if it needs them. UniRPG will not instantiate them.
	public GameObject[] selectionRingPrefabs = new GameObject[5]; // 0:friendly, 1:neutral, 2:hostile, 3:item, 4:object

	// ================================================================================================================
	// settings that player can change

	public float audioMainVolume = 1f;
	public float musicVolume = 0.7f;
	public float guiAudioVolume = 1f;
	public float fxAudioVolume = 1f;

	public bool affectEnviroAudioVolume = true;	// choose whether this volume setting is used for environment audio - sounds placed in scene
	public float enviroAudioVolume = 1f;

	public int gfxQuality = 1;
	public int gfxResolution = 1;
	public bool gfxFullscreen = true;

	// ================================================================================================================

	public bool playerCanSelectCharacter = true;	// can the player select from a list of characters when he starts a new game
	public bool playerCanSelectClass = true;		// can the player select from a list of actor classes (warrior, wizard, etc) 
	public bool playerCanChooseName = true;			// can the plaeyr choose a name

	public GameObject defaultPlayerCharacterPrefab;	// this should only be set if you do not plan on giving the player an option to select a character

	// ================================================================================================================

	public List<InputDefinition> inputDefs = new List<InputDefinition>(0);

	// ================================================================================================================

	public List<string> menuAutoCalls = new List<string>(0); // these are called after the game was loaded
	public List<string> gameAutoCalls = new List<string>(0); // these are called after player wet from menu to in-game

	#endregion
	// ================================================================================================================
	#region definitions

	// these are the Global variables
	public List<NumericVar> numericVars = new List<NumericVar>(0);
	public List<StringVar> stringVars = new List<StringVar>(0);
	public List<ObjectVar> objectVars = new List<ObjectVar>(0);

	// the scenes/maps that the game world consist of
	public List<string> gameScenePaths = new List<string>(0);	// relative paths
	public List<string> gameSceneNames = new List<string>(0);	// cache of the scene names

	// definitions
	public List<RPGActorClass> classes = new List<RPGActorClass>(0);	// (also see ClassNames) warrior, wizard, hunter, monster, player, wolfmonster, etc
	public List<RPGAttribute> attributes = new List<RPGAttribute>(0);	// (also see AttributeNames) health, experience, intelligence, melee, etc
	public List<RPGState> states = new List<RPGState>(0);				// poisoned, cant_equip, etc
	public List<RPGLoot> loot = new List<RPGLoot>(0);					// loot tables (specifies the stuff that can be dropped by npc for player to pickup)

	// categories of items, for example, Armour, Weapon, Consumable, etc. - see ItemDef.category which related to an index into this list
	public List<RPGItem.Category> itemCategories = new List<RPGItem.Category>(0); // (also see ItemCategoryNames)
	
	// definition of the slots that can be used to place items, only one item per slot (this is just name definitions, actual slots are in Actor class)
	public List<string> equipSlots = new List<string>(0);

	// these are different from other definitions in that they are prefabs
	// rather use the Skills, RPGEvent, etc. properties (defined later below) to access the actual instanced object if not manipulating the prefab
	public List<GameObject> skillPrefabs = new List<GameObject>(0);		// use Skills at runtime
	public List<GameObject> rpgEventPrefabs = new List<GameObject>(0);	// use RPGEvents at runtime 
	public List<GameObject> cameraPrefabs = new List<GameObject>(0);	// ue Cameras at runtime
	public List<GameObject> rpgItemPrefabs = new List<GameObject>();	// use RPGItems at rungime

	#endregion
	// ================================================================================================================
	#region start/init

	// UniRPGGlobal will call this after it created an instance of the DB 
	public void Initialise()
	{
		InitRPGItems();
	}
	
	private void InitRPGItems()
	{
		// create a cache of the RPGItems that are available
		_rpgItems = new Dictionary<System.Guid, RPGItem>();
		for (int i = 0; i < rpgItemPrefabs.Count; i++)
		{
			if (rpgItemPrefabs[i] == null)
			{
				Debug.LogError("There are invalid Items in the Database, please refresh the Item List. Database -> Main -> RPG Items -> Refresh.");
				continue;
			}
			RPGItem item = rpgItemPrefabs[i].GetComponent<RPGItem>();
			if (item) _rpgItems.Add(item.prefabId.Value, item);
			else Debug.LogError(rpgItemPrefabs[i].name + " is not an RPGItem.");
		}
	}

	#endregion
	// ================================================================================================================
	#region pub/cached values

	/// <summary>A list of all defined RPGItems. <RPGItem.prefabId, RPGItem></summary>
	public Dictionary<System.Guid, RPGItem> RPGItems
	{
		get
		{
#if UNITY_EDITOR
			// this can be called during editor time when Item counts can inc/decr, so 1st check
			if (_rpgItems != null)
			{
				if (_rpgItems.Count == rpgItemPrefabs.Count) return _rpgItems;
			}
			InitRPGItems();
#endif
			if (_rpgItems == null) InitRPGItems();
			return _rpgItems;
		}
	}
	private Dictionary<System.Guid, RPGItem> _rpgItems = null;

	/// <summary>A list of all defined Skills.</summary>
	public RPGSkill[] Skills
	{
		get
		{
			if (_skillsCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more skills where added
				if (_skillsCache.Length == skillPrefabs.Count && (_skillsCache.Length==0 || (_skillsCache.Length > 0 && _skillsCache[0] != null))) return _skillsCache;
#else			
				return _skillsCache;
#endif
			}
			_skillsCache = new RPGSkill[skillPrefabs.Count];
			for (int i = 0; i < skillPrefabs.Count; i++) _skillsCache[i] = skillPrefabs[i].GetComponent<RPGSkill>();
			return _skillsCache;
		}
	}
	private RPGSkill[] _skillsCache = null;

	/// <summary>A list of all defined Skills.</summary>
	public RPGEvent[] RPGEvents
	{
		get
		{
			if (_rpgEventsCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more skills where added
				if (_rpgEventsCache.Length == rpgEventPrefabs.Count && (_rpgEventsCache.Length == 0 || (_rpgEventsCache.Length > 0 && _rpgEventsCache[0] != null))) return _rpgEventsCache;
#else			
				return _rpgEventsCache;
#endif
			}
			_rpgEventsCache = new RPGEvent[rpgEventPrefabs.Count];
			for (int i = 0; i < rpgEventPrefabs.Count; i++) _rpgEventsCache[i] = rpgEventPrefabs[i].GetComponent<RPGEvent>();
			return _rpgEventsCache;
		}
	}
	private RPGEvent[] _rpgEventsCache = null;

	/// <summary>A list of all defined Cameras.</summary>
	public GameCameraBase[] Cameras
	{
		get
		{
			if (_gameCamCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more skills where added
				if (_gameCamCache.Length == cameraPrefabs.Count && (_gameCamCache.Length == 0 || (_gameCamCache.Length > 0 && _gameCamCache[0] != null))) return _gameCamCache;
#else			
				return _gameCamCache;
#endif
			}
			_gameCamCache = new GameCameraBase[cameraPrefabs.Count];
			for (int i = 0; i < cameraPrefabs.Count; i++)
			{
				if (cameraPrefabs[i] == null) _gameCamCache[i] = null;
				else _gameCamCache[i] = cameraPrefabs[i].GetComponent<GameCameraBase>();
			}
			return _gameCamCache;
		}
	}
	private GameCameraBase[] _gameCamCache = null;

	/// <summary>The names of all defined actor classes. This will indexed 1 to 1 with classes above</summary>
	public string[] ClassNames
	{
		get {
			if (_classNameCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more classes where added
				if (_classNameCache.Length == classes.Count && (_classNameCache.Length == 0 || (_classNameCache.Length > 0 && _classNameCache[0] != null))) return _classNameCache;
#else			
				return _classNameCache;
#endif
			}
			_classNameCache = new string[classes.Count];
			for (int i = 0; i < classes.Count; i++) _classNameCache[i] = classes[i].screenName;
			return _classNameCache;
		}
	}
	private string[] _classNameCache = null;

	/// <summary>The names of all defined Item Categories. This will indexed 1 to 1 with itemCategories above</summary>
	public string[] ItemCategoryNames
	{
		get
		{
			if (_itemCategoryNameCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more categories where added
				if (_itemCategoryNameCache.Length == itemCategories.Count && (_itemCategoryNameCache.Length == 0 || (_itemCategoryNameCache.Length > 0 && _itemCategoryNameCache[0] != null))) return _itemCategoryNameCache;
#else			
				return _itemCategoryNameCache;
#endif
			}
			_itemCategoryNameCache = new string[itemCategories.Count];
			for (int i = 0; i < itemCategories.Count; i++) _itemCategoryNameCache[i] = itemCategories[i].screenName;
			return _itemCategoryNameCache;
		}
	}
	private string[] _itemCategoryNameCache = null;

	/// <summary>The names of all defined Attributes. This will indexed 1 to 1 with attributes above</summary>
	public string[] AttributeNames
	{
		get
		{
			if (_attributeNameCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more categories where added
				if (_attributeNameCache.Length == attributes.Count && (_attributeNameCache.Length == 0 || (_attributeNameCache.Length > 0 && _attributeNameCache[0] != null))) return _attributeNameCache;
#else			
				return _attributeNameCache;
#endif
			}
			_attributeNameCache = new string[attributes.Count];
			for (int i = 0; i < attributes.Count; i++) _attributeNameCache[i] = attributes[i].screenName;
			return _attributeNameCache;
		}
	}
	private string[] _attributeNameCache = null;

	/// <summary>The names of all defined Loot Tables. This will indexed 1 to 1 with loot above</summary>
	public string[] LootNames
	{
		get
		{
			if (_lootNameCache != null)
			{
#if UNITY_EDITOR // if in unity editor then need to check if more categories where added
				if (_lootNameCache.Length == loot.Count && (_lootNameCache.Length == 0 || (_lootNameCache.Length > 0 && _lootNameCache[0] != null))) return _lootNameCache;
#else			
				return _lootNameCache;
#endif
			}
			_lootNameCache = new string[loot.Count];
			for (int i = 0; i < loot.Count; i++) _lootNameCache[i] = loot[i].screenName;
			return _lootNameCache;
		}
	}
	private string[] _lootNameCache = null;


#if UNITY_EDITOR
	/// <summary>This is only available to the editor scripts. Do not use in runtime scripts.</summary>
	/// <param name="t">0= ClassNames, 1= ItemCategoryNames, 2= AttributeNames 3= LootTableNames</param>
	public void ForceUpdateCache(int t)
	{
		if (t == 0)
		{
			_classNameCache = new string[classes.Count];
			for (int i = 0; i < classes.Count; i++) _classNameCache[i] = classes[i].screenName;
		}

		else if (t == 1)
		{
			_itemCategoryNameCache = new string[itemCategories.Count];
			for (int i = 0; i < itemCategories.Count; i++) _itemCategoryNameCache[i] = itemCategories[i].screenName;
		}

		else if (t == 2)
		{
			_attributeNameCache = new string[attributes.Count];
			for (int i = 0; i < attributes.Count; i++) _attributeNameCache[i] = attributes[i].screenName;
		}

		else if (t == 3)
		{
			_lootNameCache = new string[loot.Count];
			for (int i = 0; i < loot.Count; i++) _lootNameCache[i] = loot[i].screenName;
		}
	}
#endif

	#endregion
	// ================================================================================================================
	#region Global Variables related

	/// <summary>Will set the global variable, else add it if not exist. name is case sensitive</summary>
	public void SetGlobalNumber(string name, float val)
	{
		for (int i = 0; i < numericVars.Count; i++)
		{
			if (numericVars[i].name.Equals(name)) { numericVars[i].val = val; return; }
		}
		numericVars.Add(new NumericVar() { name = name, val = val });
	}

	/// <summary>Will set the global variable, else add it if not exist. name is case sensitive</summary>
	public void SetGlobalString(string name, string val)
	{
		for (int i = 0; i < stringVars.Count; i++)
		{
			if (stringVars[i].name.Equals(name)) { stringVars[i].val = val; return; }
		}
		stringVars.Add(new StringVar() { name = name, val = val });
	}

	/// <summary>Will set the global variable, else add it if not exist. name is case sensitive</summary>
	public void SetGlobalObject(string name, UnityEngine.Object val)
	{
		for (int i = 0; i < objectVars.Count; i++)
		{
			if (objectVars[i].name.Equals(name)) { objectVars[i].val = val; return; }
		}
		objectVars.Add(new ObjectVar() { name = name, val = val });
	}

	public NumericVar GetGlobalNumericByName(string name)
	{
		for (int i = 0; i < numericVars.Count; i++)
		{
			if (numericVars[i].name.Equals(name)) return numericVars[i];
		}
		return null;
	}

	public StringVar GetGlobalStringByName(string name)
	{
		for (int i = 0; i < stringVars.Count; i++)
		{
			if (stringVars[i].name.Equals(name)) return stringVars[i];
		}
		return null;
	}

	public ObjectVar GetGlobalObjectByName(string name)
	{
		for (int i = 0; i < objectVars.Count; i++)
		{
			if (objectVars[i].name.Equals(name)) return objectVars[i];
		}
		return null;
	}

	#endregion
	// ================================================================================================================
	#region Attrib/ State/ Skill/ Item/ Event/ Loot getters

	/// <summary>find an attribute by its guid. return null if not found</summary>
	public RPGAttribute GetAttribute(GUID guid)
	{
		if (guid.IsEmpty) return null;
		for (int i = 0; i < attributes.Count; i++)
		{
			if (attributes[i].id == guid) return attributes[i];
		}
		return null;
	}

	/// <summary>find the index in AttributeNames of the attribute. return -1 if not found</summary>
	public int GetAttribNameIdx(GUID guid)
	{
		if (guid.IsEmpty) return -1;
		// AttributeNames should be in edact same order as attributes list
		for (int i = 0; i < attributes.Count; i++)
		{	
			if (attributes[i].id == guid) return i;
		}
		return -1; // not found
	}

	public RPGSkill GetSkill(GUID guid)
	{
		if (guid.IsEmpty) return null;
		for (int j = 0; j < Skills.Length; j++)
		{
			if (Skills[j].id == guid) return Skills[j];
		}
		return null;
	}

	public GameObject GetSkillPrefab(GUID guid)
	{
		if (guid.IsEmpty) return null;
		for (int j = 0; j < Skills.Length; j++)
		{
			if (Skills[j].id == guid) return Skills[j].gameObject;
		}
		return null;
	}

	public RPGState GetState(GUID guid)
	{
		if (guid.IsEmpty) return null;
		for (int j = 0; j < states.Count; j++)
		{
			if (states[j].id == guid) return states[j];
		}
		return null;
	}

	public RPGItem GetItem(GUID guid)
	{
		if (guid == null) return null;
		if (guid.IsEmpty) return null;
		if (RPGItems.ContainsKey(guid.Value)) return RPGItems[guid.Value];
		return null;
	}

	public RPGLoot GetLoot(GUID guid)
	{
		if (guid == null) return null;
		if (guid.IsEmpty) return null;
		for (int i = 0; i < loot.Count; i++)
		{
			if (loot[i].id == guid) return loot[i];
		}
		return null;
	}

	public RPGEvent GetEvent(GUID guid)
	{
		if (guid == null) return null;
		if (guid.IsEmpty) return null;
		for (int i = 0; i < RPGEvents.Length; i++)
		{
			if (RPGEvents[i].id == guid) return RPGEvents[i];
		}
		return null;
	}

	#endregion
	// ================================================================================================================
} }