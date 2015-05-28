// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================
	
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace DiaQEditor
{
	public class DiaQEdGUI
	{
		public const string EditorResourcePath = "Assets/DiaQ/Editor/Res/";

		// Colours
		public static Color Col_SelectedNode = new Color(0.4f, 0.7f, 1f, 1f);
		public static Color Col_Back = new Color(0.16f, 0.16f, 0.16f, 1f);
		public static Color Col_LinkBlue = new Color(0.074f, 0.140f, 0.3f, 1f);
		public static Color Col_LinkGreen = new Color(0.082f, 0.3f, 0.074f, 1f);
		public static Color Col_LinkRed = new Color(0.3f, 0.074f, 0.078f, 1f);
		public static Color Col_LinkYellow = new Color(0.3f, 0.3f, 0.074f, 1f);
		public static Color Col_Comment = new Color(0.95f, 0.95f, 0.95f, 0.85f);

		// Styles
		public static GUIStyle AboutLogoAreaStyle;
		public static GUIStyle LineStyle;
		public static GUIStyle SplitterStyle;
		public static GUIStyle NodeWindowStyle;
		public static GUIStyle ButtonLeftStyle;
		public static GUIStyle ButtonMidStyle;
		public static GUIStyle ButtonRightStyle;
		public static GUIStyle ButtonNormStyle;
		public static GUIStyle IconButtonStyle;
		public static GUIStyle RightAlignTextStyle;
		public static GUIStyle LeftAlignTextStyle;
		public static GUIStyle TextAreaStyle;
		public static GUIStyle CommentBoxStyle;

		// Skin Resources
		public static Texture2D Texture_Logo;
		public static Texture2D Texture_Bezier;
		public static Texture2D Texture_FlatBack;
		
		// Icons
		public static Texture2D Icon_Help;
		public static Texture2D Icon_ResetView;
		public static Texture2D Icon_Tag;

		public static Texture2D Icon_Play;
		public static Texture2D Icon_Dialogue;
		public static Texture2D Icon_Decision;
		public static Texture2D Icon_Quest;
		public static Texture2D Icon_Stop;
		public static Texture2D Icon_Bug;
		public static Texture2D Icon_Script;
		public static Texture2D Icon_Variable;
		public static Texture2D Icon_Reward;
		public static Texture2D Icon_Dice;
		public static Texture2D Icon_Check;
		public static Texture2D Icon_Event;
		public static Texture2D Icon_QuestCheck;

		public static Texture2D Icon_Unlink;
		public static Texture2D Icon_ConnectionIn;
		public static Texture2D Icon_ArrowBlue;
		public static Texture2D Icon_ArrowGreen;
		public static Texture2D Icon_ArrowRed;
		public static Texture2D Icon_ArrowGrey;
		public static Texture2D Icon_ArrowYellow;

		// ...
		public static GUISkin Skin = null;
		private static GUIStyle[] customStyles;

		// ============================================================================================================		

		// Checks that the skin is loaded for use by DiaQ Editor OnGUI
		public static void UseSkin()
		{
			if (EditorGUIUtility.isProSkin)
			{	// fix a bug present in EditorStyles.miniButtonMid
				EditorStyles.miniButtonMid.border = new RectOffset(2, 2, 0, 0);
			}
			else
			{
				Col_Back = new Color(0.633f, 0.633f, 0.633f, 1f);
			}

			if (Skin != null)
			{	// is ready to use
				GUI.skin = Skin;
				return;
			}

			Skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			customStyles = Skin.customStyles;
			LoadResources();

			// ------------------------------------------------------------------------------------------------------------

			Col_Comment = Vector4.Scale(GUI.color, Col_Comment);
			Col_Comment.a = 0.85f;

			// ------------------------------------------------------------------------------------------------------------
			// Define Styles

			AboutLogoAreaStyle = new GUIStyle(Skin.box)
			{
				name = "PLYAboutLogoAreaStyle",
				stretchWidth = true,
				margin = new RectOffset(0, 0, 0, 10),
				padding = new RectOffset(10, 10, 10, 10),
				normal = { background = EditorGUIUtility.whiteTexture },
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, AboutLogoAreaStyle);

			LineStyle = new GUIStyle()
			{
				name = "PLYLineStyle",
				normal = { background = EditorGUIUtility.whiteTexture },
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, LineStyle);

			SplitterStyle = new GUIStyle()
			{
				name = "PLYSplitterStyle",
				normal = { background = Texture_FlatBack },
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, SplitterStyle);

			NodeWindowStyle = new GUIStyle(Skin.window)
			{
				name = "PLYNodeWindowStyle",
				onNormal = { background = Skin.window.normal.background },
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, NodeWindowStyle);

			ButtonLeftStyle = new GUIStyle(EditorStyles.miniButtonLeft)
			{
				name = "PLYButtonLeftStyle",
				richText = false,
				fontSize = 11,
				padding = new RectOffset(0, 0, 3, 5),
				margin = new RectOffset(2, 0, 0, 2),
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonLeftStyle);

			ButtonMidStyle = new GUIStyle(EditorStyles.miniButtonMid)
			{
				name = "PLYButtonMidStyle",
				richText = false,
				fontSize = 11,
				padding = new RectOffset(0, 0, 3, 5),
				margin = new RectOffset(0, 0, 0, 2),
				border = new RectOffset(2, 2, 0, 0), // fix a bug present in EditorStyles.miniButtonMid
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonMidStyle);

			ButtonRightStyle = new GUIStyle(EditorStyles.miniButtonRight)
			{
				name = "PLYButtonRightStyle",
				richText = false,
				fontSize = 11,
				padding = new RectOffset(0, 0, 3, 5),
				margin = new RectOffset(0, 2, 0, 2),
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonRightStyle);

			ButtonNormStyle = new GUIStyle(EditorStyles.miniButton)
			{
				name = "PLYButtonNormStyle",
				richText = false,
				fontSize = 11,
				padding = new RectOffset(0, 0, 3, 5),
				margin = new RectOffset(2, 2, 0, 2),
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, ButtonNormStyle);

			IconButtonStyle = new GUIStyle()
			{
				name = "PLYIconButtonStyle",
				padding = new RectOffset(0, 0, 0, 0),
				margin = new RectOffset(0, 0, 0, 0),
				fixedWidth = 16,
				fixedHeight = 16,
				imagePosition = ImagePosition.ImageOnly,
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, IconButtonStyle);

			RightAlignTextStyle = new GUIStyle(Skin.label)
			{
				name = "PLYRightAlignTextStyle",
				alignment = TextAnchor.MiddleRight,
				fontStyle = FontStyle.Bold,
				fontSize = 10
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, RightAlignTextStyle);

			LeftAlignTextStyle = new GUIStyle(Skin.label)
			{
				name = "PLYLeftAlignTextStyle",
				alignment = TextAnchor.MiddleLeft,
				fontStyle = FontStyle.Bold,
				fontSize = 10
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, LeftAlignTextStyle);

			TextAreaStyle = new GUIStyle(Skin.textArea)
			{
				name = "PLYTextAreaStyle",
				wordWrap = true
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, TextAreaStyle);

			Color c = Skin.box.normal.textColor; c.a = 1f;
			CommentBoxStyle = new GUIStyle(Skin.box)
			{
				name = "PLYCommentBoxStyle",
				alignment = TextAnchor.UpperLeft,
				wordWrap = true,
				normal = { textColor = c },
			}; ArrayUtility.Add<GUIStyle>(ref customStyles, CommentBoxStyle);

			// ------------------------------------------------------------------------------------------------------------
			// Done

			Skin.customStyles = customStyles;
			GUI.skin = Skin;
		}

		private static void LoadResources()
		{
			Texture_Logo = LoadEditorTexture(EditorResourcePath + "Skin/logo.png");
			Texture_Bezier = LoadEditorTexture(EditorResourcePath + "Skin/bezier.png", FilterMode.Bilinear);
			Texture_FlatBack = LoadEditorTexture(EditorResourcePath + "Skin/flat_back" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png");

			Icon_Help = LoadEditorTexture(EditorResourcePath + "Icons/help.png");
			Icon_ResetView = LoadEditorTexture(EditorResourcePath + "Icons/reset_view.png");
			Icon_Tag = LoadEditorTexture(EditorResourcePath + "Icons/tag.png");

			Icon_Play = LoadEditorTexture(EditorResourcePath + "Icons/play.png");
			Icon_Dialogue = LoadEditorTexture(EditorResourcePath + "Icons/dialogue.png");
			Icon_Decision = LoadEditorTexture(EditorResourcePath + "Icons/decision.png");
			Icon_Quest = LoadEditorTexture(EditorResourcePath + "Icons/quest.png");
			Icon_Stop = LoadEditorTexture(EditorResourcePath + "Icons/stop.png");
			Icon_Bug = LoadEditorTexture(EditorResourcePath + "Icons/bug.png");
			Icon_Script = LoadEditorTexture(EditorResourcePath + "Icons/script.png");
			Icon_Variable = LoadEditorTexture(EditorResourcePath + "Icons/variable.png");
			Icon_Reward = LoadEditorTexture(EditorResourcePath + "Icons/reward.png");
			Icon_Dice = LoadEditorTexture(EditorResourcePath + "Icons/dice.png");
			Icon_Check = LoadEditorTexture(EditorResourcePath + "Icons/check.png");
			Icon_Event = LoadEditorTexture(EditorResourcePath + "Icons/event.png");
			Icon_QuestCheck = LoadEditorTexture(EditorResourcePath + "Icons/questcheck.png");

			Icon_Unlink = LoadEditorTexture(EditorResourcePath + "Icons/unlink.png");
			Icon_ConnectionIn = LoadEditorTexture(EditorResourcePath + "Icons/bullet_connect.png");
			Icon_ArrowBlue = LoadEditorTexture(EditorResourcePath + "Icons/bullet_arrow_blue.png");
			Icon_ArrowGreen = LoadEditorTexture(EditorResourcePath + "Icons/bullet_arrow_green.png");
			Icon_ArrowRed = LoadEditorTexture(EditorResourcePath + "Icons/bullet_arrow_red.png");
			Icon_ArrowGrey = LoadEditorTexture(EditorResourcePath + "Icons/bullet_arrow_grey.png");
			Icon_ArrowYellow = LoadEditorTexture(EditorResourcePath + "Icons/bullet_arrow_yellow.png");
		}

		public static Texture2D LoadEditorTexture(string fn, FilterMode filter = FilterMode.Point)
		{
			Texture2D tx = AssetDatabase.LoadAssetAtPath(fn, typeof(Texture2D)) as Texture2D;
			if (tx == null) Debug.LogWarning("Failed to load texture: " + fn);
			else if (tx.wrapMode != TextureWrapMode.Clamp)
			{
				string path = AssetDatabase.GetAssetPath(tx);
				TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
				tImporter.textureType = TextureImporterType.GUI;
				tImporter.npotScale = TextureImporterNPOTScale.None;
				tImporter.filterMode = filter;
				tImporter.wrapMode = TextureWrapMode.Clamp;
				tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				EditorApplication.SaveAssets();
			}
			return tx;
		}

		// ============================================================================================================		
		// Controls

		public static bool ToggleButton(bool active, string label, GUIStyle style, params GUILayoutOption[] options)
		{
			bool new_value = active;
			new_value = GUILayout.Toggle(active, label, style, options);
			if (new_value != active) GUI.FocusControl("");
			return (new_value != active);
		}

		// ============================================================================================================		
		// Drawing

		public static bool DrawNodeCurve(Rect containedArea, Vector3 startPos, Vector3 endPos, Color color, float thickness = 1f, bool drawShadow = true, bool deleteButton = true)
		{
			// first check if is clipped and dont draw if not needed
			float left = startPos.x < endPos.x ? startPos.x : endPos.x;
			float right = startPos.x > endPos.x ? startPos.x : endPos.x;
			float top = startPos.y < endPos.y ? startPos.y : endPos.y;
			float bottom = startPos.y > endPos.y ? startPos.y : endPos.y;
			Rect bounds = new Rect(left, top, right - left, bottom - top);
			if (bounds.xMin > containedArea.xMax
				|| bounds.xMax < containedArea.xMin
				|| bounds.yMin > containedArea.yMax
				|| bounds.yMax < containedArea.yMin)
			{
				return false;
			}

			float distance = Mathf.Abs(startPos.x - endPos.x);
			Vector3 startTan = new Vector3(startPos.x + distance / 2.5f, startPos.y);
			Vector3 endTan = new Vector3(endPos.x - distance / 2.5f, endPos.y);
			
			// draw shadow
			if (drawShadow)
			{
				Color shadowCol = new Color(0, 0, 0, 0.1f);
				for (int i = 0; i < 3; i++) Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * thickness * 3f);
			}

			// draw line
			Handles.DrawBezier(startPos, endPos, startTan, endTan, color, Texture_Bezier, thickness);

			if (deleteButton)
			{
				Rect r = new Rect(startPos.x + (endPos.x - startPos.x) * 0.5f, startPos.y + (endPos.y - startPos.y) * 0.5f, 16, 16);
				if (GUI.Button(r, new GUIContent(Icon_Unlink), IconButtonStyle)) return true;
			}
			return false;
		}

		public static void DrawHorizontalLine(float thickness, Color color, float paddingTop = 0f, float paddingBottom = 0f, float width = 0f)
		{
			GUILayoutOption[] options = new GUILayoutOption[2] { GUILayout.ExpandHeight(false), (width > 0.0f ? GUILayout.Width(width) : GUILayout.ExpandWidth(true)) };
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = color;
			GUILayoutUtility.GetRect(0f, (thickness + paddingTop + paddingBottom), options);
			Rect r = GUILayoutUtility.GetLastRect();
			r.y += paddingTop;
			r.height = thickness;
			GUI.Box(r, GUIContent.none, LineStyle);
			GUI.backgroundColor = prevColor;
		}

		// ============================================================================================================
	}
}