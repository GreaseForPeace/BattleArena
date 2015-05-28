// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class StartupSettings : MonoBehaviour 
{
	public GUISkin guiSkin;
	public int designW = 1024;
	public int designH = 768;

	public bool allowSkip = true;							// allow player to click through
	public float delay = 1f;								// how long to display an image (in seconds)
	public List<Texture2D> images = new List<Texture2D>();	// the images to show
	public GUIElementTransform trImages = new GUIElementTransform() { stretch = GUIElementTransform.Stretch.BothAspectRatio, xAlign = GUIElementTransform.XAlign.Center, yAlign = GUIElementTransform.YAlign.Middle };

	public string loadText = "Now Loading ...";
	public Texture2D loadBackground = null;
	public GUIElementTransform trLoadBackground = new GUIElementTransform() { stretch = GUIElementTransform.Stretch.BothAspectRatio, xAlign = GUIElementTransform.XAlign.Center, yAlign = GUIElementTransform.YAlign.Middle };

	// ================================================================================================================
} }