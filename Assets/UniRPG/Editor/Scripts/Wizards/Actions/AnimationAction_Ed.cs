// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

[ActionInfo(typeof(AnimationAction), "Object/Animation: Play or Stop", Description = "Play or Stop an animation")]
public class AnimationAction_Ed : ActionsEdBase
{
	private string[] aniClipNames = new string[0];
	private Vector2 scroll = Vector2.zero;

	public override string ActionShortNfo(Object actionObj)
	{
		AnimationAction action = actionObj as AnimationAction;
		if (action == null) return "!ERROR!";
		System.Text.StringBuilder s = new System.Text.StringBuilder();
		if (action.doWhat == AnimationAction.DoWhat.Stop) s.Append("Stop Animations");
		else
		{
			if (action.useRandomClips) s.AppendFormat("Start an Animation");
			else s.AppendFormat("Start Animation ({0})", action.clipName.GetValOrName());
		}
		if (!action.useRandomClips) s.AppendFormat(" on {0} {1}", action.subject.type, action.reversed ? "in Reverse" : "");
		return s.ToString();
	}

	public override void OnGUI(Object actionObj)
	{
		AnimationAction action = actionObj as AnimationAction;
		if (action == null) { GUILayout.Label("Error: Delete this Action."); return; }

		UniRPGEdGui.TargetTypeField(this.ed, "Subject", action.subject, TargetTypeHelp);
		EditorGUILayout.Space();

		if (action.subject.type == Action.TargetType.Self)
		{
			if (aniClipNames.Length == 0) UpdateClipCache(action.gameObject);
		}
		else if (aniClipNames.Length > 0) aniClipNames = new string[0];

		if (GUILayout.Toggle(action.doWhat == AnimationAction.DoWhat.Stop, " Stop Animation")) action.doWhat = AnimationAction.DoWhat.Stop;
		if (GUILayout.Toggle(action.doWhat == AnimationAction.DoWhat.Play, " Play Animation")) action.doWhat = AnimationAction.DoWhat.Play;
		if (action.doWhat == AnimationAction.DoWhat.Play)
		{
			EditorGUILayout.BeginHorizontal();
			{
				action.useRandomClips = GUILayout.Toggle(action.useRandomClips, "Use Random Clips");
				if (action.useRandomClips)
				{
					EditorGUILayout.Space();
					if (GUILayout.Button("add clip", EditorStyles.miniButton))
					{
						action.aniList.Add(new AnimationAction.AniInfo());
						EditorUtility.SetDirty(action);
					}
				}
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			if (action.useRandomClips)
			{
				if (action.aniList.Count == 0)
				{
					action.aniList.Add(new AnimationAction.AniInfo());
					EditorUtility.SetDirty(action);
				}

				AnimationAction.AniInfo del = null;
				EditorGUILayout.Space();
				scroll = UniRPGEdGui.BeginScrollView(scroll, UniRPGEdGui.ScrollViewNoTLMarginStyle, GUILayout.Height(190));
				foreach (AnimationAction.AniInfo a in action.aniList)
				{
					EditorGUILayout.BeginVertical(GUI.skin.box);
					{
						EditorGUILayout.BeginHorizontal();
						{
							a.crossFade = GUILayout.Toggle(a.crossFade, " Crossfade");
							GUILayout.FlexibleSpace();
							if (GUILayout.Button("x", EditorStyles.miniButton)) del = a;
						}
						EditorGUILayout.EndHorizontal();

						a.reversed = GUILayout.Toggle(a.reversed, " Play Backward (reversed)");
						if (aniClipNames.Length > 0)
						{
							EditorGUILayout.BeginHorizontal();
							{
								a.clipName = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Clip Name", a.clipName, 80);
								int aniIdx = ArrayUtility.IndexOf<string>(aniClipNames, a.clipName.GetValOrName());
								int prevAniIdx = aniIdx;
								aniIdx = EditorGUILayout.Popup(aniIdx, aniClipNames, GUILayout.Width(60));
								if (aniIdx != prevAniIdx && aniIdx >= 0) a.clipName.SetAsValue = aniClipNames[aniIdx];
							}
							EditorGUILayout.EndHorizontal();
						}
						else a.clipName = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Clip Name", a.clipName, 80);
						a.speed = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Speed", a.speed, 80);
						EditorGUIUtility.LookLikeControls(90);
						a.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode", a.wrapMode);
					}
					EditorGUILayout.EndVertical();
				}
				UniRPGEdGui.EndScrollView();

				if (del != null)
				{
					action.aniList.Remove(del);
					EditorUtility.SetDirty(action);
				}
			}

			else
			{
				if (action.aniList.Count > 0) action.aniList.Clear();

				EditorGUILayout.BeginVertical(GUI.skin.box);
				{
					action.crossFade = GUILayout.Toggle(action.crossFade, " Crossfade");
					action.reversed = GUILayout.Toggle(action.reversed, " Play Backward (reversed)");
					if (aniClipNames.Length > 0)
					{
						EditorGUILayout.BeginHorizontal();
						{
							action.clipName = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Clip Name", action.clipName, 80);
							int aniIdx = ArrayUtility.IndexOf<string>(aniClipNames, action.clipName.GetValOrName());
							int prevAniIdx = aniIdx;
							aniIdx = EditorGUILayout.Popup(aniIdx, aniClipNames, GUILayout.Width(60));
							if (aniIdx != prevAniIdx && aniIdx >= 0) action.clipName.SetAsValue = aniClipNames[aniIdx];
						}
						EditorGUILayout.EndHorizontal();
					}
					else action.clipName = UniRPGEdGui.GlobalStringVarOrValueField(this.ed, "Clip Name", action.clipName, 80);
					action.speed = UniRPGEdGui.GlobalNumericVarOrValueField(this.ed, "Speed", action.speed, 80);
					EditorGUIUtility.LookLikeControls(90);
					action.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode", action.wrapMode);
				}
				EditorGUILayout.EndVertical();
			}
		}
	}

	private void UpdateClipCache(GameObject obj)
	{
		// fill the aniClipNames cache
		Animation ani = obj.GetComponent<Animation>();
		if (ani)
		{
			List<string> tmp = new List<string>();
			foreach (AnimationState state in ani) tmp.Add(state.name);
			aniClipNames = tmp.ToArray();
		}
		else
		{
			aniClipNames = new string[0];
		}
	}

	// ================================================================================================================
} }