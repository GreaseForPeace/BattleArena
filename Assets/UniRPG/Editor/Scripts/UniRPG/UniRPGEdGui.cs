// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

public class UniRPGEdGui
{
	// ================================================================================================================
	#region gui style/skin

	public const string EditorResourcePath = "Assets/UniRPG/Editor/Res/";

	public static GUISkin Skin = null;

	// colours
	public static Color DividerColor = new Color(0f, 0f, 0f, 0.25f);
	public static Color InspectorDividerColor = new Color(0.35f, 0.35f, 0.35f, 1f);
	public static Color ToolbarDividerColor = new Color(0f, 0.5f, 0.8f, 1f);
	public static Color ButtonOnColor = new Color(0.24f, 0.5f, 0.87f);

	// text & labels
	public static GUIStyle WarningLabelStyle;			// yellow text (in dark theme, else red)
	public static GUIStyle BoldLabelStyle;				// bold label
	public static GUIStyle RichLabelStyle;				// label that can parse html tags
	public static GUIStyle CenterLabelStyle;			// a centered label
	public static GUIStyle Head1Style;					// big heading style
	public static GUIStyle Head2Style;					// heading style
	public static GUIStyle Head3Style;					// heading style
	public static GUIStyle Head4Style;					// heading style
	public static GUIStyle LabelRightStyle;				// right-aligned label	
	public static GUIStyle InspectorHeadStyle;			// heading used in the inspector (similar in size to head3 but with different padding)
	public static GUIStyle InspectorHeadBoxStyle;		// a seperator like heading used in the inspector
	public static GUIStyle InspectorHeadFoldStyle;		// similar to InspectorHeadStyle, but has foldout icon
	public static GUIStyle HeadingFoldoutStyle;			// a foldout that has big text (like a heading style)

	// boxes & frames
	public static GUIStyle DividerStyle;
	public static GUIStyle BoxStyle;
	public static GUIStyle ScrollViewStyle;
	public static GUIStyle ScrollViewNoTLMarginStyle;
	public static GUIStyle ScrollViewNoMarginPaddingStyle;

	// buttons
	public static GUIStyle ButtonStyle;
	public static GUIStyle ButtonLeftStyle;
	public static GUIStyle ButtonMidStyle;
	public static GUIStyle ButtonRightStyle;
	public static GUIStyle LeftTabStyle;
	public static GUIStyle TinyButton;
	public static GUIStyle ToolbarStyle;
	public static GUIStyle ToolbarButtonLeftStyle;
	public static GUIStyle ToolbarButtonMidStyle;
	public static GUIStyle ToolbarButtonRightStyle;

	// fields
	public static GUIStyle DelTextFieldStyle;
	public static GUIStyle TextFieldStyle;					// like EditorStyles.textField but with wordwrap=false

	// misc
	public static GUIStyle MenuBoxStyle;
	public static GUIStyle MenuItemStyle;
	public static GUIStyle MenuHeadStyle;
	public static GUIStyle ListItemBackDarkStyle;
	public static GUIStyle ListItemBackLightStyle;
	public static GUIStyle ListItemSelectedStyle;
	public static GUIStyle AboutLogoAreaStyle;

	// resources - icons
	public static Texture2D Icon_Tag;
	public static Texture2D Icon_Help;
	public static Texture2D Icon_Plus;
	public static Texture2D Icon_Minus;
	public static Texture2D Icon_Accept;
	public static Texture2D Icon_Cancel;
	public static Texture2D Icon_Refresh;
	public static Texture2D Icon_Exclaim_Red;
	public static Texture2D Icon_User;
	public static Texture2D Icon_Users;
	public static Texture2D Icon_UserPlus;
	public static Texture2D Icon_UserMinus;
	public static Texture2D Icon_Bag;
	public static Texture2D Icon_Star;
	public static Texture2D Icon_Screen;
	public static Texture2D Icon_Arrow_Up;
	public static Texture2D Icon_Arrow2_Up;
	public static Texture2D Icon_Arrow2_Down;
	public static Texture2D Icon_Arrow3_Left;
	public static Texture2D Icon_Pencil;
	public static Texture2D Icon_Page;
	public static Texture2D Icon_Copy;
	public static Texture2D Icon_Skull;
	public static Texture2D Icon_Weapon;

	public static Texture2D Icon_GameObject;
	public static Texture2D Icon_Animation;
	public static Texture2D Icon_Collider;

	// resources - skin
	public static Texture2D Texture_Logo;
	public static Texture2D Texture_LogoIcon;
	public static Texture2D Texture_Box;
	public static Texture2D Texture_FlatBox;
	public static Texture2D Texture_NoPreview;
	public static Texture2D Texture_BoxSides;
	public static Texture2D Texture_Selected;
	public static Texture2D Texture_FlatDarker;
	public static Texture2D Texture_FlatLighter;
	public static Texture2D Texture_ToolButtonSelected;
	public static Texture2D Texture_ToolButtonSelectedLeft;
	public static Texture2D Texture_ToolButtonSelectedMid;
	public static Texture2D Texture_ToolButtonSelectedRight;

	// ...
	private static GUIStyle[] customStyles;

	// ================================================================================================================

	public static void UseSkin()
	{
		// some stuff that can be set before test for inited
		EditorStyles.textField.wordWrap = true;
		if (EditorGUIUtility.isProSkin)
		{
			EditorStyles.miniButtonMid.border = new RectOffset(2, 2, 0, 0); // fix a bug present in EditorStyles.miniButtonMid
		}
		else
		{
			InspectorDividerColor = new Color(0.45f, 0.45f, 0.45f, 1f);
		}

		// load the skin
		if (Skin != null)
		{
			GUI.skin = Skin;
			return;
		}

		LoadSkinTextures();

		// ----------------------------------------------------------------------------------------------------
		// init some styles that don't need skin file to define
		Skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
		customStyles = Skin.customStyles;

		// text
		WarningLabelStyle = new GUIStyle(EditorStyles.label)
		{
			name = "PLYWarningLabel",
			richText = false,
			normal = { textColor = (EditorGUIUtility.isProSkin ? Color.yellow : Color.red) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, WarningLabelStyle);

		BoldLabelStyle = new GUIStyle(EditorStyles.boldLabel)
		{
			name = "PLYBoldLabel",
			richText = false,
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(3, 3, 0, 3),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, BoldLabelStyle);


		RichLabelStyle = new GUIStyle(EditorStyles.label)
		{
			name = "PLYRichLabel",
			richText = true,
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, RichLabelStyle);

		CenterLabelStyle = new GUIStyle(EditorStyles.label)
		{
			name = "PLYCenterLabel",
			richText = false,
			alignment = TextAnchor.MiddleCenter
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, CenterLabelStyle);

		Head1Style = new GUIStyle(EditorStyles.boldLabel)
		{
			name = "PLYHead1",
			richText = false,
			fontSize = 20,
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(12, 0, 3, 3),
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.5f, 0.5f, 0.5f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, Head1Style);

		Head2Style = new GUIStyle(EditorStyles.boldLabel)
		{
			name = "PLYHead2",
			richText = false,
			fontSize = 16,
			fontStyle = FontStyle.Bold,
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 10),
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, Head2Style);

		Head3Style = new GUIStyle(EditorStyles.boldLabel)
		{
			name = "PLYHead3",
			richText = false,
			fontSize = 15,
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 5),
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, Head3Style);

