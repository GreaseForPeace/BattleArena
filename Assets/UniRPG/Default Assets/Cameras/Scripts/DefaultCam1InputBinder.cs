// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[InputBinder(EditorAutoLoad = true)]
public class DefaultCam1InputBinder : InputBinderBase
{
	public override List<InputDefinition> GetInputBinds()
	{
		List<InputDefinition> defs = new List<InputDefinition>()
		{	
			// Camera
			new InputDefinition()
			{
				inputName = "Rotate Camera",
				groupName = "DefaultCam1",
				order = 1, // order is important (default 0), I want this input definition to be checked after action buttons
				triggerOnHeld = true,
				primaryButton = KeyCode.Mouse1,
				callback = OnInput_RotateCam,
				callbackParams = new System.Object[] { 0 }, // check mouse-x
			},

			new InputDefinition()
			{
				inputName = "Rotate Left",
				groupName = "DefaultCam1",
				triggerOnHeld = true,
				primaryButton = KeyCode.Q,
				callback = OnInput_RotateCam,
				callbackParams = new System.Object[] { -1 }, // Rotate left
			},

			new InputDefinition()
			{
				inputName = "Rotate Right",
				groupName = "DefaultCam1",
				triggerOnHeld = true,
				primaryButton = KeyCode.E,
				callback = OnInput_RotateCam,
				callbackParams = new System.Object[] { 1 }, // Rotate right
			},
		};

		return defs;
	}

	// ================================================================================================================

	private static void OnInput_RotateCam(InputManager.InputEvent e, System.Object[] args)
	{
		// if this binder is active then it means that the camera associated 
		// with it is active so I can savely make this call
		((DefaultCam1)UniRPGGlobal.ActiveCamera).OnInput_UpdateRotation((int)args[0]);
	}

	// ================================================================================================================
} }