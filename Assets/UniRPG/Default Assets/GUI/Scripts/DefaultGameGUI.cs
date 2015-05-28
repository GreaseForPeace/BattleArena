// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class DefaultGameGUI : MonoBehaviour 
{
	// all the gui settings and data is in here
	private DefaultGameGUIData gui;

	// ================================================================================================================

	private enum State { Hide = 0, Running }
	private State state = State.Hide;

	// 0:action bar rect, 1:right-hand panel (like bag), 2:left-hand panel (like charactersheet), 3:menubar, 4:menutooltip,
	// 5:bag slots, 6:Quest Log Panel, 7:Shop, 8:item hover info, 9:playerstatus, 10:playerportrait, 11:targetstatus, 12:targetportrait, 13:options menu
	private Rect[] r = new Rect[14];	

	private float queuedSkillTimer = 0f;	// helpers for showing a GFX over the queued skill
	private bool queuedSkillShow = false;

	private GUIContent[] menuOptionsContent;

	private bool showOptions = false;
	private bool showCharacterSheet = false;
	private bool showSkills = false;
	private bool showBag = false;
	private bool showDialogue = false;
	private bool showJournal = false;
	private bool showShop = false;

	private Rect rPopMenu;				// pop menu rect happened
	private Rect rPopMenuScroll;		// scroll rect inside pop menu

	private bool popRectCalced = false;
	private int _showLeftPopMenu = -1;
	private int _showRightPopMenu = -1;
	private RPGItem popMenuHelperItem = null;
	private RPGItem popMenuHelperItem2 = null;
	private RPGSkill popMenuHelperSkill = null;
	private string[] popMenuHelperStrings;
	private int ShowLeftPopMenu			// menu that comes up when clicking on item in character sheet, etc (-1 if closed, else the slot the menu is showing for)
	{
		get { return _showLeftPopMenu; }
		set
		{
			popRectCalced = false;
			_showLeftPopMenu = value;
			_showRightPopMenu = -1;
			popMenuHelperItem = null;
			popMenuHelperSkill = null;
		}
	}
	private int ShowRightPopMenu		// menu that comes up when clicking on item on bag, etc (-1 if closed, else the slot the menu is showing for)
	{
		get { return _showRightPopMenu; }
		set
		{
			popRectCalced = false;
			_showRightPopMenu = value;
			_showLeftPopMenu = -1;
			popMenuHelperItem = null;
			popMenuHelperSkill = null;
		}
	}

	private Vector2[] scroll = { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero }; // 0&1:used for left, 2&3:used for right panel, 4:center panel, 5:used for left too, 6:hover scroll

	private Actor lastTargetedActor = null; // helper for targeted's status bars
	private GUIDialogueData dialogueData = null;
	private List<GUIQuestData> questData = null;
	private int selectedQuest = 0;
	private int menuBarItems = 0;		// if 0 then bar wont be drawn
	private int actionSlotCount = 0;	// if 0 then bar wont be drawn

	private Actor shopActor = null;
	private float buyMod = 1f;
	private float sellMod = 1f;
	private bool useShopCurrency = false; // if true then shopkeeper can only buy as long as it has money
	private bool shopUnlimited = false;
	private string shopBuyLabel = "Buy";
	private string shopSellLabel = "Sell";

	private RPGSkill hoverSkill = null;
	private RPGItem hoverItem = null;
	private int hoverFromPanel = 1; // 1:left, 2:right
	private bool hoverDetected = false;
	private bool hoverSkip = true;
	private string hoverItemPrice = null;

	private AudioSource sfxButton = null;

	private bool menuVisible = true;
	private bool actionsVisible = true;
	private bool statusVisible = true;

	private int fading = 0;			// -1: out, 1:in
	private float fadeAlpha = 0f;	// 0:no black, 1:full black
	private Texture2D fadeTex;

	// ================================================================================================================
	#region awake/start/init

#if UNITY_EDITOR
	void Awake()
	{
		// check if UniRPGGLobal is loaded, if not then load it now. This is a hack which is only needed during development
		// time to allow develper to run this scene in unity editor but still load the needed global scene
		if (!UniRPGGlobal.Instance) Application.LoadLevelAdditive("unirpg");
	}
#endif

	IEnumerator Start()
	{
		gui = UniRPGGlobal.DB.gameGUIData.GetComponent<DefaultGameGUIData>();
		gui.InitSkin();

		fadeTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
		fadeTex.SetPixel(0, 0, Color.white);
		fadeTex.SetPixel(1, 0, Color.white);
		fadeTex.SetPixel(0, 1, Color.white);
		fadeTex.SetPixel(1, 1, Color.white);
		fadeTex.Apply();

		UniRPGGlobal.RegisterLoadGameSceneListener(OnLoadingScene);

		// load the input bindings related to this gui theme
		// it is loading the shortcuts to open the various
		// windows like bag, skills window, etc
		InputManager.Instance.LoadInputFromBinder(new DefaultGUIInputBinder());

		// wait a frame before doing the following
		yield return null;

		UniRPGGUI.CalcGUIScale(gui.width, gui.height);

		actionSlotCount = gui.actionSlotsCount;
		gui.trActionBar.size = new Vector2(actionSlotCount * gui.actionIconWidth + actionSlotCount, gui.actionIconWidth);
		r[0] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trActionBar);

		r[1] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trRightPanel);
		r[2] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trLeftPanel);

		menuBarItems = 0;
		for (int i = 0; i < gui.menuOptions.Count; i++) if (gui.menuOptions[i].active) menuBarItems++;
		if (menuBarItems > 0)
		{
			gui.trMenuBar.size = new Vector2(gui.menuIconWidth * menuBarItems + menuBarItems, gui.menuIconWidth);
			r[3] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trMenuBar);
		}

		r[4] = r[3];
		r[4].y += r[3].height + 5;

		menuOptionsContent = new GUIContent[gui.menuOptions.Count];
		for (int i = 0; i < gui.menuOptions.Count; i++)
		{
			menuOptionsContent[i] = new GUIContent(gui.menuOptions[i].icon, gui.menuOptions[i].name);
		}

		r[6] = GUIElementTransform.CalcRect(gui.width, gui.height, gui.trLeftWidePanel);

		r[8] = new Rect(gui.width / 2 - 130, gui.height / 2 - 250, 260, 300);

		if (gui.showPlayerStatus)
		{
			r[10] = new Rect(0, 5, gui.statusPortraitWidth, gui.statusPortraitWidth);
			r[9] = new Rect(gui.statusPortraitWidth, 5, 0, 0);

			// init the player bars' attrib vars
			for (int i = 0; i < gui.playerStatusBars.Count; i++)
			{
				gui.playerStatusBars[i].attrib = UniRPGGlobal.Player.Actor.ActorClass.GetAttribute(gui.playerStatusBars[i].attribId);
			}
		}

		//if (gui.showTargetStatus)
		//{
			r[12] = new Rect(gui.width - gui.statusPortraitWidth - gui.statusBarWidth, 5, gui.statusPortraitWidth, gui.statusPortraitWidth);
			r[11] = new Rect(gui.width - gui.statusBarWidth, 5, 0 ,0);
		//}

		r[13] = new Rect(gui.width / 2 - 110, gui.height / 2 - 65, 220, 130);

		// create buton click audio source if needed
		if (gui.sfxButton)
		{
			sfxButton = gameObject.AddComponent<AudioSource>();
			sfxButton.clip = gui.sfxButton;
			sfxButton.volume = UniRPGGlobal.DB.guiAudioVolume;
			sfxButton.playOnAwake = false;
			sfxButton.bypassEffects = true;
			sfxButton.bypassReverbZones = true;
			sfxButton.bypassListenerEffects = true;
			sfxButton.loop = false;
		}

		// ready
		state = State.Running;
	}

	void OnDestroy()
	{
		if (InputManager.InstanceExist) InputManager.Instance.UnloadInputBinder(new DefaultGUIInputBinder());
		UniRPGGlobal.RemoveLoadGameSceneListener(OnLoadingScene);
	}

	#endregion
	// ================================================================================================================
	#region update/ongui

	void Update()
	{
		if (fading != 0)
		{
			UniRPGGlobal.GUIConsumedInput = true;
			if (fading == -1)
			{
				fadeAlpha += Time.deltaTime;
				if (fadeAlpha >= 1f) fadeAlpha = 1f;
			}
			else if (fading == 1)
			{
				fadeAlpha -= Time.deltaTime;
				if (fadeAlpha <= 0f)
				{
					fadeAlpha = 0f;
					fading = 0;
				}
			}
		}

		if (state == State.Hide) return;
		queuedSkillTimer -= Time.deltaTime;
		if (queuedSkillTimer <= 0.0f)
		{
			queuedSkillTimer = 0.3f;
			queuedSkillShow = !queuedSkillShow;
		}
	}

	void OnGUI()
	{
		if (fading != 0)
		{
			if (state == State.Hide) DrawFade();
			else if (fadeAlpha >= 0.5f)
			{
				DrawFade();
				return;
			}
			GUI.enabled = false;
		}
		if (state == State.Hide) return;
		gui.UseSkin();
		UniRPGGUI.BeginScaledGUI();

		if (showOptions) GUI.enabled = false;
		hoverDetected = false;

		if (statusVisible) DrawStatusPanels();
		if (menuVisible) DrawMenuBar();

		if (showCharacterSheet || showSkills || showBag || showDialogue || showJournal || showShop) GUI.enabled = false;
		if (actionsVisible) DrawActionBar();
		if (!showOptions) GUI.enabled = true;

		if (showCharacterSheet) DrawCharacterSheet();
		if (showDialogue) DrawDialogue();
		if (showJournal) DrawJournal();
		if (showBag) DrawBag();
		if (showSkills) DrawSkills();
		if (showShop) DrawShop();

		if (fading == 0) GUI.enabled = true;
		if (showOptions) DrawOptionsMenu();

		UniRPGGUI.EndScaledGUI();
		if (fading != 0)
		{
			GUI.enabled = true;
			DrawFade();
		}
	}

	private void DrawFade()
	{
		UniRPGGlobal.GUIConsumedInput = true;
		GUI.color = new Color(0, 0, 0, fadeAlpha);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTex);
	}

	private void PlayButtonFX()
	{
		if (sfxButton) sfxButton.Play();
	}

	#endregion
	// ================================================================================================================
	#region options menu

	private void DrawOptionsMenu()
	{
		UniRPGGlobal.GUIConsumedInput = true;
		GUILayout.BeginArea(r[13], GUIContent.none, GUI.skin.box);
		{
			if (GUILayout.Button("Resume Game"))
			{
				PlayButtonFX();
				showOptions = false;
			}
			if (GUILayout.Button("Save Game"))
			{
				PlayButtonFX();
				showOptions = false;
				UniRPGGlobal.SaveGameState();
			}
			if (GUILayout.Button("Exit Game"))
			{
				PlayButtonFX();
				state = State.Hide;
				UniRPGGlobal.Instance.LoadGameToMenu();
			}
		}
		GUILayout.EndArea();
	}

	#endregion
	// ================================================================================================================
	#region status panels (attribute bars)

	private void DrawStatusPanels()
	{
		if (gui.showPlayerStatus)
		{
			DrawStatus(r[9], r[10], gui.showPlayerPortrait, gui.showPlayerLevel, gui.playerStatusShowValue, gui.playerStatusBars, 
				null, UniRPGGlobal.Player.Actor.portrait[0], UniRPGGlobal.Player.Actor.ActorClass.Level);
		}

		if (UniRPGGlobal.Player.TargetedCharacter != null)
		{
			DefaultGameGUIData.TargetStatusShow d = null;
			if (UniRPGGlobal.Player.TargetedCharacter.Actor.ActorType == UniRPGGlobal.ActorType.Hostile) d = gui.targetStatus[0];
			else if (UniRPGGlobal.Player.TargetedCharacter.Actor.ActorType == UniRPGGlobal.ActorType.Neutral) d = gui.targetStatus[1];
			else if (UniRPGGlobal.Player.TargetedCharacter.Actor.ActorType == UniRPGGlobal.ActorType.Friendly) d = gui.targetStatus[2];

			if (d == null) return;
			if (d.show == false) return;

			if (lastTargetedActor != UniRPGGlobal.Player.TargetedCharacter.Actor)
			{	// init the targetStatusBars var
				lastTargetedActor = UniRPGGlobal.Player.TargetedCharacter.Actor;
				for (int i = 0; i < gui.targetStatusBars.Count; i++)
				{
					gui.targetStatusBars[i].attrib = lastTargetedActor.ActorClass.GetAttribute(gui.targetStatusBars[i].attribId);
				}
			}

			DrawStatus(r[11], r[12], d.img, d.level, gui.targetStatusShowValue, d.bars ? gui.targetStatusBars : null,
				d.name ? UniRPGGlobal.Player.TargetedCharacter.Actor.screenName : null,
				UniRPGGlobal.Player.TargetedCharacter.Actor.portrait[0], UniRPGGlobal.Player.TargetedCharacter.Actor.ActorClass.Level);
		}
		else
		{
			lastTargetedActor = null;
			if (gui.targetStatus[3].show && UniRPGGlobal.Player.TargetedItem != null)
			{
				DrawStatus(r[11], r[12], gui.targetStatus[3].img, false, 0, null,
					gui.targetStatus[3].name ? UniRPGGlobal.Player.TargetedItem.screenName : null,
					UniRPGGlobal.Player.TargetedItem.icon[0], 0);
			}
			else if (gui.targetStatus[4].show && UniRPGGlobal.Player.TargetedObject != null)
			{
				DrawStatus(r[11], r[12], gui.targetStatus[3].img, false, 0, null,
					gui.targetStatus[3].name ? UniRPGGlobal.Player.TargetedObject.screenName : null,
					UniRPGGlobal.Player.TargetedObject.icon[0], 0);
			}
		}
	}

	private void DrawStatus(Rect offset, Rect portraitRect, bool showPortrait, bool showLevel, int num, List<DefaultGameGUIData_StatusBar> bars, string name, Texture2D icon, int level)
	{
		Rect fr = new Rect(offset.x, offset.y, gui.statusBarWidth, gui.StatusFrame.padding.top + gui.StatusFrame.padding.bottom);
		if (bars != null)
		{	
			int barCount = 0; // i need to count how many valid attribs where send
			for (int i = 0; i < bars.Count; i++) barCount += (bars[i].attrib != null ? 1 : 0);
			if (barCount > 0) fr.height += (gui.statusBarHeight * barCount) + (5 * (barCount - 1));
		}

		Rect rect = fr;

		if (name != null)
		{
			GUI.Box(fr, name, gui.StatusFrame);
			rect.y += gui.StatusFrame.padding.top;
			rect.x = rect.x + gui.StatusFrame.padding.left;
			rect.width = fr.width - gui.StatusFrame.padding.left - gui.StatusFrame.padding.right;
			rect.height = gui.statusBarHeight;
		}
		else if (bars != null)
		{
			fr.height += gui.StatusFrame.contentOffset.y;
			GUI.Box(fr, GUIContent.none, gui.StatusFrame);
			rect.y += gui.StatusFrame.padding.top;
			rect.y += gui.StatusFrame.contentOffset.y;
			rect.x = rect.x + gui.StatusFrame.padding.left;
			rect.width = fr.width - gui.StatusFrame.padding.left - gui.StatusFrame.padding.right;
			rect.height = gui.statusBarHeight;
		}

		if (bars != null)
		{
			for (int i = 0; i < bars.Count; i++)
			{
				if (bars[i].attrib == null) continue;

				GUI.DrawTexture(rect, gui.statusBarBack == null ? gui.txNoIcon : gui.statusBarBack);
				float val = (bars[i].attrib.Value / bars[i].attrib.MaxValue);
				rect.width *= val;
				GUI.DrawTexture(rect, bars[i].texture == null ? gui.txNoIcon : bars[i].texture);
				rect.width = fr.width - gui.StatusFrame.padding.left - gui.StatusFrame.padding.right;
					
				if (num == 1) GUI.Label(rect, string.Format("{0:N0}/{1:N0}", bars[i].attrib.Value, bars[i].attrib.MaxValue), gui.StatusBarText);
				else if (num == 2)
				{
					val = (val * 100); // make into percentage
					GUI.Label(rect, string.Format("{0:N0}%", val), gui.StatusBarText);
				}
				rect.y += gui.statusBarHeight + 5;
			}
		}

		if (showPortrait)
		{
			rect = new Rect(portraitRect.x + 3, portraitRect.y + 3, portraitRect.width - 6, portraitRect.height - 6);
			GUI.DrawTexture(rect, icon == null ? gui.txNoIcon : icon);
			GUI.Box(portraitRect, GUIContent.none, gui.StatusPortraitFrame);

			if (showLevel)
			{
				rect.x += 4; rect.y += 4; rect.width -= 8; rect.height -= 8;
				GUI.Label(rect, level.ToString(), gui.StatusLevelLabel);
			}
		}
	}

	#endregion
	// ================================================================================================================
	#region action bars

	private void DrawActionBar()
	{
		if (actionSlotCount == 0) return;

		Rect rFrame = new Rect(0, 0, gui.actionIconWidth, gui.actionIconWidth);
		Rect rIcon = new Rect(3, 3, rFrame.width - 6, rFrame.height - 6); // assumes the frame will have rounded border so the actual rectangular icon must be a bit smaller to fit nicely

		if (gui.actionSlotsCount > 0)
		{
			GUI.BeginGroup(r[0]);
			for (int i = 0; i < gui.actionSlotsCount; i++)
			{
				// draw the background frame type if there is no action slotted
				// else draw the action's icon and then a frame over it
				//RPGSkill s = UniRPGGlobal.Player.Actor.GetSkillFromSlot(i);
				//DrawSkillIcon(rFrame, rIcon, s, i);

				ActionSlot slot = UniRPGGlobal.Player.Actor.GetActionSlot(i);
				DrawActionSlot(rFrame, rIcon, slot, i);

				rFrame.x += rFrame.width + 1f;
				rIcon.x += rFrame.width + 1f;
			}
			GUI.EndGroup();
		}
	}

	private void DrawActionSlot(Rect rFrame, Rect rIcon, ActionSlot s, int slot)
	{
		if (s == null)
		{
			GUI.Box(rFrame, GUIContent.none, gui.ActionIconEmpty);
			return;
		}

		if (s.IsEmpty)
		{
			GUI.Box(rFrame, GUIContent.none, gui.ActionIconEmpty);
			return;
		}

		// icon
		if (GUI.Button(rIcon, s.GetIcon(0, gui.txNoIcon), gui.ActionIconButton))
		{
			// right mouse button is used to remove something from the slot
			if (Input.GetMouseButtonUp(1))
			{
				UniRPGGlobal.Player.Actor.ClearActionSlot(slot);
			}
			else
			{
				UniRPGGlobal.Player.Actor.UseActionSlot(slot, (UniRPGGlobal.Player.TargetInteract == null ? null : UniRPGGlobal.Player.TargetInteract.gameObject));
			}
		}

		// Cool down GFX
		if (s.IsSkill)
		{
			if (s.Skill.cooldownTimer > 0.0f && gui.txCooldown != null)
			{
				if (gui.showSkillCooldown)
				{
					int idx = Mathf.RoundToInt(gui.txCooldown.Count / s.Skill.cooldownTimeSetting * s.Skill.cooldownTimer) - 1;
					if (idx >= 0) GUI.DrawTexture(rIcon, gui.txCooldown[idx]);
					// fixme, add the cooldown number too? GUI.Label(rIcon, s.cooldownTimer.ToString("F1"));
				}
			}

			// invalid/out-of-range skill
			else if (gui.showWhenCantUseSkill)
			{
				if (!UniRPGGlobal.Player.IsSkillTargetInRange(s.Skill)) GUI.DrawTexture(rIcon, gui.txInvalid);
			}

			// Queue GFX
			if (gui.showSkillQueued && s.Skill == UniRPGGlobal.Player.Actor.nextSkill && queuedSkillShow)
			{
				GUI.DrawTexture(rIcon, gui.txQueued);
			}

		}

		// Frame
		GUI.Box(rFrame, GUIContent.none, gui.ActionIconFrame);
	}

	//private void DrawSkillIcon(Rect rFrame, Rect rIcon, RPGSkill s, int slot)
	//{
	//	if (s == null)
	//	{
	//		GUI.Box(rFrame, GUIContent.none, gui.ActionIconEmpty);
	//	}
	//	else
	//	{
	//		// Skill icon
	//		if (GUI.Button(rIcon, s.icon[0] == null ? gui.txNoIcon : s.icon[0], gui.ActionIconButton))
	//		{
	//			// right mouse button is used to remove something from the slot
	//			if (Input.GetMouseButtonUp(1))
	//			{
	//				UniRPGGlobal.Player.Actor.ClearActionSlot(slot);
	//			}

	//			// left mouse button is used to perform skill
	//			else if (Input.GetMouseButtonUp(0))
	//			{
	//				UniRPGGlobal.Player.Actor.UseSkill(s, (UniRPGGlobal.Player.TargetInteract == null ? null : UniRPGGlobal.Player.TargetInteract.gameObject), false);
	//			}
	//		}

	//		// Cooldown GFX
	//		if (s.cooldownTimer > 0.0f && gui.txCooldown != null)
	//		{
	//			if (gui.showSkillCooldown)
	//			{
	//				int idx = Mathf.RoundToInt(gui.txCooldown.Count / s.cooldownTimeSetting * s.cooldownTimer) - 1;
	//				if (idx >= 0) GUI.DrawTexture(rIcon, gui.txCooldown[idx]);
	//				// fixme, add the cooldown number too? GUI.Label(rIcon, s.cooldownTimer.ToString("F1"));
	//			}
	//		}

	//		// invalid/out-of-range skill
	//		else if (gui.showWhenCantUseSkill)
	//		{
	//			if (!UniRPGGlobal.Player.IsSkillTargetInRange(s)) GUI.DrawTexture(rIcon, gui.txInvalid);
	//		}

	//		// Queue GFX
	//		if (gui.showSkillQueued && s == UniRPGGlobal.Player.Actor.nextSkill && queuedSkillShow)
	//		{
	//			GUI.DrawTexture(rIcon, gui.txQueued);
	//		}

	//		// Frame
	//		GUI.Box(rFrame, GUIContent.none, gui.ActionIconFrame);
	//	}
	//}

	#endregion
	// ================================================================================================================
	#region menu bar

	private void DrawMenuBar()
	{
		if (menuBarItems > 0)
		{
			Rect rFrame = new Rect(0, 0, gui.menuIconWidth, gui.menuIconWidth);
			Rect rIcon = new Rect(3, 3, rFrame.width - 6, rFrame.height - 6);
			GUI.BeginGroup(r[3]);
			{
				for (int i = 0; i < gui.menuOptions.Count; i++)
				{
					if (false == gui.menuOptions[i].active) continue;
					if (GUI.Button(rIcon, menuOptionsContent[i], gui.MenuBarButton))
					{
						PlayButtonFX();
						switch (gui.menuOptions[i].showWhat)
						{
							case DefaultGameGUIData_MenuOption.MenuOption.Bag: ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.Bag, true, true); break;
							case DefaultGameGUIData_MenuOption.MenuOption.Skills: ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.Skills, true, true); break;
							case DefaultGameGUIData_MenuOption.MenuOption.Character: ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.Character, true, true); break;
							case DefaultGameGUIData_MenuOption.MenuOption.Options: ShowCenterPanel(DefaultGameGUIData_MenuOption.MenuOption.Options, true, true); break;
							case DefaultGameGUIData_MenuOption.MenuOption.Journal:
							{
								if (UniRPGGlobal.Instance.QuestListProvider != null) // not supposed to be null, but just in case
								{
									ShowJournal(UniRPGGlobal.Instance.QuestListProvider.QuestList());
								}
							} break;
						}
					}
					GUI.Box(rFrame, GUIContent.none, gui.MenuIconFrame);
					rFrame.x += rFrame.width + 1;
					rIcon.x += rFrame.width + 1;
				}
			}
			GUI.EndGroup();

			GUI.Label(r[4], GUI.tooltip, gui.MenuTooltip);
		}
	}

	#endregion
	// ================================================================================================================
	#region Bag panel

	private void DrawBag()
	{
		if (!gui.plrMoveBag) UniRPGGlobal.GUIConsumedInput = true;
		if (ShowRightPopMenu >= 0) GUI.enabled = false;
		GUI.BeginGroup(r[1], gui.bagPanelName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[1].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				showBag = false;
			}

			Rect rect = new Rect(GUI.skin.window.padding.left, GUI.skin.window.padding.top,
								r[1].width - GUI.skin.window.padding.left - GUI.skin.window.padding.right,
								r[1].height - GUI.skin.window.padding.top - GUI.skin.window.padding.bottom - gui.BagCurrencyLabel.fixedHeight - gui.BagCurrencyLabel.margin.top - gui.BagCurrencyLabel.margin.bottom);

			Rect rButton = new Rect(0, 0, gui.ListButton.fixedHeight, gui.ListButton.fixedHeight);

			int countOver = (int)Mathf.Floor((r[1].width - GUI.skin.verticalScrollbar.fixedWidth) / gui.bagIconWidth);
			r[5] = new Rect(0, 0, r[1].width - GUI.skin.verticalScrollbar.fixedWidth, gui.bagIconWidth * (UniRPGGlobal.Player.Actor.bagSize / countOver));

			scroll[2] = GUI.BeginScrollView(rect, scroll[2], r[5]);
			{
				for (int i = 0; i < UniRPGGlobal.Player.Actor.bagSize; i++)
				{
					if (i < UniRPGGlobal.Player.Actor.bag.Count)
					{
						if (UniRPGGlobal.Player.Actor.bag[i] != null && UniRPGGlobal.Player.Actor.bag[i].item != null)
						{
							if (GUI.Button(rButton, new GUIContent(UniRPGGlobal.Player.Actor.bag[i].item.icon[0] == null ? gui.txNoIcon : UniRPGGlobal.Player.Actor.bag[i].item.icon[0], i.ToString()), gui.ListButton))
							{
								PlayButtonFX();
								ShowRightPopMenu = i;
								//rPopMenu = new Rect(r[1].x + rButton.x + rect.x, r[1].y + rButton.y + rect.y - scroll[2].y, 200, 135);
							}
							if (GUI.tooltip == i.ToString() && ShowRightPopMenu == -1) SetHoverItem(UniRPGGlobal.Player.Actor.bag[i].item, "Value: " + UniRPGGlobal.Player.Actor.bag[i].item.price, 2);
							if (UniRPGGlobal.Player.Actor.bag[i].stack > 1) GUI.Label(rButton, UniRPGGlobal.Player.Actor.bag[i].stack.ToString(), gui.BagStackLabel);
						}
						else GUI.Button(rButton, GUIContent.none, gui.ListButton);
					}
					else GUI.Button(rButton, GUIContent.none, gui.ListButton);

					rButton.x += gui.bagIconWidth;
					if ((rButton.x + gui.bagIconWidth) > r[5].width)
					{
						rButton.x = 0;
						rButton.y += gui.bagIconWidth;
					}
				}
			}
			GUI.EndScrollView();

			rect.y += rect.height + gui.BagCurrencyLabel.margin.top;
			rect.height = gui.BagCurrencyLabel.fixedHeight;
			GUI.Box(rect, UniRPGGlobal.DB.currency + ": " + UniRPGGlobal.Player.Actor.currency, gui.BagCurrencyLabel);
		}
		GUI.EndGroup();
		if (!showOptions) GUI.enabled = true;

		int cnt = 4;
		if (popMenuHelperItem != null)
		{
			cnt = popMenuHelperStrings.Length + 1;
		}
		else if (popMenuHelperItem2 != null)
		{
			cnt = gui.actionSlotsCount + 1;
		}

		if (ShowRightPopMenu >= 0)
		{
			GUILayout.BeginArea(rPopMenu, GUI.skin.box);
			{
				if (popMenuHelperItem != null)
				{
					for (int i = 0; i < popMenuHelperStrings.Length; i++)
					{
						if (GUILayout.Button(popMenuHelperStrings[i]))
						{
							PlayButtonFX();
							int slot = UniRPGGlobal.DB.equipSlots.IndexOf(popMenuHelperStrings[i]);
							if (slot >= 0)
							{	// check if equip slot is a valid target
								if (UniRPGGlobal.Player.Actor.CanEquip(slot, popMenuHelperItem))
								{
									bool failed = false;

									// remove item from bag
									UniRPGGlobal.Player.Actor.RemoveFromBag(ShowRightPopMenu, 1);

									// get the old item that will be removed from equip slot (if any)
									RPGItem oldItem = UniRPGGlobal.Player.Actor.GetEquippedItem(slot);
									if (oldItem != null)
									{	// and try to place the old item in the bag
										if (!UniRPGGlobal.Player.Actor.AddToBag(oldItem, 1))
										{
											failed = true;
										}
									}

									if (!failed)
									{	// so far so good, now equip the item (which will also cause the old equipped item to be actually unequipped)
										if (!UniRPGGlobal.Player.Actor.Equip(slot, popMenuHelperItem))
										{
											failed = true;
										}
									}

									if (failed)
									{	// put the item back in bag (equip failed)
										UniRPGGlobal.Player.Actor.AddToBag(popMenuHelperItem, 1);
									}

									popMenuHelperItem = null;
									ShowRightPopMenu = -1;
									GUIUtility.ExitGUI();
									return;
								}
							}
						}
					}
					if (GUILayout.Button("Cancel"))
					{
						PlayButtonFX();
						popMenuHelperItem = null;
						ShowRightPopMenu = -1;
					}
				}

				else if (popMenuHelperItem2 != null)
				{
					scroll[3] = GUILayout.BeginScrollView(scroll[3]);
					{
						if (GUILayout.Button("Cancel"))
						{
							PlayButtonFX();
							popMenuHelperItem2 = null;
							ShowRightPopMenu = -1;
						}
						for (int i = 0; i < gui.actionSlotsCount; i++)
						{
							if (GUILayout.Button("Slot " + (i + 1)))
							{
								PlayButtonFX();
								UniRPGGlobal.Player.Actor.SetActionSlot(i, popMenuHelperItem2);
								popMenuHelperItem2 = null;
								ShowRightPopMenu = -1;
							}
						}
					}
					GUILayout.EndScrollView();
				}

				else
				{
					if (GUILayout.Button("Use"))
					{	// get the item
						PlayButtonFX();
						RPGItem item = UniRPGGlobal.Player.Actor.GetBagItem(ShowRightPopMenu);
						if (item != null)
						{
							if (item.equipWhenUseFromBag)
							{	// can be equipped by player?
								if ((item.equipTargetMask & UniRPGGlobal.Target.Player) != 0 && item.validEquipSlots.Count > 0)
								{
									popRectCalced = false;
									popMenuHelperItem = item;
									popMenuHelperStrings = new string[item.validEquipSlots.Count];
									for (int i = 0; i < item.validEquipSlots.Count; i++)
									{
										popMenuHelperStrings[i] = UniRPGGlobal.DB.equipSlots[item.validEquipSlots[i]];
									}
								}
							}
							else
							{
								UniRPGGlobal.Player.Actor.UseBagItem(ShowRightPopMenu);
								ShowRightPopMenu = -1;
							}
						}
					}
					if (GUILayout.Button("Slot"))
					{
						PlayButtonFX();
						RPGItem item = UniRPGGlobal.Player.Actor.GetBagItem(ShowRightPopMenu);
						if (item != null)
						{
							//if (false == item.equipWhenUseFromBag)
							//{	// items that can't be equipped but are used from bag can be placed onto slot
								popRectCalced = false;
								popMenuHelperItem2 = item;
							//}
						}
					}
					if (GUILayout.Button("Destroy"))
					{
						PlayButtonFX();
						UniRPGGlobal.Player.Actor.RemoveFromBag(ShowRightPopMenu, 1);
						ShowRightPopMenu = -1;
					}
					if (GUILayout.Button("Cancel"))
					{
						PlayButtonFX();
						ShowRightPopMenu = -1;
					}
				}
			}
			GUILayout.EndArea();
		}
		CalcPopMenuRect(cnt, ShowRightPopMenu, popMenuHelperItem != null || popMenuHelperItem2 != null);
		if (hoverFromPanel == 2) DrawHoverItemInfo();
	}

	private void SetHoverItem(RPGItem item, string price, int fromPanel)
	{
		hoverFromPanel = fromPanel;
		if (item == null)
		{
			if (Event.current.type == EventType.repaint && !hoverDetected)
			{
				hoverItem = null;
				hoverItemPrice = null;
			}
		}
		else
		{
			if (hoverItem == null) hoverSkip = true;
			hoverDetected = true;
			hoverItem = item;
			hoverItemPrice = price;
			GUI.tooltip = "";
		}
	}

	private void DrawHoverItemInfo()
	{
		if (hoverSkip)
		{
			hoverSkip = false;
			return;
		}

		if (hoverItem == null) return;
		UniRPGGlobal.GUIConsumedInput = true;
		GUILayout.BeginArea(r[8], GUIContent.none, GUI.skin.box);
		
		GUILayout.Label(hoverItem.screenName);
		GUILayout.Space(10);
		scroll[6] = GUILayout.BeginScrollView(scroll[6]);
		{
			GUILayout.Label(hoverItem.description);
		}
		GUILayout.EndScrollView();
		GUILayout.Space(10);
		if (!string.IsNullOrEmpty(hoverItemPrice)) GUILayout.Label(hoverItemPrice + " " + UniRPGGlobal.DB.currency);
		GUILayout.EndArea();
		SetHoverItem(null, "", hoverFromPanel);
	}

	#endregion
	// ================================================================================================================
	#region character sheet

	private void DrawCharacterSheet()
	{
		if (!gui.plrMoveCharSheet) UniRPGGlobal.GUIConsumedInput = true;
		if (ShowLeftPopMenu >= 0) GUI.enabled = false;
		GUILayout.BeginArea(r[2], gui.charaPanelName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[2].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				showCharacterSheet = false;
			}

			scroll[0] = GUILayout.BeginScrollView(scroll[0], GUILayout.Height(r[2].height / 2));
			{
				for (int i = 0; i < UniRPGGlobal.DB.equipSlots.Count; i++)
				{
					RPGItem item = UniRPGGlobal.Player.Actor.GetEquippedItem(i);

					if (item == null)
					{
						GUILayout.Button(new GUIContent(" " + UniRPGGlobal.DB.equipSlots[i], gui.txEmptySlot), gui.ListButton);
					}
					else
					{
						if (GUILayout.Button(new GUIContent(" " + UniRPGGlobal.DB.equipSlots[i], item.icon[0] == null ? gui.txNoIcon : item.icon[0], i.ToString()), gui.ListButton))
						{
							PlayButtonFX();
							ShowLeftPopMenu = i;
						}
						if (GUI.tooltip == i.ToString() && ShowLeftPopMenu == -1) SetHoverItem(item, "Value: " + item.price, 1);
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Box(GUIContent.none, gui.HorizontalLine);
			scroll[1] = GUILayout.BeginScrollView(scroll[1]);
			{
				if (gui.charaPanelShowLevel)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Level", gui.CharaSheetLabel1, GUILayout.Width(r[2].width / 2));
					GUILayout.Label(UniRPGGlobal.Player.ActorClass.Level.ToString(), gui.CharaSheetLabel2);
					GUILayout.EndHorizontal();
				}

 				for (int i = 0; i < UniRPGGlobal.Player.ActorClass.Attributes.Count; i++)
				{
					if (!string.IsNullOrEmpty(UniRPGGlobal.Player.ActorClass.Attributes[i].guiHelper)) continue;
					GUILayout.BeginHorizontal();
					{
						GUILayout.Label(UniRPGGlobal.Player.ActorClass.Attributes[i].screenName, gui.CharaSheetLabel1, GUILayout.Width(r[2].width / 2));
						GUILayout.Label(string.Format("{0}", (int)UniRPGGlobal.Player.ActorClass.Attributes[i].Value), gui.CharaSheetLabel2);
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();
		if (!showOptions) GUI.enabled = true;

		if (ShowLeftPopMenu >= 0 && popRectCalced)
		{
			GUILayout.BeginArea(rPopMenu, GUI.skin.box);
			{
				if (GUILayout.Button("Remove"))
				{	// get the item
					PlayButtonFX();
					RPGItem item = UniRPGGlobal.Player.Actor.GetEquippedItem(ShowLeftPopMenu);
					if (item != null)
					{	// now try to add it to the bag
						if (UniRPGGlobal.Player.Actor.AddToBag(item, 1))
						{	// finally, remove from slot
							UniRPGGlobal.Player.Actor.UnEquip(ShowLeftPopMenu);
						}
					}
					ShowLeftPopMenu = -1;
				}
				if (GUILayout.Button("Destroy"))
				{
					PlayButtonFX();
					UniRPGGlobal.Player.Actor.UnEquip(ShowLeftPopMenu);
					ShowLeftPopMenu = -1;
				}
				if (GUILayout.Button("Cancel"))
				{
					PlayButtonFX();
					ShowLeftPopMenu = -1;
				}
			}
			GUILayout.EndArea();
		}
		CalcPopMenuRect(3, ShowLeftPopMenu, false);
		if (hoverFromPanel == 1) DrawHoverItemInfo();
	}

	#endregion
	// ================================================================================================================
	#region Skills panel

	private void DrawSkills()
	{
		if (!gui.plrMoveSkills) UniRPGGlobal.GUIConsumedInput = true;
		if (ShowRightPopMenu >= 0) GUI.enabled = false;
		GUILayout.BeginArea(r[1], gui.skillPanelName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[1].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				showSkills = false;
			}

			scroll[2] = GUILayout.BeginScrollView(scroll[2]);
			{
				for (int i = 0; i < UniRPGGlobal.Player.Actor.skills.Count; i++)
				{
					if (!string.IsNullOrEmpty(UniRPGGlobal.Player.Actor.skills[i].guiHelper)) continue;
					if (GUILayout.Button(new GUIContent(" " + UniRPGGlobal.Player.Actor.skills[i].screenName, UniRPGGlobal.Player.Actor.skills[i].icon[0] == null ? gui.txNoIcon : UniRPGGlobal.Player.Actor.skills[i].icon[0], i.ToString()), gui.ListButton))
					{
						PlayButtonFX();
						ShowRightPopMenu = i;
					}
					if (GUI.tooltip == i.ToString() && GUI.enabled) SetHoverSkill(UniRPGGlobal.Player.Actor.skills[i]);
				}
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();
		if (!showOptions) GUI.enabled = true;

		int cnt = 2;
		if (popMenuHelperSkill != null)
		{
			cnt = gui.actionSlotsCount + 1;
		}

		if (ShowRightPopMenu >= 0 && popRectCalced)
		{
			GUILayout.BeginArea(rPopMenu, GUI.skin.box);
			{
				if (popMenuHelperSkill != null)
				{
					scroll[3] = GUILayout.BeginScrollView(scroll[3]);
					{
						if (GUILayout.Button("Cancel"))
						{
							PlayButtonFX();
							popMenuHelperSkill = null;
							ShowRightPopMenu = -1;
						}
						for (int i = 0; i < gui.actionSlotsCount; i++)
						{							
							if (GUILayout.Button("Slot " + (i + 1)))
							{
								PlayButtonFX();
								UniRPGGlobal.Player.Actor.SetActionSlot(i, popMenuHelperSkill);
								popMenuHelperSkill = null;
								ShowRightPopMenu = -1;
							}
						}
					}
					GUILayout.EndScrollView();
				}
				else
				{
					if (GUILayout.Button("Equip"))
					{
						PlayButtonFX();
						popRectCalced = false;
						scroll[3] = Vector2.zero;
						popMenuHelperSkill = UniRPGGlobal.Player.Actor.skills[ShowRightPopMenu];
					}
					if (GUILayout.Button("Cancel"))
					{
						PlayButtonFX();
						popMenuHelperSkill = null;
						ShowRightPopMenu = -1;
					}
				}
			}
			GUILayout.EndArea();
		}
		CalcPopMenuRect(cnt, ShowRightPopMenu, popMenuHelperSkill!=null);
		DrawHoverSkillInfo();
	}

	private void SetHoverSkill(RPGSkill skill)
	{
		if (skill == null)
		{
			if (Event.current.type == EventType.repaint && !hoverDetected)
			{
				hoverSkill = null;
			}
		}
		else
		{
			if (hoverSkill == null) hoverSkip = true;
			hoverDetected = true;
			hoverSkill = skill;
			GUI.tooltip = "";
		}
	}

	private void DrawHoverSkillInfo()
	{
		if (hoverSkip)
		{
			hoverSkip = false;
			return;
		}

		if (hoverSkill == null) return;
		UniRPGGlobal.GUIConsumedInput = true;
		GUILayout.BeginArea(r[8], GUIContent.none, GUI.skin.box);

		GUILayout.Label(hoverSkill.screenName);
		GUILayout.Space(10);
		scroll[6] = GUILayout.BeginScrollView(scroll[6]);
		{
			GUILayout.Label(hoverSkill.description);
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		SetHoverSkill(null);
	}

	#endregion
	// ================================================================================================================
	#region Dialogue & Quest (and journal/quest log)

	private void DrawDialogue()
	{
		if (!gui.plrMoveDialogue) if (dialogueData == null) return;
		UniRPGGlobal.GUIConsumedInput = true;
		GUILayout.BeginArea(r[2], string.IsNullOrEmpty(dialogueData.screenName) ? " " : dialogueData.screenName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[2].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				if (dialogueData.buttonCallback != null) dialogueData.buttonCallback(-1, dialogueData);
				dialogueData = null;
				showDialogue = false;
				GUIUtility.ExitGUI();
				return;
			}

			float height = r[2].height - GUI.skin.window.padding.top - GUI.skin.window.padding.bottom;
			float h = height;
			if (dialogueData.showRewards) h = (height * 0.4f);
			else h = (height * 0.7f);
			height -= h;

			// *** SHOW THE CONVERSATION TEXT

			if (dialogueData.conversationText != null)
			{
				scroll[0] = GUILayout.BeginScrollView(scroll[0], GUILayout.Height(h));
				{
					GUILayout.Label(dialogueData.conversationText);
				}
				GUILayout.EndScrollView();
			}

			// *** SHOW THE REWARDS

			if (dialogueData.showRewards)
			{
				GUILayout.Box(GUIContent.none, gui.HorizontalLine);
				height -= gui.HorizontalLine.fixedHeight;
				h = (height * 0.5f);
				height -= h;

				string currency = null;
				if (dialogueData.currencyReward > 0f) currency = string.Format("{0}: +{1}", UniRPGGlobal.DB.currency, dialogueData.currencyReward);

				scroll[5] = GUILayout.BeginScrollView(scroll[5], GUILayout.Height(h));
				{
					GUILayout.Label("Rewards", gui.RewardsLabel);

					// attribute rewards
					if (dialogueData.attributeRewards != null)
					{
						for (int i = 0; i < dialogueData.attributeRewards.Count; i++)
						{
							if (dialogueData.attributeRewards[i].attrib == null) continue;
							GUILayout.Label(string.Format("{0}: +{1}", dialogueData.attributeRewards[i].attrib.screenName, dialogueData.attributeRewards[i].valueAdd));
						}
					}

					// currency reward
					if (dialogueData.currencyReward > 0f)
					{
						GUILayout.Label(currency);
					}

					// item rewards
					if (dialogueData.itemRewards != null)
					{
						for (int i = 0; i < dialogueData.itemRewards.Count; i++)
						{
							GUILayout.BeginHorizontal();
							RPGItem item = dialogueData.itemRewards[i].item;
							if (item != null) GUILayout.Box(item.icon[0] == null ? gui.txNoIcon : item.icon[0], gui.ListButton, GUILayout.Width(gui.ListButton.fixedHeight));
							else GUILayout.Box(GUIContent.none, gui.ListButton, GUILayout.Width(gui.ListButton.fixedHeight));
							GUILayout.Label(string.Format("{0}: {1}", item.screenName, dialogueData.itemRewards[i].count));
							GUILayout.EndHorizontal();
						}
					}
				}
				GUILayout.EndScrollView();
			}

			GUILayout.Box(GUIContent.none, gui.HorizontalLine);
			h = height - gui.HorizontalLine.fixedHeight;

			// *** SHOW THE OPTIONS

			if (dialogueData.options != null)
			{
				scroll[1] = GUILayout.BeginScrollView(scroll[1]);
				{
					for (int i = 0; i < dialogueData.options.Length; i++)
					{
						if (GUILayout.Button(dialogueData.options[i]))
						{
							PlayButtonFX();
							if (dialogueData.buttonCallback != null)
							{
								dialogueData.buttonCallback(i, dialogueData);

								// callback might have messed with dialogueData so rather drop out now
								GUIUtility.ExitGUI();
								return;
							}
						}
					}
				}
				GUILayout.EndScrollView();
			}

		}
		GUILayout.EndArea();
	}

	private void DrawJournal()
	{
		if (!gui.plrMoveJournal) UniRPGGlobal.GUIConsumedInput = true;
		GUILayout.BeginArea(r[6], gui.logPanelName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[6].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				questData = null;
				showJournal = false;
			}

			bool noQuests = true;
			if (questData != null)
			{
				if (questData.Count > 0)
				{
					noQuests = false;
					if (selectedQuest >= questData.Count) selectedQuest = 0;

					GUILayout.BeginHorizontal();
					{
						GUILayout.BeginVertical(GUILayout.Width(150));
						scroll[0] = GUILayout.BeginScrollView(scroll[0], false, true);
						{
							for (int i = 0; i < questData.Count; i++)
							{
								if (ListItem(selectedQuest==i, questData[i].screenName, gui.ListItem)) selectedQuest = i;
							}
						}
						GUILayout.EndScrollView();
						GUILayout.EndVertical();
						GUILayout.Space(5);
						GUILayout.BeginVertical();
						scroll[1] = GUILayout.BeginScrollView(scroll[1]);
						{
							if (questData[selectedQuest].completed)
							{
								if (questData[selectedQuest].mustHandIn) GUILayout.Label(new GUIContent(questData[selectedQuest].screenName, gui.txQuestComplete[1]), gui.QuestLabel);
								else GUILayout.Label(questData[selectedQuest].screenName, gui.QuestLabel);
							}
							else
							{
								GUILayout.Label(new GUIContent(questData[selectedQuest].screenName, gui.txQuestComplete[0]), gui.QuestLabel);
							}
							
							GUILayout.Label(questData[selectedQuest].description);

							if (questData[selectedQuest].showRewards)
							{
								GUILayout.Box(GUIContent.none, gui.HorizontalLine);
								GUILayout.Label("Rewards", gui.QuestLabel);

								string currency = null;
								if (questData[selectedQuest].currencyReward > 0f) currency = string.Format("{0}: +{1}", UniRPGGlobal.DB.currency, questData[selectedQuest].currencyReward);

								// attribute rewards
								if (questData[selectedQuest].attributeRewards != null)
								{
									for (int i = 0; i < questData[selectedQuest].attributeRewards.Count; i++)
									{
										if (questData[selectedQuest].attributeRewards[i].attrib == null) continue;
										GUILayout.Label(string.Format("{0}: +{1}", questData[selectedQuest].attributeRewards[i].attrib.screenName, questData[selectedQuest].attributeRewards[i].valueAdd));
									}
								}

								// currency reward
								if (questData[selectedQuest].currencyReward > 0f)
								{
									GUILayout.Label(currency);
								}

								// item rewards
								if (questData[selectedQuest].itemRewards != null)
								{
									for (int i = 0; i < questData[selectedQuest].itemRewards.Count; i++)
									{
										GUILayout.BeginHorizontal();
										RPGItem item = questData[selectedQuest].itemRewards[i].item;
										if (item != null) GUILayout.Box(item.icon[0] == null ? gui.txNoIcon : item.icon[0], gui.ListButton, GUILayout.Width(gui.ListButton.fixedHeight));
										else GUILayout.Box(GUIContent.none, gui.ListButton, GUILayout.Width(gui.ListButton.fixedHeight));
										GUILayout.Label(string.Format("{0}: {1}", item.screenName, questData[selectedQuest].itemRewards[i].count));
										GUILayout.EndHorizontal();
									}
								}

							}

						}
						GUILayout.EndScrollView();
						GUILayout.EndVertical();
					}
					GUILayout.EndHorizontal();
				}
			}

			if (noQuests)
			{
				GUILayout.Label("You have no active quests");
			}
		}
		GUILayout.EndArea();
	}

	#endregion
	// ================================================================================================================
	#region Shop

	private void DrawShop()
	{
		if (!gui.plrMoveShop) UniRPGGlobal.GUIConsumedInput = true;
		Shop_ShopkeeperBag();
		Shop_PlayerBag();
		DrawHoverItemInfo();
	}

	private void Shop_ShopkeeperBag()
	{
		if (ShowRightPopMenu >= 0 || ShowLeftPopMenu >= 0) GUI.enabled = false;
		GUI.BeginGroup(r[2], shopActor.screenName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[2].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				showShop = false;
				UniRPGGlobal.Player.ClearTarget();
			}

			Rect rect = new Rect(GUI.skin.window.padding.left, GUI.skin.window.padding.top,
											r[1].width - GUI.skin.window.padding.left - GUI.skin.window.padding.right,
											r[1].height - GUI.skin.window.padding.top - GUI.skin.window.padding.bottom - gui.BagCurrencyLabel.fixedHeight - gui.BagCurrencyLabel.margin.top - gui.BagCurrencyLabel.margin.bottom);

			Rect rButton = new Rect(0, 0, gui.ListButton.fixedHeight, gui.ListButton.fixedHeight);

			int countOver = (int)Mathf.Floor((r[1].width - GUI.skin.verticalScrollbar.fixedWidth) / gui.bagIconWidth);
			r[7] = new Rect(0, 0, r[1].width - GUI.skin.verticalScrollbar.fixedWidth, gui.bagIconWidth * (shopActor.bagSize / countOver));

			scroll[0] = GUI.BeginScrollView(rect, scroll[0], r[7]);
			{
				bool fail = true;
				for (int i = 0; i < shopActor.bagSize; i++)
				{
					fail = true;
					if (i < shopActor.bag.Count)
					{
						if (shopActor.bag[i] != null)
						{
							if (shopActor.bag[i].item != null)
							{
								if (shopActor.bag[i].stack > 0)
								{
									fail = false;
									int price = ItemPrice(shopActor.bag[i].item.price, sellMod);
									if (UniRPGGlobal.Player.Actor.currency < price) GUI.enabled = false;
									if (GUI.Button(rButton, new GUIContent(shopActor.bag[i].item.icon[0] == null ? gui.txNoIcon : shopActor.bag[i].item.icon[0], i.ToString()), gui.ListButton))
									{
										PlayButtonFX();
										if (UniRPGGlobal.Player.Actor.BagHasSpaceForItem(shopActor.bag[i].item))
										{
											ShowLeftPopMenu = i;
										}
									}
									if (GUI.tooltip == i.ToString() && ShowRightPopMenu < 0 && ShowLeftPopMenu < 0) SetHoverItem(shopActor.bag[i].item, (price > 0 ? "Buy: " + price.ToString() : ""), 1);
									if (shopActor.bag[i].stack > 1) GUI.Label(rButton, shopActor.bag[i].stack.ToString(), gui.BagStackLabel);
									if (ShowRightPopMenu < 0 && ShowLeftPopMenu < 0) GUI.enabled = true;
								}
							}
						}
					}

					if (fail) GUI.Button(rButton, GUIContent.none, gui.ListButton);

					rButton.x += gui.bagIconWidth;
					if ((rButton.x + gui.bagIconWidth) > r[7].width)
					{
						rButton.x = 0;
						rButton.y += gui.bagIconWidth;
					}
				}
			}
			GUI.EndScrollView();

			if (useShopCurrency)
			{
				rect.y += rect.height + gui.BagCurrencyLabel.margin.top;
				rect.height = gui.BagCurrencyLabel.fixedHeight;
				GUI.Box(rect, UniRPGGlobal.DB.currency + ": " + shopActor.currency, gui.BagCurrencyLabel);
			}
		}
		GUI.EndGroup();
		if (!showOptions) GUI.enabled = true;

		if (ShowLeftPopMenu >= 0)
		{
			GUILayout.BeginArea(rPopMenu, GUI.skin.box);
			{
				if (GUILayout.Button(shopBuyLabel))
				{
					PlayButtonFX();
					int price = ItemPrice(shopActor.bag[ShowLeftPopMenu].item.price, sellMod); // sellMod = what rate shop sells at
					UniRPGGlobal.Player.Actor.currency -= price;
					if (useShopCurrency) shopActor.currency += price;

					RPGItem item = shopActor.bag[ShowLeftPopMenu].item;
					if (!shopUnlimited) shopActor.RemoveFromBag(ShowLeftPopMenu, 1);
					UniRPGGlobal.Player.Actor.AddToBag(item, 1, true);
					
					ShowLeftPopMenu = -1;
				}
				if (GUILayout.Button("Cancel"))
				{
					PlayButtonFX();
					ShowLeftPopMenu = -1;
				}
			}
			GUILayout.EndArea();
		}
		CalcPopMenuRect(2, ShowLeftPopMenu, false);			
	}

	private void Shop_PlayerBag()
	{
		if (ShowRightPopMenu >= 0 || ShowLeftPopMenu >= 0) GUI.enabled = false;
		GUI.BeginGroup(r[1], gui.bagPanelName, GUI.skin.window);
		{
			if (GUI.Button(new Rect(r[1].width - gui.WindowCloseButton.fixedWidth - GUI.skin.window.padding.right,
									GUI.skin.window.padding.top + GUI.skin.window.contentOffset.y,
									gui.WindowCloseButton.fixedWidth, gui.WindowCloseButton.fixedHeight), GUIContent.none, gui.WindowCloseButton))
			{
				PlayButtonFX();
				showShop = false;
				UniRPGGlobal.Player.ClearTarget();
			}

			Rect rect = new Rect(GUI.skin.window.padding.left, GUI.skin.window.padding.top,
											r[1].width - GUI.skin.window.padding.left - GUI.skin.window.padding.right,
											r[1].height - GUI.skin.window.padding.top - GUI.skin.window.padding.bottom - gui.BagCurrencyLabel.fixedHeight - gui.BagCurrencyLabel.margin.top - gui.BagCurrencyLabel.margin.bottom);

			Rect rButton = new Rect(0, 0, gui.ListButton.fixedHeight, gui.ListButton.fixedHeight);

			int countOver = (int)Mathf.Floor((r[1].width - GUI.skin.verticalScrollbar.fixedWidth) / gui.bagIconWidth);
			r[5] = new Rect(0, 0, r[1].width - GUI.skin.verticalScrollbar.fixedWidth, gui.bagIconWidth * (UniRPGGlobal.Player.Actor.bagSize / countOver));

			scroll[2] = GUI.BeginScrollView(rect, scroll[2], r[5]);
			{
				for (int i = 0; i < UniRPGGlobal.Player.Actor.bagSize; i++)
				{
					if (i < UniRPGGlobal.Player.Actor.bag.Count)
					{
						if (UniRPGGlobal.Player.Actor.bag[i] != null && UniRPGGlobal.Player.Actor.bag[i].item != null)
						{
							int price = ItemPrice(UniRPGGlobal.Player.Actor.bag[i].item.price, buyMod);
							if (useShopCurrency)
							{
								if (shopActor.currency < price) GUI.enabled = false;
							}
							if (GUI.Button(rButton, new GUIContent(UniRPGGlobal.Player.Actor.bag[i].item.icon[0] == null ? gui.txNoIcon : UniRPGGlobal.Player.Actor.bag[i].item.icon[0], i.ToString()), gui.ListButton))
							{
								PlayButtonFX();
								ShowRightPopMenu = i;
							}
							if (GUI.tooltip == i.ToString() && ShowRightPopMenu < 0 && ShowLeftPopMenu < 0) SetHoverItem(UniRPGGlobal.Player.Actor.bag[i].item, (price > 0 ? "Sell: " + price.ToString() : ""), 2);
							if (UniRPGGlobal.Player.Actor.bag[i].stack > 1) GUI.Label(rButton, UniRPGGlobal.Player.Actor.bag[i].stack.ToString(), gui.BagStackLabel);
							if (ShowRightPopMenu < 0 && ShowLeftPopMenu < 0) GUI.enabled = true;
						}
						else GUI.Button(rButton, GUIContent.none, gui.ListButton);
					}
					else GUI.Button(rButton, GUIContent.none, gui.ListButton);

					rButton.x += gui.bagIconWidth;
					if ((rButton.x + gui.bagIconWidth) > r[5].width)
					{
						rButton.x = 0;
						rButton.y += gui.bagIconWidth;
					}
				}
			}
			GUI.EndScrollView();

			rect.y += rect.height + gui.BagCurrencyLabel.margin.top;
			rect.height = gui.BagCurrencyLabel.fixedHeight;
			GUI.Box(rect, UniRPGGlobal.DB.currency + ": " + UniRPGGlobal.Player.Actor.currency, gui.BagCurrencyLabel);
		}
		GUI.EndGroup();
		if (!showOptions) GUI.enabled = true;

		if (ShowRightPopMenu >= 0)
		{
			GUILayout.BeginArea(rPopMenu, GUI.skin.box);
			{
				if (GUILayout.Button(shopSellLabel))
				{
					PlayButtonFX();
					int price = ItemPrice(UniRPGGlobal.Player.Actor.bag[ShowRightPopMenu].item.price, buyMod); // buyMod = what rate shop buy at
					UniRPGGlobal.Player.Actor.currency += price;
					if (useShopCurrency) shopActor.currency -= price;

					RPGItem item = UniRPGGlobal.Player.Actor.bag[ShowRightPopMenu].item;
					UniRPGGlobal.Player.Actor.RemoveFromBag(ShowRightPopMenu, 1);
					shopActor.AddToBag(item, 1, true);

					ShowRightPopMenu = -1;
				}
				if (GUILayout.Button("Cancel"))
				{
					PlayButtonFX();
					ShowRightPopMenu = -1;
				}
			}
			GUILayout.EndArea();
		}
		CalcPopMenuRect(2, ShowRightPopMenu, false);
	}

	private int ItemPrice(int price, float mod)
	{
		price = (int)Mathf.Floor(price * mod);
		if (price < 0) price = 0;
		return price;
	}

	#endregion
	// ================================================================================================================
	#region callback/ message handlers

	public void FadeOut()
	{
		fading = -1;
		fadeAlpha = 0f;
	}

	public void FadeIn()
	{
		fading = 1;
		fadeAlpha = 1f;
	}

	/// <summary>
	/// Can be used to ask GUI to open something. What the options are should be explained to the designer and depends on the GUI Theme.
	/// This Theme (Default Fnatasy) does not make use of this function
	/// </summary>
	public void ShowCustom(string param)
	{
		if (param=="menu") menuVisible = true;
		else if (param=="slots") actionsVisible = true;
		else if (param=="status") statusVisible = true;
		Debug.Log("This GUI Theme does not support ShowCustom(" + param + "). You may use 'menu', 'slots', or 'status'");
	}

	public void ShowPlayerInfo()
	{
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.Character, false, true);
	}

	public void ShowBag()
	{
		ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.Bag, false, true);
	}

	public void ShowSkills()
	{
		ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.Skills, false, true);
	}

	public void ShowOptions()
	{
		ShowCenterPanel(DefaultGameGUIData_MenuOption.MenuOption.Options, false, true);
	}

	public void ShowDialogue(GUIDialogueData data)
	{
		if (data == null)
		{
			Debug.LogError("Error: Could not show the Dialogue Panel. The Data parameter was null.");
			return;
		}

		dialogueData = data;
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.Dialogue, false, true);
	}

	public void ShowJournal()
	{
		questData = UniRPGGlobal.Instance.QuestListProvider.QuestList();
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.Journal, false, true);
	}

	public void ShowJournal(List<GUIQuestData> data)
	{
		questData = data;
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.Journal, true, true);
	}

	public void ShowShop(GameObject shopKeeperObj)
	{
		if (shopKeeperObj == null)
		{
			Debug.LogError("Error: Could not show the Shop Interface. The shop keeper object was null.");
			return;
		}

		CharacterBase shopCharacter = shopKeeperObj.GetComponent<CharacterBase>();
		shopActor = shopKeeperObj.GetComponent<Actor>();
		if (shopCharacter == null || shopActor == null)
		{
			Debug.LogError("Error: Could not show the Shop Interface. The object is not a valid character.");
			return;
		}

		// set defaults
		buyMod = UniRPGGlobal.DB.shopGlobalBuyMod;
		sellMod = UniRPGGlobal.DB.shopGlobalSellMod;
		useShopCurrency = UniRPGGlobal.DB.shopGlobalUsesCurrency;
		shopUnlimited = UniRPGGlobal.DB.shopGlobalUnlimited;
		shopBuyLabel = "Buy";
		shopSellLabel = "Sell";

		// now check if character has modifier vars and apply
		float n = 0f;
		string s = shopCharacter.GetCustomVariable("buy_mod");
		if (!string.IsNullOrEmpty(s))
		{
			if (float.TryParse(s, out n)) buyMod = n;
		}
		s = shopCharacter.GetCustomVariable("sell_mod");
		if (!string.IsNullOrEmpty(s))
		{
			if (float.TryParse(s, out n)) sellMod = n;
		}
		s = shopCharacter.GetCustomVariable("shop_use_currency");
		if (!string.IsNullOrEmpty(s))
		{
			if (s == "1") useShopCurrency = true;
			else if (s == "0") useShopCurrency = false;
		}
		s = shopCharacter.GetCustomVariable("shop_unlimited");
		if (!string.IsNullOrEmpty(s))
		{
			if (s == "1") shopUnlimited = true;
			else if (s == "0") shopUnlimited = false;
		}
		s = shopCharacter.GetCustomVariable("shop_buy_label");
		if (!string.IsNullOrEmpty(s)) shopBuyLabel = s;
		s = shopCharacter.GetCustomVariable("shop_sell_label");
		if (!string.IsNullOrEmpty(s)) shopSellLabel = s;

		// show shop
		ShowCenterPanel(DefaultGameGUIData_MenuOption.MenuOption.Shop, false, true);
	}

	// ----------------------------------------------------------------------------------------------------------------

	public void HideCustom(string param)
	{
		if (param == "menu") menuVisible = false;
		else if (param == "slots") actionsVisible = false;
		else if (param == "status") statusVisible = false;
		Debug.Log("This GUI Theme does not support HideCustom(" + param + "). You may use 'menu', 'slots', or 'status'");
	}

	public void HidePlayerInfo()
	{
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
	}

	public void HideBag()
	{
		ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
	}

	public void HideSkills()
	{
		ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
	}

	public void HideOptions()
	{
		ShowCenterPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
	}

	public void HideDialogue()
	{
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
		dialogueData = null;
	}

	public void HideJournal()
	{
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
		questData = null;
	}

	public void HideShop()
	{
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
	}

	#endregion
	// ================================================================================================================
	#region helpers & events

	private void CalcPopMenuRect(int itemCount, int pop, bool dontChangePos)
	{
		if (pop == -1 || popRectCalced) return;
		if (Event.current.type == EventType.Repaint)
		{
			popRectCalced = true;
			if (!dontChangePos)
			{
				rPopMenu.x = Event.current.mousePosition.x;
				rPopMenu.y = Event.current.mousePosition.y;
			}
			rPopMenu.width = 200;
			float h = GUI.skin.button.CalcHeight(new GUIContent("ABC"), 150) + GUI.skin.button.margin.top + GUI.skin.button.margin.bottom;
			rPopMenu.height = GUI.skin.box.padding.top + GUI.skin.box.padding.bottom + (itemCount * h);

			if (rPopMenu.x + rPopMenu.width > gui.width) rPopMenu.x = rPopMenu.x - (rPopMenu.x + rPopMenu.width - gui.width);
			if (rPopMenu.y + rPopMenu.height > gui.height) rPopMenu.y = rPopMenu.y - (rPopMenu.y + rPopMenu.height - gui.height);
		}
	}

	/// <summary>
	/// will be called when UniRPGGlobal broadcasts that it will start or stop loading a game scene.
	/// UniRPG will be showing the Load screen, so do not show this GUI then
	/// </summary>
	public void OnLoadingScene(bool starting)
	{
		if (starting) state = State.Hide;
		else state = State.Running;
	}

	public void ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption option, bool isToggle, bool show)
	{
		ShowLeftPopMenu = -1;
		ShowRightPopMenu = -1;
		scroll[0] = Vector2.zero;
		scroll[1] = Vector2.zero;
		scroll[5] = Vector2.zero;
		scroll[6] = Vector2.zero;
		showCharacterSheet = (option == DefaultGameGUIData_MenuOption.MenuOption.Character ? (isToggle ? !showCharacterSheet : show) : false);
		showDialogue = (option == DefaultGameGUIData_MenuOption.MenuOption.Dialogue ? (isToggle ? !showDialogue : show) : false);
		showJournal = (option == DefaultGameGUIData_MenuOption.MenuOption.Journal ? (isToggle ? !showJournal : show) : false);
	}

	public void ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption option, bool isToggle, bool show)
	{
		ShowLeftPopMenu = -1;
		ShowRightPopMenu = -1;
		scroll[2] = Vector2.zero;
		scroll[3] = Vector2.zero;
		scroll[6] = Vector2.zero;
		showBag = (option == DefaultGameGUIData_MenuOption.MenuOption.Bag ? (isToggle ? !showBag : show) : false);
		showSkills = (option == DefaultGameGUIData_MenuOption.MenuOption.Skills ? (isToggle ? !showSkills : show) : false);
	}

	public void ShowCenterPanel(DefaultGameGUIData_MenuOption.MenuOption option, bool isToggle, bool show)
	{
		ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);
		ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.None, false, false);

		ShowLeftPopMenu = -1;
		ShowRightPopMenu = -1;
		scroll[0] = Vector2.zero;
		scroll[1] = Vector2.zero;
		scroll[2] = Vector2.zero;
		scroll[3] = Vector2.zero;
		scroll[4] = Vector2.zero;
		scroll[5] = Vector2.zero;
		scroll[6] = Vector2.zero;
		showOptions = (option == DefaultGameGUIData_MenuOption.MenuOption.Options ? (isToggle ? !showOptions : show) : false);
		showShop = (option == DefaultGameGUIData_MenuOption.MenuOption.Shop ? (isToggle ? !showShop : show) : false);
	}

	private bool ListItem(bool selected, string content, GUIStyle style)
	{
		if (GUILayout.Toggle(selected, content, style))
		{
			if (selected == false) return true; // was not previously selected
		}
		return false;
	}

	#endregion
	// ================================================================================================================
} }