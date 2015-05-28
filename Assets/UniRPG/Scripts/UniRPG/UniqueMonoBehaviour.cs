// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class UniqueMonoBehaviour : MonoBehaviour 
{
	public GUID id;	// unique identifier mostly used with the LoadSave System 

	// extra properties that the designer can define. Will be saved and loaded by LoadSave System
	public List<StringVar> customVars = new List<StringVar>(0);

	// tells if this object will save when UniRPG calls for saving (only if object supports the SaveLoad System)
	[System.NonSerialized]private bool _isPersistent = true;
	public bool IsPersistent
	{
		get { return _isPersistent; }
		set 
		{
			_isPersistent = value;
			if (!_isPersistent)
			{
				_loading = false;
				UniRPGGlobal.RemoveFromSaveEvent(OnSave);
			}
		}
	}

	// false if this object is done loading (should not be assumed to be valid during Awake/Start) (only used when isPersistant = true)
	[System.NonSerialized]private bool _loading = true;
	public bool IsLoading { get { return _loading; } }

	// set this to false before calling base.SaveState/LoadState so that this do not handle 
	// the saving and loading of the objects active state and component's enabled state
	[System.NonSerialized]protected bool autoSaveLoadEnabled = true;
	[System.NonSerialized]protected bool autoSaveDestroy = true;
	[System.NonSerialized]protected string saveKey = null;

	// ================================================================================================================

	public void CreateNewSaveKey()
	{
		id = GUID.Create();
		saveKey = id.ToString();
	}

	public virtual void Awake()
	{
		id = GUID.Create(id);
		saveKey = id.ToString();
	}

	public virtual void Reset()
	{
		id = GUID.Create(id);
	}

	public virtual void OnApplicationQuit()
	{
		_isPersistent = false; // do not save when app is quitting
	}

	public virtual void OnDestroy()
	{
		if (_isPersistent)
		{
			UniRPGGlobal.RemoveFromSaveEvent(OnSave);
			if (autoSaveDestroy && !Application.isLoadingLevel)
			{
				UniRPGGlobal.SaveBool(saveKey + "destroy", true);
			}
		}
	}

	protected virtual void SaveState()
	{
		if (autoSaveLoadEnabled)
		{
			UniRPGGlobal.SaveBool(saveKey + "enable", gameObject.activeSelf);
			UniRPGGlobal.SaveBool(saveKey + "enable_com_" + this.GetType().ToString(), enabled);
		}

		// save custom variables
		UniRPGGlobal.SaveInt(saveKey + "vars", customVars.Count);
		for (int i=0; i < customVars.Count; i++)
		{
			if (string.IsNullOrEmpty(customVars[i].name))
			{	// delete the key if present
				UniRPGGlobal.DeleteSaveKey(saveKey + "var" + i);
			}
			else
			{
				UniRPGGlobal.SaveString(saveKey + "var" + i, customVars[i].name + "|" + customVars[i].val);
			}
		}
	}

	protected virtual void LoadState()
	{
		if (UniRPGGlobal.Instance.DoNotLoad) return;

		// load custom variables
		int count = UniRPGGlobal.LoadInt(saveKey + "vars", 0);
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				string s = UniRPGGlobal.LoadString(saveKey + "var" + i, null);
				if (!string.IsNullOrEmpty(s))
				{
					int p = s.IndexOf('|');
					if (p > 0)
					{
						string key = s.Substring(0, p);
						SetCustomVariable(key, s.Substring(p + 1));
					}
				}
			}
		}

		// do rest of loading

		if (autoSaveDestroy)
		{
			bool b = UniRPGGlobal.LoadBool(saveKey + "destroy", false);
			if (b)
			{	// this object needs to be destroyed
				GameObject.Destroy(gameObject);
				return;
			}
		}

		if (autoSaveLoadEnabled)
		{
			// check if this object/component needs to be disabled
			bool b = UniRPGGlobal.LoadBool(saveKey + "enable_obj", gameObject.activeSelf);
			if (b)
			{
				b = UniRPGGlobal.LoadBool(saveKey + "enable_com_" + this.GetType().ToString(), enabled);
				if (!b) enabled = false;
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
	}

	private void OnSave(System.Object sender)
	{
		SaveState();
	}

	public virtual void Update()
	{
		if (_loading)
		{
			if (_isPersistent)
			{
				if (string.IsNullOrEmpty(saveKey)) saveKey = id.ToString();
				UniRPGGlobal.RegisterForSaveEvent(OnSave);
				LoadState();
			}
			_loading = false;
		}
	}

	public void SetCustomVariable(string name, string value)
	{
		for (int i = 0; i < customVars.Count; i++)
		{
			if (customVars[i].name.Equals(name))
			{
				customVars[i].val = value;
				return;
			}
		}

		// not found, add
		customVars.Add(new StringVar() { name = name, val = value });
	}

	public string GetCustomVariable(string name)
	{
		for (int i = 0; i < customVars.Count; i++)
		{
			if (customVars[i].name.Equals(name))
			{
				return customVars[i].val;
			}
		}

		// not found
		return "";
	}

	public bool HasCustomVariable(string name)
	{
		if (string.IsNullOrEmpty(name)) return false;
		for (int i = 0; i < customVars.Count; i++)
		{
			if (customVars[i].name.Equals(name)) return true;
		}
		return false;
	}

	// ================================================================================================================
} }