		Head4Style = new GUIStyle(Head3Style)
		{
			name = "PLYHead4",
			richText = false,
			fontSize = 12,
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, Head4Style);

		LabelRightStyle = new GUIStyle(Skin.label)
		{
			name = "PLYLabelRight",
			richText = false,
			alignment = TextAnchor.MiddleRight,
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, LabelRightStyle);

		InspectorHeadStyle = new GUIStyle(Head3Style)
		{
			name = "PLYInspectorHead",
			padding = new RectOffset(15, 0, 3, 3),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, InspectorHeadStyle);

		InspectorHeadBoxStyle = new GUIStyle(Skin.box)
		{
			name = "PLYInspectorHeadBox",
			richText = false,
			fontSize = 13,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleLeft,
			padding = new RectOffset(25, 0, 3, 3),
			margin = new RectOffset(0, 0, 20, 10),
			stretchWidth = true,
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, InspectorHeadBoxStyle);

		InspectorHeadFoldStyle = new GUIStyle(EditorStyles.foldout)
		{
			name = "PLYInspectorHeadFold",
			richText = false,
			fontSize = 14,
			fontStyle = FontStyle.Bold,
			padding = new RectOffset(17, 0, 0, 0),
			margin = new RectOffset(10, 10, 2, 0),
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, InspectorHeadFoldStyle);

