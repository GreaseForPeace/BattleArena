// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class LoadSaveProviderBase : MonoBehaviour 
{
	public string providerName = ""; // this is set by UniRPG Editor code. don't mess with it

	public virtual void Load() { }
	public virtual void Save() { }
	public virtual bool HasKey(string key) { return false; }
	public virtual void DeleteKey(string key) { }
	public virtual void DeleteAll() { }

	public virtual void SetString(string key, string value)
	{ 
	}

	public virtual void SetInt(string key, int value) 
	{ 
	}

	public virtual void SetFloat(string key, float value) 
	{ 
	}

	public virtual void SetBool(string key, bool value)
	{
		SetInt(key, (value==true ? 1 : 0));
	}

	public virtual void SetVector3(string key, Vector3 value) 
	{
		SetString(key, string.Format("{0:N3}|{1:N3}|{2:N3}", value.x, value.y, value.z));
	}

	public virtual string GetString(string key, string defaultVal) 
	{ 
		return defaultVal; 
	}

	public virtual int GetInt(string key, int defaultVal) 
	{ 
		return defaultVal; 
	}

	public virtual float GetFloat(string key, float defaultVal) 
	{ 
		return defaultVal; 
	}

	public virtual bool GetBool(string key, bool defaultVal)
	{
		return (GetInt(key, (defaultVal ? 1 : 0)) == 1);
	}

	public virtual Vector3 GetVector3(string key, Vector3 defaultVal)
	{
		string s = GetString(key, null);
		if (!string.IsNullOrEmpty(s))
		{
			Vector3 v = Vector3.zero;
			string[] vs = s.Split('|');
			if (vs.Length >= 3)
			{
				if (float.TryParse(vs[0], out v.x))
				{
					if (float.TryParse(vs[1], out v.y))
					{
						if (float.TryParse(vs[2], out v.z))
						{
							return v;
						}
					}
				}
			}
		}
		return defaultVal;
	}

	// ================================================================================================================
} }
