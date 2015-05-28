// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

public class UniRPGEdUtil 
{
	// ================================================================================================================
	#region File/Path

	/// <summary>the full path of the project (just before the "Assets" folder of the project) (excludes last slash)</summary>
	public static string FullProjectPath
	{	
		get
		{
			if (_projectPath == null)
			{
				_projectPath = Application.dataPath;
				_projectPath = _projectPath.Substring(0, _projectPath.LastIndexOf("Assets"));
			}
			return _projectPath;
		}
	}
	private static string _projectPath;

	/// <summary>the full path up to to the "Assets" folder of the project (excludes last slash)</summary>
	public static string FullProjectAssetsPath
	{
		get
		{
			if (_projectAssetsPath == null) _projectAssetsPath = Application.dataPath;
			return _projectAssetsPath;
		}
	}
	private static string _projectAssetsPath;

	/// <summary> 
	/// return a relative path from a full path, or null if given path does not lead to a file in this project's Assets folder
	/// this path will start with Assets/
	/// </summary>
	public static string ProjectRelativePath(string fullPath)
	{
		// first convert \ to / cause in unity we want / not \
		fullPath = fullPath.Replace("\\", "/");

		// ...
		if (fullPath.StartsWith(Application.dataPath))
		{
			return ("Assets" + fullPath.Remove(0, Application.dataPath.Length));
		}
		return null;
	}

	/// <summary> removes the strating 'Assets/' from a relative path</summary>
	public static string ToAssetsRelativePath(string relativePath)
	{
		if (relativePath.StartsWith("Assets/"))
		{
			return relativePath.Remove(0, "Assets/".Length);
		}
		return relativePath;
	}

	/// <summary> return true if the path exist (filePath should start with 'Assets')</summary>
	public static bool RelativeFileExist(string filePath)
	{
		filePath = FullProjectPath + "/" + filePath;
		return File.Exists(filePath);
	}

	/// <summary> return true if the path exist (path should start with 'Assets')</summary>
	public static bool RelativePathExist(string path)
	{
		path = FullProjectPath + "/" + path;
		return Directory.Exists(path);
	}

	#endregion
	// ================================================================================================================
	#region AssetDatabase & Assets

	/// <summary> will create the asset file if it does not exist and add the object and save the file</summary>
	public static void AddObjectToAssetFile(Object obj, string assetFile)
	{
		if (!RelativeFileExist(assetFile))
		{
			AssetDatabase.CreateAsset(obj, assetFile);
		}
		else
		{
			AssetDatabase.AddObjectToAsset(obj, assetFile);
		}
		obj.hideFlags = HideFlags.HideInHierarchy;
		EditorApplication.SaveAssets();
	}

	public static GameObject CreatePrefab(string fileName, System.Type addComponentType, string name = null)
	{
		fileName = AssetDatabase.GenerateUniqueAssetPath(fileName);
		//Debug.Log("Creating: " + fileName);
		Object prefab = PrefabUtility.CreateEmptyPrefab(fileName);

		// create temp object in scene and add component
		GameObject go = new GameObject();
		if (!string.IsNullOrEmpty(name)) go.name = name;
		go.AddComponent(addComponentType);

		// save prefab and wipe temp object from scene
		GameObject prefabGo = PrefabUtility.ReplacePrefab(go, prefab);
		GameObject.DestroyImmediate(go);

		return prefabGo;
	}

	/// <summary>
	/// this will return all prefabs of type from the project folder
	/// </summary>
	public static List<T> FindPrefabsOfTypeAll<T>(string progressbarTitle, string progressbarInfo)
		where T : Component
	{
		DirectoryInfo dir = new DirectoryInfo(UniRPGEdUtil.FullProjectAssetsPath);
		FileInfo[] files = dir.GetFiles("*.prefab", SearchOption.AllDirectories);
		string fn = "";

		float progress = 0f;
		float step = 1f / (float)files.Length;
		EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, progress);

