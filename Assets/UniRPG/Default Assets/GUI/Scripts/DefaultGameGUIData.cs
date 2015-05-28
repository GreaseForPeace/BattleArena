// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[System.Serializable]
public class DefaultGameGUIData_MenuOption
{
	public enum MenuOption { None, Options, Character, Bag, Skills, Dialogue, Journal, Shop };
	public bool active = true;
	public MenuOption showWhat;
	public string name;
	public Texture2D icon;
}

[System.Serializable]
public class DefaultGameGUIData_StatusBar
{
	public GUID attribId = new GUID();
	public Texture2D texture;

	// this is init and used during runtime
	public RPGAttribute attrib { get; set; }
}

[AddComponentMenu("")]
public class DefaultGameGUIData : MonoBehaviour
{
	// ================================================================================================================
	// common data

	public int width = 1024;		// gui is designed to fit into this width
	public int height = 768;		// gui is designed to fit into this height

	public Texture2D txQueued;		// the texture used to show the queued skill
	public Texture2D txInvalid;		// the texture used to show a skill that cant be used yet
	public List<Texture2D> txCooldown = new List<Texture2D>(); // the textures used to play cooldown on icons
	public Texture2D txNoIcon;		// used where an icon should be used but could not be found

	public GUISkin skin;

	public AudioClip sfxButton;

	// ================================================================================================================

	public GUIElementTransform trMenuBar = new GUIElementTransform();
	public GUIElementTransform trActionBar = new GUIElementTransform();
	public GUIElementTransform trRightPanel = new GUIElementTransform();	// used with panels like, bags
	public GUIElementTransform trLeftPanel = new GUIElementTransform();		// used with panels like, charactersheet
	public GUIElementTransform trLeftWidePanel = new GUIElementTransform();	// used with panels like, Quest Log (that needs wider area)

	public int menuIconWidth = 40;
	public List<DefaultGameGUIData_MenuOption> menuOptions = new List<DefaultGameGUIData_MenuOption>(0);

	public int actionSlotsCount = 6;
	public int actionIconWidth = 50;
	public bool showWhenCantUseSkill = true;
	public bool showSkillCooldown = true;
	public bool showSkillQueued = true;

	public Texture2D txEmptySlot;

	public string bagPanelName = "Bag";
	public float bagIconWidth = 50f;

	public string charaPanelName = "Character";
	public bool charaPanelShowLevel = true;
	public float equipIconWidth = 50f;

	public string skillPanelName = "Skills";
	public float skillIconWidth = 50f;

	public int statusPortraitWidth = 50;
	public int statusBarWidth = 200;
	public int statusBarHeight = 15;
	public Texture2D statusBarBack;
	public bool showPlayerStatus = true;
	public int playerStatusShowValue = 1; // 0:none, 1:number, 2:percentage
	public bool showPlayerPortrait = true;
	public bool showPlayerLevel = false;
	public List<DefaultGameGUIData_StatusBar> playerStatusBars = new List<DefaultGameGUIData_StatusBar>(0);

	[System.Serializable]
	public class TargetStatusShow
	{
		public string nm;
		public bool show;
		public bool name;
		public bool img ;
		public bool level;
		public bool bars;
	}

	public TargetStatusShow[] targetStatus = new TargetStatusShow[]
	{
		new TargetStatusShow() { nm = "Hostile",	show = true, name = true, img = true, level = false, bars = true },
		new TargetStatusShow() { nm = "Neutral",	show = true, name = true, img = true, level = false, bars = true },
		new TargetStatusShow() { nm = "Friendly",	show = true, name = true, img = true, level = false, bars = false },
		new TargetStatusShow() { nm = "Item",		show = true, name = true, img = true, level = false, bars = false },
		new TargetStatusShow() { nm = "Object",		show = true, name = true, img = true, level = false, bars = false },
	};

	public int targetStatusShowValue = 1; // 0:none, 1:number, 2:percentage
	public List<DefaultGameGUIData_StatusBar> targetStatusBars = new List<DefaultGameGUIData_StatusBar>(0);

