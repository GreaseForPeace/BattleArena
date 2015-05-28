// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

[UniRPGGUITheme("Default Fantasy", "Assets/UniRPG/Default Assets/GUI/Fantasy/menugui.unity", "Assets/UniRPG/Default Assets/GUI/Fantasy/gamegui.unity", typeof(DefaultMainMenuGUIData), typeof(DefaultGameGUIData) )]
public class DefaultGUIEditor : GUIEditorBase
{
	// ================================================================================================================
	#region GUI editor

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
	private static readonly string[] MenuItems = { "Basic Settings", "-Menu Screens", "Main Menu", "New Game", "Options", "-Game GUI", "Menu Bar", "Action Bar", "Status Panels", "Misc" };
	private int selected = 0;

	public override void InitDefaults(GameObject menuGUIDataPrefab, GameObject gameGUIDataPrefab)
	{
		DefaultMainMenuGUIData menuData = menuGUIDataPrefab.GetComponent<DefaultMainMenuGUIData>();
		DefaultGameGUIData gameData = gameGUIDataPrefab.GetComponent<DefaultGameGUIData>();

		gameData.skin = menuData.skin; // set the ingame gui to use same skin as as menu

		// *** Menu GUI Data ***

		menuData.labelNewGame = "New Game";
		menuData.labelContinue = "Continue";
		menuData.labelOptions = "Options";
		menuData.labelExit = "Quit";

		menuData.menuMusic = new List<AudioClip>(0);
		menuData.sfxButton = null;

		menuData.skin = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Fantasy/Fantasy GUISkin.guiskin", typeof(GUISkin)) as GUISkin;
		menuData.trMenu.xAlign = GUIElementTransform.XAlign.Center;
		menuData.trMenu.yAlign = GUIElementTransform.YAlign.Bottom;
		menuData.trMenu.offset = new Vector2(0f, 100f);
		menuData.trMenu.size = new Vector2(250f, 163f);

		menuData.texMenuBack = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/menuback.png", typeof(Texture2D)) as Texture2D;

		menuData.showAudioMainVolume = true;
		menuData.showMusicVolume = true;
		menuData.showGUIAudioVolume = true;
		menuData.showFXAudioVolume = true;
		menuData.showEnviroAudioVolume = true;

		menuData.trOptions.xAlign = GUIElementTransform.XAlign.Center;
		menuData.trOptions.yAlign = GUIElementTransform.YAlign.Bottom;
		menuData.trOptions.offset = new Vector2(0f, 100f);
		menuData.trOptions.size = new Vector2(0f, 0f);

		menuData.texNewGameBack = null;
		menuData.newGameCharaBackFab = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/NewGameBackground.prefab", typeof(GameObject)) as GameObject;

		menuData.texLogo = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/logo.png", typeof(Texture2D)) as Texture2D;
		menuData.trLogo.xAlign = GUIElementTransform.XAlign.Center;
		menuData.trLogo.offset = new Vector2(0f, 30f);
		menuData.trLogo.size = new Vector2(menuData.texLogo.width, menuData.texLogo.height);

		menuData.trNewCharaArea.xAlign = GUIElementTransform.XAlign.Left;
		menuData.trNewCharaArea.size = new Vector2(220f, 500f);
		menuData.trNewCharaInfoArea.xAlign = GUIElementTransform.XAlign.Left;
		menuData.trNewCharaInfoArea.yAlign = GUIElementTransform.YAlign.Top;
		menuData.trNewCharaInfoArea.size = new Vector2(280f, 200f);
		menuData.trNewCharaInfoArea.offset = new Vector2(0f, 505f);

		menuData.trNewClassArea.xAlign = GUIElementTransform.XAlign.Right;
		menuData.trNewClassArea.size = new Vector2(220f, 500f);
		menuData.trNewClassInfoArea.xAlign = GUIElementTransform.XAlign.Right;
		menuData.trNewClassInfoArea.yAlign = GUIElementTransform.YAlign.Top;
		menuData.trNewClassInfoArea.size = new Vector2(280f, 200f);
		menuData.trNewClassInfoArea.offset = new Vector2(0f, 505f);

		menuData.trNewButtonsArea.xAlign = GUIElementTransform.XAlign.Center;
		menuData.trNewButtonsArea.yAlign = GUIElementTransform.YAlign.Bottom;
		menuData.trNewButtonsArea.size = new Vector2(450f, 100f);
		menuData.trNewButtonsArea.offset.y = 30;

		// *** InGame GUI Data ***

		gameData.plrMoveCharSheet = false;
		gameData.plrMoveSkills = false;
		gameData.plrMoveBag = false;
		gameData.plrMoveDialogue = false;
		gameData.plrMoveJournal = false;
		gameData.plrMoveShop = false;

		gameData.bagPanelName = "Bag";
		gameData.bagIconWidth = 50f;
		gameData.charaPanelName = "Character";
		gameData.charaPanelShowLevel = true;
		gameData.equipIconWidth = 50f;
		gameData.skillPanelName = "Skills";
		gameData.skillIconWidth = 50f;
		gameData.logPanelName = "Journal";

		gameData.txQuestComplete[0] = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/star1.png", typeof(Texture2D)) as Texture2D;
		gameData.txQuestComplete[1] = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/star2.png", typeof(Texture2D)) as Texture2D;

		gameData.menuOptions = new List<DefaultGameGUIData_MenuOption>(4);
		gameData.menuOptions.Add(new DefaultGameGUIData_MenuOption { showWhat = DefaultGameGUIData_MenuOption.MenuOption.Character, name = "Character", icon = (AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/icon_character.png", typeof(Texture2D)) as Texture2D) } );
		gameData.menuOptions.Add(new DefaultGameGUIData_MenuOption { showWhat = DefaultGameGUIData_MenuOption.MenuOption.Bag, name = "Bag", icon = (AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/icon_bag.png", typeof(Texture2D)) as Texture2D) });
		gameData.menuOptions.Add(new DefaultGameGUIData_MenuOption { showWhat = DefaultGameGUIData_MenuOption.MenuOption.Skills, name = "Skills", icon = (AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/icon_skills.png", typeof(Texture2D)) as Texture2D) });
		gameData.menuOptions.Add(new DefaultGameGUIData_MenuOption { showWhat = DefaultGameGUIData_MenuOption.MenuOption.Journal, name = "Journal", icon = (AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/icon_log.png", typeof(Texture2D)) as Texture2D) });
		gameData.menuOptions.Add(new DefaultGameGUIData_MenuOption { showWhat = DefaultGameGUIData_MenuOption.MenuOption.Options, name = "Options", icon = (AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/icon_options.png", typeof(Texture2D)) as Texture2D) });
		
		gameData.menuIconWidth = 40;
		gameData.trMenuBar.xAlign = GUIElementTransform.XAlign.Center;
		gameData.trMenuBar.yAlign = GUIElementTransform.YAlign.Top;
		gameData.trMenuBar.offset.y = 2;

		gameData.txCooldown = new List<Texture2D>(8);
		for (int i = 1; i <= 8; i++) gameData.txCooldown.Add(AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/cooldown" + i + ".png", typeof(Texture2D)) as Texture2D);
		gameData.txQueued = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/queued.png", typeof(Texture2D)) as Texture2D;
		gameData.txInvalid = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/invalid.png", typeof(Texture2D)) as Texture2D;
		gameData.txNoIcon = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/icon_none.png", typeof(Texture2D)) as Texture2D;
		
		gameData.txEmptySlot = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Fantasy/icon_empty.png", typeof(Texture2D)) as Texture2D;

		gameData.actionSlotsCount = 6;
		gameData.actionIconWidth = 50;
		gameData.showWhenCantUseSkill = true;
		gameData.showSkillCooldown = true;
		gameData.showSkillQueued = true;
		gameData.trActionBar.xAlign = GUIElementTransform.XAlign.Center;
		gameData.trActionBar.yAlign = GUIElementTransform.YAlign.Bottom;
		gameData.trActionBar.offset.y = 5;

		gameData.statusPortraitWidth = 50;
		gameData.statusBarWidth = 200;
		gameData.statusBarHeight = 15;
		gameData.statusBarBack = AssetDatabase.LoadAssetAtPath(UniRPGEditorGlobal.PackagePath + "Default Assets/GUI/Art/Common/statusbarback.png", typeof(Texture2D)) as Texture2D;
		gameData.showPlayerStatus = true;
		gameData.playerStatusShowValue = 1; // 0:none, 1:number, 2:percentage
		gameData.showPlayerPortrait = true;
		gameData.showPlayerLevel = false;
		gameData.playerStatusBars = new List<DefaultGameGUIData_StatusBar>(0);
		gameData.targetStatusShowValue = 1; // 0:none, 1:number, 2:percentage
		gameData.targetStatusBars = new List<DefaultGameGUIData_StatusBar>(0);

		//gameData.showTargetStatus = true;
		//gameData.showTargetPortrait = true;
		//gameData.showTargetLevel = false;
		//gameData.showTargetName = true;
		//gameData.showStatusForFriendly = true;
		//gameData.showStatusForNeutral = true;
		//gameData.showStatusForHostile = true;
		//gameData.showStatusForItem = true;
		//gameData.showStatusForObject = true;

		gameData.targetStatus = new DefaultGameGUIData.TargetStatusShow[]
		{
			new DefaultGameGUIData.TargetStatusShow() { nm = "Hostile",		show = true, name = true, img = true, level = false, bars = true },
			new DefaultGameGUIData.TargetStatusShow() { nm = "Neutral",		show = true, name = true, img = true, level = false, bars = true },
			new DefaultGameGUIData.TargetStatusShow() { nm = "Friendly",	show = true, name = true, img = true, level = false, bars = false },
			new DefaultGameGUIData.TargetStatusShow() { nm = "Item",		show = true, name = true, img = true, level = false, bars = false },
			new DefaultGameGUIData.TargetStatusShow() { nm = "Object",		show = true, name = true, img = true, level = false, bars = false },
		};

		gameData.trRightPanel.xAlign = GUIElementTransform.XAlign.Right;
		gameData.trRightPanel.yAlign = GUIElementTransform.YAlign.Middle;
		gameData.trRightPanel.size = new Vector2(300, 600);

		gameData.trLeftPanel.xAlign = GUIElementTransform.XAlign.Left;
		gameData.trLeftPanel.yAlign = GUIElementTransform.YAlign.Middle;
		gameData.trLeftPanel.size = new Vector2(300, 600);

		gameData.trLeftWidePanel.xAlign = GUIElementTransform.XAlign.Left;
		gameData.trLeftWidePanel.yAlign = GUIElementTransform.YAlign.Middle;
		gameData.trLeftWidePanel.size = new Vector2(450, 600);	

		// save
		EditorUtility.SetDirty(menuGUIDataPrefab);
		EditorUtility.SetDirty(gameGUIDataPrefab);
	}

	public override InputBinderBase GetInputBinder(GameObject menuGUIDataPrefab, GameObject gameGUIDataPrefab)
	{
		return new DefaultGUIInputBinder();
	}

	public override void Update(GameObject menuGUIDataPrefab, GameObject gameGUIDataPrefab) { }

	public override void OnGUI(DatabaseEditor ed, GameObject menuGUIDataPrefab, GameObject gameGUIDataPrefab)
	{
		DefaultMainMenuGUIData menuData = menuGUIDataPrefab.GetComponent<DefaultMainMenuGUIData>();
		DefaultGameGUIData gameData = gameGUIDataPrefab.GetComponent<DefaultGameGUIData>();

		EditorGUILayout.BeginHorizontal();
		{
			selected = UniRPGEdGui.Menu(selected, MenuItems, GUILayout.Width(180));

			scroll[0] = EditorGUILayout.BeginScrollView(scroll[0]);
			{
				switch (selected)
				{
					case 0: DrawBasicSettings(menuData, gameData); break;
					case 1: break; // is a seperator/heading
					case 2: DrawMenuSettings(menuData); break;
					case 3: DrawNewGameSettings(menuData); break;
					case 4: DrawOptions(menuData); break;
					case 5: break; // is a seperator/heading
					case 6: DrawMenuBarOptions(gameData); break;
					case 7: DrawActionBarSettings(gameData); break;
					case 8: DrawStatusPanels(gameData); break;
					case 9: DrawMisc(gameData); break;
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndScrollView();
		}
		EditorGUILayout.EndHorizontal();

		// check if data changed and should be saved
		if (GUI.changed)
		{
			EditorUtility.SetDirty(menuGUIDataPrefab);
			EditorUtility.SetDirty(gameGUIDataPrefab);
		}
	}

	private void DrawBasicSettings(DefaultMainMenuGUIData menuData, DefaultGameGUIData ingameData)
	{
		EditorGUIUtility.LookLikeControls(50, 120);

		GUILayout.Space(15);
		GUILayout.Label("Basic Settings", UniRPGEdGui.Head2Style);

		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(236));
		{
			EditorGUILayout.LabelField("Theme", "Default Fantasy");
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Reset Theme", GUILayout.Width(120))) InitDefaults(menuData.gameObject, ingameData.gameObject);
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			menuData.skin = (GUISkin)EditorGUILayout.ObjectField("Skin", menuData.skin, typeof(GUISkin), false);
			EditorGUILayout.Space();
			UniRPGEdGui.IntVector2Field("Design Size", "width", "height", ref menuData.width, ref menuData.height, 200);

			ingameData.skin = menuData.skin;
			ingameData.width = menuData.width;
			ingameData.height = menuData.height;
		}
		EditorGUILayout.EndVertical();
	}

	#endregion
	// ================================================================================================================
	#region Main Menu

	private void DrawMenuSettings(DefaultMainMenuGUIData menuData)
	{
		GUILayout.Space(15);
		GUILayout.Label("Main Menu", UniRPGEdGui.Head2Style);

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{
					EditorGUIUtility.LookLikeControls(100);
					menuData.labelContinue = EditorGUILayout.TextField("Continue", menuData.labelContinue);
					menuData.labelNewGame = EditorGUILayout.TextField("New Game", menuData.labelNewGame);
					menuData.labelOptions = EditorGUILayout.TextField("Options", menuData.labelOptions);
					menuData.labelExit = EditorGUILayout.TextField("Exit", menuData.labelExit);
					EditorGUILayout.Space();
					menuData.labelSelectChara = EditorGUILayout.TextField("Select Chara", menuData.labelSelectChara);
					menuData.labelSelectClass = EditorGUILayout.TextField("Select Class", menuData.labelSelectClass);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{	// Menu offset and such
					EditorGUIUtility.LookLikeControls(50, 120);
					GUILayout.Label("Menu Area", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					menuData.trMenu = ElementTransformField(menuData.trMenu, null, false, true, true, true, true);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{	// Background Image
					GUILayout.Label("Background", UniRPGEdGui.Head4Style);
					menuData.texMenuBack = EditorGUILayout.ObjectField(menuData.texMenuBack, typeof(Texture2D), false, GUILayout.Width(180), GUILayout.Height(120)) as Texture2D;
					EditorGUILayout.HelpBox("Stretched to fill screen.", MessageType.Info);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{	// Logo Image
					GUILayout.Label("Logo", UniRPGEdGui.Head4Style);
					menuData.texLogo = TextureWithTransformCheck(null, menuData.texLogo, menuData.trLogo, GUILayout.Width(180), GUILayout.Height(120));
					menuData.trLogo = ElementTransformField(menuData.trLogo);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{
					EditorGUIUtility.LookLikeControls(100);
					GUILayout.Label("Menu Sounds", UniRPGEdGui.Head4Style);
					menuData.sfxButton = (AudioClip)EditorGUILayout.ObjectField("Button sound", menuData.sfxButton, typeof(AudioClip), false);

					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label("Menu Music", UniRPGEdGui.Head4Style);
						EditorGUILayout.Space();
						if (UniRPGEdGui.IconButton(" Add Clip", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton)) menuData.menuMusic.Add(null);
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
					int del = -1;
					for (int i = 0; i < menuData.menuMusic.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						{
							menuData.menuMusic[i] = (AudioClip)EditorGUILayout.ObjectField(menuData.menuMusic[i], typeof(AudioClip), false);
							if (GUILayout.Button("x", EditorStyles.miniButton)) del = i;
						}
						EditorGUILayout.EndHorizontal();
						if (del >= 0) menuData.menuMusic.RemoveAt(del);
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void DrawNewGameSettings(DefaultMainMenuGUIData menuData)
	{
		EditorGUIUtility.LookLikeControls(50, 120);
		
		GUILayout.Space(15);
		GUILayout.Label("New Game Screen", UniRPGEdGui.Head2Style);

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(236));
			{
				menuData.trNewButtonsArea = ElementTransformField(menuData.trNewButtonsArea, "Button & Name Area", false, true, true, true, true);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(236));
			{
				menuData.trNewCharaArea = ElementTransformField(menuData.trNewCharaArea, "Character List Area", false, true, true, true, true);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(236));
			{
				menuData.trNewClassArea = ElementTransformField(menuData.trNewClassArea, "Classes List Area", false, true, true, true, true);
			}
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{	// Background Image
					GUILayout.Label("Character Background", UniRPGEdGui.Head4Style);
					menuData.newGameCharaBackFab = EditorGUILayout.ObjectField(menuData.newGameCharaBackFab, typeof(GameObject), false) as GameObject;
					EditorGUILayout.HelpBox("An object that will be used as the background/floor for the character to stand on.", MessageType.Info);
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{	// Background Image
					GUILayout.Label("Background", UniRPGEdGui.Head4Style);
					menuData.texNewGameBack = EditorGUILayout.ObjectField(menuData.texNewGameBack, typeof(Texture2D), false, GUILayout.Width(180), GUILayout.Height(120)) as Texture2D;
					EditorGUILayout.HelpBox("Stretched to fill screen. Only used when Character Background is not set.", MessageType.Info);
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(236));
			{
				menuData.trNewCharaInfoArea = ElementTransformField(menuData.trNewCharaInfoArea, "Character Info Area", false, true, true, true, true);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.MaxWidth(236));
			{
				menuData.trNewClassInfoArea = ElementTransformField(menuData.trNewClassInfoArea, "Classes Info Area", false, true, true, true, true);
			}
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void DrawOptions(DefaultMainMenuGUIData menuData)
	{
		GUILayout.Space(15);
		GUILayout.Label("Options Screen", UniRPGEdGui.Head2Style);

		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
		{	// Menu offset and such
			EditorGUIUtility.LookLikeControls(50, 120);
			GUILayout.Label("Options Area", UniRPGEdGui.Head4Style);
			EditorGUILayout.Space();
			menuData.trOptions = ElementTransformField(menuData.trOptions, null, false, true, true, true, false);
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
		{
			EditorGUIUtility.LookLikeControls();
			GUILayout.Label("Sound options to show", UniRPGEdGui.Head4Style);
			EditorGUILayout.Space();
			menuData.showAudioMainVolume = EditorGUILayout.Toggle("Main volume", menuData.showAudioMainVolume);
			menuData.showMusicVolume = EditorGUILayout.Toggle("Music volume", menuData.showMusicVolume);
			menuData.showGUIAudioVolume = EditorGUILayout.Toggle("GUI volume", menuData.showGUIAudioVolume);
			menuData.showFXAudioVolume = EditorGUILayout.Toggle("Effects volume", menuData.showFXAudioVolume);
			menuData.showEnviroAudioVolume = EditorGUILayout.Toggle("Environment volume", menuData.showEnviroAudioVolume);
		}
		EditorGUILayout.EndVertical();
	}

	#endregion
	// ================================================================================================================
	#region In-Game

	private void DrawMenuBarOptions(DefaultGameGUIData gameData)
	{
		GUILayout.Space(15);
		GUILayout.Label("Menu Bar", UniRPGEdGui.Head2Style);

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Settings", UniRPGEdGui.Head4Style);
				EditorGUILayout.Space();
				gameData.menuIconWidth = EditorGUILayout.IntField("Icon size", gameData.menuIconWidth);
				EditorGUILayout.Space();
				foreach (DefaultGameGUIData_MenuOption opt in gameData.menuOptions)
				{
					EditorGUILayout.BeginHorizontal();
					{
						opt.active = EditorGUILayout.Toggle(opt.active, GUILayout.Width(15));
						EditorGUILayout.Space();
						GUI.enabled = opt.active;
						GUILayout.Label(opt.showWhat.ToString(), GUILayout.Width(60));
						EditorGUILayout.Space();
						opt.name = EditorGUILayout.TextField(opt.name);
						EditorGUILayout.Space();
						opt.icon = (Texture2D)EditorGUILayout.ObjectField(opt.icon, typeof(Texture2D), false, GUILayout.Width(40), GUILayout.Height(40));
						GUI.enabled = true;
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
			{
				EditorGUIUtility.LookLikeControls(50, 120);
				GUILayout.Label("Menu Bar Area", UniRPGEdGui.Head4Style);
				EditorGUILayout.Space();
				gameData.trMenuBar = ElementTransformField(gameData.trMenuBar, null, false, true, true, true, false);
			}
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
	}

	private void DrawActionBarSettings(DefaultGameGUIData gameData)
	{
		GUILayout.Space(15);
		GUILayout.Label("Action/Skill Bar", UniRPGEdGui.Head2Style);
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();	
			{
				EditorGUIUtility.LookLikeControls(170, 30);
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{
					GUILayout.Label("Settings", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					gameData.actionSlotsCount = EditorGUILayout.IntField("Number of Action Slots", gameData.actionSlotsCount);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{
					EditorGUIUtility.LookLikeControls();
					GUILayout.Label("Position & Size", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					gameData.actionIconWidth = EditorGUILayout.IntField("Icon Size", gameData.actionIconWidth);
					EditorGUIUtility.LookLikeControls(50, 120);
					gameData.trActionBar = ElementTransformField(gameData.trActionBar, null, false, true, true, true, false);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();
			{
				EditorGUIUtility.LookLikeControls(50);
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{
					GUILayout.Label("Queued Skill Indicator", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					gameData.showSkillQueued = EditorGUILayout.Toggle("Use", gameData.showSkillQueued);
					if (gameData.showSkillQueued)
					{
						gameData.txQueued = EditorGUILayout.ObjectField(gameData.txQueued, typeof(Texture2D), false, GUILayout.Width(50), GUILayout.Height(50)) as Texture2D;
					}
					else gameData.txQueued = null; // make sure the texture is not referenced when this is not used
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();

				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(236));
				{
					GUILayout.Label("Out-of-Range Indicator", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					gameData.showWhenCantUseSkill = EditorGUILayout.Toggle("Use", gameData.showWhenCantUseSkill);
					if (gameData.showWhenCantUseSkill)
					{
						gameData.txInvalid = EditorGUILayout.ObjectField(gameData.txInvalid, typeof(Texture2D), false, GUILayout.Width(50), GUILayout.Height(50)) as Texture2D;
					}
					else gameData.txQueued = null; // make sure the texture is not referenced when this is not used				
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(600));
		{
			GUILayout.Label("Skill Cooldown Animation", UniRPGEdGui.Head4Style);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			{
				gameData.showSkillCooldown = EditorGUILayout.Toggle("Use", gameData.showSkillCooldown);
				EditorGUILayout.Space();
				if (gameData.showSkillCooldown)
				{
					if (UniRPGEdGui.IconButton("Add Frame", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton))
					{
						if (gameData.txCooldown == null) gameData.txCooldown = new List<Texture2D>();
						gameData.txCooldown.Add(null);
					}
				}
				else gameData.txCooldown = null;
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(15);
			if (gameData.showSkillCooldown && gameData.txCooldown != null)
			{
				scroll[1] = EditorGUILayout.BeginScrollView(scroll[1], true, false, UniRPGEdGui.Skin.horizontalScrollbar, UniRPGEdGui.Skin.verticalScrollbar, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(100));
				{
					EditorGUILayout.BeginHorizontal();
					{
						int del = -1;
						for (int i = 0; i < gameData.txCooldown.Count; i++)
						{
							gameData.txCooldown[i] = EditorGUILayout.ObjectField(gameData.txCooldown[i], typeof(Texture2D), false, GUILayout.Width(50), GUILayout.Height(50)) as Texture2D;
							if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(20))) del = i;
							EditorGUILayout.Space();
						}
						if (del >= 0) gameData.txCooldown.RemoveAt(del);
						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndScrollView();
			}
		}
		EditorGUILayout.EndVertical();
	}

	private void DrawStatusPanels(DefaultGameGUIData gameData)
	{
		GUILayout.Space(15);
		GUILayout.Label("Status Panels", UniRPGEdGui.Head2Style);

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
				{
					EditorGUIUtility.LookLikeControls(130);
					GUILayout.Label("Player", UniRPGEdGui.Head4Style);
					gameData.showPlayerStatus = EditorGUILayout.Toggle("Show Player Status", gameData.showPlayerStatus);
					if (gameData.showPlayerStatus)
					{
						EditorGUILayout.BeginHorizontal();
						{
							gameData.showPlayerPortrait = EditorGUILayout.Toggle("Show Portrait", gameData.showPlayerPortrait);
							EditorGUILayout.Space();
							if (gameData.showPlayerPortrait)
							{
								EditorGUIUtility.LookLikeControls(80);
								gameData.showPlayerLevel = EditorGUILayout.Toggle("Show Level", gameData.showPlayerLevel);
							}
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
						EditorGUIUtility.LookLikeControls();
						EditorGUILayout.Space();
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Label("Show value: ");
							if (GUILayout.Toggle(gameData.playerStatusShowValue == 0, " No")) gameData.playerStatusShowValue = 0;
							if (GUILayout.Toggle(gameData.playerStatusShowValue == 1, " Number")) gameData.playerStatusShowValue = 1;
							if (GUILayout.Toggle(gameData.playerStatusShowValue == 2, " Percentage")) gameData.playerStatusShowValue = 2;
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space();
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Label("Attributes to Show", UniRPGEdGui.Head4Style);
							EditorGUILayout.Space();
							if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton))
							{
								gameData.playerStatusBars.Add(new DefaultGameGUIData_StatusBar());
							}
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space();
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Label("Attribute", GUILayout.Width(110));
							EditorGUILayout.Space();
							GUILayout.Label("Texture");
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space();
						scroll[2] = UniRPGEdGui.BeginScrollView(scroll[2], UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(120));
						{
							DefaultGameGUIData_StatusBar del = null;
							foreach (DefaultGameGUIData_StatusBar bar in gameData.playerStatusBars)
							{
								EditorGUILayout.BeginHorizontal();
								{
									int attribIdx = UniRPGEditorGlobal.DB.GetAttribNameIdx(bar.attribId);
									EditorGUI.BeginChangeCheck();
									attribIdx = EditorGUILayout.Popup(attribIdx, UniRPGEditorGlobal.DB.AttributeNames, GUILayout.Width(110));
									if (EditorGUI.EndChangeCheck() && attribIdx >= 0)
									{
										bar.attribId.Value = UniRPGEditorGlobal.DB.attributes[attribIdx].id.Value;
									}
									bar.texture = (Texture2D)EditorGUILayout.ObjectField(bar.texture, typeof(Texture2D), false, GUILayout.Width(110));
									if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) del = bar;
									GUILayout.FlexibleSpace();
								}
								EditorGUILayout.EndHorizontal();
							}

							if (del != null)
							{
								gameData.playerStatusBars.Remove(del);
								del = null;
							}
						}
						UniRPGEdGui.EndScrollView();
					}
					else
					{
						if (gameData.playerStatusBars.Count > 0) gameData.playerStatusBars = new List<DefaultGameGUIData_StatusBar>(0);
					}
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
				{
					gameData.statusPortraitWidth = EditorGUILayout.IntField("Portrait Size", gameData.statusPortraitWidth);
					gameData.statusBarWidth = EditorGUILayout.IntField("Statusbar Width", gameData.statusBarWidth);
					gameData.statusBarHeight = EditorGUILayout.IntField("Statusbar Height", gameData.statusBarHeight);
					gameData.statusBarBack = (Texture2D)EditorGUILayout.ObjectField("Statusbar Background", gameData.statusBarBack, typeof(Texture2D), false);
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Target", UniRPGEdGui.Head4Style);

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label(" ", GUILayout.Width(55));
					GUILayout.Label("Show", GUILayout.Width(40));
					GUILayout.Label("Name", GUILayout.Width(40));
					GUILayout.Label("Img", GUILayout.Width(40));
					GUILayout.Label("Lv", GUILayout.Width(40));
					GUILayout.Label("Bars", GUILayout.Width(35));
				}
				EditorGUILayout.EndHorizontal();

				for (int i = 0; i < gameData.targetStatus.Length; i++)
				{
					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Label(gameData.targetStatus[i].nm, GUILayout.Width(60));
						gameData.targetStatus[i].show = EditorGUILayout.Toggle(gameData.targetStatus[i].show);
						gameData.targetStatus[i].name = EditorGUILayout.Toggle(gameData.targetStatus[i].name);
						gameData.targetStatus[i].img = EditorGUILayout.Toggle(gameData.targetStatus[i].img);
						gameData.targetStatus[i].level = EditorGUILayout.Toggle(gameData.targetStatus[i].level);
						gameData.targetStatus[i].bars = EditorGUILayout.Toggle(gameData.targetStatus[i].bars);
					}
					EditorGUILayout.EndHorizontal();
				}

				//EditorGUIUtility.LookLikeControls(130);
				//gameData.showTargetStatus = EditorGUILayout.Toggle("Show Target Status", gameData.showTargetStatus);
				//gameData.showTargetName = EditorGUILayout.Toggle("Show Target Name", gameData.showTargetName);
				//if (gameData.showTargetStatus)
				//{
				//	EditorGUILayout.BeginHorizontal();
				//	{
				//		gameData.showTargetPortrait = EditorGUILayout.Toggle("Show Portrait", gameData.showTargetPortrait);
				//		EditorGUILayout.Space();
				//		if (gameData.showTargetPortrait)
				//		{
				//			EditorGUIUtility.LookLikeControls(80);
				//			gameData.showTargetLevel = EditorGUILayout.Toggle("Show Level", gameData.showTargetLevel);
				//		}
				//		GUILayout.FlexibleSpace();
				//	}
				//	EditorGUILayout.EndHorizontal();

				//	EditorGUILayout.Space();
				//	GUILayout.Label("Show status for...");
				//	EditorGUILayout.BeginHorizontal();
				//	{
				//		gameData.showStatusForFriendly = GUILayout.Toggle(gameData.showStatusForFriendly, " Friendly", GUILayout.Width(100));
				//		gameData.showStatusForNeutral = GUILayout.Toggle(gameData.showStatusForNeutral, " Neutral", GUILayout.Width(100));
				//		gameData.showStatusForHostile = GUILayout.Toggle(gameData.showStatusForHostile, " Hostile", GUILayout.Width(100));
				//		GUILayout.FlexibleSpace();
				//	}
				//	EditorGUILayout.EndHorizontal();
				//	EditorGUILayout.BeginHorizontal();
				//	{
				//		gameData.showStatusForItem = GUILayout.Toggle(gameData.showStatusForItem, " Item", GUILayout.Width(100));
				//		gameData.showStatusForObject = GUILayout.Toggle(gameData.showStatusForObject, " Object", GUILayout.Width(100));
				//		GUILayout.FlexibleSpace();
				//	}
				//	EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();
				EditorGUIUtility.LookLikeControls();
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Show value: ");
					if (GUILayout.Toggle(gameData.targetStatusShowValue == 0, " No")) gameData.targetStatusShowValue = 0;
					if (GUILayout.Toggle(gameData.targetStatusShowValue == 1, " Number")) gameData.targetStatusShowValue = 1;
					if (GUILayout.Toggle(gameData.targetStatusShowValue == 2, " Percentage")) gameData.targetStatusShowValue = 2;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Attributes to Show", UniRPGEdGui.Head4Style);
					EditorGUILayout.Space();
					if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton))
					{
						gameData.targetStatusBars.Add(new DefaultGameGUIData_StatusBar());
					}
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Attribute", GUILayout.Width(110));
					EditorGUILayout.Space();
					GUILayout.Label("Texture");
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				scroll[2] = UniRPGEdGui.BeginScrollView(scroll[2], UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(120));
				{
					DefaultGameGUIData_StatusBar del = null;
					foreach (DefaultGameGUIData_StatusBar bar in gameData.targetStatusBars)
					{
						EditorGUILayout.BeginHorizontal();
						{
							int attribIdx = UniRPGEditorGlobal.DB.GetAttribNameIdx(bar.attribId);
							EditorGUI.BeginChangeCheck();
							attribIdx = EditorGUILayout.Popup(attribIdx, UniRPGEditorGlobal.DB.AttributeNames, GUILayout.Width(110));
							if (EditorGUI.EndChangeCheck() && attribIdx >= 0)
							{
								bar.attribId.Value = UniRPGEditorGlobal.DB.attributes[attribIdx].id.Value;
							}
							bar.texture = (Texture2D)EditorGUILayout.ObjectField(bar.texture, typeof(Texture2D), false, GUILayout.Width(110));
							if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) del = bar;
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}

					if (del != null)
					{
						gameData.targetStatusBars.Remove(del);
						del = null;
					}
				}
				UniRPGEdGui.EndScrollView();
				//}
				//else
				//{
				//	if (gameData.targetStatusBars.Count > 0) gameData.targetStatusBars = new List<DefaultGameGUIData_StatusBar>(0);
				//}
			}
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();		
	}

	private void DrawMisc(DefaultGameGUIData gameData)
	{
		GUILayout.Space(15);
		GUILayout.Label("Misc Settings", UniRPGEdGui.Head2Style);
		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				gameData.sfxButton = (AudioClip)EditorGUILayout.ObjectField("Button sound", gameData.sfxButton, typeof(AudioClip), false);
				EditorGUILayout.Space();
				gameData.txEmptySlot = (Texture2D)EditorGUILayout.ObjectField("Empty Slot Icon", gameData.txEmptySlot, typeof(Texture2D), false, GUILayout.Width(20), GUILayout.Height(20));
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Character Panel", UniRPGEdGui.Head4Style);
				gameData.charaPanelName = EditorGUILayout.TextField("Name", gameData.charaPanelName);
				gameData.charaPanelShowLevel = EditorGUILayout.Toggle("Show Level", gameData.charaPanelShowLevel);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Skill Panel", UniRPGEdGui.Head4Style);
				gameData.skillPanelName = EditorGUILayout.TextField("Name", gameData.skillPanelName);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Bag Panel", UniRPGEdGui.Head4Style);
				gameData.bagPanelName = EditorGUILayout.TextField("Name", gameData.bagPanelName);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Journal/ Quest Log Panel", UniRPGEdGui.Head4Style);
				gameData.logPanelName = EditorGUILayout.TextField("Name", gameData.logPanelName);
				gameData.txQuestComplete[0] = (Texture2D)EditorGUILayout.ObjectField("Quest Incomplete", gameData.txQuestComplete[0], typeof(Texture2D), false, GUILayout.Width(20), GUILayout.Height(20));
				gameData.txQuestComplete[1] = (Texture2D)EditorGUILayout.ObjectField("Quest Complete", gameData.txQuestComplete[1], typeof(Texture2D), false, GUILayout.Width(20), GUILayout.Height(20));
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(300));
			{
				GUILayout.Label("Player can still move while\nthese panels are open...\n", UniRPGEdGui.Head4Style);
				gameData.plrMoveCharSheet = EditorGUILayout.Toggle("Character Sheet", gameData.plrMoveCharSheet);
				gameData.plrMoveSkills = EditorGUILayout.Toggle("Skills", gameData.plrMoveSkills);
				gameData.plrMoveBag = EditorGUILayout.Toggle("Inventory (Bag)", gameData.plrMoveBag);
				gameData.plrMoveDialogue = EditorGUILayout.Toggle("Dialogue/Conversation", gameData.plrMoveDialogue);
				gameData.plrMoveJournal = EditorGUILayout.Toggle("Journal", gameData.plrMoveJournal);
				gameData.plrMoveShop = EditorGUILayout.Toggle("Shop", gameData.plrMoveShop);
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	#endregion
	// ================================================================================================================
	#region Helpers

	public static Texture2D TextureWithTransformCheck(string label, Texture2D texture, GUIElementTransform tr, params GUILayoutOption[] options)
	{
		Texture2D prev = texture;
		if (!string.IsNullOrEmpty(label)) texture = (Texture2D)EditorGUILayout.ObjectField(label, texture, typeof(Texture2D), false, options);
		else texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, options);
		if (prev != texture)
		{
			if (texture)
			{
				tr.size.x = texture.width;
				tr.size.y = texture.height;
			}
			else
			{
				tr.size.x = 0;
				tr.size.y = 0;
			}
		}
		return texture;
	}

	public static GUIElementTransform ElementTransformField(GUIElementTransform tr, string label=null, bool stretch=true, bool xalign=true, bool yalign=true, bool offset=true, bool size=true)
	{
		if (!string.IsNullOrEmpty(label)) { GUILayout.Label(label, EditorStyles.boldLabel); EditorGUILayout.Space(); }
		if (stretch) tr.stretch = (GUIElementTransform.Stretch)EditorGUILayout.Popup("Stretch", (int)tr.stretch, System.Enum.GetNames(typeof(GUIElementTransform.Stretch)));
		if (xalign) tr.xAlign = (GUIElementTransform.XAlign)EditorGUILayout.Popup("X-Align", (int)tr.xAlign, System.Enum.GetNames(typeof(GUIElementTransform.XAlign)));
		if (yalign) tr.yAlign = (GUIElementTransform.YAlign)EditorGUILayout.Popup("Y-Align", (int)tr.yAlign, System.Enum.GetNames(typeof(GUIElementTransform.YAlign)));
		if (offset) tr.offset = EditorGUILayout.Vector2Field("Offset", tr.offset);
		if (size) tr.size = UniRPGEdGui.Vector2Field("Size", "width", "height", tr.size, 200);
		return tr;
	}

	#endregion
	// ================================================================================================================
} }