		List<T> res = new List<T>();
		for (int i = 0; i < files.Length; i++)
		{
			progress += step;
			EditorUtility.DisplayProgressBar(progressbarTitle, progressbarInfo, progress);
			if (files[i] == null) continue;
			fn = UniRPGEdUtil.ProjectRelativePath(files[i].FullName);
			if (!string.IsNullOrEmpty(fn))
			{
				Object obj = AssetDatabase.LoadAssetAtPath(fn, typeof(T));
				if (obj != null) res.Add((T)obj);
			}
		}
		EditorUtility.ClearProgressBar();

		return res;
	}

	#endregion
	// ================================================================================================================
	#region gameobjects related

	/// <summary>Creates new gameobject and try place it around where scene view cam is looking</summary>
	public static GameObject NewGameObjectInSceneView(string name, bool checkColliders=false, int mask=-1)
	{
		// position the object around where scene view cam is looking
		// first attemp to find colliders if checkColliders=true, and use the given mask if needed
		if (!SceneView.currentDrawingSceneView) return null;
		Camera cam = SceneView.currentDrawingSceneView.camera;

		Vector3 pos = Vector3.zero;
		if (checkColliders)
		{
			Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
			{
				pos = hit.point;
			}
			else
			{
				pos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
			}
		}
		else
		{
			pos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
		}

		GameObject obj = new GameObject(name);
		obj.transform.position = pos;
		return obj;
	}

	#endregion
	// ================================================================================================================
	#region callbacks related

	/// <summary>Makes sure an event handler is created that will delete all objects taged with "EditorOnly" when Play is pressed in editor</summary>
	public static void RegisterPreviewObjectsChecker()
	{
		RegisterPlaymodeStateChanged(DeleteAllEditorOnlyObjects_Callback);
	}

	/// <summary>Deletes all preview objects</summary>
	public static void DeleteAllEditorOnlyObjects()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag("EditorOnly");
		foreach (GameObject go in objs)
		{
			GameObject.DestroyImmediate(go);
		}
	}

	public static void DeleteAllEditorOnlyObjects_Callback()
	{
		// this function is called twice when Play or Stop is used (if reged on EditorApplication.playmodeStateChanged)
		// on play: isPlayingOrWillChangePlaymode will be true and isPlaying=false then true
		// on stop: isPlayingOrWillChangePlaymode will be false and isPlaying=rue then false
		// it is also called during pause and such, so watch out for that

		//Debug.Log("mode = " + EditorApplication.isPlayingOrWillChangePlaymode + ", " +
		//	(EditorApplication.isCompiling ? "Compiling, " : "") +
		//	(EditorApplication.isPaused ? "Paused, " : "") +
		//	(EditorApplication.isPlaying ? "Playing, " : "") +
		//	(EditorApplication.isUpdating ? "Updating, " : ""));

		if (!EditorApplication.isCompiling && !EditorApplication.isPaused && !EditorApplication.isUpdating)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				Selection.activeObject = null;
			}

			// when play was pressed I want to wipe the temp objects
			if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			{
				DeleteAllEditorOnlyObjects();
			}
		}
	}

	/// <summary>Add callback to EditorApplication.playmodeStateChanged (prevents adding duplicate)</summary>
	public static void RegisterPlaymodeStateChanged(EditorApplication.CallbackFunction callback)
	{
		// first check if not alrleady in list,
		if (EditorApplication.playmodeStateChanged != null)
		{
			foreach (EditorApplication.CallbackFunction d in EditorApplication.playmodeStateChanged.GetInvocationList())
			{
				if (d == callback) return;
			}
		}
		// else add
		EditorApplication.playmodeStateChanged += callback;
	}

	/// <summary>Remove callback from EditorApplication.playmodeStateChanged</summary>
	public static void RemovePlaymodeStateChanged(EditorApplication.CallbackFunction callback)
	{
		EditorApplication.playmodeStateChanged -= callback;
	}

	#endregion
	// ================================================================================================================
} }