		HeadingFoldoutStyle = new GUIStyle(EditorStyles.foldout)
		{
			name = "PLYHeadingFoldout",
			richText = false,
			fontSize = 15,
			padding = new RectOffset(17, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 5),
			normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f)) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, HeadingFoldoutStyle);

		// buttons
		ButtonStyle = new GUIStyle(Skin.button)
		{
			name = "PLYButton",
			richText = false,
			fontSize = 12,
			padding = new RectOffset(20, 20, 5, 5),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonStyle);

		ButtonLeftStyle = new GUIStyle(EditorStyles.miniButtonLeft)
		{
			name = "PLYButtonLeft",
			richText = false,
			fontSize = 11,
			padding = new RectOffset(0, 0, 3, 5),
			margin = new RectOffset(2, 0, 0, 2),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonLeftStyle);

		ButtonMidStyle = new GUIStyle(EditorStyles.miniButtonMid)
		{
			name = "PLYButtonMid",
			richText = false,
			fontSize = 11,
			padding = new RectOffset(0, 0, 3, 5),
			margin = new RectOffset(0, 0, 0, 2),
			border = new RectOffset(2, 2, 0, 0), // fix a bug present in EditorStyles.miniButtonMid
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonMidStyle);

		ButtonRightStyle = new GUIStyle(EditorStyles.miniButtonRight)
		{
			name = "PLYButtonRight",
			richText = false,
			fontSize = 11,
			padding = new RectOffset(0, 0, 3, 5),
			margin = new RectOffset(0, 2, 0, 2),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonRightStyle);

		LeftTabStyle = new GUIStyle(EditorStyles.miniButtonLeft)
		{
			name = "PLYLeftTab",
			richText = false,
			alignment = TextAnchor.MiddleLeft,
			fontSize = 10,
			fixedHeight = 24,
			stretchWidth = true,
			padding = new RectOffset(3, 3, 5, 5),
			margin = new RectOffset(3, 0, 0, 3),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, LeftTabStyle);

		TinyButton = new GUIStyle(EditorStyles.miniButton)
		{
			name = "PLYTinyButton",
			richText = false,
			fontSize = 10,
			padding = new RectOffset(2, 2, 0, 0),
			margin = new RectOffset(0, 0, 0, 0),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, TinyButton);

		ToolbarStyle = new GUIStyle(Skin.button)
		{
			name = "PLYToolbar",
			fontStyle = FontStyle.Bold,
			fontSize = 11,
			padding = new RectOffset(5, 5, 5, 5),
			overflow = new RectOffset(0, 0, 0, 1),
			onNormal = { background = Texture_ToolButtonSelected, textColor = Color.white },
			onActive = { background = Texture_ToolButtonSelected, textColor = Color.white },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ToolbarStyle);

		ToolbarButtonLeftStyle = new GUIStyle(Skin.FindStyle("ButtonLeft"))
		{
			name = "PLYToolbarLeft",
			fontStyle = FontStyle.Bold,
			fontSize = 11,
			padding = new RectOffset(5, 5, 5, 5),
			border = new RectOffset(4, 2, 2, 2),
			onNormal = { background = Texture_ToolButtonSelectedLeft },
			onActive = { background = Texture_ToolButtonSelectedLeft },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ToolbarButtonLeftStyle);

		ToolbarButtonMidStyle = new GUIStyle(Skin.FindStyle("ButtonMid"))
		{
			name = "PLYToolbarMid",
			fontStyle = FontStyle.Bold,
			fontSize = 11,
			padding = new RectOffset(5, 5, 5, 5),
			border = new RectOffset(2, 2, 2, 2),
			onNormal = { background = Texture_ToolButtonSelectedMid },
			onActive = { background = Texture_ToolButtonSelectedMid },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ToolbarButtonMidStyle);

		ToolbarButtonRightStyle = new GUIStyle(Skin.FindStyle("ButtonRight"))
		{
			name = "PLYToolbarRight",
			fontStyle = FontStyle.Bold,
			fontSize = 11,
			padding = new RectOffset(5, 5, 5, 5),
			border = new RectOffset(2, 4, 2, 2),
			onNormal = { background = Texture_ToolButtonSelectedRight },
			onActive = { background = Texture_ToolButtonSelectedRight },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ToolbarButtonRightStyle);

		// boxes and such
		DividerStyle = new GUIStyle()
		{
			name = "PLYDivider",
			border = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 0),
			normal = { background = EditorGUIUtility.whiteTexture },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, DividerStyle);

		BoxStyle = new GUIStyle(Skin.box)
		{
			name = "PLYBox",
			padding = new RectOffset(10, 10, 10, 10),
			margin = new RectOffset(5, 0, 5, 0),
			normal = { background = (EditorGUIUtility.isProSkin ? Texture_Box : Skin.box.normal.background) },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, BoxStyle);

		ScrollViewStyle = new GUIStyle(Skin.box)
		{
			name = "PLYScrollView",
			padding = new RectOffset(10, 10, 10, 10),
			margin = new RectOffset(5, 5, 5, 5),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ScrollViewStyle);

		ScrollViewNoTLMarginStyle = new GUIStyle(ScrollViewStyle)
		{
			name = "PLYScrollViewNoTLMargin",
			margin = new RectOffset(0, 5, 0, 5),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ScrollViewNoTLMarginStyle);

		ScrollViewNoMarginPaddingStyle = new GUIStyle(ScrollViewStyle)
		{
			name = "PLYScrollViewNoTLMarginNoPadding",
			padding = new RectOffset(0, 0, 3, 3),
			margin = new RectOffset(0, 0, 0, 0),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ScrollViewNoMarginPaddingStyle);

		// fields
		DelTextFieldStyle = new GUIStyle(EditorStyles.textField)
		{
			name = "PLYDelTextField",
			fixedHeight = 19,
			padding = new RectOffset(5, 5, 2, 2),
			margin = new RectOffset(EditorStyles.textField.margin.left, 0, 0, 5),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, DelTextFieldStyle);

		TextFieldStyle = new GUIStyle(EditorStyles.textField)
		{
			name = "PLYTextField",
			wordWrap = false,
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, TextFieldStyle);

		// misc
		MenuBoxStyle = new GUIStyle(BoxStyle)
		{
			name = "PLYMenuBox",
			margin = new RectOffset(1, 10, 0, 0),
			padding = new RectOffset(1, 1, 0, 0),
			normal = { background = Texture_BoxSides },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, MenuBoxStyle);

		MenuItemStyle = new GUIStyle(EditorStyles.toggle)
		{
			name = "PLYMenuItem",
			richText = false,
			fontSize = 14,
			alignment = TextAnchor.MiddleRight,
			border = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 10, 11, 7),
			normal = { background = null },
			hover = { background = null },
			active = { background = null },
			focused = { background = null },
			onNormal = { background = Texture_Selected, textColor = Color.white },
			onHover = { background = null },
			onActive = { background = Texture_Selected, textColor = Color.white },
			onFocused = { background = null },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, MenuItemStyle);

		MenuHeadStyle = new GUIStyle(EditorStyles.label)
		{
			name = "PLYMenuHead",
			richText = false,
			fontSize = 14,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleRight,
			border = new RectOffset(0, 0, 0, 0),
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 10, 20, 3),
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, MenuHeadStyle);

		ListItemBackDarkStyle = new GUIStyle(Skin.button)
		{
			name = "PLYListItemBackDark",
			richText = false,
			fontSize = 11,
			alignment = TextAnchor.MiddleLeft,
			clipping = TextClipping.Clip,
			stretchWidth = false,
			wordWrap = false,
			overflow = new RectOffset(0, 0, 0, 0),
			border = new RectOffset(1, 1, 1, 1),
			margin = new RectOffset(0, 0, 1, 0),
			hover = { background = Texture_FlatDarker },
			onHover = { background = Texture_FlatDarker },
			normal = { background = Texture_FlatDarker },
			active = { background = Texture_FlatDarker },
			onNormal = { background = Texture_FlatDarker },
			onActive = { background = Texture_FlatDarker },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ListItemBackDarkStyle);

		ListItemBackLightStyle = new GUIStyle(ListItemBackDarkStyle)
		{
			name = "PLYListItemBackLight",
			hover = { background = Texture_FlatLighter },
			onHover = { background = Texture_FlatLighter },
			normal = { background = Texture_FlatLighter },
			active = { background = Texture_FlatLighter },
			onNormal = { background = Texture_FlatLighter },
			onActive = { background = Texture_FlatLighter },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ListItemBackLightStyle);

		ListItemSelectedStyle = new GUIStyle(ListItemBackDarkStyle)
		{
			name = "PLYListItemSelected",
			hover = { background = Texture_Selected, textColor = Color.white },
			onHover = { background = Texture_Selected, textColor = Color.white },
			normal = { background = Texture_Selected, textColor = Color.white },
			active = { background = Texture_Selected, textColor = Color.white },
			onNormal = { background = Texture_Selected, textColor = Color.white },
			onActive = { background = Texture_Selected, textColor = Color.white },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, ListItemSelectedStyle);

		AboutLogoAreaStyle = new GUIStyle(Skin.box)
		{
			name = "PLYAboutLogoArea",
			stretchWidth = true,
			margin = new RectOffset(0,0,0,10),
			padding = new RectOffset(10,10,10,10),
			normal = { background = EditorGUIUtility.whiteTexture },
		}; ArrayUtility.Add<GUIStyle>(ref customStyles, AboutLogoAreaStyle);

		

		// finally, set it
		Skin.customStyles = customStyles;
		GUI.skin = Skin;
	}

	private static void LoadSkinTextures()
	{
		Icon_GameObject = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
		Icon_Animation = AssetPreview.GetMiniTypeThumbnail(typeof(Animation));
		Icon_Collider = AssetPreview.GetMiniTypeThumbnail(typeof(BoxCollider));

		Icon_Tag = LoadEditorTexture(EditorResourcePath + "Icons/tag.png");
		Icon_Help = LoadEditorTexture(EditorResourcePath + "Icons/help.png");
		Icon_Plus = LoadEditorTexture(EditorResourcePath + "Icons/plus.png");
		Icon_Minus = LoadEditorTexture(EditorResourcePath + "Icons/minus.png");
		Icon_Accept = LoadEditorTexture(EditorResourcePath + "Icons/accept.png");
		Icon_Cancel = LoadEditorTexture(EditorResourcePath + "Icons/cancel.png");
		Icon_User = LoadEditorTexture(EditorResourcePath + "Icons/user.png");
		Icon_Users = LoadEditorTexture(EditorResourcePath + "Icons/users.png");
		Icon_UserPlus = LoadEditorTexture(EditorResourcePath + "Icons/user_plus.png");
		Icon_UserMinus = LoadEditorTexture(EditorResourcePath + "Icons/user_minus.png");
		Icon_Bag = LoadEditorTexture(EditorResourcePath + "Icons/bag.png");
		Icon_Star = LoadEditorTexture(EditorResourcePath + "Icons/star.png");
		Icon_Screen = LoadEditorTexture(EditorResourcePath + "Icons/screen.png");
		Icon_Refresh = LoadEditorTexture(EditorResourcePath + "Icons/refresh.png");
		Icon_Exclaim_Red = LoadEditorTexture(EditorResourcePath + "Icons/exclaim_red.png");
		Icon_Arrow_Up = LoadEditorTexture(EditorResourcePath + "Icons/arrow_up.png");
		Icon_Arrow2_Up = LoadEditorTexture(EditorResourcePath + "Icons/arrow2_up.png");
		Icon_Arrow2_Down = LoadEditorTexture(EditorResourcePath + "Icons/arrow2_down.png");
		Icon_Arrow3_Left = LoadEditorTexture(EditorResourcePath + "Icons/arrow3_left.png");
		Icon_Pencil = LoadEditorTexture(EditorResourcePath + "Icons/pencil.png");
		Icon_Page = LoadEditorTexture(EditorResourcePath + "Icons/page.png");
		Icon_Copy = LoadEditorTexture(EditorResourcePath + "Icons/copy.png");
		Icon_Skull = LoadEditorTexture(EditorResourcePath + "Icons/skull.png");
		Icon_Weapon = LoadEditorTexture(EditorResourcePath + "Icons/weapon.png");

		Texture_Logo = LoadEditorTexture(EditorResourcePath + "Skin/logo.png");
		Texture_LogoIcon = LoadEditorTexture(EditorResourcePath + "Skin/logo_icon" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture_Box = LoadEditorTexture(EditorResourcePath + "Skin/box.png");
		Texture_FlatBox = LoadEditorTexture(EditorResourcePath + "Skin/flatbox" + (EditorGUIUtility.isProSkin?"":"_l") + ".png");
		Texture_NoPreview = LoadEditorTexture(EditorResourcePath + "Skin/no_preview.png");
		Texture_BoxSides = LoadEditorTexture(EditorResourcePath + "Skin/boxsides" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture_Selected = LoadEditorTexture(EditorResourcePath + "Skin/selected" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture_FlatDarker = LoadEditorTexture(EditorResourcePath + "Skin/flat_darker" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture_FlatLighter = LoadEditorTexture(EditorResourcePath + "Skin/flat_lighter" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");
		Texture_ToolButtonSelected = LoadEditorTexture(EditorResourcePath + "Skin/toolbar_selected.png");
		Texture_ToolButtonSelectedLeft = LoadEditorTexture(EditorResourcePath + "Skin/toolbar_selected_left.png");
		Texture_ToolButtonSelectedMid = LoadEditorTexture(EditorResourcePath + "Skin/toolbar_selected_mid.png");
		Texture_ToolButtonSelectedRight = LoadEditorTexture(EditorResourcePath + "Skin/toolbar_selected_right.png");
	}

	#endregion
	// ================================================================================================================
	#region resource loading

	public static Texture2D LoadEditorTexture(string fn)
	{
		Texture2D tx = AssetDatabase.LoadAssetAtPath(fn, typeof(Texture2D)) as Texture2D;
		if (tx == null) Debug.LogWarning("Failed to load texture: " + fn);
		else if (tx.wrapMode != TextureWrapMode.Clamp)
		{
			string path = AssetDatabase.GetAssetPath(tx);
			TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			tImporter.textureType = TextureImporterType.GUI;
			tImporter.npotScale = TextureImporterNPOTScale.None;
			tImporter.filterMode = FilterMode.Point;
			tImporter.wrapMode = TextureWrapMode.Clamp;
			tImporter.maxTextureSize = 64;
			tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			EditorApplication.SaveAssets();
		}
		return tx;
	}

	#endregion
	// ================================================================================================================
	#region misc

	//public static Texture2D GetAssetPreview(Object ob)
	//{
	//	#if UNITY_3
	//	return EditorUtility.GetAssetPreview(ob);
	//	#else
	//	return AssetPreview.GetAssetPreview(ob);
	//	#endif
	//}

	//public static void HideRendererWireframe(GameObject go)
	//{
	//	EditorUtility.SetSelectedWireframeHidden(go.renderer, true);
	//	for (int i = 0; i < go.transform.childCount; i++)
	//	{
	//		PLYEditorUtil.HideRendererWireframe(go.transform.GetChild(i).gameObject);
	//	}
	//}

	//public static void FocusSceneView()
	//{
	//	// focus the scene view
	//	if (SceneView.sceneViews.Count > 0) (SceneView.sceneViews[0] as SceneView).Focus();
	//}

	#endregion
	// ================================================================================================================
	#region GUI Controls

	// Draw button with icon and text
	public static bool IconButton(string label, Texture2D icon, params GUILayoutOption[] options) { return IconButton(label, icon, null, options); }
	public static bool IconButton(string label, Texture2D icon, GUIStyle style, params GUILayoutOption[] options)
	{
		if (style != null) return GUILayout.Button(new GUIContent(label, icon), style, options);
		else return GUILayout.Button(new GUIContent(label, icon), options);
	}

	// this toggle does not return the new state but rather if the state has changed (true) or not (false)
	public static bool ToggleButton(bool active, string label, GUIStyle style, params GUILayoutOption[] options)
	{
		bool new_value = active;
		if (string.IsNullOrEmpty(label)) new_value = GUILayout.Toggle(active, GUIContent.none, style, options);
		else new_value = GUILayout.Toggle(active, label, style, options);
		return (new_value != active);
	}

	// this toggle does not return the new state but rather if the state has changed (true) or not (false)
	public static bool ToggleButton(bool active, string label, Texture2D icon, GUIStyle style, params GUILayoutOption[] options)
	{
		bool new_value = active;
		new_value = GUILayout.Toggle(active, new GUIContent(label, icon), style, options);
		return (new_value != active);
	}

	// this toggle does not return the new state but rather if the state has changed (true) or not (false)
	public static bool ToggleButton(bool active, GUIContent label, GUIStyle style, params GUILayoutOption[] options)
	{
		bool new_value = active;
		new_value = GUILayout.Toggle(active, label, style, options);
		return (new_value != active);
	}

	// this toggle does not return the new state but rather if the state has changed (true) or not (false)
	public static bool ToggleButton(bool active, string label, GUIStyle style, Color onTint, params GUILayoutOption[] options)
	{
		Color c = GUI.backgroundColor;
		if (active) GUI.backgroundColor = onTint;
		bool new_value = active;
		if (string.IsNullOrEmpty(label)) new_value = GUILayout.Toggle(active, GUIContent.none, style, options);
		else new_value = GUILayout.Toggle(active, label, style, options);
		GUI.backgroundColor = c;
		return (new_value != active);
	}

	// this toggle does not return the new state but rather if the state has changed (true) or not (false)
	public static bool ToggleButton(bool active, GUIContent label, GUIStyle style, Color onTint, params GUILayoutOption[] options)
	{
		Color c = GUI.backgroundColor;
		if (active) GUI.backgroundColor = onTint;
		bool new_value = active;
		if (label == null) new_value = GUILayout.Toggle(active, GUIContent.none, style, options);
		else new_value = GUILayout.Toggle(active, label, style, options);
		GUI.backgroundColor = c;
		return (new_value != active);
	}

	public static bool TintedButton(GUIContent label, Color tint, params GUILayoutOption[] options)
	{
		Color c = GUI.backgroundColor;
		GUI.backgroundColor = tint;
		bool ret = GUILayout.Button(label, options);
		GUI.backgroundColor = c;
		return ret;
	}

	public static bool TintedButton(string label, Color tint, params GUILayoutOption[] options)
	{
		Color c = GUI.backgroundColor;
		GUI.backgroundColor = tint;
		bool ret = GUILayout.Button(label, options);
		GUI.backgroundColor = c;
		return ret;
	}

	// foldout for use in inspector where you can click on label too, unlike EditorGUILayout.Foldout
	public static bool Foldout(bool foldout, string label, GUIStyle style, params GUILayoutOption[] options)
	{
		if (foldout)
		{
			GUIStyle style2 = new GUIStyle(style);
			style2.normal = style.onNormal;
			style2.active = style.onActive;
			if (GUILayout.Button(label, style2)) return false;
		}
		else
		{
			if (GUILayout.Button(label, style)) return true;
		}
		return foldout;
	}

	// menu (similar to the one in Unity Preferences). If the menuItem's name starts with "-" then that will be a separator
	// if the "-" is followed by more characters then that separator will have a heading. 
	// a null/empty string for menuItem will also cause a separator/space in the menu
	public static int Menu(int selected, string[] menuItems, params GUILayoutOption[] options)
	{
		if (selected >= menuItems.Length || selected < 0) selected = 0;
		EditorGUILayout.BeginVertical(MenuBoxStyle, options);
		{
			GUILayout.Space(20);

			for (int i = 0; i < menuItems.Length; i++)
			{
				if (string.IsNullOrEmpty(menuItems[i])) { GUILayout.Space(15); continue; }
				
				if (menuItems[i][0] == '-')
				{
					if (menuItems[i].Length > 1)
					{
						GUILayout.Label(menuItems[i].Substring(1), UniRPGEdGui.MenuHeadStyle);
					}
					else
					{
						UniRPGEdGui.DrawHorizontalLine(1, UniRPGEdGui.DividerColor, 5, 5);
					}
					continue;
				}

				if (GUILayout.Toggle((i == selected), menuItems[i], UniRPGEdGui.MenuItemStyle)) selected = i;
			}

			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndVertical();
		return selected;
	}

	#endregion
	// ================================================================================================================
	#region GUI fields

	/// <summary>show a field where the designer can either enter a value or choose a global variable</summary>
	public static StringValue GlobalStringVarOrValueField(EditorWindow ed, string label, StringValue stringVal, int labelWidth = 100, int fieldWidth = 0)
	{
		if (stringVal == null) return stringVal;
		EditorGUILayout.BeginHorizontal();
		{
			if (!string.IsNullOrEmpty(label))
			{
				if (labelWidth > 0) GUILayout.Label(label, GUILayout.Width(labelWidth));
				else GUILayout.Label(label);
				EditorGUILayout.Space();
			}
			if (!string.IsNullOrEmpty(stringVal.stringVarName))
			{
				GUI.enabled = false;
				if (fieldWidth > 0) EditorGUILayout.TextField(stringVal.stringVarName, GUILayout.Width(fieldWidth));
				else EditorGUILayout.TextField(stringVal.stringVarName);
				GUI.enabled = true;
			}
			else
			{
				if (fieldWidth > 0) stringVal.SetAsValue = EditorGUILayout.TextField(stringVal.GetValue(null), GUILayout.Width(fieldWidth));
				else stringVal.SetAsValue = EditorGUILayout.TextField(stringVal.GetValue(null));
			}

			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Tag, "Select variable"), EditorStyles.miniButton, GUILayout.Width(25)))
			{
				GlobalVarSelectWiz.Show(stringVal, ed);
			}
		}
		EditorGUILayout.EndHorizontal();
		return stringVal;
	}

	/// <summary>show a field where the designer can either enter a value or choose a global variable</summary>
	public static NumericValue GlobalNumericVarOrValueField(EditorWindow ed, string label, NumericValue numericVal, int labelWidth = 100, int fieldWidth = 0)
	{
		if (numericVal==null) return numericVal;
		EditorGUILayout.BeginHorizontal();
		{
			if (!string.IsNullOrEmpty(label))
			{
				if (labelWidth > 0) GUILayout.Label(label, GUILayout.Width(labelWidth));
				else GUILayout.Label(label);
				EditorGUILayout.Space();
			}
			if (!string.IsNullOrEmpty(numericVal.numericVarName))
			{
				GUI.enabled = false;
				if (fieldWidth > 0) EditorGUILayout.TextField(numericVal.numericVarName, GUILayout.Width(fieldWidth));
				else EditorGUILayout.TextField(numericVal.numericVarName);
				GUI.enabled = true;
			}
			else
			{
				if (fieldWidth > 0) numericVal.SetAsValue = EditorGUILayout.FloatField(numericVal.GetValue(null), GUILayout.Width(fieldWidth));
				else numericVal.SetAsValue = EditorGUILayout.FloatField(numericVal.GetValue(null));
			}

			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Tag, "Select variable"), EditorStyles.miniButton, GUILayout.Width(25)))
			{
				GlobalVarSelectWiz.Show(numericVal, ed);
			}
		}
		EditorGUILayout.EndHorizontal();
		return numericVal;
	}

	/// <summary>show a field where the designer can either enter a value or choose a global variable</summary>
	public static ObjectValue GlobalObjectVarOrValueField(EditorWindow ed, string label, ObjectValue objectVal, System.Type objectType, bool allowSceneObject = true, int labelWidth = 100, int fieldWidth = 0)
	{
		if (objectVal == null)
		{
			Debug.LogWarning("GlobalObjectVarOrValueField: ObjectValue not initialised.");
			return objectVal;
		}
		EditorGUILayout.BeginHorizontal();
		{
			if (!string.IsNullOrEmpty(label))
			{
				if (labelWidth > 0) GUILayout.Label(label, GUILayout.Width(labelWidth));
				else GUILayout.Label(label);
				EditorGUILayout.Space();
			}
			if (!string.IsNullOrEmpty(objectVal.objectVarName))
			{
				GUI.enabled = false;
				if (fieldWidth > 0) EditorGUILayout.TextField(objectVal.objectVarName, GUILayout.Width(fieldWidth));
				else EditorGUILayout.TextField(objectVal.objectVarName);
				GUI.enabled = true;
			}
			else
			{
				if (fieldWidth > 0) objectVal.SetAsValue = EditorGUILayout.ObjectField(objectVal.GetValue(null), objectType, allowSceneObject, GUILayout.Width(fieldWidth));
				else objectVal.SetAsValue = EditorGUILayout.ObjectField(objectVal.GetValue(null), typeof(Object), allowSceneObject);
			}

			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Tag, "Select variable"), EditorStyles.miniButton, GUILayout.Width(25)))
			{
				GlobalVarSelectWiz.Show(objectVal, ed);
			}
		}
		EditorGUILayout.EndHorizontal();
		return objectVal;
	}

	/// <summary>Show a field where the designer can select the action.targetType</summary>
	public static void TargetTypeField(EditorWindow ed, string label, ActionTarget actionTarget, string help, int labelWidth = 0)
	{
		EditorGUILayout.BeginHorizontal();
		{
			if (label != null)
			{
				if (labelWidth == 0) GUILayout.Label(label);
				else GUILayout.Label(label, GUILayout.Width(labelWidth));
				EditorGUILayout.Space();
			}
			EditorGUILayout.BeginVertical();
			{
				actionTarget.type = (Action.TargetType)EditorGUILayout.EnumPopup(actionTarget.type);
				if (actionTarget.type == Action.TargetType.Specified)
				{
					//actionTarget.targetObject = (GameObject)EditorGUILayout.ObjectField(actionTarget.targetObject, typeof(GameObject), true);
					actionTarget.targetObject = UniRPGEdGui.GlobalObjectVarOrValueField(ed, null, actionTarget.targetObject, typeof(GameObject), true);
				}
				else
				{	// make sure it is null so that there is not an unintended link with prefabs or other assets
					actionTarget.targetObject.SetAsValue = null;
				}
			}
			EditorGUILayout.EndVertical();

			if (help != null)
			{
				GUILayout.Label(new GUIContent(UniRPGEdGui.Icon_Help, help), GUILayout.Width(20));
				EditorGUILayout.Space();
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	// draw objectfield with preview
	public static GameObject GameObjectFieldWithPreview(string label, GameObject obj, bool allowSceneObjects, GUIStyle style = null, int previewWidth=64, int previewHeight=64)
	{
		int fieldW = previewWidth;
		int fieldH = previewHeight + 50;
		RectOffset padding = new RectOffset(0, 0, 0, 0);
		if (style != null)
		{
			padding = style.padding;
			fieldW += padding.left + padding.right;
			fieldH += padding.top + padding.bottom;
		}

		Rect r = EditorGUILayout.BeginVertical(style, GUILayout.Width(fieldW), GUILayout.Height(fieldH), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		{
			Rect r2 = new Rect(r.x + padding.left, r.y + padding.top, 16, 16);
			GUI.DrawTexture(r2, Icon_GameObject);
			r2.x += 18; r2.width = r.width - 25; r2.height = 25;
			GUI.Label(r2, (!string.IsNullOrEmpty(label) ? label : "Object"));

			r2.x = r.x + padding.left; r2.y += 25;
			r2.width = previewWidth; r2.height = previewHeight;
			if (obj == null)
			{
				GUI.DrawTexture(r2, UniRPGEdGui.Texture_FlatBox);
				GUI.Label(new Rect(r2.x + 2, r2.y + 2, r2.width - 4, r2.height - 4), "no preview");
			}
			else
			{
				Texture2D t = AssetPreview.GetAssetPreview(obj);
				if (t == null)
				{
					GUI.DrawTexture(r2, UniRPGEdGui.Texture_FlatBox);
					GUI.Label(new Rect(r2.x + 2, r2.y + 2, r2.width - 4, r2.height - 4), "loading preview...");
				}
				else GUI.DrawTexture(r2, t);
			}

			r2.y += previewHeight + 5; r2.height = 18;
			obj = (GameObject)EditorGUI.ObjectField(r2, obj, typeof(GameObject), allowSceneObjects);
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical();
		return obj;
	}

	// show a button that has a label on same line
	public static bool LabelButton(string label, string button, int labelWidth = 100, int buttonWidth = 0)
	{
		bool ret = false;
		EditorGUILayout.BeginHorizontal();
		{
			if (label == null) label = " ";
			if (labelWidth > 0) GUILayout.Label(label, GUILayout.Width(labelWidth));
			else GUILayout.Label(label);
			EditorGUILayout.Space();
			if (buttonWidth > 0) ret = GUILayout.Button(button, GUILayout.Width(buttonWidth));
			else ret = GUILayout.Button(button);
		}
		EditorGUILayout.EndHorizontal();
		return ret;
	}

	// Show a text field and browse button which opens the SaveFolderPanel
	public static string PathField(string path, string label, string dialogTitle)
	{
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField(label, path, UniRPGEdGui.TextFieldStyle);
			if (GUILayout.Button("Browse"))
			{
				string newPath = EditorUtility.SaveFolderPanel(dialogTitle, path, "");
				if (!string.IsNullOrEmpty(newPath)) path = newPath;
			}
		}
		EditorGUILayout.EndHorizontal();
		return path;
	}

	// Show a text field and browse button which opens the OpenFilePanel (the shown and returned path is relative path)
	public static string RelativeFileField(string path, string label, string dialogTitle, string extention)
	{
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField(label, path, UniRPGEdGui.TextFieldStyle);
			if (GUILayout.Button("Browse", GUILayout.Width(80)))
			{
				string newPath = EditorUtility.OpenFilePanel(dialogTitle, path, extention);
				if (!string.IsNullOrEmpty(newPath))
				{	// make into relative path again
					path = UniRPGEdUtil.ProjectRelativePath(newPath);
				}
			}
		}
		EditorGUILayout.EndHorizontal();
		return path;
	}

	// a text field with a delete button at the end
	public static string TextListField(string text, out bool del, int width)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.ExpandWidth(false));
		{
			text = EditorGUILayout.TextField(text, DelTextFieldStyle, GUILayout.Width(width - 20));
			del = GUILayout.Button("X", ButtonRightStyle, GUILayout.Width(20));
		}
		EditorGUILayout.EndHorizontal();
		return text;
	}

	// a text field with a delete button at the end
	public static string DelTextField(string text, out bool del, int width)
	{
		EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.ExpandWidth(false));
		{
			text = EditorGUILayout.TextField(text, DelTextFieldStyle, GUILayout.Width(width - 20));
			del = GUILayout.Button("X", ButtonRightStyle, GUILayout.Width(20));
		}
		EditorGUILayout.EndHorizontal();
		return text;
	}

	// a block that shows a button to add more text field and list of text fields, each with a delete button
	public static Vector2 TextListField(List<string> data, string label, Vector2 scroll, int width, int height)
	{
		return TextListField(data, ref label, scroll, width, height, false);
	}

	public static Vector2 TextListField(List<string> data, ref string label, Vector2 scroll, int width, int height, bool canEditLabel)
	{
		int fieldToDel = -1;
		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(width), GUILayout.ExpandWidth(false));
		{
			EditorGUILayout.BeginHorizontal();
			{
				if (canEditLabel) label = EditorGUILayout.TextField(label);
				else if (label != null)
				{
					GUILayout.Label(label, UniRPGEdGui.Head3Style);
					GUILayout.FlexibleSpace();
				}

				if (UniRPGEdGui.IconButton("Add", UniRPGEdGui.Icon_Plus, EditorStyles.miniButton, GUILayout.Width(80)))
				{
					if (data == null) data = new List<string>();
					data.Add("");
					GUI.FocusControl("");
				}
				if (label == null) GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			scroll = UniRPGEdGui.BeginScrollView(scroll, GUILayout.Width(width - 20), GUILayout.Height(height));
			for (int i = 0; i < data.Count; i++)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 45), GUILayout.ExpandWidth(false));
				{
					data[i] = EditorGUILayout.TextField(data[i], DelTextFieldStyle, GUILayout.Width(width - 65));
					if (GUILayout.Button("X", ButtonRightStyle, GUILayout.Width(20))) fieldToDel = i;
					GUILayout.Space(19);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (data.Count == 0)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 20), GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}
			UniRPGEdGui.EndScrollView();
		}
		EditorGUILayout.EndVertical();

		if (fieldToDel >= 0) data.RemoveAt(fieldToDel);

		return scroll;
	}

	public static bool FoldableLabel(bool foldout, string label, int width)
	{
		EditorGUILayout.BeginHorizontal();
		{
			GUIStyle s = new GUIStyle(EditorStyles.label) { wordWrap = true, stretchWidth = true };
			if (foldout) EditorGUIUtility.LookLikeControls(0, 8);
			foldout = EditorGUILayout.Foldout(foldout, (foldout ? "" : label));
			EditorGUIUtility.LookLikeControls();
			if (foldout) GUILayout.Label(label, s);
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		return foldout;
	}

	public static System.Enum EnumPopupWithHelp(string label, System.Enum selected, string help)
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label(label);
		EditorGUILayout.Space();
		System.Enum ret = EditorGUILayout.EnumPopup(selected);
		EditorGUILayout.Space();
		GUILayout.Label(new GUIContent(Icon_Help, help));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		return ret;
	}

	// Show a Layer Mask field similar to the one in Unity Editor
	private static List<string> _layerMaskField_layers;
	private static List<int> _layerMaskField_layerNumbers;
	private static string[] _layerMaskField_layerNames;
	private static long _layerMaskField_lastUpdateTick;
	public static LayerMask LayerMaskField(string label, LayerMask selected)
	{
		if (_layerMaskField_layers == null || (System.DateTime.Now.Ticks - _layerMaskField_lastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
		{
			_layerMaskField_lastUpdateTick = System.DateTime.Now.Ticks;
			if (_layerMaskField_layers == null)
			{
				_layerMaskField_layers = new List<string>();
				_layerMaskField_layerNumbers = new List<int>();
				_layerMaskField_layerNames = new string[5];
			}
			else
			{
				_layerMaskField_layers.Clear();
				_layerMaskField_layerNumbers.Clear();
			}

			int emptyLayers = 0;
			for (int i = 0; i < 32; i++)
			{
				string layerName = LayerMask.LayerToName(i);
				if (layerName != "")
				{
					for (; emptyLayers > 0; emptyLayers--) _layerMaskField_layers.Add("Layer " + (i - emptyLayers));
					_layerMaskField_layerNumbers.Add(i);
					_layerMaskField_layers.Add(layerName);
				}
				else
				{
					emptyLayers++;
				}
			}
			if (_layerMaskField_layerNames.Length != _layerMaskField_layers.Count)
			{
				_layerMaskField_layerNames = new string[_layerMaskField_layers.Count];
			}
			for (int i = 0; i < _layerMaskField_layerNames.Length; i++) _layerMaskField_layerNames[i] = _layerMaskField_layers[i];
		}
		selected.value = EditorGUILayout.MaskField(label, selected.value, _layerMaskField_layerNames);
		return selected;
	}

	public static void IntVector2Field(string head, string label1, string label2, ref int val1, ref int val2, int maxFieldWidth)
	{
		GUILayout.Label(head, EditorStyles.label);
		EditorGUILayout.BeginHorizontal();
		{
			float w = (maxFieldWidth - 21) / 4f;
			GUILayout.Space(10);
			GUILayout.Label(label1, EditorStyles.label, GUILayout.Width(w));
			GUILayout.Space(3);
			val1 = EditorGUILayout.IntField(val1, GUILayout.Width(w));
			GUILayout.Space(5);
			GUILayout.Label(label2, EditorStyles.label, GUILayout.Width(w));
			GUILayout.Space(3);
			val2 = EditorGUILayout.IntField(val2, GUILayout.Width(w));
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
	}

	public static Vector2 Vector2Field(string head, string label1, string label2, Vector2 val, int maxFieldWidth)
	{
		GUILayout.Label(head, EditorStyles.label);
		EditorGUILayout.BeginHorizontal();
		{
			float w = (maxFieldWidth - 21) / 4f;
			GUILayout.Space(10);
			GUILayout.Label(label1, EditorStyles.label, GUILayout.Width(w));
			GUILayout.Space(3);
			val.x = EditorGUILayout.FloatField(val.x, GUILayout.Width(w));
			GUILayout.Space(5);
			GUILayout.Label(label2, EditorStyles.label, GUILayout.Width(w));
			GUILayout.Space(3);
			val.y = EditorGUILayout.FloatField(val.y, GUILayout.Width(w));
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		return val;
	}

	public static int ButtonToolBar(EditorWindow ed, List<UniRPGEditorToolbar.ToolbarButton> buttons, ref bool[] foldout, GUIStyle buttonStyle, GUIStyle foldStyleH, GUIStyle foldStyleV, float buttonWH, float buttonPadding = 0f)
	{
		int ret = -1;
		if (foldout == null)
		{
			int count = 0;
			for (int i = 0; i < buttons.Count; i++) if (buttons[i].callback==null) count++;
			foldout = new bool[count + 1];
			for (int i = 0; i < foldout.Length; i++) foldout[i] = true;
		}

		int xCount = (int)(ed.position.width / (buttonWH + buttonPadding));
		if (xCount < 1) xCount = 1;

		Rect r = new Rect(0f, 0f, buttonWH, buttonWH);
		Rect fr = new Rect(0f, 0f, buttonWH, buttonWH);
		int foldCount = 1;
		for (int i = 0; i < buttons.Count; i++)
		{
			if (buttons[i].callback == null)
			{	// draw spacer/foldout
				if (xCount > 1)
				{	// expand horizontally

					fr.x = r.x + foldStyleH.margin.left;
					fr.y = r.y;
					fr.width = foldStyleH.fixedWidth;
					fr.height = foldStyleH.fixedHeight;

					if (ed.position.width <= (fr.x + fr.width + foldStyleH.margin.right))
					{
						r.x = 0f;
						r.y += buttonWH + buttonPadding;
						fr.x = r.x;
						fr.y = r.y;
					}

					foldout[foldCount] = EditorGUI.Foldout(fr, foldout[foldCount], GUIContent.none, foldStyleH);
					r.x += foldStyleH.fixedWidth + foldStyleH.margin.right;

					if (ed.position.width <= (r.x + buttonWH + buttonPadding))
					{
						r.x = 0f;
						r.y += buttonWH + buttonPadding;
					}

				}
				else
				{	// expand vertically

					fr.x = foldStyleV.margin.left;
					fr.y = r.y + foldStyleV.margin.top;
					fr.width = foldStyleV.fixedWidth;
					fr.height = foldStyleV.fixedHeight;

					foldout[foldCount] = EditorGUI.Foldout(fr, foldout[foldCount], GUIContent.none, foldStyleV);
					r.y += foldStyleV.fixedHeight + foldStyleV.margin.bottom;
				}

				foldCount++;
			}
			else
			{	// draw button

				if (!foldout[foldCount - 1]) continue;

				if (GUI.Button(r, buttons[i].gui, buttonStyle)) ret = i;

				if (xCount > 1)
				{
					r.x += buttonWH + buttonPadding;
					if (ed.position.width <= (r.x + buttonWH + buttonPadding))
					{
						r.x = 0f;
						r.y += buttonWH + buttonPadding;
					}
				}
				else
				{
					r.x = 0f;
					r.y += buttonWH + buttonPadding;
				}
			}
		}
		return ret;
	}

	#endregion
	// ================================================================================================================
	#region Layout

	// this scrollview makes al lattempts to hide the bottom/horizontal bar while always showing the vertical/right bar
	public static Vector2 BeginScrollView(Vector2 scroll, bool hideAllBars, params GUILayoutOption[] options)
	{
		return BeginScrollView(scroll, hideAllBars, null, options);
	}
	public static Vector2 BeginScrollView(Vector2 scroll, bool hideAllBars, GUIStyle backgroundStyle, params GUILayoutOption[] options)
	{
		if (!hideAllBars) return BeginScrollView(scroll, backgroundStyle, options);
		return EditorGUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none, UniRPGEdGui.Skin.scrollView, options);
	}
	public static Vector2 BeginScrollView(Vector2 scroll, params GUILayoutOption[] options)
	{
		return BeginScrollView(scroll, null, options);
	}
	public static Vector2 BeginScrollView(Vector2 scroll, GUIStyle backgroundStyle, params GUILayoutOption[] options)
	{
		return EditorGUILayout.BeginScrollView(scroll, false, true, GUIStyle.none, UniRPGEdGui.Skin.verticalScrollbar, (backgroundStyle==null?UniRPGEdGui.Skin.scrollView:backgroundStyle), options);
	}
	public static void EndScrollView()
	{
		GUILayout.Space(30);
		EditorGUILayout.EndScrollView();
	}
	
	#endregion
	// ================================================================================================================
	#region Line/Rect/Grid/etc drawing

	public static void DrawHorizontalLine(float thickness, Color color, float paddingTop = 0f, float paddingBottom = 0f, float width = 0f)
	{
		GUILayoutOption[] options = new GUILayoutOption[2]
		{ 
			GUILayout.ExpandHeight(false), 
			(width > 0.0f ? GUILayout.Width(width) : GUILayout.ExpandWidth(true))
		};
		Color prevColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUILayoutUtility.GetRect(0f, (thickness + paddingTop + paddingBottom), options);
		Rect r = GUILayoutUtility.GetLastRect();
		r.y += paddingTop;
		r.height = thickness;
		GUI.Box(r, "", UniRPGEdGui.DividerStyle);
		GUI.backgroundColor = prevColor;
	}

	public static void DrawVerticalLine(float thickness, Color color, float paddingLeft = 0f, float paddingRight = 0f, float height = 0f)
	{
		GUILayoutOption[] options = new GUILayoutOption[2]
		{ 
			GUILayout.ExpandWidth(false), 
			(height > 0.0f ? GUILayout.Height(height) : GUILayout.ExpandHeight(true))
		};
		Color prevColor = GUI.backgroundColor;
		GUI.backgroundColor = color;
		GUILayoutUtility.GetRect((thickness + paddingLeft + paddingRight), 0f, options);
		Rect r = GUILayoutUtility.GetLastRect();
		r.x += paddingLeft;
		r.width = thickness;
		GUI.Box(r, "", UniRPGEdGui.DividerStyle);
		GUI.backgroundColor = prevColor;
	}

	#endregion
	// ================================================================================================================
	#region 3D drawing, using Handles (these are normally used in the editor scene view)


	#endregion
	// ================================================================================================================
	#region Internal Helpers


	#endregion
	// ================================================================================================================
} }