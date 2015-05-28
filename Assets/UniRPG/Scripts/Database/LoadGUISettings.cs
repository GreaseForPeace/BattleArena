// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class LoadGUISettings : MonoBehaviour
{
	public GUISkin guiSkin;
	public int designW = 1024;
	public int designH = 768;
	public string loadText = "Now Loading ...";
	public Texture2D loadBackground = null;
	public GUIElementTransform trLoadBackground = new GUIElementTransform() { stretch = GUIElementTransform.Stretch.BothAspectRatio, xAlign = GUIElementTransform.XAlign.Center, yAlign = GUIElementTransform.YAlign.Middle };

	// ================================================================================================================
} }