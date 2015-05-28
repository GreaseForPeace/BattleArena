// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class LoadingGUI : MonoBehaviour 
{
	// vars are not serialised and must be inited at runtime
	public GUISkin skin { get; set; }
	public Texture2D image { get; set; }	// image to display
	public string text { get; set; }		// loading text to display
	public int designW { get; set; }
	public int designH { get; set; }
	public Rect imgRect { get; set; }
	private Rect r2;

	// ================================================================================================================

	IEnumerator Start()
	{
		yield return null; // wait a frame before doing the following
		UniRPGGUI.CalcGUIScale(designW, designH);
	}

	void OnGUI()
	{
		UniRPGGUI.BeginScaledGUI();
		if (image) GUI.DrawTexture(imgRect, image);
		if (!string.IsNullOrEmpty(text))
		{
			if (skin) GUI.skin = skin;
			this.r2 = GUILayoutUtility.GetRect(new GUIContent(text), GUI.skin.label);
			this.r2.x = (designW - this.r2.width) * 0.5f;
			this.r2.y = (designH - this.r2.height) * 0.5f;
			GUI.Label(this.r2, text, GUI.skin.label);
		}
		UniRPGGUI.EndScaledGUI();
	}

	// ================================================================================================================
} }