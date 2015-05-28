// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================
	
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace DiaQEditor
{
	public class DiaQEdUtil
	{

		// ============================================================================================================		
		#region File and Path

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

		/// <summary> return a relative path from a full path, or null if given path does not lead to a file in this project's Assets folder this path will start with Assets/</summary>
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

		/// <summary> removes the starting 'Assets/' from a relative path</summary>
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

		/// <summary>Checks if path exist, else craetes it. Also checks that the musthaveFolder is present in the path somewhere, else add at end (if not null). return path with ending '/'</summary>
		public static string CheckRelativePathExist(string path, string musthaveFolder)
		{
			string[] folders = path.Split('/');

			string parentPath = "Assets";
			string newPath = "Assets";
			bool hadFolder = (string.IsNullOrEmpty(musthaveFolder) ? true : false);
			string checkFolder = null;
			if (!hadFolder) checkFolder = musthaveFolder.ToLower();

			// start at 1, skipping element 0 which should be "Assets"
			for (int i = 1; i < folders.Length; i++)
			{
				if (string.IsNullOrEmpty(folders[i])) continue;
				
				if (!hadFolder)
				{
					if (folders[i].ToLower().Equals(checkFolder)) hadFolder = true;
				}

				newPath += "/" + folders[i];
				if (!RelativePathExist(newPath)) AssetDatabase.CreateFolder(parentPath, folders[i]);
				parentPath = newPath;
			}

			if (!hadFolder)
			{
				newPath += "/" + musthaveFolder;
				if (!RelativePathExist(newPath)) AssetDatabase.CreateFolder(parentPath, musthaveFolder);
			}

			//AssetDatabase.Refresh();
			return newPath + "/";
		}

		#endregion
		// ============================================================================================================		
	}
}