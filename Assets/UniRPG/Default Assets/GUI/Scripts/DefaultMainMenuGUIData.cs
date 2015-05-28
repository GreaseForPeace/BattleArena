// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class DefaultMainMenuGUIData : MonoBehaviour
{
	// ================================================================================================================
	// common data

	public int width = 1024;	// gui is designed to fit into this width
	public int height = 768;	// gui is designed to fit into this height

	public GUISkin skin;

	// ================================================================================================================
	// options for "Main Menu" screen

	public Texture2D texMenuBack;
	public Texture2D texLogo;
	public GUIElementTransform trLogo = new GUIElementTransform();
	public GUIElementTransform trMenu = new GUIElementTransform();

	public List<AudioClip> menuMusic = new List<AudioClip>(0); // list of songs that can be played in menu
	public AudioClip sfxButton;

	// ================================================================================================================
	// options for "New Game" screen

	public Texture2D texNewGameBack;
	public GameObject newGameCharaBackFab;

	public GUIElementTransform trNewCharaArea = new GUIElementTransform();
	public GUIElementTransform trNewClassArea = new GUIElementTransform();
	public GUIElementTransform trNewButtonsArea = new GUIElementTransform();
	public GUIElementTransform trNewCharaInfoArea = new GUIElementTransform();
	public GUIElementTransform trNewClassInfoArea = new GUIElementTransform();

	public string labelNewGame = "New Game";
	public string labelContinue = "Continue";
	public string labelOptions = "Options";
	public string labelExit = "Quit";
	public string labelSelectChara = "Select Character";
	public string labelSelectClass = "Select Class";

	// ================================================================================================================
	// options for "Options" screen

	public GUIElementTransform trOptions = new GUIElementTransform();
	public bool showAudioMainVolume = true;
	public bool showMusicVolume = true;
	public bool showGUIAudioVolume = true;
	public bool showFXAudioVolume = true;
	public bool showEnviroAudioVolume = true;

	// ================================================================================================================
	// styles

	public GUIStyle ListItem { get; private set; }

	// ================================================================================================================
	// vars

	private bool inited = false;

	// ================================================================================================================

	public void UseSkin()
	{
		if (skin == null)
		{
			Debug.LogError("The GUI's skin is invalid. You might need to specify it in the GUI's editor.");
			return;
		}

		useGUILayout = false;
		GUI.skin = skin;
		if (inited) return;
		inited = true;

		// init some cached styles here
		ListItem = skin.FindStyle("ListItem");
	}

	// ================================================================================================================
} }