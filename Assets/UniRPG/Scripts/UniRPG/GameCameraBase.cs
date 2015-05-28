// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class GameCameraBase : MonoBehaviour
{
	public GUID id;
	public GameObject cameraPrefab;				// the camera prefab to spawn if any

	// only used when cameraPrefab is not specified
	public bool includeFlareLayer = true;		// should camera object also have FlareLayer component on it?
	public bool includeGuiLayer = true;			// should camera object also have GUILayer component on it?
	public bool includeAudioListener = true;	// should camera object also have AudioListener component on it?

	private Camera _cam = null;
	public Camera Camera 
	{ 
		get
		{
			if (_cam == null) _cam = gameObject.GetComponent<Camera>();
			return _cam;
		}
	}

	public virtual void CopyTo(GameCameraBase targetCam)
	{
		targetCam.id = this.id.Copy();
		targetCam.cameraPrefab = this.cameraPrefab;
	}

	void Start()
	{
		_cam = gameObject.GetComponent<Camera>();
	}

	/// <summary>
	/// Called after UniRPG created the camera
	/// </summary>
	public virtual void CreatedByUniRPG() 
	{ 
		gameObject.tag = "Untagged";
	}

	/// <summary>
	/// Called after UniRPG activated the camera
	/// </summary>
	public virtual void ActivatedByUniRPG() 
	{
		gameObject.tag = "MainCamera";
		enabled = true; 
	}

	/// <summary>
	/// Called after UniRPG deactivated the camera
	/// </summary>
	public virtual void DeactivatedByUniRPG() 
	{
		enabled = false;
		gameObject.tag = "Untagged";
	}

	// ================================================================================================================
} }