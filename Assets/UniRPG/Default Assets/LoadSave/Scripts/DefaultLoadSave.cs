// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class DefaultLoadSave : LoadSaveProviderBase
{

	public override void Load() 
	{  }

	public override void Save() 
	{
		PlayerPrefs.Save();
	}

	public override void SetString(string key, string value) 
	{
		PlayerPrefs.SetString(key, value);
	}

	public override void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	public override void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	public override string GetString(string key, string defaultVal)
	{
		return PlayerPrefs.GetString(key, defaultVal);
	}

	public override int GetInt(string key, int defaultVal)
	{
		return PlayerPrefs.GetInt(key, defaultVal);
	}

	public override float GetFloat(string key, float defaultVal)
	{
		return PlayerPrefs.GetFloat(key, defaultVal);
	}

	public override bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public override void DeleteKey(string key) 
	{
		PlayerPrefs.DeleteKey(key);
	}

	public override void DeleteAll() 
	{
		PlayerPrefs.DeleteAll();
	}

	// ================================================================================================================
} }

