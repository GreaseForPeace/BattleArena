// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

public class GUIDialogueData
{
	public string screenName = null;		// name of the object/actor being interacted with
	public string description = null;		// description of the object/actor being interacted with
	public Texture2D[] portrait = null;		// portraits of the object/actor being interacted with

	public string conversationText = null;	// text to show
	public string[] options = null;			// options/buttons to show
	public delegate void OnButton(int buttonIdx, GUIDialogueData currData);
	public GUIDialogueData.OnButton buttonCallback = null;	// the function to call with idx of selected button/option (send -1 if there is nothing and you just want the dialogue to end/close)

	// The GUI will only bother with the following if showRewards = true. The vars to follow are normally 
	// used in Quest Dialogues to show what the player will recieve for completing the quest

	public bool showRewards = false;
	public int currencyReward = 0; // gui should only show it if set to bigger than (0)
	public List<AttribReward> attributeRewards = null;
	public List<ItemReward> itemRewards = null;

	public class AttribReward
	{
		public RPGAttribute attrib;			// the attribute that will be increased (XP perhaps)
		public int valueAdd;				// with how much will the attribute alue increase?
	}

	public class ItemReward
	{
		public RPGItem item;				// the item that will be given
		public int count;					// how many copies of the item will be given
	}
}

// ====================================================================================================================

[System.Serializable]
public class GUIElementTransform
{
	public enum Stretch { None = 0, BothAspectRatio, Both, Width, Height }
	public enum XAlign { Left = 0, Right, Center }
	public enum YAlign { Top = 0, Bottom, Middle }

	public Stretch stretch = Stretch.None;
	public XAlign xAlign = XAlign.Left;
	public YAlign yAlign = YAlign.Top;
	public Vector2 offset = Vector2.zero;
	public Vector2 size = Vector2.zero;

	///<summary>calc a rect from given element width/height, gui/design size and GUIElementTransform</summary>
	public static Rect CalcRect(float guiW, float guiH, GUIElementTransform tr)
	{
		float eleW = tr.size.x;
		float eleH = tr.size.y;

		float x = tr.offset.x;
		float y = tr.offset.y;

		if (tr.stretch == Stretch.Width) eleW = guiW;
		if (tr.stretch == Stretch.Height) eleH = guiH;
		if (tr.stretch == Stretch.Both) { eleW = guiW; eleH = guiH; }
		else if (tr.stretch == Stretch.BothAspectRatio)
		{
			float aspect = eleW / eleH;
			if (aspect < (guiW / guiH))
			{	// image is tall
				eleW = (guiH / eleH) * eleW;
				eleH = guiH;
			}
			else
			{	// image is wide
				eleH = (guiW / eleW) * eleH;
				eleW = guiW;
			}
		}

		if (tr.xAlign == XAlign.Right) x = guiW - eleW - tr.offset.x;
		else if (tr.xAlign == XAlign.Center) x = (guiW - eleW) * 0.5f + tr.offset.x;

		if (tr.yAlign == YAlign.Bottom) y = guiH - eleH - tr.offset.y;
		else if (tr.yAlign == YAlign.Middle) y = (guiH - eleH) * 0.5f + tr.offset.y;

		return new Rect(x, y, eleW, eleH);
	}
}

// ====================================================================================================================

public class UniRPGGUI 
{

	private static Matrix4x4 prevGUIMatrix = Matrix4x4.identity;
	private static Matrix4x4 scaledGUIMatrix = Matrix4x4.identity;

	/// <summary>
	/// should be called whenever screen resolution changes (or at least once before starting to use BeginScaledGUI/EndScaledGUI)
	/// width and height is the "design/original" that you designed the gui for. for example 1024x768
	/// </summary>
	public static void CalcGUIScale(float designW, float designH)
	{
		// * this method scales and keeps aspect ratio
		float screenW = (float)Screen.width;
		float screenH = (float)Screen.height;
		float aspect = screenW / screenH;
		float scale = 1f;
		Vector3 offset = Vector3.zero;
		if (aspect < (designW / designH))
		{	// screen is taller
			scale = (screenW / designW);
			offset.y += (screenH - (designH * scale)) * 0.5f;
		}
		else
		{	// screen is wider
			scale = (screenH / designH);
			offset.x += (screenW - (designW * scale)) * 0.5f;
		}
		scaledGUIMatrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale);

		// * this method scale without keeping aspect ratio of design
		//Vector2 resizeRatio = new Vector2((float)Screen.width / designW, (float)Screen.height / designH);
		//scaledGUIMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));

		// * no scaling
		//scaledGUIMatrix = GUI.matrix;
	}

	/// <summary>call this before drawing gui elements that should scale to screne res</summary>
	public static void BeginScaledGUI()
	{
		prevGUIMatrix = GUI.matrix;
		GUI.matrix = scaledGUIMatrix;
	}

	/// <summary>restore gui matrix. call this when you are done drawing your scaled gui</summary>
	public static void EndScaledGUI()
	{
		GUI.matrix = prevGUIMatrix;
	}

	// ================================================================================================================
} }