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

[DatabaseEditor("Main", Priority = 0)]
public class SystemEditor : DatabaseEdBase
{
	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };

	private static readonly string[] MenuItems = { "Project Scenes", "Loading & Saving", "Global Settings", "Startup Settings", "Input Settings", "Game Settings", null, "Global Variables", "Item Categories", "Equip Slots", null, "Actors", "RPG Items" };
	private int selected = 0;
	private int del = -1;
	private int ren = -1;
	private int renaming = -1;

	private GenericMenu addSceneMenu;

	private static bool[] inputSettingsFoldout = new bool[10];
	private static Color saveTint = new Color(1f, 1f, 0f, 1f);
	private bool inputSettingsChanged = false;
	private bool inputSettingsLoaded = false;

	// ================================================================================================================

	public SystemEditor()
	{
		addSceneMenu = new GenericMenu(); 
		addSceneMenu.AddItem(new GUIContent("Browse"), false, OnAddScene, 0);
		addSceneMenu.AddItem(new GUIContent("New Scene"), false, OnAddScene, 1);
		for (int i = 0; i < inputSettingsFoldout.Length; i++) inputSettingsFoldout[i] = true;
	}

	public override void OnGUI(DatabaseEditor ed)
	{
		base.OnGUI(ed);
		EditorGUILayout.BeginHorizontal();
		{
			// menu (on left side of window)
			selected = UniRPGEdGui.Menu(selected, MenuItems, GUILayout.Width(180));

			scroll[0] = EditorGUILayout.BeginScrollView(scroll[0]);
			{
				switch (selected)
				{
					case 0: ProjectScenesSettings(); break;
					case 1: LoadSaveSettings(); break;
					case 2: GlobalSettings(); break;
					case 3: GameStartSettings(); break;
					case 4: InputSettings(); break;
					case 5: GameSettings(); break;
					//case 6: is a spacer
					case 7: VariablesSettings(); break;
					case 8: ItemCategoriesSettings(); break;
					case 9: EquipSlotsSettings(); break;
					// case 10 is a spacer
					case 11: ActorCache(); break;
					case 12: ListRPGItems(); break;
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndScrollView();//0
		}
		EditorGUILayout.EndHorizontal();

		// check if data changed and should be saved
		if (GUI.changed)
		{
			EditorUtility.SetDirty(ed.db);
		}
	}

	// ================================================================================================================

	private void ProjectScenesSettings()
	{
		EditorGUILayout.BeginVertical(GUILayout.MaxWidth(600));
		{
			GUILayout.Space(15);
			GUILayout.Label("Project Scenes", UniRPGEdGui.Head2Style);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(" >> Update Build Settings and Globals <<", UniRPGEdGui.ButtonStyle, GUILayout.Width(300)))
				{
					UniRPGEditorGlobal.SetupBuildSettingsAndGlobals(ed.db, true, false);
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Main Scenes", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					if (UniRPGEdGui.IconButton("Select GUI Theme", UniRPGEdGui.Icon_Screen, EditorStyles.miniButton, GUILayout.Width(140)))
					{
						GUIThemeSelectWiz.Show(OnSelectedGUITheme, ed.db.activeGUITheme);
					}
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(15);

				EditorGUILayout.LabelField("Startup", ed.db.mainScenePaths[0]);
				EditorGUILayout.LabelField("UniRPG", ed.db.mainScenePaths[1]);
				EditorGUILayout.LabelField("MenuData", ed.db.mainScenePaths[2]);
				EditorGUILayout.LabelField("Menu GUI", ed.db.mainScenePaths[3]);
				EditorGUILayout.LabelField("Game GUI", ed.db.mainScenePaths[4]);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Game Maps/Scenes", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					if (UniRPGEdGui.IconButton("Add Scene", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(100)))
					{
						addSceneMenu.ShowAsContext();
					}
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(15);

				scroll[4] = UniRPGEdGui.BeginScrollView(scroll[4], GUILayout.Width(580), GUILayout.Height(200));
				{
					int moveUp = -1;
					int moveDown = -1;
					for (int i = 0; i < ed.db.gameScenePaths.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						{
							if (i == 0) GUI.enabled = false;
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Arrow2_Up, UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(30)))
							{
								moveUp = i;
							}
							if (i == ed.db.gameScenePaths.Count-1) GUI.enabled = false;
							else GUI.enabled = true;
							if (UniRPGEdGui.IconButton(null, UniRPGEdGui.Icon_Arrow2_Down, UniRPGEdGui.ButtonMidStyle, GUILayout.Width(30)))
							{
								moveDown = i;
							}
							GUI.enabled = true;
							if (GUILayout.Button(ed.db.gameScenePaths[i], UniRPGEdGui.ButtonMidStyle, GUILayout.Width(440)))
							{
								if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
								{
									EditorApplication.OpenScene(ed.db.gameScenePaths[i]);
								}
							}
							if (GUILayout.Button("Ren", UniRPGEdGui.ButtonMidStyle, GUILayout.Width(30)))
							{
								ren = i;
							}
							if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20)))
							{
								del = i;
							}
						}
						EditorGUILayout.EndHorizontal();
					}

					if (moveUp > 0)
					{
						string s = ed.db.gameScenePaths[moveUp];
						ed.db.gameScenePaths.RemoveAt(moveUp);
						ed.db.gameScenePaths.Insert(moveUp-1, s);
						s = ed.db.gameSceneNames[moveUp];
						ed.db.gameSceneNames.RemoveAt(moveUp);
						ed.db.gameSceneNames.Insert(moveUp - 1, s);
					}
					if (moveDown >= 0)
					{
						string s = ed.db.gameScenePaths[moveDown];
						ed.db.gameScenePaths.RemoveAt(moveDown);
						ed.db.gameScenePaths.Insert(moveDown + 1, s);
						s = ed.db.gameSceneNames[moveDown];
						ed.db.gameSceneNames.RemoveAt(moveDown);
						ed.db.gameSceneNames.Insert(moveDown + 1, s);
					}

				}
				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

		}
		EditorGUILayout.EndVertical();

		if (del >= 0)
		{
			if (EditorUtility.DisplayDialog("Also Delete Scene file?", "Should the scene file also be deleted from the project? This step can't be undone.", "Yes", "No"))
			{
				AssetDatabase.DeleteAsset(ed.db.gameScenePaths[del]);
			}
			ed.db.gameSceneNames.RemoveAt(del);
			ed.db.gameScenePaths.RemoveAt(del);
			EditorUtility.SetDirty(ed.db);
			del = -1;
			ed.Repaint();
		}

		if (ren >= 0)
		{
			renaming = ren; ren = -1;
			TextInputWiz.Show("Rename Scene", "Rename the UnIRPG Scene", ed.db.gameSceneNames[renaming], OnRenameScene);
		}
	}

	private void OnAddScene(System.Object userData)
	{
		int option = (int)userData;
		if (option == 0)
		{
			string fn = EditorUtility.OpenFilePanel("Select Scene", UniRPGEdUtil.FullProjectAssetsPath, "unity");
			if (!string.IsNullOrEmpty(fn))
			{
				fn = UniRPGEdUtil.ProjectRelativePath(fn);
				ed.db.gameScenePaths.Add(fn); // add the path
				fn = fn.Remove(0, fn.LastIndexOf("/") + 1);
				fn = fn.Remove(fn.ToLower().IndexOf(".unity"));
				ed.db.gameSceneNames.Add(fn); // add the name
			}
		}
		else
		{
			UniRPGEditorGlobal.Create_Scene();
		}
	}

	private void OnRenameScene(System.Object sender)
	{
		TextInputWiz wiz = sender as TextInputWiz;
		string name = wiz.text;
		wiz.Close();

		if (renaming == -1 || string.IsNullOrEmpty(name)) return;
		
		string res = AssetDatabase.RenameAsset(ed.db.gameScenePaths[renaming], name);
		if (string.IsNullOrEmpty(res))
		{
			string fn = ed.db.gameScenePaths[renaming].Substring(0, ed.db.gameScenePaths[renaming].LastIndexOf(ed.db.gameSceneNames[renaming] + ".unity"));
			fn += name + ".unity";
			ed.db.gameScenePaths[renaming] = fn;
			ed.db.gameSceneNames[renaming] = name;
		}
		else
		{
			EditorUtility.DisplayDialog("Error", res, "Close");
		}

		EditorUtility.SetDirty(ed.db);
		renaming = -1;
		ed.Repaint();
	}

	private void OnSelectedGUITheme(System.Object sender)
	{
		GUIThemeSelectWiz wiz = sender as GUIThemeSelectWiz;
		int idx = wiz.selIdx;
		wiz.Close();
		ed.db.activeGUITheme = UniRPGEditorGlobal.GUIEditors[idx].name;
		UniRPGEditorGlobal.InitGUIThemeData(ed.db, idx);
		EditorUtility.SetDirty(ed.db);
		ed.Repaint();
	}

	// ================================================================================================================

	private void LoadSaveSettings()
	{
		if (UniRPGEditorGlobal.activeLoadSaveIdx == -1) UniRPGEditorGlobal.InitLoadSaveProvider(ed.db, -1);

		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Loading & Saving", UniRPGEdGui.Head2Style);
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(350));
			{
				GUILayout.Label("Provider: ");

				EditorGUILayout.BeginVertical();
				{
					if (GUILayout.Button(UniRPGEditorGlobal.activeLoadSaveIdx >= 0 ? UniRPGEditorGlobal.LoadSaveEditors[UniRPGEditorGlobal.activeLoadSaveIdx].name : "none"))
					{
						LoadSaveSelectWiz.Show(OnSelectedLoadSaveProvider, UniRPGEditorGlobal.activeLoadSaveIdx);
					}
					EditorGUILayout.Space();
					if (GUILayout.Button("Clear All Save Data"))
					{
						if (ed.db.loadSaveProviderPrefab)
						{
							LoadSaveProviderBase loadSaveProvider = ed.db.loadSaveProviderPrefab.GetComponent<LoadSaveProviderBase>();
							if (loadSaveProvider != null)
							{
								loadSaveProvider.DeleteAll();
							}
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();


			if (UniRPGEditorGlobal.activeLoadSaveIdx >= 0)
			{
				UniRPGEditorGlobal.LoadSaveEditors[UniRPGEditorGlobal.activeLoadSaveIdx].editor.OnGUI(ed, UniRPGEditorGlobal.currLoadSaveProvider);
			}

		}
		EditorGUILayout.EndVertical();
	}

	private void OnSelectedLoadSaveProvider(System.Object sender)
	{
		LoadSaveSelectWiz wiz = sender as LoadSaveSelectWiz;
		if (wiz.selIdx >= 0 && wiz.selIdx != UniRPGEditorGlobal.activeLoadSaveIdx)
		{
			// create the prefab and set in db
			UniRPGEditorGlobal.InitLoadSaveProvider(ed.db, wiz.selIdx);
		}

		wiz.Close();
		ed.Repaint();
	}

	// ================================================================================================================

	private void GlobalSettings()
	{
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Global Settings", UniRPGEdGui.Head2Style);
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(350));
			{
				EditorGUIUtility.LookLikeControls(200, 50);
				EditorGUILayout.HelpBox("What the player can do when starting a new game", MessageType.Info);

				EditorGUI.BeginChangeCheck();
				UniRPGEditorGlobal.DB.playerCanChooseName = EditorGUILayout.Toggle("Can choose a name", UniRPGEditorGlobal.DB.playerCanChooseName);
				UniRPGEditorGlobal.DB.playerCanSelectCharacter = EditorGUILayout.Toggle("Can choose a character", UniRPGEditorGlobal.DB.playerCanSelectCharacter);
				UniRPGEditorGlobal.DB.playerCanSelectClass = EditorGUILayout.Toggle("Can choose a class", UniRPGEditorGlobal.DB.playerCanSelectClass);
				if (EditorGUI.EndChangeCheck())
				{
					EditorUtility.SetDirty(UniRPGEditorGlobal.DB.gameObject);
				}
				EditorGUIUtility.LookLikeControls();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(350));
			{
				EditorGUILayout.HelpBox("These should only be set if you do not plan on giving the player the option to choose his own character at the start of a new game.", MessageType.Info);
				EditorGUI.BeginChangeCheck();
				ed.db.defaultPlayerCharacterPrefab = (GameObject)EditorGUILayout.ObjectField("Player Character", ed.db.defaultPlayerCharacterPrefab, typeof(GameObject), false);
				if (EditorGUI.EndChangeCheck())
				{	// check if the designer placed valid character
					if (ed.db.defaultPlayerCharacterPrefab != null)
					{
						CharacterBase charaCheck = ed.db.defaultPlayerCharacterPrefab.GetComponent<CharacterBase>();
						if (charaCheck == null)
						{
							ed.db.defaultPlayerCharacterPrefab = null;
							ed.ShowNotification(new GUIContent("That is an invalid Player Character prefab."));
						}
						else
						{
							if (charaCheck.ActorType() != UniRPGGlobal.ActorType.Player)
							{
								ed.db.defaultPlayerCharacterPrefab = null;
								ed.ShowNotification(new GUIContent("That is an invalid Player Character prefab."));
							}								
						}
					}
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(350));
			{
				GUILayout.Label("Layers", UniRPGEdGui.Head4Style);
				EditorGUILayout.Space();
				ed.db.floorLayerMask = EditorGUILayout.LayerField("Floor", ed.db.floorLayerMask);
				ed.db.playerLayerMask = EditorGUILayout.LayerField("Player", ed.db.playerLayerMask);
				ed.db.npcLayerMask = EditorGUILayout.LayerField("Non-Player (NPC)", ed.db.npcLayerMask);
				ed.db.rpgItemLayerMask = EditorGUILayout.LayerField("RPG Item", ed.db.rpgItemLayerMask);
				ed.db.rpgObjectLayerMask = EditorGUILayout.LayerField("RPG Object", ed.db.rpgObjectLayerMask);
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
	}

	// ================================================================================================================

	private void GameStartSettings()
	{
		StartupSettings sgui = ed.db.startGUISettings.GetComponent<StartupSettings>();
		LoadGUISettings lgui = ed.db.loadGUISettings.GetComponent<LoadGUISettings>();

		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Startup Settings", UniRPGEdGui.Head2Style);
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(400));
			{
				UniRPGEdGui.IntVector2Field("Design Size", "width", "height", ref sgui.designW, ref sgui.designH, 200);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginHorizontal(UniRPGEdGui.BoxStyle, GUILayout.Width(650));
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(390));
				{
					EditorGUIUtility.LookLikeControls(200);
					GUILayout.Label("Loading Screens", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					sgui.guiSkin = EditorGUILayout.ObjectField("Skin for text (Label)", sgui.guiSkin, typeof(GUISkin), false) as GUISkin;
					sgui.loadText = EditorGUILayout.TextField("Text", sgui.loadText);
					sgui.loadBackground = DefaultGUIEditor.TextureWithTransformCheck("Background", sgui.loadBackground, sgui.trLoadBackground, GUILayout.Width(100), GUILayout.Height(100));
				}
				EditorGUILayout.EndVertical();
				UniRPGEdGui.DrawVerticalLine(1, UniRPGEdGui.DividerColor, 3, 3);
				EditorGUILayout.BeginVertical(GUILayout.Width(236));
				{
					EditorGUIUtility.LookLikeControls(50, 120);
					sgui.trLoadBackground = DefaultGUIEditor.ElementTransformField(sgui.trLoadBackground, "Image settings");
				}
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(UniRPGEdGui.BoxStyle, GUILayout.Width(650));
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(390));
				{
					EditorGUIUtility.LookLikeControls(200);
					GUILayout.Label("Splash Screens", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					sgui.allowSkip = EditorGUILayout.Toggle("Allow skipping splash sceens", sgui.allowSkip);
					sgui.delay = EditorGUILayout.FloatField("Show each splash for (seconds)", sgui.delay);
					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Images", UniRPGEdGui.Head4Style);
						EditorGUILayout.Space();
						if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton)) sgui.images.Add(null);
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(15);

					scroll[5] = EditorGUILayout.BeginScrollView(scroll[5], true, false, UniRPGEdGui.Skin.horizontalScrollbar, UniRPGEdGui.Skin.verticalScrollbar, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(125));
					//scroll[5] = UniRPGEdGui.BeginScrollView(scroll[5], UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(120));
					{
						EditorGUILayout.BeginHorizontal();
						{
							int del = -1;
							for (int i = 0; i < sgui.images.Count; i++)
							{
								if (i == 0) sgui.images[i] = DefaultGUIEditor.TextureWithTransformCheck(null, sgui.images[i], sgui.trImages, GUILayout.Width(90), GUILayout.Height(90));
								else sgui.images[i] = EditorGUILayout.ObjectField(sgui.images[i], typeof(Texture2D), false, GUILayout.Width(90), GUILayout.Height(90)) as Texture2D;
								if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(20))) del = i;
								EditorGUILayout.Space();
							}
							if (del >= 0) sgui.images.RemoveAt(del);
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
				UniRPGEdGui.DrawVerticalLine(1, UniRPGEdGui.DividerColor, 3, 3);
				EditorGUILayout.BeginVertical(GUILayout.Width(236));
				{
					EditorGUIUtility.LookLikeControls(50, 120);
					sgui.trImages = DefaultGUIEditor.ElementTransformField(sgui.trImages, "Image settings");
				}
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		if (GUI.changed)
		{
			lgui.designW = sgui.designW;
			lgui.designH = sgui.designH;
			lgui.guiSkin = sgui.guiSkin;
			lgui.loadText = sgui.loadText;
			lgui.loadBackground = sgui.loadBackground;
			lgui.trLoadBackground = sgui.trLoadBackground;

			EditorUtility.SetDirty(ed.db.startGUISettings);
			EditorUtility.SetDirty(ed.db.loadGUISettings);
		}
	}

	// ================================================================================================================

	private void GameSettings()
	{
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Game Settings", UniRPGEdGui.Head2Style);
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(350));
			{
				EditorGUIUtility.LookLikeControls(220);
				ed.db.currency = EditorGUILayout.TextField("Currency name", ed.db.currency);
				ed.db.currencyShort = EditorGUILayout.TextField("Currency short-name", ed.db.currencyShort);
				ed.db.currencyDropPrefab = (GameObject)EditorGUILayout.ObjectField("Currency LootDrop Prefab", ed.db.currencyDropPrefab, typeof(GameObject), false);
				EditorGUILayout.Space();
				ed.db.shopGlobalBuyMod = EditorGUILayout.FloatField("Shop buy at", ed.db.shopGlobalBuyMod);
				ed.db.shopGlobalSellMod = EditorGUILayout.FloatField("Shop sells at", ed.db.shopGlobalSellMod);
				ed.db.shopGlobalUsesCurrency = EditorGUILayout.Toggle("Shop checks own currency", ed.db.shopGlobalUsesCurrency);
				ed.db.shopGlobalUnlimited = EditorGUILayout.Toggle("Shop has unlimited stock", ed.db.shopGlobalUnlimited);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(350));
			{
				EditorGUIUtility.LookLikeControls();
				int idx = -1;
				for (int i = 0; i < UniRPGEditorGlobal.QuestListProviderTypes.Length; i++)
				{
					if (UniRPGEditorGlobal.QuestListProviderTypes[i].Equals(UniRPGEditorGlobal.DB.questListProviderType))
					{
						idx = i; break;
					}
				}
				idx = EditorGUILayout.Popup("Quest List Provider", idx, UniRPGEditorGlobal.QuestListProviderNames);
				if (idx >= 0)
				{
					UniRPGEditorGlobal.DB.questListProviderType = UniRPGEditorGlobal.QuestListProviderTypes[idx];
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
		EditorGUIUtility.LookLikeControls();
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(350));
		{
			ed.db.skillDoubleTapSystemOn = EditorGUILayout.Toggle("Double-tap system on", ed.db.skillDoubleTapSystemOn);
			ed.db.doubleTapTimeout = EditorGUILayout.FloatField("Double-tap timeout", ed.db.doubleTapTimeout);
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(350));
		{
			if (ed.db.selectionRingPrefabs.Length < 5) ed.db.selectionRingPrefabs = new GameObject[5];
			GUILayout.Label("Selection rings/ markers", EditorStyles.boldLabel);
			ed.db.selectionRingPrefabs[0] = (GameObject)EditorGUILayout.ObjectField("Friendly", ed.db.selectionRingPrefabs[0], typeof(GameObject), false);
			ed.db.selectionRingPrefabs[1] = (GameObject)EditorGUILayout.ObjectField("Neutral", ed.db.selectionRingPrefabs[1], typeof(GameObject), false);
			ed.db.selectionRingPrefabs[2] = (GameObject)EditorGUILayout.ObjectField("Hostile", ed.db.selectionRingPrefabs[2], typeof(GameObject), false);
			ed.db.selectionRingPrefabs[3] = (GameObject)EditorGUILayout.ObjectField("RPGItem", ed.db.selectionRingPrefabs[3], typeof(GameObject), false);
			ed.db.selectionRingPrefabs[4] = (GameObject)EditorGUILayout.ObjectField("RPGObject", ed.db.selectionRingPrefabs[4], typeof(GameObject), false);
		}
		EditorGUILayout.EndVertical();
	}

	// ================================================================================================================

	private void InputSettings()
	{
		GUIEditor.CheckSelectedTheme(ed.db); // so that the gui's input binds can load if not loaded yet
		if (inputSettingsFoldout.Length < UniRPGEditorGlobal.inputBinds.Count)
		{
			inputSettingsFoldout = new bool[UniRPGEditorGlobal.inputBinds.Count];
			for (int i = 0; i < inputSettingsFoldout.Length; i++) inputSettingsFoldout[i] = true;
		}

		if (!inputSettingsLoaded)
		{
			LoadInputChangesFromDB();
		}

		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Input Settings", UniRPGEdGui.Head2Style);
				EditorGUILayout.Space();
				if (ed.db.inputDefs.Count == 0 || inputSettingsChanged)
				{
					if (UniRPGEdGui.TintedButton(new GUIContent("Save Changes", UniRPGEdGui.Icon_Exclaim_Red), saveTint, GUILayout.Height(25), GUILayout.Width(125)))
					{
						SaveInputChangesToDB();
					}
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Bind", GUILayout.Width(110));
				GUILayout.Label("Trigger on: Single, Double, Held", GUILayout.Width(205));
				GUILayout.Label("Primary", GUILayout.Width(80));
				GUILayout.Label("Secondary", GUILayout.Width(85));
				GUILayout.Label("GUI", GUILayout.Width(45));
				GUILayout.Label("Event to Call");
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(690));
			{
				scroll[8] = UniRPGEdGui.BeginScrollView(scroll[8], GUILayout.Height(480));
				int i = 0;
				string removeInputDef = null;
				string removeInputDefFromGroup = null;
				foreach (KeyValuePair<string, Dictionary<string, InputDefinition>> group in UniRPGEditorGlobal.inputBinds)
				{
					EditorGUILayout.BeginHorizontal();
					{
						inputSettingsFoldout[i] = UniRPGEdGui.Foldout(inputSettingsFoldout[i], group.Key, UniRPGEdGui.HeadingFoldoutStyle);
						EditorGUILayout.Space();
						if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(25)))
						{
							TextInputWiz.Show("Add Definition", "Enter unique definition name", "", OnAddInputDef, new object[] { group.Key });
						}
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();

					if (inputSettingsFoldout[i])
					{
						foreach (KeyValuePair<string, InputDefinition> kv in group.Value)
						{
							EditorGUILayout.BeginHorizontal();
							{
								EditorGUI.BeginChangeCheck();
								kv.Value.isUsed = GUILayout.Toggle(kv.Value.isUsed, GUIContent.none);
								if (EditorGUI.EndChangeCheck()) inputSettingsChanged = true;
								GUI.enabled = kv.Value.isUsed;

								if (kv.Value.isCustom)
								{
									if (GUILayout.Button(kv.Key, GUILayout.Width(158)))
									{
										TextInputWiz.Show("Change Definition name", "The name must be unique", kv.Key, OnChangeInputDefName, new object[] { kv.Value });
									}
								}
								else
								{
									GUILayout.Label(kv.Key, GUILayout.Width(158));
								}

								EditorGUI.BeginChangeCheck();
								kv.Value.triggerOnSingle = GUILayout.Toggle(kv.Value.triggerOnSingle, GUIContent.none, GUILayout.Width(25));
								kv.Value.triggerOnDouble = GUILayout.Toggle(kv.Value.triggerOnDouble, GUIContent.none, GUILayout.Width(25));
								kv.Value.triggerOnHeld = GUILayout.Toggle(kv.Value.triggerOnHeld, GUIContent.none, GUILayout.Width(50));

								if (GUILayout.Button(kv.Value.primaryButton.ToString(), GUILayout.Width(90))) InputSelectWiz.Show(kv.Value, true);
								if (GUILayout.Button(kv.Value.secondaryButton.ToString(), GUILayout.Width(90))) InputSelectWiz.Show(kv.Value, false);
								GUILayout.Space(10);
								kv.Value.showInGUI = GUILayout.Toggle(kv.Value.showInGUI, GUIContent.none, GUILayout.Width(25));
								if (EditorGUI.EndChangeCheck()) inputSettingsChanged = true;

								if (kv.Value.isCustom)
								{
									if (GUILayout.Button(kv.Value.cachedEventName, EditorStyles.miniButtonLeft, GUILayout.Width(100)))
									{
										EventSelectWiz.Show(OnEventSelectedForInput, null, new object[] { kv.Value });
									}

									if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(25)))
									{
										removeInputDef = kv.Key;
										removeInputDefFromGroup = group.Key;
									}
								}
								else
								{
									GUI.enabled = false;
									GUILayout.Label("-", EditorStyles.miniButtonLeft, GUILayout.Width(100));
									GUILayout.Label("x", EditorStyles.miniButtonRight, GUILayout.Width(25));
								}

								GUI.enabled = true;
								GUILayout.FlexibleSpace();
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					EditorGUILayout.Space();
					i++;
				}

				if (removeInputDef != null && removeInputDefFromGroup != null)
				{
					UniRPGEditorGlobal.inputBinds[removeInputDefFromGroup].Remove(removeInputDef);
					inputSettingsChanged = true;
				}

				EditorGUILayout.Space();
				if (GUILayout.Button("Add Input Group", GUILayout.Width(200)))
				{
					TextInputWiz.Show("Add Input Group", "Enter unique name for new group", "", OnNewGroup);
				}

				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
	}

	private void SaveInputChangesToDB()
	{
		inputSettingsChanged = false;
		ed.db.inputDefs = new List<InputDefinition>();
		foreach (Dictionary<string, InputDefinition> group in UniRPGEditorGlobal.inputBinds.Values)
		{
			foreach (InputDefinition def in group.Values)
			{
				ed.db.inputDefs.Add(def.GetCopy());
			}
		}

		//bool found = false;
		//inputSettingsChanged = false;
		//foreach (Dictionary<string, InputDefinition> group in UniRPGEditorGlobal.inputBinds.Values)
		//{
		//	foreach (InputDefinition def in group.Values)
		//	{
		//		ed.db.inputDefs.Add(def.GetCopy());
		//		// find in DB else add
		//		found = false;
		//		foreach (InputDefinition dbDef in ed.db.inputDefs)
		//		{
		//			if (def.groupName.Equals(dbDef.groupName) && def.inputName.Equals(dbDef.inputName))
		//			{
		//				found = true;
		//				def.CopyTo(dbDef);
		//				break;
		//			}
		//		}

		//		if (!found)
		//		{
		//			ed.db.inputDefs.Add(def.GetCopy());
		//		}
		//	}
		//}
	}

	private void LoadInputChangesFromDB()
	{
		inputSettingsLoaded = true;
		for (int i = 0; i < ed.db.inputDefs.Count; i++)
		{
			if (ed.db.inputDefs[i].isCustom)
			{
				InputDefinition def = ed.db.inputDefs[i].GetCopy();
				if (!string.IsNullOrEmpty(def.eventGUID))
				{
					RPGEvent e = ed.db.GetEvent(new GUID(def.eventGUID));
					if (e != null) def.cachedEventName = e.screenName;
					else def.cachedEventName = "-";
				}
				else def.cachedEventName = "-";

				if (!UniRPGEditorGlobal.inputBinds.ContainsKey(ed.db.inputDefs[i].groupName))
				{
					Dictionary<string, InputDefinition> inputDef = new Dictionary<string, InputDefinition>();
					inputDef.Add(def.inputName, def);
					UniRPGEditorGlobal.inputBinds.Add(def.groupName, inputDef);
				}
				else
				{
					if (!UniRPGEditorGlobal.inputBinds[ed.db.inputDefs[i].groupName].ContainsKey(ed.db.inputDefs[i].inputName))
					{
						UniRPGEditorGlobal.inputBinds[ed.db.inputDefs[i].groupName].Add(def.inputName, def);
					}
				}
			}
			else
			{
				if (UniRPGEditorGlobal.inputBinds.ContainsKey(ed.db.inputDefs[i].groupName))
				{
					if (UniRPGEditorGlobal.inputBinds[ed.db.inputDefs[i].groupName].ContainsKey(ed.db.inputDefs[i].inputName))
					{
						InputDefinition def = UniRPGEditorGlobal.inputBinds[ed.db.inputDefs[i].groupName][ed.db.inputDefs[i].inputName];
						ed.db.inputDefs[i].CopyTo(def);
					}
				}
			}
		}
	}

	private void OnNewGroup(object sender)
	{
		TextInputWiz wiz = sender as TextInputWiz;
		if (!string.IsNullOrEmpty(wiz.text))
		{
			if (!UniRPGEditorGlobal.inputBinds.ContainsKey(wiz.text))
			{
				inputSettingsChanged = true;
				InputDefinition def = new InputDefinition();
				def.inputName = wiz.text + "_input";
				def.groupName = wiz.text;
				def.isCustom = true;
				Dictionary<string, InputDefinition> inputDef = new Dictionary<string, InputDefinition>();
				inputDef.Add(def.inputName, def);
				UniRPGEditorGlobal.inputBinds.Add(def.groupName, inputDef);
			}
		}
		wiz.Close();
		ed.Repaint();
	}

	private void OnChangeInputDefName(object sender, object[] args)
	{
		TextInputWiz wiz = sender as TextInputWiz;
		InputDefinition def = args[0] as InputDefinition;
		if (!string.IsNullOrEmpty(wiz.text) && def != null)
		{
			if (!def.inputName.Equals(wiz.text))
			{
				if (UniRPGEditorGlobal.inputBinds.ContainsKey(def.groupName))
				{
					if (!UniRPGEditorGlobal.inputBinds[def.groupName].ContainsKey(wiz.text))
					{
						inputSettingsChanged = true;
						
						UniRPGEditorGlobal.inputBinds[def.groupName].Remove(def.inputName);
						
						def.inputName = wiz.text;
						UniRPGEditorGlobal.inputBinds[def.groupName].Add(def.inputName, def);
					}
					else Debug.LogWarning("The new name was same as an existing input definition's name");
				}
			}
		}
		wiz.Close();
		ed.Repaint();
	}

	private void OnAddInputDef(object sender, object[] args)
	{
		TextInputWiz wiz = sender as TextInputWiz;
		string groupName = args[0] as string;
		if (!string.IsNullOrEmpty(wiz.text) && !string.IsNullOrEmpty(groupName))
		{
			if (UniRPGEditorGlobal.inputBinds.ContainsKey(groupName))
			{
				if (!UniRPGEditorGlobal.inputBinds[groupName].ContainsKey(wiz.text))
				{
					inputSettingsChanged = true;
					InputDefinition def = new InputDefinition();
					def.isCustom = true;
					def.inputName = wiz.text;
					def.groupName = groupName;
					UniRPGEditorGlobal.inputBinds[groupName].Add(def.inputName, def);
				}
				else Debug.LogWarning("The given name was same as an existing input definition's name");
			}
		}
		wiz.Close();
		ed.Repaint();
	}

	private void OnEventSelectedForInput(object sender, object[] args)
	{
		EventSelectWiz wiz = sender as EventSelectWiz;
		InputDefinition def = args[0] as InputDefinition;

		if (def != null)
		{
			inputSettingsChanged = true;
			def.eventGUID = wiz.selectedEvent.id.ToString();
			def.cachedEventName = wiz.selectedEvent.screenName;
		}

		wiz.Close();
		ed.Repaint();
	}

	// ================================================================================================================

	private void VariablesSettings()
	{
		NumericVar delNumVar = null;
		StringVar delStrVar = null;
		ObjectVar delObjVar = null;
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Global Variables", UniRPGEdGui.Head2Style);

			EditorGUILayout.BeginHorizontal();
			{
				// *** num vars

				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
				{
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Numeric Vars", UniRPGEdGui.Head3Style);
						EditorGUILayout.Space();
						if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
						{
							ed.db.numericVars.Add(new NumericVar(){ name = "num" + (ed.db.numericVars.Count+1).ToString(), val = 0 });
							EditorUtility.SetDirty(ed.db);
						}
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(20);

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Name", GUILayout.Width(100));
						GUILayout.Space(5);
						GUILayout.Label("Value", GUILayout.Width(100));
					}
					EditorGUILayout.EndHorizontal();
					scroll[6] = UniRPGEdGui.BeginScrollView(scroll[6], GUILayout.Width(280), GUILayout.Height(180));
					{
						foreach(NumericVar v in ed.db.numericVars)
						{
							EditorGUILayout.BeginHorizontal(GUILayout.Width(160), GUILayout.ExpandWidth(false));
							{
								EditorGUI.BeginChangeCheck();
								v.name = EditorGUILayout.TextField(v.name, GUILayout.Width(100));
								if (EditorGUI.EndChangeCheck())
								{	// make sure the name don't contain an invalid character like "|" which is used by save system
									if (v.name.Contains("|")) v.name = v.name.Replace("|", "");
								}

								GUILayout.Space(5);
								v.val = EditorGUILayout.FloatField(v.val, GUILayout.Width(100));
								GUILayout.Space(5);
								if (GUILayout.Button("X", GUILayout.Width(20))) delNumVar = v;
								GUILayout.Space(18);
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					UniRPGEdGui.EndScrollView();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();

				// *** string vars

				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
				{
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("String Vars", UniRPGEdGui.Head3Style);
						EditorGUILayout.Space();
						if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
						{
							ed.db.stringVars.Add(new StringVar() { name = "str" + (ed.db.stringVars.Count + 1).ToString(), val = "" });
							EditorUtility.SetDirty(ed.db);
						}
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(20);

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Name", GUILayout.Width(100));
						GUILayout.Space(5);
						GUILayout.Label("Value", GUILayout.Width(100));
					}
					EditorGUILayout.EndHorizontal();
					scroll[7] = UniRPGEdGui.BeginScrollView(scroll[7], GUILayout.Width(280), GUILayout.Height(180));
					{
						foreach (StringVar v in ed.db.stringVars)
						{
							EditorGUILayout.BeginHorizontal(GUILayout.Width(160), GUILayout.ExpandWidth(false));
							{
								EditorGUI.BeginChangeCheck();
								v.name = EditorGUILayout.TextField(v.name, GUILayout.Width(100));
								if (EditorGUI.EndChangeCheck())
								{	// make sure te name dont contain an invalid character like "|" which is used by save system
									if (v.name.Contains("|")) v.name = v.name.Replace("|", "");
								}
								GUILayout.Space(5);
								v.val = EditorGUILayout.TextField(v.val, GUILayout.Width(100));
								GUILayout.Space(5);
								if (GUILayout.Button("X", GUILayout.Width(20))) delStrVar = v;
								GUILayout.Space(18);
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					UniRPGEdGui.EndScrollView();
				}
				EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			// *** object vars

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Object Vars", UniRPGEdGui.Head3Style);
					EditorGUILayout.Space();
					if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
					{
						ed.db.objectVars.Add(new ObjectVar() { name = "obj" + (ed.db.objectVars.Count + 1).ToString(), val = null });
						EditorUtility.SetDirty(ed.db);
					}
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Name", GUILayout.Width(100));
					GUILayout.Space(5);
					GUILayout.Label("Value", GUILayout.Width(100));
				}
				EditorGUILayout.EndHorizontal();
				scroll[10] = UniRPGEdGui.BeginScrollView(scroll[10], GUILayout.Width(280), GUILayout.Height(180));
				{
					foreach (ObjectVar v in ed.db.objectVars)
					{
						EditorGUILayout.BeginHorizontal(GUILayout.Width(160), GUILayout.ExpandWidth(false));
						{
							EditorGUI.BeginChangeCheck();
							v.name = EditorGUILayout.TextField(v.name, GUILayout.Width(100));
							if (EditorGUI.EndChangeCheck())
							{	// make sure te name dont contain an invalid character like "|" which is used by save system
								if (v.name.Contains("|")) v.name = v.name.Replace("|", "");
							}
							GUILayout.Space(5);
							v.val = EditorGUILayout.ObjectField(v.val, typeof(Object), false, GUILayout.Width(100));
							GUILayout.Space(5);
							if (GUILayout.Button("X", GUILayout.Width(20))) delObjVar = v;
							GUILayout.Space(18);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();

		if (delNumVar != null)
		{
			ed.db.numericVars.Remove(delNumVar);
			delNumVar = null;
			EditorUtility.SetDirty(ed.db);
		}

		if (delStrVar != null)
		{
			ed.db.stringVars.Remove(delStrVar);
			delStrVar = null;
			EditorUtility.SetDirty(ed.db);
		}

		if (delObjVar != null)
		{
			ed.db.objectVars.Remove(delObjVar);
			delObjVar = null;
			EditorUtility.SetDirty(ed.db);
		}
	}

	// ================================================================================================================

	private RPGItem.Category currItemCat = null;
	private RPGItem.Category delItemCat = null;
	private void ItemCategoriesSettings()
	{
		if (currItemCat == null && ed.db.itemCategories.Count > 0) currItemCat = ed.db.itemCategories[0];

		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Item Categories", UniRPGEdGui.Head2Style);

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(420));
			{
				if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
				{
					currItemCat = new RPGItem.Category();
					ed.db.itemCategories.Add(currItemCat);
					GUI.FocusControl("");
					EditorUtility.SetDirty(ed.db);
				}
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					// show list of categries
					scroll[1] = UniRPGEdGui.BeginScrollView(scroll[1], GUILayout.Width(180), GUILayout.Height(200));
					foreach (RPGItem.Category ic in ed.db.itemCategories)
					{
						EditorGUILayout.BeginHorizontal(GUILayout.Width(160), GUILayout.ExpandWidth(false));
						{
							if (UniRPGEdGui.ToggleButton(currItemCat == ic, ic.screenName, UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(138), GUILayout.ExpandWidth(false)))
							{
								currItemCat = ic;
								GUI.FocusControl("");
							}
							if (ed.db.itemCategories.Count == 1) GUI.enabled = false; // cant delete if only one left
							if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) delItemCat = ic;
							GUI.enabled = true;
							GUILayout.Space(18);
						}
						EditorGUILayout.EndHorizontal();
					}
					UniRPGEdGui.EndScrollView();

					// show name edit field for and sub types of selected category
					EditorGUILayout.Space();

					if (currItemCat != null)
					{
						EditorGUI.BeginChangeCheck();
						scroll[2] = UniRPGEdGui.TextListField(currItemCat.types, ref currItemCat.screenName, scroll[2], 200, 145, true);
						if (EditorGUI.EndChangeCheck()) UniRPGEditorGlobal.DB.ForceUpdateCache(1);
					}

				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();

		if (delItemCat != null)
		{
			if (currItemCat == delItemCat) currItemCat = null;
			ed.db.itemCategories.Remove(delItemCat);
			delItemCat = null;
			EditorUtility.SetDirty(ed.db);
		}
	}

	// ================================================================================================================

	private void EquipSlotsSettings()
	{
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			GUILayout.Label("Equip Slots", UniRPGEdGui.Head2Style);

			scroll[3] = UniRPGEdGui.TextListField(ed.db.equipSlots, null, scroll[3], 200, 200);
		}
		EditorGUILayout.EndVertical();
	}

	// ================================================================================================================

	private void ActorCache()
	{
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Actors", UniRPGEdGui.Head2Style);
				GUILayout.Space(20);
				if (UniRPGEdGui.IconButton("Refresh", UniRPGEdGui.Icon_Refresh, GUILayout.Width(85)))
				{
					UniRPGEditorGlobal.Cache.RefreshActorCache(true);
					EditorGUIUtility.ExitGUI();
					return;
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(650));
			{
				scroll[9] = UniRPGEdGui.BeginScrollView(scroll[9], GUILayout.Height(450));
				{
					foreach (Actor a in UniRPGEditorGlobal.Cache.actors)
					{
						if (a == null)
						{
							GUILayout.Label("Error! This Actor was deleted. Please Refresh.");
							continue; // it might have been deleted from the project
						}

						GUILayout.Label(string.Format("{0} - {1}", (!string.IsNullOrEmpty(a.screenName) ? a.screenName : a.name), a.ActorType));
					}
				}
				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
	}

	// ================================================================================================================

	private void ListRPGItems()
	{
		EditorGUILayout.BeginVertical();
		{
			GUILayout.Space(15);
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("RPG Items", UniRPGEdGui.Head2Style);
				GUILayout.Space(20);
				if (UniRPGEdGui.IconButton("Refresh", UniRPGEdGui.Icon_Refresh, GUILayout.Width(85)))
				{
					FindAllRPGItems();
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(650));
			{
				scroll[9] = UniRPGEdGui.BeginScrollView(scroll[9], GUILayout.Height(450));
				{
					foreach (GameObject go in ed.db.rpgItemPrefabs)
					{
						if (go == null)
						{
							GUILayout.Label("Error! This Item was deleted. Please Refresh.");
							continue; // it might have been deleted from the project
						}

						RPGItem item = go.GetComponent<RPGItem>();
						if (item)
						{
							GUILayout.Label((!string.IsNullOrEmpty(item.screenName) ? item.screenName : go.name));
						}
						else
						{
							GUILayout.Label("Invalid RPG Item [" + go.name + "] found. Please update the Build & GLobal Settings via Main -> Project Scenes.");
						}
					}
				}
				UniRPGEdGui.EndScrollView();
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
	}

	public static void FindAllRPGItems()
	{
		// Update Database with all available RPGItem Prefabs
		List<System.Guid> ids = new List<System.Guid>();

		// remove all NULL items from list (those that might have been deleted from project)
		UniRPGEditorGlobal.DB.rpgItemPrefabs = UniRPGUtil.CleanupList<GameObject>(UniRPGEditorGlobal.DB.rpgItemPrefabs);

		List<RPGItem> items = UniRPGEdUtil.FindPrefabsOfTypeAll<RPGItem>("Searching", "Finding all RPG Items.");
		foreach (RPGItem item in items)
		{
			// only items that are enabled are added
			if (item.gameObject.activeSelf)
			{
				if (!UniRPGEditorGlobal.DB.rpgItemPrefabs.Contains(item.gameObject))
				{
					UniRPGEditorGlobal.DB.rpgItemPrefabs.Add(item.gameObject);
				}

				if (item.prefabId.IsEmpty)
				{
					item.prefabId = GUID.Create();
					EditorUtility.SetDirty(item);
				}

				// and make sure they got unique GUIDs
				while (ids.Contains(item.prefabId.Value))
				{
					item.prefabId = GUID.Create();
					EditorUtility.SetDirty(item);
				}
				ids.Add(item.prefabId.Value);
			}
			else
			{
				// remove the item if allready in the List - because its prefab is disabled
				if (UniRPGEditorGlobal.DB.rpgItemPrefabs.Contains(item.gameObject))
				{
					UniRPGEditorGlobal.DB.rpgItemPrefabs.Remove(item.gameObject);
				}
			}
		}
		EditorUtility.SetDirty(UniRPGEditorGlobal.DB.gameObject);
		AssetDatabase.SaveAssets();
	}

	// ================================================================================================================
} }