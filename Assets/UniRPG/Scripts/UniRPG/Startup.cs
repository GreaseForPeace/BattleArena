// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class Startup : MonoBehaviour 
{
	public GameObject settings;
	
	private StartupSettings sgui;
	private enum State { Spash, StartLoading, Loading }
	private State state = State.StartLoading;
	private float timer = 0f;
	private int currImage = 0;
	private Rect r;
	private Rect r2;

	// ================================================================================================================

	void Awake()
	{
		if (settings) sgui = settings.GetComponent<StartupSettings>();
		if (sgui == null)
		{
			Debug.LogError("The startup scene's settings are not set. This problem can be fixed by pressing the 'Update Build Settings' button.");
			enabled = false;
		}
	}

	IEnumerator Start()
	{
		state = State.Spash;
		timer = sgui.delay;
		if (sgui.images.Count == 0) state = State.StartLoading;
		else r = GUIElementTransform.CalcRect(sgui.designW, sgui.designH, sgui.trImages);

		yield return null; // wait a frame before doing following
		UniRPGGUI.CalcGUIScale(sgui.designW, sgui.designH);
	}
	
	void Update() 
	{
		if (state == State.Spash)
		{
			if (sgui.allowSkip && Input.anyKeyDown) timer = 0f;
			timer -= Time.deltaTime;
			if (timer <= 0.0f)
			{
				timer = sgui.delay;
				currImage++;
				if (currImage >= sgui.images.Count) state = State.StartLoading;
			}
		}

		if (state == State.StartLoading)
		{
			state = State.Loading;
			if (sgui.loadBackground) r = GUIElementTransform.CalcRect(sgui.designW, sgui.designH, sgui.trLoadBackground);
			//Application.LoadLevelAsync("unirpg");
			Application.LoadLevel("unirpg");
		}
	}

	void OnGUI()
	{
		UniRPGGUI.BeginScaledGUI();
		if (state == State.Spash)
		{
			GUI.DrawTexture(this.r, sgui.images[currImage]);
		}
		
		else if (state == State.Loading)
		{
			if (sgui.loadBackground)
			{
				GUI.DrawTexture(this.r, sgui.loadBackground);
			}

			if (!string.IsNullOrEmpty(sgui.loadText))
			{
				if (sgui.guiSkin) GUI.skin = sgui.guiSkin;
				this.r2 = GUILayoutUtility.GetRect(new GUIContent(sgui.loadText), GUI.skin.label);
				this.r2.x = (sgui.designW - this.r2.width) * 0.5f;
				this.r2.y = (sgui.designH - this.r2.height) * 0.5f;
				GUI.Label(this.r2, sgui.loadText, GUI.skin.label);
			}
		}
		UniRPGGUI.EndScaledGUI();
	}

	// ================================================================================================================
} }