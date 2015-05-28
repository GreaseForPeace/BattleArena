// Misc
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniRPGEditor
{
	public class FindMissingScripts : EditorWindow 
	{
		[MenuItem("UniRPG/Misc/Find Objects with Missing Scripts")]
		public static void Show_EdTest()
		{
			EditorWindow.GetWindow<FindMissingScripts>(true, "Find Missing Scripts", true);
		}

		private Vector2 scroll = Vector2.zero;

		public class Info
		{
			public string objName;
			public string path;
			public GameObject obj;
			public GameObject mainObj;
			public string scene;
		}

		private bool hasRun = false;
		private List<Info> r = new List<Info>();

		public void OnGUI()
		{
			if (GUILayout.Button("Run"))
			{
				GUI.FocusControl("");
				ProcessObjects();
				GUIUtility.ExitGUI();
				return;
			}
			EditorGUILayout.Space();

			if (r.Count > 0)
			{
				scroll = EditorGUILayout.BeginScrollView(scroll);
				{
					foreach (Info nfo in r)
					{
						EditorGUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("*", GUILayout.Width(25)))
							{
								if (!string.IsNullOrEmpty(nfo.scene))
								{
									if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
									{
										if (EditorApplication.OpenScene(nfo.scene))
										{
											GameObject go = GameObject.Find(nfo.objName);
											EditorGUIUtility.PingObject(go);
										}
									}
								}
								else
								{
									EditorGUIUtility.PingObject(nfo.mainObj);
								}
							}

							GUILayout.Label(nfo.path);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.EndScrollView();
			}

			else if (hasRun)
			{
				GUILayout.Label("All good");
			}
		}

		private string ProjectRelativePath(string fullPath)
		{
			fullPath = fullPath.Replace("\\", "/");
			return ("Assets" + fullPath.Remove(0, Application.dataPath.Length));
		}

		private void ProcessObjects()
		{
			string currScene = EditorApplication.currentScene;
			if (!EditorApplication.SaveCurrentSceneIfUserWantsTo()) return;

			r = new List<Info>();
			string progressbarTitle = "Please wait";
			string progressbarInfo = "Searching ...";

			EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, 0f);

			// First look in all prefabs
			progressbarInfo = "Looking in Prefabs ...";
			DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
			FileInfo[] files = dir.GetFiles("*.prefab", SearchOption.AllDirectories);
			string fn = "";
			float progress = 0f;
			float step = 1f / (float)files.Length;
			EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, progress);

			for (int i = 0; i < files.Length; i++)
			{
				progress += step;
				EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, progress);
				if (files[i] == null) continue;
				fn = ProjectRelativePath(files[i].FullName);
				if (!string.IsNullOrEmpty(fn))
				{
					GameObject obj = AssetDatabase.LoadAssetAtPath(fn, typeof(GameObject)) as GameObject;
					CheckGo(null, fn, obj);
				}
			}
			EditorUtility.ClearProgressBar();

			// Now look in all scenes
			progressbarInfo = "Looking in Scenes ...";
			files = dir.GetFiles("*.unity", SearchOption.AllDirectories);
			fn = "";
			progress = 0f;
			step = 1f / (float)files.Length;
			EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, progress);

			for (int i = 0; i < files.Length; i++)
			{
				progress += step;
				EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, progress);
				if (files[i] == null) continue;
				fn = ProjectRelativePath(files[i].FullName);
				if (!string.IsNullOrEmpty(fn))
				{
					if (EditorApplication.OpenScene(fn))
					{
						Object[] objs = GameObject.FindObjectsOfType(typeof(GameObject)); 
						for (int j = 0; j < objs.Length; j++)
						{
							CheckGo(fn, fn, objs[j] as GameObject);
						}
					}
				}
			}

			EditorUtility.ClearProgressBar();

			if (string.IsNullOrEmpty(currScene)) EditorApplication.NewScene();
			else EditorApplication.OpenScene(currScene);

			hasRun = true;
		}

		private void CheckGo(string scene, string fn, GameObject g)
		{
			if (g == null) return;

			if (!Present(g))
			{
				Component[] components = g.GetComponents<Component>();
				for (int j = 0; j < components.Length; j++)
				{
					if (components[j] == null) 
					{
						string path = GetPath(g.transform, "");

						r.Add(new Info() 
						{ 
							mainObj = PrefabUtility.FindPrefabRoot(g),
							obj = g,
							objName = path + g.name,
							path = fn + ": " + path + g.name,
							scene = scene,
						});
						break; 
					}
				}
			}

			for (int i = 0; i < g.transform.childCount; i++)
			{
				CheckGo(scene, fn, g.transform.GetChild(i).gameObject);
			}
		}

		private string GetPath(Transform t, string path)
		{
			if (t.parent == null) return path;
			return GetPath(t.parent, t.parent.name + "/" + path);
		}

		private bool Present(GameObject obj)
		{
			foreach (Info nfo in r)
			{
				if (nfo.obj == obj) return true;
			}
			return false;
		}
	}
}