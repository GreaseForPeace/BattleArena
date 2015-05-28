// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniRPG {

public class UniRPGUtil 
{
	// ================================================================================================================
	#region Encode/Decode

	public static string GetMd5Hash(string input, MD5 md5 = null)
	{
		if (md5 == null) md5 = MD5.Create();
		byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder sBuilder = new StringBuilder();
		for (int i = 0; i < data.Length; i++)
		{	// Loop through each byte of the hashed data and format each one as a hexadecimal string
			sBuilder.Append(data[i].ToString("x2"));
		}
		return sBuilder.ToString();
	}

	public static bool VerifyMd5Hash(string input, string hash)
	{
		MD5 md5 = MD5.Create();
		string hashOfInput = GetMd5Hash(input, md5);
		System.StringComparer comparer = System.StringComparer.OrdinalIgnoreCase;
		if (0 == comparer.Compare(hashOfInput, hash)) return true;
		return false;
	}

	#endregion
	// ================================================================================================================
	#region Maths 

	/// <summary>will pick a random point within 2D circle of radius and return the values as [x, 0, z]</summary>
	public static Vector3 PickPointInCircle(float radius)
	{
		float t = 2 * Mathf.PI * Random.Range(0f, 1f);
		float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
		float r = (u > 1 ? 2 - u : u);
		return (new Vector3(r * Mathf.Cos(t), 0f, r * Mathf.Sin(t)) * radius);
	}

	public static Vector3 PickPointInRectanle(Vector2 wh)
	{
		float x = Random.Range(-wh.x, +wh.x);
		float z = Random.Range(-wh.y, +wh.y);
		return new Vector3(x, 0f, z);
	}

	#endregion
	// ================================================================================================================
	#region Lists

	/// <summary>runs through and remove all NULL values from the list of Unity objects</summary>
	public static List<T> CleanupList<T>(List<T> list)
		where T: UnityEngine.Object
	{
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (list[i] == null || !list[i]) list.RemoveAt(i);
		}

		return list;
	}

	#endregion
	// ================================================================================================================
	#region Funtions used by not only editor scripts but also some runtime scripts like in OnGizmo()

#if UNITY_EDITOR

	/// <summary>Create a preview object, but only if not in play mode. Link is the object this is a preview for, for example a SpawnPoint</summary>
	public static GameObject InstantiatePreview(GameObject prefab, GameObject link, Vector3 position, Quaternion rotation)
	{
		if (prefab && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
		{
			GameObject go = GameObject.Instantiate(prefab, position, rotation) as GameObject;

			// add the preview component to it since this is used to tract and delete the preview as needed
			PreviewObject p = go.AddComponent<PreviewObject>();
			p.link = link;

			// hide the preview object in hierarchy
			UniRPGUtil.HideAndDontSave(go);	
			return go;
		}
		return null;
	}

	/// <summary>Sets the hide flags for the object and also tag with "EditorOnly"</summary>
	public static void HideAndDontSave(GameObject go, bool hideWireframeToo = true, bool dontSetTag = false)
	{
		if (!dontSetTag) go.tag = "EditorOnly";
		go.hideFlags = HideFlags.HideAndDontSave;
		if (go.transform.childCount > 0)
		{
			for (int i = 0; i < go.transform.childCount; i++)
			{	// dont want the tag set on children as it is not needed (tag is used to find obejcts to delete)
				HideAndDontSave(go.transform.GetChild(i).gameObject, hideWireframeToo, true);
			}
		}
		if (hideWireframeToo) HideRendererWireframe(go);
	}

	/// <summary>Prevent the wireframe from showing when object is active in scene view</summary>
	public static void HideRendererWireframe(GameObject go)
	{
		if (go.renderer) EditorUtility.SetSelectedWireframeHidden(go.renderer, true);
		for (int i = 0; i < go.transform.childCount; i++)
		{
			HideRendererWireframe(go.transform.GetChild(i).gameObject);
		}
	}

#endif

	#endregion
	// ================================================================================================================
} }