	//public bool showTargetStatus = true;
	//public bool showTargetPortrait = true;
	//public bool showTargetLevel = false;
	//public bool showTargetName = true;
	//public bool showStatusForFriendly = true;
	//public bool showStatusForNeutral = true;
	//public bool showStatusForHostile = true;
	//public bool showStatusForItem = true;
	//public bool showStatusForObject = true;

	public string logPanelName = "Journal";
	public Texture2D[] txQuestComplete = new Texture2D[2];

	public bool plrMoveCharSheet = false;
	public bool plrMoveSkills = false;
	public bool plrMoveBag = false;
	public bool plrMoveDialogue = false;
	public bool plrMoveJournal = false;
	public bool plrMoveShop = false;

	// ================================================================================================================
	// styles

	public GUIStyle ListItem { get; private set; }
	public GUIStyle ListButton { get; private set; }
	public GUIStyle HorizontalLine { get; private set; }
	public GUIStyle ActionIconFrame { get; private set; }
	public GUIStyle ActionIconEmpty { get; private set; }
	public GUIStyle ActionIconButton { get; private set; }
	public GUIStyle MenuBarButton { get; private set; }
	public GUIStyle MenuIconFrame { get; private set; }
	public GUIStyle MenuTooltip { get; private set; }
	public GUIStyle BagStackLabel { get; private set; }
	public GUIStyle BagIconFrame { get; private set; }
	public GUIStyle BagIconButton { get; private set; }
	public GUIStyle BagCurrencyLabel { get; private set; }
	public GUIStyle CharaSheetLabel1 { get; private set; }
	public GUIStyle CharaSheetLabel2 { get; private set; }
	public GUIStyle RewardsLabel { get; private set; }
	public GUIStyle QuestLabel { get; private set; }
	public GUIStyle StatusPortraitFrame { get; private set; }
	public GUIStyle StatusLevelLabel { get; private set; }
	public GUIStyle StatusFrame { get; private set; }
	public GUIStyle StatusBarText { get; private set; }
	public GUIStyle WindowCloseButton { get; private set; }
	
	// ================================================================================================================
	// vars

	private bool inited = false;

	// ================================================================================================================

	public void InitSkin()
	{
		inited = true;

		// init some cached styles here
		ListItem = skin.FindStyle("ListItem");
		ListButton = skin.FindStyle("ListButton");
		HorizontalLine = skin.FindStyle("HorizontalLine");
		ActionIconFrame = skin.FindStyle("ActionIconFrame");
		ActionIconEmpty = skin.FindStyle("ActionIconEmpty");
		ActionIconButton = skin.FindStyle("ActionIconButton");
		MenuBarButton = skin.FindStyle("MenuBarButton");
		MenuIconFrame = skin.FindStyle("MenuIconFrame");
		MenuTooltip = skin.FindStyle("MenuTooltip");
		BagStackLabel = skin.FindStyle("BagStackLabel");
		BagIconFrame = skin.FindStyle("BagIconFrame");
		BagIconButton = skin.FindStyle("BagIconButton");
		BagCurrencyLabel = skin.FindStyle("BagCurrencyLabel");
		CharaSheetLabel1 = skin.FindStyle("CharaSheetLabel1");
		CharaSheetLabel2 = skin.FindStyle("CharaSheetLabel2");
		RewardsLabel = skin.FindStyle("RewardsLabel");
		QuestLabel = skin.FindStyle("QuestLabel");
		StatusPortraitFrame = skin.FindStyle("StatusPortraitFrame");
		StatusLevelLabel = skin.FindStyle("StatusLevelLabel");
		StatusFrame = skin.FindStyle("StatusFrame");
		StatusBarText = skin.FindStyle("StatusBarText");
		WindowCloseButton = skin.FindStyle("WindowCloseButton");
	}

	public void UseSkin()
	{
		useGUILayout = false;
		GUI.skin = skin;
		if (inited) return;
		InitSkin();
	}

	// ================================================================================================================
} }