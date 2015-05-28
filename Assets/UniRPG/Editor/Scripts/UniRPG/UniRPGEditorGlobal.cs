// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UniRPG;

namespace UniRPGEditor {

[InitializeOnLoad]
public class UniRPGEditorGlobal
{
	private const string UNIRPG_SYMBOL = "UNIRPG_CORE";
	
	// I have defined these like this so that syntax/typing errors can't cause bugs when
	// refering to these paths and file names. These can but should not really be changed.
	// If changed then these paths must stay relative to each other and must have the
	// last '/' as defined below.

	public const string PackagePath = "Assets/UniRPG/";								// where the package is installed

	public const string DB_FILE = "Assets/UniRPG Data/Database.prefab";				// location of the database prefab
	public const string DB_PATH = "Assets/UniRPG Data/";							// main database path

	public const string DB_SCENE_PATH = "Assets/UniRPG Data/Scenes/";				// default place to dump new UniRPG scenes
	public const string DB_SYS_SCENE_PATH = "Assets/UniRPG Data/Scenes/System/";	// default place to dump new UniRPG System/Main scenes
	public const string DB_DATA_PATH = "Assets/UniRPG Data/Database_Data/";			// main path to database's supporting data and prefabs

	public const string DB_GUI_PATH = "Assets/UniRPG Data/Database_Data/GUI/";		// location of GUI prefabs
	public const string DB_DEFS_PATH = "Assets/UniRPG Data/Database_Data/Defs/";	// location of Definitions for Classes, Skills, States and Attributes
	public const string DB_SKILLS_PATH = "Assets/UniRPG Data/Database_Data/Skills/";// where all skill definition prefabs are saved
	public const string DB_EVENTS_PATH = "Assets/UniRPG Data/Database_Data/Events/";// where all event definition prefabs are saved
	public const string DB_CAMERAS_PATH = "Assets/UniRPG Data/Database_Data/Cameras/";//where all camera definitions prefabs are saved

	public const string DB_DEF_CLASSES_FILE = "Assets/UniRPG Data/Database_Data/Defs/ActorClasses.asset";
	public const string DB_DEF_STATES_FILE = "Assets/UniRPG Data/Database_Data/Defs/States.asset";
	public const string DB_DEF_ATTRIBS_FILE = "Assets/UniRPG Data/Database_Data/Defs/Attributes.asset";
	public const string DB_DEF_LOOT_FILE = "Assets/UniRPG Data/Database_Data/Defs/Loot.asset";

	public const string LOADSAVE_PREFAB = "Assets/UniRPG Data/Database_Data/Defs/loadsave.prefab";	// location of the selected LoadSave Provider prefab

	public const string CACHE_FILE = "Assets/UniRPG Data/Cache.prefab";				// location of the cache prefab

	private static Database _db = null;	// easy access to the UniRPG Database for the project/editors
	public static Database DB
	{
		get
		{
			if (_db == null) LoadDatabase(true);
			return _db;
		}
	}

	// this cache is used for things that must be scanned for in the project and which 
	// would slow down the dev process if the designer have to wait for the scan
	// to run each time he wants to test or the code recompiles
	private static UniRPGEditorCache _cache = null;
	public static UniRPGEditorCache Cache { get { if (_cache == null) RefreshCache(false); return _cache; } }

	// cache of actions editors
	public static string[] ActionEdNames { get; private set; }
	public static UniRPGActionEdInfo[] ActionEditors { get; private set; }

	// cache of actions database editors
	public static string[] DBEdNames { get; private set; }
	public static DatabaseEdBase[] DBEditors { get; private set; }

	// cache for gui editors
	public static UniRPGGUIEdInfo[] GUIEditors { get; private set; }
	public static int activeGUIThemeIdx = -1; // the selected gui theme

	// cache for camera editors
	public static UniRPGCameraEdInfo[] CameraEditors { get; private set; }

	// cache for LoadSave provider editors
	public static LoadSaveEdInfo[] LoadSaveEditors { get; private set; }
	public static int activeLoadSaveIdx = -1; // the selected LoadSave Provider LoadSaveEditors index
	public static LoadSaveProviderBase currLoadSaveProvider = null;

	// cache of Quest List providers that are installed
	public static string[] QuestListProviderNames { get; private set; }		// string array which can be used in popup (dropdown list)
	public static string[] QuestListProviderTypes { get; private set; }

	// cache of input layouts (<group_name, <bind_name, def>>)
	public static Dictionary<string, Dictionary<string, InputDefinition>> inputBinds = new Dictionary<string, Dictionary<string, InputDefinition>>();

	// reged toolbar buttons
	public static List<UniRPGEditorToolbar.ToolbarButton> ToolbarButtons = new List<UniRPGEditorToolbar.ToolbarButton>();

	// reged auto callers. unirpg need to check that all these are in the DB when building
	public static List<string> menuAutoCalls = new List<string>();	// these are called after the game was loaded
	public static List<string> gameAutoCalls = new List<string>();	// these are called after player wet from menu to in-game

	// private vars and helpers
	private static string currentScene = string.Empty;
	private static int playTestingMode = 0; // 0:not inited, 1:inited, 2:game started from unirpg menu

	// ================================================================================================================
	#region pub and helpers for UniRPG Editors

	public static string GetActionShortNfoString(Action action)
	{
		if (action == null || UniRPGEditorGlobal.ActionEditors.Length == 0) return "!ERROR!";

		// first find the action's editor, since it will have the needed info
		int idx =-1;
		for (int i = 0; i < UniRPGEditorGlobal.ActionEditors.Length; i++)
		{
			// if the action could be assigned from the given type then I assume it is of that type
			if (action.GetType().IsAssignableFrom(UniRPGEditorGlobal.ActionEditors[i].actionType))
			{
				idx = i;
				break;
			}
		}

		if (idx >= 0)
		{
			return UniRPGEditorGlobal.ActionEditors[idx].editor.ActionShortNfo(action);
		}

		return "!ERROR!";
	}

	public static void AddToolbarButton(UniRPGEditorToolbar.ToolbarButton button)
	{
		ToolbarButtons.Add(button);
		SortToolbarButtons();
	}

	public static void AddToolbarButtons(List<UniRPGEditorToolbar.ToolbarButton> buttons)
	{
		ToolbarButtons.AddRange(buttons);
		SortToolbarButtons();
	}

	private static void SortToolbarButtons()
	{
		ToolbarButtons.Sort((a, b) => a.order.CompareTo(b.order));
	}

	/// <summary>
	/// Register a class to "auto call". This must be class derived from AutoCallBase
	/// callOnMenuLoaded = true means the auto call happens when the main menu is loaded, which is after game
	/// has started or after player went back to the menu from being in a game scene
	/// callOnMenuLoaded = false means the auto call happens when the player goes from the menu
	/// to being in a game scene. It will nto be called when the player moves betwene game scenes.
	/// </summary>
	public static void RegisterAutoCall(string className, bool callOnMenuLoaded)
	{
		if (callOnMenuLoaded)
		{
			if (!menuAutoCalls.Contains(className)) menuAutoCalls.Add(className);
		}
		else
		{
			if (!gameAutoCalls.Contains(className)) gameAutoCalls.Add(className);
		}
	}

