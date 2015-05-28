// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

// set EditorAutoLoad = false since it will be loaded by editor when needed (when it makes the call to activate this gui theme)
[InputBinder(EditorAutoLoad = false)]
public class DefaultGUIInputBinder : InputBinderBase
{
	public override List<InputDefinition> GetInputBinds()
	{
		List<InputDefinition> defs = new List<InputDefinition>()
		{
			new InputDefinition()
			{
				inputName = "Options",
				groupName = "GUI",
				triggerOnSingle = true,
				primaryButton = KeyCode.O,
				callback = ToggleOptions,
			},
			new InputDefinition()
			{
				inputName = "Character Profile",
				groupName = "GUI",
				triggerOnSingle = true,
				primaryButton = KeyCode.P,
				secondaryButton = KeyCode.C,
				callback = ToggleCharacterSheet,
			},
			new InputDefinition()
			{
				inputName = "Open Bag",
				groupName = "GUI",
				triggerOnSingle = true,
				primaryButton = KeyCode.B,
				secondaryButton = KeyCode.I,
				callback = ToggleBag,
			},
			new InputDefinition()
			{
				inputName = "Skills",
				groupName = "GUI",
				triggerOnSingle = true,
				primaryButton = KeyCode.K,
				callback = ToggleSkillWindow,
			},
			new InputDefinition()
			{
				inputName = "Journal",
				groupName = "GUI",
				triggerOnSingle = true,
				primaryButton = KeyCode.J,
				secondaryButton = KeyCode.L,
				callback = ToggleJournal,
			},
		};

		return defs;
	}

	public void ToggleOptions(InputManager.InputEvent e, System.Object[] args)
	{
		UniRPGGlobal.GameGUIObject.GetComponent<DefaultGameGUI>().ShowCenterPanel(DefaultGameGUIData_MenuOption.MenuOption.Options, true, true);
	}

	public void ToggleCharacterSheet(InputManager.InputEvent e, System.Object[] args)
	{
		UniRPGGlobal.GameGUIObject.GetComponent<DefaultGameGUI>().ShowLeftPanel(DefaultGameGUIData_MenuOption.MenuOption.Character, true, true);
	}

	public void ToggleBag(InputManager.InputEvent e, System.Object[] args)
	{
		UniRPGGlobal.GameGUIObject.GetComponent<DefaultGameGUI>().ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.Bag, true, true);
	}

	public void ToggleSkillWindow(InputManager.InputEvent e, System.Object[] args)
	{
		UniRPGGlobal.GameGUIObject.GetComponent<DefaultGameGUI>().ShowRightPanel(DefaultGameGUIData_MenuOption.MenuOption.Skills, true, true);
	}

	public void ToggleJournal(InputManager.InputEvent e, System.Object[] args)
	{
		if (UniRPGGlobal.Instance.QuestListProvider != null) // not supposed to be null, but just in case
		{
			UniRPGGlobal.GameGUIObject.GetComponent<DefaultGameGUI>().ShowJournal(UniRPGGlobal.Instance.QuestListProvider.QuestList());
		}
	}

	// ================================================================================================================
} }