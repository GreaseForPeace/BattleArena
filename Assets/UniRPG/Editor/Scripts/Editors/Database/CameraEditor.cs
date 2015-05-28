// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using UniRPG;

namespace UniRPGEditor {

[DatabaseEditor("Cameras", Priority = 2)]
public class CameraEditor : DatabaseEdBase
{

	private Vector2[] scroll = { Vector2.zero, Vector2.zero };
	private GameCameraBase curr = null;
	private GameCameraBase del = null;
	private GameCameraEditorBase currEd = null;

	// ================================================================================================================

	public override void OnEnable(DatabaseEditor ed)
	{
		base.OnEnable(ed);
		
		// make sure there are not any NULL values in the list
		int cnt = ed.db.cameraPrefabs.Count;
		ed.db.cameraPrefabs = UniRPGUtil.CleanupList<GameObject>(ed.db.cameraPrefabs);
		if (cnt != ed.db.cameraPrefabs.Count)
		{
			EditorUtility.SetDirty(ed.db);
			AssetDatabase.SaveAssets();
		}
	}

	public override void OnGUI(DatabaseEditor ed)
	{
		base.OnGUI(ed);
		EditorGUILayout.BeginHorizontal();
		{
			LeftPanel();
			UniRPGEdGui.DrawVerticalLine(2f, UniRPGEdGui.DividerColor, 0f, 10f);
			RightPanel();
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void LeftPanel()
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		GUILayout.Space(5);
		// -------------------------------------------------------------

		// the add button
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(new GUIContent("Add Camera", UniRPGEdGui.Icon_Plus), EditorStyles.miniButtonLeft))
			{
				GUI.FocusControl("");
				CameraSelectWiz.Show(OnCameraSelected);
			}
			if (curr == null) GUI.enabled = false;
			if (GUILayout.Button(new GUIContent(UniRPGEdGui.Icon_Copy, "Copy"), EditorStyles.miniButtonMid))
			{
				GUI.FocusControl("");
				GameCameraBase cam = CameraEditor.CreateCamera(curr.name, curr.GetType());
				curr.CopyTo(cam);
				cam.id = GUID.Create();
				curr = cam;
				ed.db.cameraPrefabs.Add(curr.gameObject);
				GetCurrentEd();
			}
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		scroll[0] = UniRPGEdGui.BeginScrollView(scroll[0], GUILayout.Width(DatabaseEditor.LeftPanelWidth));
		{
			if (ed.db.Cameras.Length > 0)
			{
				foreach (GameCameraBase cam in ed.db.Cameras)
				{
					if (cam == null)
					{	// detected a null value, 1st cleanup the list
						ed.db.cameraPrefabs = UniRPGUtil.CleanupList<GameObject>(ed.db.cameraPrefabs);
						EditorUtility.SetDirty(ed.db);
						AssetDatabase.SaveAssets();
						GUIUtility.ExitGUI();
						return;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(DatabaseEditor.LeftPanelWidth - 20), GUILayout.ExpandWidth(false));
					{
						if (UniRPGEdGui.ToggleButton(curr == cam, cam.name, UniRPGEdGui.ButtonLeftStyle, GUILayout.Width(160), GUILayout.ExpandWidth(false)))
						{
							GUI.FocusControl("");
							curr = cam;
							GetCurrentEd();
						}

						//if (ed.db.Cameras.Length == 1) GUI.enabled = false; // can't allow deleting the camera if there is only one left since runtime depends on at least one being present
						if (GUILayout.Button("X", UniRPGEdGui.ButtonRightStyle, GUILayout.Width(20))) del = cam;
						//GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.Label("No Cameras are defined.\nThis is acceptable but then\nyou need to add your own\nMain Camera to the game\nscene(s).", UniRPGEdGui.WarningLabelStyle);
			}
		}
		UniRPGEdGui.EndScrollView(); // 0

		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();

		if (del != null)
		{
			if (curr == del) { curr = null; currEd = null; }
			ed.db.cameraPrefabs.Remove(del.gameObject);
			EditorUtility.SetDirty(ed.db);
			AssetDatabase.SaveAssets();

			string path = AssetDatabase.GetAssetPath(del.gameObject);
			AssetDatabase.DeleteAsset(path);
			AssetDatabase.Refresh();
			del = null;
		}
	}

	private void OnCameraSelected(System.Object sender)
	{
		CameraSelectWiz wiz = sender as CameraSelectWiz;
		if (wiz.selected != null)
		{
			curr = CameraEditor.CreateCamera(wiz.selected.name, wiz.selected.cameraType);
			ed.db.cameraPrefabs.Add(curr.gameObject);
			EditorUtility.SetDirty(ed.db);
			AssetDatabase.SaveAssets();
			GetCurrentEd();
		}
		wiz.Close();
	}

	public static GameCameraBase CreateCamera(string name, System.Type camType)
	{
		string fn = UniRPGEditorGlobal.DB_CAMERAS_PATH + name + ".prefab";
		if (UniRPGEdUtil.RelativeFileExist(fn)) fn = AssetDatabase.GenerateUniqueAssetPath(fn);

		Object prefab = PrefabUtility.CreateEmptyPrefab(fn);
		GameObject go = new GameObject(name);							// create temp object in scene 
		GameCameraBase c = (GameCameraBase)go.AddComponent(camType);
		c.name = name; c.id = new GUID(); c.id = GUID.Create();
		GameObject toRef = PrefabUtility.ReplacePrefab(go, prefab);		// save prefab
		GameObject.DestroyImmediate(go);								// clear temp object from scene
		return (GameCameraBase)toRef.GetComponent(camType);
	}

	private void GetCurrentEd()
	{
		currEd = null;
		foreach (UniRPGCameraEdInfo camNfo in UniRPGEditorGlobal.CameraEditors)
		{
			if (camNfo.cameraType.IsAssignableFrom(curr.GetType()))
			{
				currEd = camNfo.editor;
			}
		}
	}

	private void RightPanel()
	{
		EditorGUILayout.BeginVertical();
		GUILayout.Space(5);
		// -------------------------------------------------------------

		GUILayout.Label("Camera Definition", UniRPGEdGui.Head1Style);
		if (curr == null) { EditorGUILayout.EndVertical(); return; }
		scroll[1] = EditorGUILayout.BeginScrollView(scroll[1]);

		EditorGUILayout.BeginVertical(UniRPGEdGui.BoxStyle, GUILayout.Width(400));
		{
			EditorGUIUtility.LookLikeControls(160);
			EditorGUILayout.LabelField("Camera Type", curr.GetType().ToString());
			curr.name = EditorGUILayout.TextField("Name", curr.name);
			curr.cameraPrefab = (GameObject)EditorGUILayout.ObjectField("Camera Prefab (optional)", curr.cameraPrefab, typeof(GameObject), false);
			curr.includeFlareLayer = EditorGUILayout.Toggle("Include FlareLayer", curr.includeFlareLayer);
			curr.includeGuiLayer = EditorGUILayout.Toggle("Include GUILayer", curr.includeGuiLayer);
			curr.includeAudioListener = EditorGUILayout.Toggle("Iclude AudioListener", curr.includeAudioListener);
			EditorGUIUtility.LookLikeControls();
		}
		EditorGUILayout.EndVertical();

		if (currEd != null) currEd.OnGUI(ed, curr);

		// check if data changed and should be saved
		if (GUI.changed && curr != null)
		{
			EditorUtility.SetDirty(curr);
		}

		EditorGUILayout.EndScrollView();//1
		// -------------------------------------------------------------
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();
	}

	// ================================================================================================================
} }