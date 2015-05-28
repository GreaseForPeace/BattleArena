// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class AutoDestroyAudio : MonoBehaviour
{
	private AudioSource au;

	void Start()
	{
		au = gameObject.GetComponent<AudioSource>();
		if (au == null) Destroy(this);
	}
	
	void Update()
	{
		if (au.isPlaying == false)
		{
			Destroy(gameObject);
		}
	}

	// ================================================================================================================
} }
