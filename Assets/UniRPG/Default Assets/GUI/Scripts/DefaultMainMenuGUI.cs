// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class DefaultMainMenuGUI : MonoBehaviour 
{
	public GameObject camObject;
	public GameObject backgroundImage;

	// all the gui settings and data is in here
	private DefaultMainMenuGUIData gui;

	// ================================================================================================================

	// cached rects so they dont get calculated each frame
	private Rect[] rMenu = new Rect[2];
	private Rect[] rNew = new Rect[5];
	private Rect rOpt;

	private enum State { None=0, MainMenu, SelectCharacter, SelectName, CreateNewGame, LoadGame, Options, OptionsSound, OptionsGraphics, ChooseSaveSlot }
	private State state = State.None;

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };

	private GameObject newGameBackObj;
	private GameObject previewObject;

	private string newName = "Player";
	private CharacterBase newCharacter = null;
	private RPGActorClass newClass = null;

	private AudioSource sfxButton = null;
	private AudioSource music = null;

	private float gfxSelectedQuality = 0f;
	private float gfxSelectedResolution = 0f;

	private bool recalcRecs = false;
	private int[] newRes = new int[] { 0, 0 };

	// ================================================================================================================
	#region awake/start/init

	void Awake()
	{
#if UNITY_EDITOR
		// check if UniRPGGLobal is loaded, if not then load it now. This is a hack which is only needed during development
		// time to allow develper to run this scene in unity editor but still load the needed global scene
		if (!UniRPGGlobal.Instance) Application.LoadLevelAdditive("unirpg");
#endif
	}

	IEnumerator Start() 
	{
		UniRPGGlobal.Instance.OnMenusLoaded(); // tell UnIRPG that the menu is loaded

		gui = UniRPGGlobal.DB.menuGUIData.GetComponent<DefaultMainMenuGUIData>();

		// init New Screen defaults
		newCharacter = UniRPGGlobal.MainMenuData.GetDefaultPlayerCharacter(UniRPGGlobal.DB.playerCanSelectCharacter);
		if (newCharacter == null) Debug.LogError("Could not find a default player character. There must be at least one Actor (PlayerCharacter) set as 'Avail at Start'");

		if (UniRPGGlobal.DB.playerCanSelectClass)
		{
			if (newCharacter != null) newClass = UniRPGGlobal.MainMenuData.GetDefaultPlayerClass(newCharacter.Actor);
		}

		// wait a frame before doing the following
		yield return null;

		CalcRecs();

		// ready
		SetState(State.MainMenu);

		AudioListener.volume = UniRPGGlobal.DB.audioMainVolume;

		// start random song
		if (gui.menuMusic.Count > 0)
		{
			int id = 0;
			if (gui.menuMusic.Count > 1) id = Random.Range(0, gui.menuMusic.Count);
			music = camObject.AddComponent<AudioSource>();
			music.clip = gui.menuMusic[id];
			music.volume = UniRPGGlobal.DB.musicVolume;
			music.bypassEffects = true;
			music.bypassReverbZones = true;
			music.bypassListenerEffects = true;
			music.loop = true;
			music.Play();
		}

		// create buton click audio source if needed
		if (gui.sfxButton)
		{
			sfxButton = camObject.AddComponent<AudioSource>();
			sfxButton.clip = gui.sfxButton;
			sfxButton.volume = UniRPGGlobal.DB.guiAudioVolume;
			sfxButton.playOnAwake = false;
			sfxButton.bypassEffects = true;
			sfxButton.bypassReverbZones = true;
			sfxButton.bypassListenerEffects = true;
			sfxButton.loop = false;
		}
	}

	private void CalcRecs()
	{
		recalcRecs = false;
		UniRPGGUI.CalcGUIScale(gui.width, gui.height);

		// calc the element rects now that the screen width & height is known
		rMenu[0] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trLogo);
		rMenu[1] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trMenu);

		rNew[0] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trNewCharaArea);
		rNew[1] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trNewClassArea);
		rNew[2] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trNewButtonsArea);
		rNew[3] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trNewCharaInfoArea);
		rNew[4] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trNewClassInfoArea);

		gui.trOptions.size = new Vector2(320f, 110f);
		if (gui.showAudioMainVolume) gui.trOptions.size.y += 32;
		if (gui.showMusicVolume) gui.trOptions.size.y += 32;
		if (gui.showGUIAudioVolume) gui.trOptions.size.y += 32;
		if (gui.showFXAudioVolume) gui.trOptions.size.y += 32;
		if (gui.showEnviroAudioVolume) gui.trOptions.size.y += 32;

		rOpt = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trOptions);
	}

	#endregion
	// ================================================================================================================
	#region GUI && Update

	void Update()
	{
		if (state == State.CreateNewGame)
		{
			state = State.None;
			camObject.SetActive(false); // disable the camera cause the load screen has its own
			//UniRPGGlobal.Instance.ShowLoading();

			// setup the gameplay settings - the selected player/class/etc
			if (UniRPGGlobal.DB.playerCanChooseName && !string.IsNullOrEmpty(newName)) UniRPGGlobal.Instance.startPlayerName = newName;
			if (UniRPGGlobal.DB.playerCanSelectCharacter) UniRPGGlobal.Instance.startCharacterDef = newCharacter.gameObject;
			if (UniRPGGlobal.DB.playerCanSelectClass) UniRPGGlobal.Instance.startCharacterClass = newClass;

			// tell UnIRPG to load the 1st scene (new-game)
			UniRPGGlobal.Instance.LoadNewGame();
		}
	}

	void OnGUI()
	{
		if (state == State.None) return;
		
		if (recalcRecs)
		{
			// i needa wait till the res is actually applied before updating the recs
			if (newRes[0] == Screen.width && newRes[1] == Screen.height)
			{
				CalcRecs();
			}
		}

		gui.UseSkin();
		UniRPGGUI.BeginScaledGUI();
		
		switch (state)
		{
			case State.MainMenu: DrawMainMenu(); break;
			case State.SelectName: Draw_SelectCharacter(); break;
			case State.SelectCharacter: Draw_SelectCharacter(); break;
			case State.LoadGame: DrawLoadGame(); break;
			case State.Options: DrawOptions(); break;
			case State.OptionsSound: DrawSoundOptions(); break;
			case State.OptionsGraphics: DrawGraphicsOptions(); break;
			case State.ChooseSaveSlot: DrawChooseSlot(); break;
		}

		UniRPGGUI.EndScaledGUI();
	}

	private void SetState(State newState)
	{
		State prev = state;
		state = newState;

		// --- remove old objects as needed
		if (prev == State.SelectCharacter)
		{
			LoadActorPreview(null);
			if (newGameBackObj) GameObject.Destroy(newGameBackObj);
		}

		// --- set new state
		if (gui.texMenuBack && (state == State.MainMenu || state == State.ChooseSaveSlot || state == State.LoadGame || state == State.Options || state == State.OptionsSound || state == State.OptionsGraphics || state == State.SelectName))
		{
			backgroundImage.GetComponent<GUITexture>().texture = gui.texMenuBack;
		}
		else backgroundImage.GetComponent<GUITexture>().texture = null;

		// --- instantiate objects as needed
		if (state == State.SelectCharacter)
		{
			if (newCharacter) LoadActorPreview(newCharacter.gameObject);
			if (gui.newGameCharaBackFab) newGameBackObj = GameObject.Instantiate(gui.newGameCharaBackFab) as GameObject;
			else if (gui.texNewGameBack) backgroundImage.GetComponent<GUITexture>().texture = gui.texMenuBack;
		}

		// --- reset some gui elements that are reused
		for (int i = 0; i < scroll.Length; i++) scroll[i] = Vector2.zero;

	}

	/// <summary>Works like a GUI.Toggle but returns TRUE only if the state went true</summary>
	private bool ListItem(bool selected, GUIContent content, GUIStyle style)
	{
		if (GUILayout.Toggle(selected, content, style))
		{
			if (selected == false) return true; // was not previously selected
		}
		return false;
	}

	private void ButtonClickFX()
	{
		if (sfxButton != null) sfxButton.Play();
	}

	#endregion
	// ================================================================================================================
	#region main menu

	private void DrawMainMenu()
	{
		if (gui.texLogo) GUI.DrawTexture(rMenu[0], gui.texLogo);

		GUILayout.BeginArea(rMenu[1], GUI.skin.box);
		{
			if (UniRPGGlobal.Instance.SaveSlots.Count == 0) GUI.enabled = false;
			if (GUILayout.Button(gui.labelContinue))
			{
				ButtonClickFX();
				SetState(State.LoadGame);
			}
			GUI.enabled = true;

			if (GUILayout.Button(gui.labelNewGame))
			{
				ButtonClickFX();
				SetState(State.ChooseSaveSlot);
			}

			if (GUILayout.Button(gui.labelOptions))
			{
				ButtonClickFX();
				SetState(State.Options);
			}

			if (GUILayout.Button(gui.labelExit)) { Application.Quit(); }
		}
		GUILayout.EndArea();
	}

	#endregion
	// ================================================================================================================
	#region New Game

	private void DrawChooseSlot()
	{
		if (gui.texLogo) GUI.DrawTexture(rMenu[0], gui.texLogo);

		int chosen_slot = 0;
		GUILayout.BeginArea(rMenu[1], gui.skin.box);
		{
			if (GUILayout.Button("Save to Slot 1" + (UniRPGGlobal.Instance.SaveSlots.ContainsKey("1") ? " (used)" : ""))) chosen_slot = 1;
			if (GUILayout.Button("Save to Slot 2" + (UniRPGGlobal.Instance.SaveSlots.ContainsKey("2") ? " (used)" : ""))) chosen_slot = 2;
			if (GUILayout.Button("Save to Slot 3" + (UniRPGGlobal.Instance.SaveSlots.ContainsKey("3") ? " (used)" : ""))) chosen_slot = 3;
			if (GUILayout.Button("Cancel"))
			{
				ButtonClickFX();
				SetState(State.MainMenu);
			}
		}
		GUILayout.EndArea();

		if (chosen_slot > 0)
		{
			ButtonClickFX();
			UniRPGGlobal.SetActiveSaveSlot(chosen_slot.ToString());

			if (UniRPGGlobal.DB.playerCanSelectCharacter || UniRPGGlobal.DB.playerCanSelectClass) SetState(State.SelectCharacter);
			else if (UniRPGGlobal.DB.playerCanChooseName) SetState(State.SelectName);
			else SetState(State.CreateNewGame);
		}
	}

	private void Draw_SelectCharacter()
	{
		if (gui.texLogo && (state == State.SelectName || gui.newGameCharaBackFab == null)) GUI.DrawTexture(rMenu[0], gui.texLogo);

		if (UniRPGGlobal.DB.playerCanSelectCharacter)
		{
			GUILayout.BeginArea(rNew[0], gui.labelSelectChara, GUI.skin.window);
			scroll[0] = GUILayout.BeginScrollView(scroll[0]);
			{
				foreach (CharacterBase chara in UniRPGGlobal.MainMenuData.playerCharacters)
				{	// show the characters that can be chosen from
					if (chara.Actor.availAtStart)
					{
						if (ListItem(newCharacter == chara, new GUIContent("  " + chara.Actor.screenName, chara.Actor.portrait[0]), gui.ListItem))
						{
							newCharacter = chara;
							LoadActorPreview(newCharacter.gameObject);
						}
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();

			if (newCharacter)
			{
				if (!string.IsNullOrEmpty(newCharacter.Actor.description))
				{
					GUILayout.BeginArea(rNew[3], GUI.skin.box);
					scroll[2] = GUILayout.BeginScrollView(scroll[2]);
					GUILayout.Label(newCharacter.Actor.description);
					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}

		}

		if (UniRPGGlobal.DB.playerCanSelectClass)
		{
			GUILayout.BeginArea(rNew[1], gui.labelSelectClass, GUI.skin.window);
			scroll[1] = GUILayout.BeginScrollView(scroll[1]);
			{
				foreach (RPGActorClass c in UniRPGGlobal.DB.classes)
				{	// show the characters that can be chosen from
					if (c.availAtStart)
					{
						if (ListItem(newClass == c, new GUIContent("  " + c.screenName, c.icon[0]), gui.ListItem)) newClass = c;
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();

			if (newClass)
			{
				if (!string.IsNullOrEmpty(newClass.description))
				{
					GUILayout.BeginArea(rNew[4], GUI.skin.box);
					scroll[3] = GUILayout.BeginScrollView(scroll[3]);
					GUILayout.Label(newClass.description);
					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}
		}

		GUILayout.BeginArea(rNew[2], GUI.skin.box);
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical();
				{
					if (UniRPGGlobal.DB.playerCanChooseName)
					{
						GUILayout.Label("Name");
						newName = GUILayout.TextField(newName);
					}
				}
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				{
					if (GUILayout.Button("Start"))
					{
						ButtonClickFX();
						SetState(State.CreateNewGame);
					}
					if (GUILayout.Button("Back"))
					{
						ButtonClickFX();
						SetState(State.MainMenu);
					}
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
	}

	private void LoadActorPreview(GameObject prefab)
	{
		if (previewObject) GameObject.Destroy(previewObject);
		if (prefab)
		{
			previewObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
		}
	}

	#endregion
	// ================================================================================================================
	#region Load Game

	private void DrawLoadGame()
	{
		if (gui.texLogo) GUI.DrawTexture(rMenu[0], gui.texLogo);

		int chosen_slot = 0;
		GUILayout.BeginArea(rMenu[1], gui.skin.box);
		{
			GUI.enabled = UniRPGGlobal.Instance.SaveSlots.ContainsKey("1");
			if (GUILayout.Button("Load Slot 1")) chosen_slot = 1;
			GUI.enabled = UniRPGGlobal.Instance.SaveSlots.ContainsKey("2");
			if (GUILayout.Button("Load Slot 2")) chosen_slot = 2;
			GUI.enabled = UniRPGGlobal.Instance.SaveSlots.ContainsKey("3");
			if (GUILayout.Button("Load Slot 3")) chosen_slot = 3;
			GUI.enabled = true;
			if (GUILayout.Button("Cancel"))
			{
				ButtonClickFX();
				SetState(State.MainMenu);
			}
		}
		GUILayout.EndArea();

		if (chosen_slot > 0)
		{
			ButtonClickFX();
			state = State.None;
			camObject.SetActive(false); // disable the camera cause the load screen has its own

			// tell UnIRPG to load the game scene
			UniRPGGlobal.SetActiveSaveSlot(chosen_slot.ToString());
			if (false == UniRPGGlobal.LoadGameState())
			{
				camObject.SetActive(true);
				SetState(State.MainMenu);
			}
		}
	}

	#endregion
	// ================================================================================================================
	#region Options

	private void DrawOptions()
	{
		if (gui.texLogo) GUI.DrawTexture(rMenu[0], gui.texLogo);

		GUILayout.BeginArea(rMenu[1], GUI.skin.box);
		{
			if (GUILayout.Button("Graphics"))
			{
				if (UniRPGGlobal.DB.gfxQuality == 0) gfxSelectedQuality = 0;
				else gfxSelectedQuality = (float)UniRPGGlobal.DB.gfxQuality / (float)(QualitySettings.names.Length);
				if (UniRPGGlobal.DB.gfxResolution == 0) gfxSelectedResolution = 0;
				else gfxSelectedResolution = (float)UniRPGGlobal.DB.gfxResolution / (float)(Screen.resolutions.Length);
				ButtonClickFX();
				SetState(State.OptionsGraphics);				
			}

			if (GUILayout.Button("Sound"))
			{
				ButtonClickFX();
				SetState(State.OptionsSound);
			}

			GUILayout.Space(37);

			if (GUILayout.Button("Back")) 
			{
				ButtonClickFX();
				SetState(State.MainMenu);
			}
		}
		GUILayout.EndArea();
	}

	private void DrawSoundOptions()
	{
		if (gui.texLogo) GUI.DrawTexture(rMenu[0], gui.texLogo);

		GUILayout.BeginArea(rOpt, GUI.skin.box);
		{
			ShowSoundOptions();

			if (GUILayout.Button("Done"))
			{
				ButtonClickFX();
				UpdateSoundVol();
				UniRPGGlobal.Instance.SaveGameSettings();
				SetState(State.Options);
			}
		}
		GUILayout.EndArea();
	}

	private void ShowSoundOptions()
	{
		//if (gui.showAudioMainVolume || gui.showMusicVolume || gui.showGUIAudioVolume || gui.showFXAudioVolume || gui.showEnviroAudioVolume)
		{
			GUILayout.Label("Sound Volume");
			GUILayout.Space(20);
		}

		if (gui.showAudioMainVolume)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Master", GUILayout.Width(120));
			UniRPGGlobal.DB.audioMainVolume = GUILayout.HorizontalSlider(UniRPGGlobal.DB.audioMainVolume, 0f, 1f);
			GUILayout.EndHorizontal();
		}
		if (gui.showMusicVolume)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Music", GUILayout.Width(120));
			UniRPGGlobal.DB.musicVolume = GUILayout.HorizontalSlider(UniRPGGlobal.DB.musicVolume, 0f, 1f);
			GUILayout.EndHorizontal();
		}
		if (gui.showGUIAudioVolume)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Interface", GUILayout.Width(120));
			UniRPGGlobal.DB.guiAudioVolume = GUILayout.HorizontalSlider(UniRPGGlobal.DB.guiAudioVolume, 0f, 1f);
			GUILayout.EndHorizontal();
		}
		if (gui.showFXAudioVolume)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Effects", GUILayout.Width(120));
			UniRPGGlobal.DB.fxAudioVolume = GUILayout.HorizontalSlider(UniRPGGlobal.DB.fxAudioVolume, 0f, 1f);
			GUILayout.EndHorizontal();
		}
		if (gui.showEnviroAudioVolume)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Environment", GUILayout.Width(120));
			UniRPGGlobal.DB.enviroAudioVolume = GUILayout.HorizontalSlider(UniRPGGlobal.DB.enviroAudioVolume, 0f, 1f);
			GUILayout.EndHorizontal();
		}
	}

	private void UpdateSoundVol()
	{
		UniRPGGlobal.Instance.UpdateSoundVol();
		if (gui.showAudioMainVolume) AudioListener.volume = UniRPGGlobal.DB.audioMainVolume;
		if (gui.showMusicVolume && music != null) music.volume = UniRPGGlobal.DB.musicVolume;
		if (gui.showGUIAudioVolume && sfxButton != null) sfxButton.volume = UniRPGGlobal.DB.guiAudioVolume;
	}

	private void DrawGraphicsOptions()
	{
		if (gui.texLogo) GUI.DrawTexture(rMenu[0], gui.texLogo);

		GUILayout.BeginArea(rOpt, GUI.skin.box);
		{
			GUILayout.Label("Graphics Options");
			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Quality", GUILayout.Width(120));
			gfxSelectedQuality = GUILayout.HorizontalSlider(gfxSelectedQuality, 0f, 1f);
			GUILayout.EndHorizontal();
			
			UniRPGGlobal.DB.gfxQuality = Mathf.RoundToInt(gfxSelectedQuality * QualitySettings.names.Length);
			if (UniRPGGlobal.DB.gfxQuality >= QualitySettings.names.Length) UniRPGGlobal.DB.gfxQuality = QualitySettings.names.Length - 1;
			if (UniRPGGlobal.DB.gfxQuality < 0) UniRPGGlobal.DB.gfxQuality = 0;

			GUILayout.BeginHorizontal();
			GUILayout.Space(120);
			GUILayout.Label(QualitySettings.names[UniRPGGlobal.DB.gfxQuality]);
			GUILayout.EndHorizontal();

			GUILayout.Space(20);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Resolution", GUILayout.Width(120));
			gfxSelectedResolution = GUILayout.HorizontalSlider(gfxSelectedResolution, 0f, 1f);
			GUILayout.EndHorizontal();

			UniRPGGlobal.DB.gfxResolution = Mathf.RoundToInt(gfxSelectedResolution * Screen.resolutions.Length);
			if (UniRPGGlobal.DB.gfxResolution >= Screen.resolutions.Length) UniRPGGlobal.DB.gfxResolution = Screen.resolutions.Length - 1;
			if (UniRPGGlobal.DB.gfxResolution < 0) UniRPGGlobal.DB.gfxResolution = 0;

			GUILayout.BeginHorizontal();
			GUILayout.Space(120);
			Resolution res = Screen.resolutions[UniRPGGlobal.DB.gfxResolution];
			GUILayout.Label(res.width + "x" + res.height);
			GUILayout.EndHorizontal();

			UniRPGGlobal.DB.gfxFullscreen = GUILayout.Toggle(UniRPGGlobal.DB.gfxFullscreen, " Fullscreen");
			GUILayout.Space(10);

			if (GUILayout.Button("Done"))
			{
				newRes[0] = res.width;
				newRes[1] = res.height;
				recalcRecs = true;
				ButtonClickFX();
				UniRPGGlobal.Instance.SaveGameSettings();
				UniRPGGlobal.Instance.UpdateGFX();				
				SetState(State.Options);
			}
		}
		GUILayout.EndArea();
	}

	#endregion
	// ================================================================================================================
} }