	#endregion
	// ================================================================================================================
	#region Unity startup/loading & editor callback

	/// <summary>this constructor is called when Unity starts up cause of the (InitializeOnLoad) attribute of the class</summary>
 	static UniRPGEditorGlobal()
	{
		CheckForOldInstall();

		DefineSymbols();

		// load some settings
		UniRPGSettingsEditor.LoadSettings();

		// load caches
		Assembly[] asms = System.AppDomain.CurrentDomain.GetAssemblies();
		LoadGUIEditors(asms);
		LoadCameraEditors(asms);
		LoadLoadSaveEditors(asms);
		LoadActionEditors(asms);
		LoadDatabaseEditors(asms);
		LoadQuestListProviders(asms);
		AutoLoadInputBinders(asms);

		// hook some callbacks
		UniRPGEdUtil.RegisterPreviewObjectsChecker();
		EditorApplication.hierarchyWindowChanged += UniRPGEditorGlobal.OnHierarchyChanged;
		EditorApplication.update += UniRPGEditorGlobal.OnUpdate;
	}

	private static void CheckForOldInstall()
	{
		// Resources was renamed to "Res" for update 0.7.0
		if (UniRPGEdUtil.RelativePathExist("Assets/UniRPG/Editor/Resources/"))
		{
			Debug.LogWarning("Upgrading from older version of UniRPG. Renaming 'Assets/UniRPG/Editor/Resources' to 'Assets/UniRPG/Editor/Res'. Please make sure that this action took place before you continue to use UniRPG.");
			AssetDatabase.MoveAsset("Assets/UniRPG/Editor/Resources", "Assets/UniRPG/Editor/Res");
		}
	}

	private static void DefineSymbols()
	{
		// run through all the build targets and add the define symbol if needed
		foreach (BuildTargetGroup btg in System.Enum.GetValues(typeof(BuildTargetGroup)))
		{
			// extract existing defines to check if the ones to be added
			// does not allready exist before adding the new defines

			string defines_field = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
			List<string> defines = new List<string>(defines_field.Split(';'));
			if (!defines.Contains(UNIRPG_SYMBOL))
			{
				defines.Add(UNIRPG_SYMBOL);
				PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, string.Join(";", defines.ToArray()));
			}
		}
	}

