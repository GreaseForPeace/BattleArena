// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG
{
	[InputBinder(EditorAutoLoad = true)]
	public class Chara2_Player_Input : InputBinderBase
	{
		public override List<InputDefinition> GetInputBinds()
		{
			List<InputDefinition> defs = new List<InputDefinition>()
			{	
				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Click-to-Move",
					triggerOnHeld = true,
					triggerOnSingle = true,
					primaryButton = KeyCode.Mouse0,
					callback = OnInput_ClickMove,
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Select Target",
					triggerOnSingle = true,
					primaryButton = KeyCode.Mouse0,
					callback = OnInput_SelectTarget,
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Interact",
					triggerOnDouble = true,
					primaryButton = KeyCode.Mouse0,
					callback = OnInput_Interact,
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Clear Target",
					triggerOnSingle = true,
					primaryButton = KeyCode.Escape,
					callback = OnInput_ClearTarget,
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Move Forward",
					triggerOnHeld = true,
					primaryButton = KeyCode.W,
					callback = OnInput_AWSDMovement,
					callbackParams = new System.Object[] { 1 }, // forward
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Move Back",
					triggerOnHeld = true,
					primaryButton = KeyCode.S,
					callback = OnInput_AWSDMovement,
					callbackParams = new System.Object[] { 2 }, // backward
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Move Left",
					triggerOnHeld = true,
					primaryButton = KeyCode.A,
					callback = OnInput_AWSDMovement,
					callbackParams = new System.Object[] { 3 }, // left
				},

				new InputDefinition()
				{
					groupName = "Player",
					inputName = "Move Right",
					triggerOnHeld = true,
					primaryButton = KeyCode.D,
					callback = OnInput_AWSDMovement,
					callbackParams = new System.Object[] { 4 }, // right
				},
			};

			// binds for 10 Action Slots
			for (int i = 0; i < 10; i++)
			{
				defs.Add(new InputDefinition()
				{
					groupName = "Player",
					inputName = "Action Slot " + (i + 1).ToString(),
					order = 10,
					triggerOnSingle = true,
					triggerOnDouble = true,
					primaryButton = (i < 9 ? KeyCode.Alpha1 + i : KeyCode.Alpha0),
					callback = OnInput_SkillSlot,
					callbackParams = new System.Object[] { i },
				});
			}

			return defs;
		}

		// ================================================================================================================

		private static void OnInput_AWSDMovement(InputManager.InputEvent e, System.Object[] args)
		{
			if (UniRPGGlobal.GUIConsumedInput) return;
			((Chara2_Player)UniRPGGlobal.Player).OnInput_AWSDMovement((int)args[0]);
		}

		private static void OnInput_ClickMove(InputManager.InputEvent e, System.Object[] args)
		{
			if (UniRPGGlobal.GUIConsumedInput) return;
			((Chara2_Player)UniRPGGlobal.Player).OnInput_ClickMove();
		}

		private static void OnInput_SelectTarget(InputManager.InputEvent e, System.Object[] args)
		{
			if (UniRPGGlobal.GUIConsumedInput) return;
			((Chara2_Player)UniRPGGlobal.Player).OnInput_SelectTarget();
		}

		private static void OnInput_Interact(InputManager.InputEvent e, System.Object[] args)
		{
			if (UniRPGGlobal.GUIConsumedInput) return;
			((Chara2_Player)UniRPGGlobal.Player).OnInput_Interact();
		}

		private static void OnInput_ClearTarget(InputManager.InputEvent e, System.Object[] args)
		{
			if (UniRPGGlobal.GUIConsumedInput) return;
			((Chara2_Player)UniRPGGlobal.Player).OnInput_ClearTarget();
		}

		private static void OnInput_SkillSlot(InputManager.InputEvent e, System.Object[] args)
		{
			if (UniRPGGlobal.GUIConsumedInput) return;
			((Chara2_Player)UniRPGGlobal.Player).OnInput_UseSlot((int)args[0], e.isDouble);
		}

		// ================================================================================================================
	}
}