	private static void LoadGUIEditors(Assembly[] asms)
	{
		// find all the classes that inherit from DatabaseEdBase
		List<System.Type> foundEdTypes = new List<System.Type>();

		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating GUI Editor cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating GUI Editor cache", progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(GUIEditorBase).IsAssignableFrom(types[j]) && types[j].Name != "GUIEditorBase")
				{
					foundEdTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data and create the editor instances
		List<UniRPGGUIEdInfo> eds = new List<UniRPGGUIEdInfo>();

		progress = 0f;
		step = 1f / (float)foundEdTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating GUI Editor cache", progress);

		for (int i = 0; i < foundEdTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating GUI Editor cache", progress);

			bool err = true;
			UniRPGGUIThemeAttribute att = null;
			System.Object[] attribs = foundEdTypes[i].GetCustomAttributes(typeof(UniRPGGUIThemeAttribute), false);
			if (attribs.Length > 0)
			{	// find the ALL occurance of UniRPGGUIThemeAttribute
				for (int j = 0; j < attribs.Length; j++)
				{
					att = (attribs[j] as UniRPGGUIThemeAttribute);
					if (att != null)
					{
						err = false;
						UniRPGGUIEdInfo nfo = new UniRPGGUIEdInfo();
						nfo.name = att.Name;
						nfo.menuGUIPath = att.menuGUIPath;
						nfo.gameGUIPath = att.gameGUIPath;
						nfo.menuGUIDataType = att.menuGUIDataType;
						nfo.gameGUIDataType = att.gameGUIDataType;
						nfo.editor = (GUIEditorBase)System.Activator.CreateInstance(foundEdTypes[i]);
						eds.Add(nfo);
					}
				}
			}
		
			if (err)
			{
				Debug.LogError("Invalid GUI Editor [" + foundEdTypes[i].ToString() + "] encountered. Please check the documentation on how to create custom GUI Theme and Editor.");
			}
		}

		// sort the editors according to priority
		eds.Sort(delegate(UniRPGGUIEdInfo a, UniRPGGUIEdInfo b) { return a.name.CompareTo(b.name); });

		// update the caches
		GUIEditors = eds.ToArray();
		EditorUtility.ClearProgressBar();
	}

	private static void LoadCameraEditors(Assembly[] asms)
	{
		// find all the classes that inherit from GameCameraEditorBase
		List<System.Type> foundEdTypes = new List<System.Type>();

		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Camera Editor cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Camera Editor cache", progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(GameCameraEditorBase).IsAssignableFrom(types[j]) && types[j].Name != "GameCameraEditorBase")
				{
					foundEdTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data and create the editor instances
		List<UniRPGCameraEdInfo> eds = new List<UniRPGCameraEdInfo>();

		progress = 0f;
		step = 1f / (float)foundEdTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Camera Editor cache", progress);

		for (int i = 0; i < foundEdTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Camera Editor cache", progress);

			bool err = true;
			GameCameraAttribute att = null;
			System.Object[] attribs = foundEdTypes[i].GetCustomAttributes(typeof(GameCameraAttribute), false);
			if (attribs.Length > 0)
			{	// find the ALL occurance of GameCameraAttribute
				for (int j = 0; j < attribs.Length; j++)
				{
					att = (attribs[j] as GameCameraAttribute);
					if (att != null)
					{
						err = false;
						UniRPGCameraEdInfo nfo = new UniRPGCameraEdInfo();
						nfo.name = att.Name;
						nfo.cameraType = att.cameraType;
						nfo.editor = (GameCameraEditorBase)System.Activator.CreateInstance(foundEdTypes[i]);
						eds.Add(nfo);
					}
				}
			}

			if (err)
			{
				Debug.LogError("Invalid Camera Editor [" + foundEdTypes[i].ToString() + "] encountered. Please check the documentation on how to create custom Cameras and Editor.");
			}
		}

		// sort the editors according to priority/ name
		eds.Sort(delegate(UniRPGCameraEdInfo a, UniRPGCameraEdInfo b) { return a.name.CompareTo(b.name); });

		// update the caches
		CameraEditors = eds.ToArray();
		EditorUtility.ClearProgressBar();
	}

	private static void LoadLoadSaveEditors(Assembly[] asms)
	{
		// find all the classes that inherit from LoadSaveProviderEdBase
		List<System.Type> foundEdTypes = new List<System.Type>();

		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating LoadSave Provider Editor cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating LoadSave Provider Editor cache", progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(LoadSaveProviderEdBase).IsAssignableFrom(types[j]) && types[j].Name != "LoadSaveProviderEdBase")
				{
					foundEdTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data and create the editor instances
		List<LoadSaveEdInfo> eds = new List<LoadSaveEdInfo>();

		progress = 0f;
		step = 1f / (float)foundEdTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating LoadSave Provider Editor cache", progress);

		for (int i = 0; i < foundEdTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating LoadSave Provider Editor cache", progress);

			bool err = true;
			LoadSaveProviderAttribute att = null;
			System.Object[] attribs = foundEdTypes[i].GetCustomAttributes(typeof(LoadSaveProviderAttribute), false);
			if (attribs.Length > 0)
			{	// find the ALL occurance of GameCameraAttribute
				for (int j = 0; j < attribs.Length; j++)
				{
					att = (attribs[j] as LoadSaveProviderAttribute);
					if (att != null)
					{
						err = false;
						LoadSaveEdInfo nfo = new LoadSaveEdInfo();
						nfo.name = att.Name;
						nfo.providerType = att.providerType;
						nfo.editor = (LoadSaveProviderEdBase)System.Activator.CreateInstance(foundEdTypes[i]);
						eds.Add(nfo);
					}
				}
			}

			if (err)
			{
				Debug.LogError("Invalid LoadSave Provider Editor [" + foundEdTypes[i].ToString() + "] encountered. Please check the documentation on how to create custom LoadSave Provider and Editor.");
			}
		}

		// sort the editors according to priority/ name
		eds.Sort(delegate(LoadSaveEdInfo a, LoadSaveEdInfo b) { return a.name.CompareTo(b.name); });

		// update the caches
		LoadSaveEditors = eds.ToArray();
		EditorUtility.ClearProgressBar();
	}

	private static void LoadDatabaseEditors(Assembly[] asms)
	{
		// find all the classes that inherit from DatabaseEdBase
		List<System.Type> foundEdTypes = new List<System.Type>();

		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Database Editor cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Database Editor cache" , progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(DatabaseEdBase).IsAssignableFrom(types[j]) && types[j].Name != "DatabaseEdBase")
				{
					foundEdTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data and create the editor instances
		List<UniRPGDBEdInfo> eds = new List<UniRPGDBEdInfo>();

		progress = 0f;
		step = 1f / (float)foundEdTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Database Editor cache", progress);

		for (int i = 0; i < foundEdTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Database Editor cache", progress);

			DatabaseEditorAttribute att = null;
			System.Object[] attribs = foundEdTypes[i].GetCustomAttributes(typeof(DatabaseEditorAttribute), false);
			if (attribs.Length > 0)
			{	// find the 1st occurance of DatabaseEditorAttribute
				for (int j = 0; j< attribs.Length; j++) 
				{
					att = (attribs[j] as DatabaseEditorAttribute);
					if (att != null) break;
				}
			}

			if (att != null)
			{	// now create the editor instance
				UniRPGDBEdInfo nfo = new UniRPGDBEdInfo();
				nfo.priority = att.Priority;
				nfo.name = att.Name;
				nfo.editor = (DatabaseEdBase)System.Activator.CreateInstance(foundEdTypes[i]);
				eds.Add(nfo);
			}
			else
			{
				Debug.LogError("Invalid Database Editor [" + foundEdTypes[i].ToString() + "] encountered. Please check the documentation on how to create custom Database Editors.");
			}
		}

		// sort the editors according to priority
		eds.Sort(delegate(UniRPGDBEdInfo a, UniRPGDBEdInfo b) {return a.priority.CompareTo(b.priority);});

		// update the caches
		DBEditors = new DatabaseEdBase[eds.Count];
		DBEdNames = new string[eds.Count];
		for (int i = 0; i < eds.Count; i++)
		{
			DBEdNames[i] = eds[i].name;
			DBEditors[i] = eds[i].editor;
		}

		EditorUtility.ClearProgressBar();
	}

	private static void LoadActionEditors(Assembly[] asms)
	{
		// find all the classes that inherit from ActionsEdBase
		List<System.Type> foundEdTypes = new List<System.Type>();

		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Action Editor cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Action Editor cache", progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(ActionsEdBase).IsAssignableFrom(types[j]) && types[j].Name != "ActionsEdBase")
				{
					foundEdTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data and created the editors
		List<UniRPGActionEdInfo> eds = new List<UniRPGActionEdInfo>();

		progress = 0f;
		step = 1f / (float)foundEdTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Action Editor cache", progress);

		for (int i = 0; i < foundEdTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Action Editor cache", progress);

			ActionInfoAttribute att = null;
			System.Object[] attribs = foundEdTypes[i].GetCustomAttributes(typeof(ActionInfoAttribute), false);
			if (attribs.Length > 0)
			{	// find the 1st occurance of ActionInfoAttribute
				for (int j = 0; j < attribs.Length; j++)
				{
					att = (attribs[j] as ActionInfoAttribute);
					if (att != null) break;
				}
			} 
			
			if (att != null)
			{	// create editor instance
				UniRPGActionEdInfo nfo = new UniRPGActionEdInfo();
				nfo.name = att.Name;
				nfo.descr = att.Description;
				nfo.editor = (ActionsEdBase)System.Activator.CreateInstance(foundEdTypes[i]);
				nfo.actionType = att.Action;
				eds.Add(nfo);
			}
			else
			{
				Debug.LogError("Invalid Action Editor [" + foundEdTypes[i].ToString() + "] encountered. Please check the documentation on how to create custom Actions.");
			}
		}

		// sort the editors according to name
		eds.Sort(delegate(UniRPGActionEdInfo a, UniRPGActionEdInfo b) { return a.name.CompareTo(b.name); });

		// update the caches
		ActionEditors = new UniRPGActionEdInfo[eds.Count];
		ActionEdNames = new string[eds.Count];
		for (int i = 0; i < eds.Count; i++)
		{
			ActionEdNames[i] = eds[i].name;
			ActionEditors[i] = eds[i];
		}

		EditorUtility.ClearProgressBar();
	}

	private static void LoadQuestListProviders(Assembly[] asms)
	{
		Dictionary<string, string> questListProviders = new Dictionary<string, string>();
		List<System.Type> foundTypes = new List<System.Type>();
		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Quest List Provider cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Quest List Provider cache", progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(QuestListProviderBase).IsAssignableFrom(types[j]) && types[j].Name != "QuestListProviderBase")
				{
					foundTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data
		progress = 0f;
		step = 1f / (float)foundTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Quest List Provider cache", progress);

		for (int i = 0; i < foundTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Quest List Provider cache", progress);

			QuestListProviderAttribute att = null;
			System.Object[] attribs = foundTypes[i].GetCustomAttributes(typeof(QuestListProviderAttribute), false);
			if (attribs.Length > 0)
			{	// find the 1st occurance of ActionInfoAttribute
				for (int j = 0; j < attribs.Length; j++)
				{
					att = (attribs[j] as QuestListProviderAttribute);
					if (att != null) break;
				}
			}

			if (att != null)
			{
				questListProviders.Add(att.Name, foundTypes[i].Name);
			}
			else
			{
				Debug.LogError("Invalid Quest List Provider [" + foundTypes[i].ToString() + "] encountered. Please check the documentation on how to create custom Quest List Provider.");
			}
		}
		EditorUtility.ClearProgressBar();

		QuestListProviderNames = new string[questListProviders.Count];
		QuestListProviderTypes = new string[questListProviders.Count];
		int c = 0;
		foreach (KeyValuePair<string, string> kv in questListProviders)
		{
			QuestListProviderNames[c] = kv.Key;
			QuestListProviderTypes[c] = kv.Value;
			c++;
		}
	}

	private static void AutoLoadInputBinders(Assembly[] asms)
	{
		// Find all the input binders that should auto-load
		// Find all the classes that inherit from InputLayoutBase
		List<System.Type> foundTypes = new List<System.Type>();

		float progress = 0f;
		float step = 1f / (float)asms.Length;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Input cache", progress);

		for (int i = 0; i < asms.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Input cache", progress);
			System.Type[] types = asms[i].GetExportedTypes();
			for (int j = 0; j < types.Length; j++)
			{
				if (types[j].IsClass && typeof(InputBinderBase).IsAssignableFrom(types[j]) && types[j].Name != "InputBinderBase")
				{
					foundTypes.Add(types[j]);
				}
			}
		}

		// extract some meta data and load input binders that are set to AutoLoad
		progress = 0f;
		step = 1f / (float)foundTypes.Count;
		EditorUtility.DisplayProgressBar("Updating ...", "Updating Input cache", progress);

		for (int i = 0; i < foundTypes.Count; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar("Updating ...", "Updating Input cache", progress);

			InputBinderAttribute att = null;
			System.Object[] attribs = foundTypes[i].GetCustomAttributes(typeof(InputBinderAttribute), false);
			if (attribs.Length > 0)
			{	// find the 1st occurance of ActionInfoAttribute
				for (int j = 0; j < attribs.Length; j++)
				{
					att = (attribs[j] as InputBinderAttribute);
					if (att != null) break;
				}
			}

			if (att != null)
			{	// load the binder
				if (att.EditorAutoLoad)
				{
					InputBinderBase binder = (InputBinderBase)System.Activator.CreateInstance(foundTypes[i]);
					LoadInputsFromBinder(binder);
				}
			}
		}

		EditorUtility.ClearProgressBar();
	}

	public static void RefreshCache(bool forceRefresh)
	{
		// first try to load from the prefab object that must be holding the info, else create it now
		if (UniRPGEdUtil.RelativeFileExist(CACHE_FILE) && !forceRefresh)
		{
			_cache = AssetDatabase.LoadAssetAtPath(CACHE_FILE, typeof(UniRPGEditorCache)) as UniRPGEditorCache;
		}

		if (_cache == null || forceRefresh)
		{
			CheckDatabasePaths(); // make sure the path, where the cache will be saved, exists
			if (UniRPGEdUtil.RelativeFileExist(CACHE_FILE))
			{
				// delete the old file if there was one
				AssetDatabase.DeleteAsset(CACHE_FILE);
			}

			_cache = ScriptableObject.CreateInstance<UniRPGEditorCache>();
			_cache.RefreshAll();
			UniRPGEdUtil.AddObjectToAssetFile(_cache, CACHE_FILE);
		}
	}

	public static void RefreshAll()
	{
		UniRPGEditorGlobal.RefreshCache(true);
		SystemEditor.FindAllRPGItems();
	}

	public static void LoadInputsFromBinder(InputBinderBase binder)
	{
		List<InputDefinition> defs = binder.GetInputBinds();
		foreach (InputDefinition d in defs)
		{
			if (!inputBinds.ContainsKey(d.groupName)) inputBinds.Add(d.groupName, new Dictionary<string, InputDefinition>());
			if (!inputBinds[d.groupName].ContainsKey(d.inputName))
			{
				inputBinds[d.groupName].Add(d.inputName, d);
			}
		}
	}

	public static void UnloadInputsFromBinder(InputBinderBase binder)
	{
	}

	private static void OnHierarchyChanged()
	{
		// check if the currentScene changed to detect when another scene is loaded and to wipe any preview ojects that remain
		if (currentScene != EditorApplication.currentScene)
		{
			currentScene = EditorApplication.currentScene;
			UniRPGEdUtil.DeleteAllEditorOnlyObjects();
		}
	}

	private static void OnUpdate()
	{
		if (playTestingMode != 1)
		{	// when unity start (or play mode starts) then playTestingMode will be 0
			// use the EditorPrefs to carry the correct state of playTestingMode around
			// I need playTestingMode to see when play mode is ended and to then reload
			// the scene the dev was working on since I changed it to the unirpg startup 
			// scene for the play test was started 
			if (playTestingMode == 0)
			{
				playTestingMode = EditorPrefs.GetInt("UniRPG_CurrentPlayTestingMode", 1);
				if (!EditorApplication.isPlayingOrWillChangePlaymode && playTestingMode != 1)
				{
					playTestingMode = 1;
					EditorPrefs.SetInt("UniRPG_CurrentPlayTestingMode", 1);
				}
			}
			else if (playTestingMode == 2)
			{
				if (!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					playTestingMode = 1;
					EditorPrefs.SetInt("UniRPG_CurrentPlayTestingMode", 1);
					string prevScene = EditorPrefs.GetString("UniRPG_PlayTestingModeScene", "");
					if (!string.IsNullOrEmpty(prevScene)) EditorApplication.OpenScene(prevScene);
				}
			}
		}

	}

	#endregion
	// ================================================================================================================
	#region menus

	[MenuItem("Window/UniRPG/Database", false, 1)]
	public static void OpenDatabaseEditor()
	{
		LoadOrCreateDatabase();
		DatabaseEditor.ShowEditor();
	}

	[MenuItem("Help/UniRPG Documentation", false, 99)]
	[MenuItem("Window/UniRPG/Documentation", false, 20)]
	public static void ShowUniRPGDocs()
	{
		//Application.OpenURL("file://" + UniRPGEdUtil.FullProjectPath + PackagePath + "Documentation/Documentation.html");
		Application.OpenURL("http://plyoung.com/unirpg/docs/index.html");
	}

	[MenuItem("Window/UniRPG/About", false, 21)]
	public static void ShowUniRPGAbout()
	{
		UniRPGAbout.ShowAbout();
	}

	[MenuItem("Edit/Run UniRPG Game", false, 2)]
	public static void RunGame()
	{
		if (UniRPGEditorGlobal.LoadDatabase())
		{
			if (!EditorApplication.SaveCurrentSceneIfUserWantsTo()) return;

			EditorPrefs.SetString("UniRPG_PlayTestingModeScene", EditorApplication.currentScene);
			UniRPGEditorGlobal.SetupBuildSettingsAndGlobals(UniRPGEditorGlobal._db, false, true);
			if (EditorApplication.OpenScene(UniRPGEditorGlobal._db.mainScenePaths[0]))
			{
				EditorPrefs.SetInt("UniRPG_CurrentPlayTestingMode", 2);
				EditorApplication.isPlaying = true;
				playTestingMode = 2;
			}
		}
	}

	[MenuItem("Assets/Create/UniRPG Scene", false, 2000)]
	public static void Create_Scene()
	{
		if (!EditorApplication.SaveCurrentSceneIfUserWantsTo()) return;

		// first make sure there is a database
		if (!UniRPGEditorGlobal.LoadDatabase()) return;

		// make a copy of the default UniRPG Game/Map Scene
		CheckDatabasePath(DB_PATH, DB_SCENE_PATH);
		string fn = AssetDatabase.GenerateUniqueAssetPath(DB_SCENE_PATH + "gamescene.unity");
		Debug.Log("Creating: " + fn);
		if (!AssetDatabase.CopyAsset(PackagePath + "System/gamescene.unity", fn))
		{
			Debug.LogError("Could not create new UniRPG Scene at '"+fn+"'");
			return;
		}

		AssetDatabase.Refresh();

		// add it to the database scene list
		UniRPGEditorGlobal._db.gameScenePaths.Add(fn);
		string nm = fn.Substring(fn.LastIndexOf("/")+1);
		nm = nm.Substring(0, nm.LastIndexOf(".unity"));
		UniRPGEditorGlobal._db.gameSceneNames.Add(nm);

		// open it for editing
		EditorApplication.OpenScene(fn);
	}

	[MenuItem("GameObject/Create Other/UniRPG Spawn Point", false, 2000)]
	public static void Create_SpawnPoint()
	{
		// 1st create the parent if it does not exist
		GameObject parent = GameObject.Find("SpawnPoints");
		if (!parent) parent = new GameObject("SpawnPoints");

		// now create spawn point
		GameObject obj = UniRPGEdUtil.NewGameObjectInSceneView("SpawnPoint", true, (1 << UniRPGSettings.floorLayeMask));
		if (!obj) return;
		obj.transform.position += new Vector3(0f, 0.08f, 0f); // want it a little heigher
		obj.transform.parent = parent.transform;
		obj.AddComponent<SpawnPoint>();

		// select the new object
		Selection.activeGameObject = obj;
	}

	[MenuItem("GameObject/Create Other/UniRPG Patrol Path", false, 2000)]
	public static void Create_PatrolPath()
	{
		// 1st create the parent if it does not exist
		GameObject parent = GameObject.Find("PatrolPaths");
		if (!parent) parent = new GameObject("PatrolPaths");

		// now create spawn point
		GameObject obj = UniRPGEdUtil.NewGameObjectInSceneView("PatrolPath " + (parent.transform.childCount + 1).ToString(), true, (1 << UniRPGSettings.floorLayeMask));
		if (!obj) return;
		obj.transform.parent = parent.transform;
		PatrolPath path = obj.AddComponent<PatrolPath>();
		path.CreateDefaultPoints();

		// select the new object
		Selection.activeGameObject = obj;
	}

	[MenuItem("GameObject/Create Other/UniRPG Trigger", false, 2000)]
	public static void Create_Trigger()
	{
		// 1st create the parent if it does not exist
		GameObject parent = GameObject.Find("Triggers");
		if (!parent) parent = new GameObject("Triggers");

		// now create trigger
		GameObject obj = UniRPGEdUtil.NewGameObjectInSceneView("Trigger " + (parent.transform.childCount + 1).ToString(), true, (1 << UniRPGSettings.floorLayeMask));
		if (!obj) return;
		obj.transform.parent = parent.transform;
		obj.AddComponent<Trigger>();	//Trigger t = obj.AddComponent<Trigger>();		

		// select the new object
		Selection.activeGameObject = obj;
	}

	#endregion
	// ================================================================================================================
	#region init & defaults

	public static bool LoadDatabase(bool silent = false)
	{
		if (UniRPGEditorGlobal._db != null) return true;
		if (!UniRPGEdUtil.RelativeFileExist(DB_FILE))
		{
			Debug.LogError("You must first create a UniRPG Database. From the menu, select: Window -> UniRPG -> Database");
			if (!silent) EditorUtility.DisplayDialog("Warning", "You must first create a UniRPG Database. From the menu, select: Window -> UniRPG -> Database", "Close");
			return false;
		}
		else
		{
			GameObject go = AssetDatabase.LoadAssetAtPath(DB_FILE, typeof(GameObject)) as GameObject;
			UniRPGEditorGlobal._db = go.GetComponent<Database>();
			if (_db != null)
			{
				PerformAfterDBLoaded(_db);
				return true;
			}
		}
		return false;
	}

	public static void LoadOrCreateDatabase()
	{
		if (!UniRPGEdUtil.RelativeFileExist(DB_FILE))
		{
			CreateDatabase();
		}
		else
		{
			GameObject go = AssetDatabase.LoadAssetAtPath(DB_FILE, typeof(GameObject)) as GameObject;
			_db = go.GetComponent<Database>();
			PerformAfterDBLoaded(_db);
		}
	}

	private static void CreateDatabase()
	{
		//CheckTags();

		if (!EditorApplication.SaveCurrentSceneIfUserWantsTo()) return;

		Debug.Log("Creating UniRPG Database");

		// delete old data
		DeleteOldAssets();

		// create data folders
		CheckDatabasePaths();

		// copy system scenes
		CopySystemScenes();

		// create object & save as prefab
		Object prefab = PrefabUtility.CreateEmptyPrefab(DB_FILE);
		GameObject go = new GameObject("Database"); // create temp object in scene 
		go.AddComponent<Database>();				// add database component
		GameObject dbPrefab = PrefabUtility.ReplacePrefab(go, prefab); // save prefab
		GameObject.DestroyImmediate(go);			// wipe temp object from scene

		Database db = dbPrefab.GetComponent<Database>();
		UniRPGEditorGlobal.InitDatabaseDefaults(db);
		PerformAfterDBLoaded(db);

		// open the unirpg scene and set the db property
		bool err = true;
		if (EditorApplication.OpenScene(db.mainScenePaths[1]))
		{
			go = GameObject.Find("UniRPGGlobal");
			if (go)
			{
				UniRPGGlobal urpg = go.GetComponent<UniRPGGlobal>();
				if (urpg)
				{
					err = false;
					urpg.dbPrefab = dbPrefab;
					EditorApplication.SaveScene();
				}
			}
		}

		AssetDatabase.Refresh();

		if (err)
		{
			// show a blank scene
			EditorApplication.NewScene();		
			EditorUtility.DisplayDialog("Error!", "An error occured while trying to set the Database property of the main UniRPG scene. Your UniRPG install might be corrupt.", "Close");
			return;
		}
		else
		{
			UpdateStartupScene(db, false);
		}
	}

	private static void PerformAfterDBLoaded(Database db)
	{
		// Init the LoadSave Provider vars/ or select the default one now if one is not selected
		InitLoadSaveProvider(db, -1);

		// make sure autocall are valid
		UpdateAutoCallList(db);
	}

	public static void InitLoadSaveProvider(Database db, int forceUsing)
	{
		activeLoadSaveIdx = -1;
		if (LoadSaveEditors.Length >= 0)
		{
			if (db.loadSaveProviderPrefab && forceUsing == -1)
			{	// first try and find out what provider was selected previously and init with it, if possible
				LoadSaveProviderBase provider = db.loadSaveProviderPrefab.GetComponent<LoadSaveProviderBase>();
				if (provider)
				{
					for (int i = 0; i < LoadSaveEditors.Length; i++)
					{
						if (provider.providerName.Equals(LoadSaveEditors[i].name))
						{
							activeLoadSaveIdx = i;
							currLoadSaveProvider = provider;
							break;
						}
					}
				}
			}

			// if idx == -1 then one is not selected and must be added now
			if (activeLoadSaveIdx == -1)
			{
				if (forceUsing == -1)
				{
					// will select UniRPG's default provider
					for (int i = 0; i < LoadSaveEditors.Length; i++)
					{
						if (LoadSaveEditors[i].providerType == typeof(DefaultLoadSave)) { activeLoadSaveIdx = i; break; }
					}

					if (activeLoadSaveIdx == -1) activeLoadSaveIdx = 0; // else grab first avail
				}
				else activeLoadSaveIdx = forceUsing;

				// create object & save as prefab
				Object prefab = PrefabUtility.CreateEmptyPrefab(LOADSAVE_PREFAB);
				GameObject go = new GameObject("LoadSave Provider");			// create temp object in scene 
				go.AddComponent(LoadSaveEditors[activeLoadSaveIdx].providerType);	
				GameObject lspPrefab = PrefabUtility.ReplacePrefab(go, prefab); // save prefab
				GameObject.DestroyImmediate(go);								// wipe temp object from scene

				db.loadSaveProviderPrefab = lspPrefab;
				currLoadSaveProvider = db.loadSaveProviderPrefab.GetComponent<LoadSaveProviderBase>();
				currLoadSaveProvider.providerName = LoadSaveEditors[activeLoadSaveIdx].name;

				EditorUtility.SetDirty(db.loadSaveProviderPrefab);
				EditorUtility.SetDirty(db);
				AssetDatabase.SaveAssets();
			}
		}
	}

	private static void CopySystemScenes()
	{
		AssetDatabase.CopyAsset(UniRPGEditorGlobal.PackagePath + "System/startup.unity", DB_SYS_SCENE_PATH + "startup.unity");
		AssetDatabase.CopyAsset(UniRPGEditorGlobal.PackagePath + "System/unirpg.unity", DB_SYS_SCENE_PATH + "unirpg.unity");
		AssetDatabase.CopyAsset(UniRPGEditorGlobal.PackagePath + "System/menudata.unity", DB_SYS_SCENE_PATH + "menudata.unity");	
	}

	public static void CheckDatabasePath(string parentPath, string newPath)
	{
		if (!UniRPGEdUtil.RelativePathExist(newPath))
		{
			//Debug.Log("Creating: " + newPath);
			parentPath = parentPath.Substring(0, parentPath.LastIndexOf('/'));	// remove last '/'
			newPath = newPath.Substring(0, newPath.LastIndexOf('/'));			// remove last '/'
			newPath = newPath.Substring(newPath.LastIndexOf('/')+1);			// extract the dir to be created
			AssetDatabase.CreateFolder(parentPath, newPath);
		}
	}

	private static void CheckDatabasePaths()
	{
		CheckDatabasePath("Assets/", DB_PATH);
		
		CheckDatabasePath(DB_PATH, DB_SCENE_PATH);
		CheckDatabasePath(DB_SCENE_PATH, DB_SYS_SCENE_PATH);

		CheckDatabasePath(DB_PATH, DB_DATA_PATH);

		CheckDatabasePath(DB_DATA_PATH, DB_GUI_PATH);
		CheckDatabasePath(DB_DATA_PATH, DB_DEFS_PATH);
		CheckDatabasePath(DB_DATA_PATH, DB_SKILLS_PATH);
		CheckDatabasePath(DB_DATA_PATH, DB_EVENTS_PATH);
		CheckDatabasePath(DB_DATA_PATH, DB_CAMERAS_PATH);
		AssetDatabase.Refresh();
	}

	private static void DeleteDatabasePath(string path)
	{
		if (UniRPGEdUtil.RelativePathExist(path))
		{
			Debug.Log("Deleting all assets in: " + path);
			path = path.Substring(0, path.LastIndexOf('/'));	// remove last '/'
			AssetDatabase.DeleteAsset(path);
		}
	}

	private static void DeleteOldAssets()
	{
		// wipe the Database_Data folder
		DeleteDatabasePath(DB_DATA_PATH);

		// delete the database prefab
		DeleteDatabasePath(DB_FILE);
		
		AssetDatabase.Refresh();
	}

	private static void InitDatabaseDefaults(Database db)
	{
		// create default startup settings
		InitDefaultLoadGUI(db);

		// create default camera
		GameCameraBase cam = CameraEditor.CreateCamera("Default", typeof(DefaultCam1));
		//Debug.Log(cam);
		EditorUtility.SetDirty(cam);
		db.cameraPrefabs.Add(cam.gameObject); 

		// create default actorclass
		RPGActorClass ac = ScriptableObject.CreateInstance<RPGActorClass>();
		UniRPGEdUtil.AddObjectToAssetFile(ac, DB_DEF_CLASSES_FILE);
		ac.screenName = "Default";
		ac.availAtStart = false;
		EditorUtility.SetDirty(ac);
		db.classes = new List<RPGActorClass>();
		db.classes.Add(ac);

		// other defaults
		db.itemCategories = new List<RPGItem.Category>() 
		{ 
			new RPGItem.Category() { screenName = "Undefined" },
			new RPGItem.Category() { screenName = "Weapon", types = { "Wand", "Trinket", "Knife", "Dagger", "Sword", "Axe", "Hammer", "Staff", "Two-Handed Sword", "Two-Handed Axe", "Two-Handed Hammer", "Ranged" } },
			new RPGItem.Category() { screenName = "Armour", types = { "Cloth", "Leather", "Metal" } },
			new RPGItem.Category() { screenName = "Accessory" },
			new RPGItem.Category() { screenName = "Consumable" },
		};

		db.equipSlots = new List<string>() { "Head", "Body", "Main-Hand", "Off-Hand", "Necklace", "Ring1", "Ring2" };

		// select a defauilt gui theme and init scene paths
		db.mainScenePaths[0] = DB_SYS_SCENE_PATH + "startup.unity";
		db.mainScenePaths[1] = DB_SYS_SCENE_PATH + "unirpg.unity";
		db.mainScenePaths[2] = DB_SYS_SCENE_PATH + "menudata.unity";

		if (UniRPGEditorGlobal.GUIEditors.Length >= 0)
		{
			db.activeGUITheme = UniRPGEditorGlobal.GUIEditors[0].name;
			InitGUIThemeData(db, 0);
		}
		else
		{
			Debug.LogError("Error: No GUI Themes are defined.");
		}

		// --------
		// save
		EditorUtility.SetDirty(db);
		AssetDatabase.SaveAssets();
	}

	private static void InitDefaultLoadGUI(Database db)
	{
		// Default Startup Settings
		string assetPath = DB_GUI_PATH + "StartupGUI.prefab";
		if (!UniRPGEdUtil.RelativeFileExist(assetPath))
		{
			//Debug.Log("Creating: " + assetPath);
			Object prefab = PrefabUtility.CreateEmptyPrefab(assetPath);		// craete prefab space
			GameObject go = new GameObject("StartupGUI");					// create temp object in scene
			StartupSettings c = go.AddComponent<StartupSettings>();			// add component to temp objects

			c.images = new List<Texture2D>(2);
			c.guiSkin = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Startup/DefaultStartSkin.guiskin", typeof(GUISkin)) as GUISkin;
			c.images.Add((Texture2D)AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Startup/splash_sample1.png", typeof(Texture2D)));
			c.images.Add((Texture2D)AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Startup/splash_sample2.png", typeof(Texture2D)));
			c.trImages.size = new Vector2(c.images[0].width, c.images[0].height);

			GameObject prefabGo = PrefabUtility.ReplacePrefab(go, prefab);	// save object as prefab
			GameObject.DestroyImmediate(go);								// delete temp object from scene
			db.startGUISettings = prefabGo;
		}
		else
		{
			Debug.Log("Loading: " + assetPath);
			db.startGUISettings = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
		}

		// Default LoadeGUI Settings
		assetPath = DB_GUI_PATH  + "LoadGUI.prefab";
		if (!UniRPGEdUtil.RelativeFileExist(assetPath))		
		{
			//Debug.Log("Creating: " + assetPath);
			Object prefab = PrefabUtility.CreateEmptyPrefab(assetPath);		// craete prefab space
			GameObject go = new GameObject("LoadGUI");						// create temp object in scene
			LoadGUISettings l = go.AddComponent<LoadGUISettings>();	// add component to temp objects

			l.guiSkin = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Startup/DefaultStartSkin.guiskin", typeof(GUISkin)) as GUISkin;

			GameObject prefabGo = PrefabUtility.ReplacePrefab(go, prefab);	// save object as prefab
			GameObject.DestroyImmediate(go);								// delete temp object from scene
			db.loadGUISettings = prefabGo;
		}
		else
		{
			Debug.Log("Loading: " + assetPath);
			db.loadGUISettings = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
		}
	}

	public static void InitGUIThemeData(Database db, int idx)
	{
		activeGUIThemeIdx = idx;
		db.mainScenePaths[3] = UniRPGEditorGlobal.GUIEditors[idx].menuGUIPath;
		db.mainScenePaths[4] = UniRPGEditorGlobal.GUIEditors[idx].gameGUIPath;
		
		// load/create the data files for the selected gui theme
		bool created = false;
		string assetPath = DB_GUI_PATH + "GuiTheme " + db.activeGUITheme + " MenuData.prefab";
		if (!UniRPGEdUtil.RelativeFileExist(assetPath))
		{
			//Debug.Log("Creating: " + assetPath);
			Object prefab = PrefabUtility.CreateEmptyPrefab(assetPath);
			GameObject go = new GameObject("GuiTheme " + db.activeGUITheme);
			go.AddComponent(UniRPGEditorGlobal.GUIEditors[idx].menuGUIDataType);
			GameObject prefabGo = PrefabUtility.ReplacePrefab(go, prefab);
			GameObject.DestroyImmediate(go);
			db.menuGUIData = prefabGo;
			created = true;
		}
		else
		{
			Debug.Log("Loading: " + assetPath);
			db.menuGUIData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
		}

		assetPath = DB_GUI_PATH + "GuiTheme " + db.activeGUITheme + " InGameData.prefab";
		if (!UniRPGEdUtil.RelativeFileExist(assetPath))
		{
			//Debug.Log("Creating: " + assetPath);
			Object prefab = PrefabUtility.CreateEmptyPrefab(assetPath);
			GameObject go = new GameObject("GuiTheme " + db.activeGUITheme);
			go.AddComponent(UniRPGEditorGlobal.GUIEditors[idx].gameGUIDataType);
			GameObject prefabGo = PrefabUtility.ReplacePrefab(go, prefab);
			GameObject.DestroyImmediate(go);
			db.gameGUIData = prefabGo;
			created = true;
		}
		else
		{
			Debug.Log("Loading: " + assetPath);
			db.gameGUIData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
		}

		if (created)
		{
			UniRPGEditorGlobal.GUIEditors[idx].editor.InitDefaults(db.menuGUIData, db.gameGUIData);
		}
	}

	private static void UpdateStartupScene(Database db, bool promptSave)
	{	// update the start scene with the startup settings and gui
		if (promptSave)
		{
			if (!EditorApplication.SaveCurrentSceneIfUserWantsTo()) return;
		}

		bool err = true;
		if (EditorApplication.OpenScene(db.mainScenePaths[0]))
		{
			GameObject go = GameObject.Find("StartupGUI");
			if (go)
			{
				Startup obj = go.GetComponent<Startup>();
				if (obj)
				{
					err = false;
					obj.settings = db.startGUISettings;
					EditorApplication.SaveScene();
				}
			}
		}

		// show a blank scene
		EditorApplication.NewScene();		

		if (err)
		{
			EditorUtility.DisplayDialog("Error!", "An error occured while trying to set the Startup Settings. Your UniRPG install might be corrupt.", "Close");
		}
	}

	public static void SetupBuildSettingsAndGlobals(Database db, bool promptSave, bool setupForPlaytest)
	{
		//CheckTags();

		string prevScene = EditorApplication.currentScene;
		if (promptSave)
		{
			if (!EditorApplication.SaveCurrentSceneIfUserWantsTo()) return;
		}

		// ----------------------------------------------------------------------------
		// Make sure autocall list is valid
		UpdateAutoCallList(db);

		// ----------------------------------------------------------------------------
		// Update the Build Settings - adding the scenes as needed
		ArrayList scenes = new ArrayList(db.mainScenePaths.Length + db.gameScenePaths.Count);
		for (int i = 0; i < db.mainScenePaths.Length; i++)
		{
			scenes.Add(new EditorBuildSettingsScene { enabled = true, path = db.mainScenePaths[i] });
		}
		for (int i = 0; i < db.gameScenePaths.Count; i++)
		{
			scenes.Add(new EditorBuildSettingsScene { enabled = true, path = db.gameScenePaths[i] });
		}
		EditorBuildSettings.scenes = scenes.ToArray(typeof(EditorBuildSettingsScene)) as EditorBuildSettingsScene[];

		// ----------------------------------------------------------------------------
		// Check that the Startup scene is still correctly setup
		bool err = true;
		if (EditorApplication.OpenScene(db.mainScenePaths[0]))
		{
			GameObject go = GameObject.Find("StartupGUI");
			if (go)
			{
				Startup data = go.GetComponent<Startup>();
				if (data)
				{
					err = false;
					if (!data.settings)
					{
						data.settings = db.startGUISettings;
						EditorApplication.SaveScene();
					}
				}
			}
		}
		if (err) EditorUtility.DisplayDialog("Error!", "An error occured while trying to set the Startup scene Data. Your UniRPG install might be corrupt.", "Close");

		// ----------------------------------------------------------------------------
		// Check that the main UniRPG scene is still correctly setup
		err = true;
		if (EditorApplication.OpenScene(db.mainScenePaths[1]))
		{
			GameObject go = GameObject.Find("UniRPGGlobal");
			if (go)
			{
				UniRPGGlobal data = go.GetComponent<UniRPGGlobal>();
				if (data)
				{
					err = false;
					if (!data.dbPrefab)
					{
						data.dbPrefab = db.gameObject;
						EditorApplication.SaveScene();
					}
				}
			}
		}
		if (err) EditorUtility.DisplayDialog("Error!", "An error occured while trying to set the main UniRPG scene Data. Your UniRPG install might be corrupt.", "Close");

		// ----------------------------------------------------------------------------
		// Update the MainMenuData
		err = true;
		if (EditorApplication.OpenScene(db.mainScenePaths[2]))
		{
			GameObject go = GameObject.Find("MainMenuData");
			if (go)
			{
				MainMenuData data = go.GetComponent<MainMenuData>();
				if (data)
				{
					data.startupPlayerPrefabs = new List<GameObject>();

					// only set the player lists if the designer wants player selection at start
 					// of if game is run now and a default chara is not set in the db (need something to playtest with)

					if (db.playerCanSelectCharacter || (setupForPlaytest && db.defaultPlayerCharacterPrefab == null))
					{
						// find all player character prefabs and check which are set as avail at startup
						if (Cache == null) RefreshCache(true);
						else if (Cache.actors == null) RefreshCache(true);
						if (Cache.actors.Count > 0)
						{
							foreach (Actor a in Cache.actors)
							{
								if (a.Character == null)
								{
									Debug.LogError("Encountered broken character. Found an Actor with no Character related component (like Chara2_Player or Chara2_NPC): " + a.gameObject.name);
									continue;
								}

								if (a.availAtStart && a.ActorType == UniRPGGlobal.ActorType.Player)
								{
									data.startupPlayerPrefabs.Add(a.gameObject);
								}
							}
						}
						else Debug.LogError("You need to define a Player Character and Refresh the cache in Database -> Main -> Actors. Play testing will fail.");

						//List<Player> charas = UniRPGEdUtil.FindPrefabsOfTypeAll<Player>("Searching", "Finding all defined Player Characters.");
						//foreach (Player c in charas)
						//{
						//	if (c.Actor.availAtStart) data.startupPlayerPrefabs.Add(c.gameObject);
						//}
					}

					err = false;
					EditorApplication.SaveScene();
				}
			}
		}
		if (err) EditorUtility.DisplayDialog("Error!", "An error occured while trying to set the Menu scene Data. Your UniRPG install might be corrupt.", "Close");

		// ----------------------------------------------------------------------------
		// check that the IDs for all objects in scenes are unique
		CheckGUIDs(db);

		// ----------------------------------------------------------------------------
		// done, reopen open previous scene
		EditorApplication.OpenScene(prevScene);
	}

	private static void UpdateAutoCallList(Database db)
	{
		db.menuAutoCalls = new List<string>(0);
		db.gameAutoCalls = new List<string>(0);
		if (menuAutoCalls.Count > 0) db.menuAutoCalls.AddRange(menuAutoCalls);
		if (gameAutoCalls.Count > 0) db.gameAutoCalls.AddRange(gameAutoCalls);
	}

	private static void CheckGUIDs(Database db)
	{
		float progress = 0f;
		float step = 1f;
		List<GUID> ids = new List<GUID>();
		foreach (string scenePath in db.gameScenePaths)
		{
			EditorApplication.OpenScene(scenePath);

			Object[] objs = GameObject.FindObjectsOfType(typeof(UniqueMonoBehaviour));
			if (objs.Length > 0)
			{
				progress = 0f; step = 1f / objs.Length;
				foreach (UniqueMonoBehaviour o in objs)
				{
					EditorUtility.DisplayProgressBar("Chacking IDs ...", "Checking GUIDs for uniqueness", progress);
					if (o.id.IsEmpty) { o.id = GUID.Create(); EditorUtility.SetDirty(o); }
					while (GUID.ListContains(ids, o.id)) { o.id = GUID.Create(); EditorUtility.SetDirty(o); }
					ids.Add(o.id);
					progress += step;
				}
			}

			EditorApplication.SaveScene();
		}
		EditorUtility.ClearProgressBar();
		ids.Clear();
		ids = null;
	}

	//private static void CheckTags()
	//{	// make sure all the tags that UniRPG uses are defined

	//	// first find the tags used by unirpg
	//	List<string> tagStrings = new List<string>();
	//	FieldInfo[] fields = typeof(UniRPGGlobal.Tag).GetFields();
	//	foreach (FieldInfo f in fields) tagStrings.Add(f.GetRawConstantValue().ToString());

	//	// open tag manager
	//	SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
	//	SerializedProperty tags = tagManager.FindProperty("tags");

	//	foreach (string s in tagStrings)
	//	{
	//		// check if the tag is defined
	//		bool found = false;
	//		for (int i = 0; i < tags.arraySize; i++)
	//		{
	//			SerializedProperty t = tags.GetArrayElementAtIndex(i);
	//			if (t.stringValue.Equals(s)) { found = true; break; }
	//		}

	//		// if not found, add it
	//		if (!found)
	//		{
	//			tags.InsertArrayElementAtIndex(0);
	//			SerializedProperty n = tags.GetArrayElementAtIndex(0);
	//			n.stringValue = s;
	//		}
	//	}

	//	// save my changes
	//	tagManager.ApplyModifiedProperties();
	//}

	#endregion
	// ================================================================================================================
} }