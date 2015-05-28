// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
[ExecuteInEditMode]
public class PreviewObject : MonoBehaviour 
{
	public GameObject link = null;

	void Update()
	{
		// for (ExecuteInEditMode) this is called when something in the scene changed
		// so check here to see if the object this one was linked with was removed
		// so that this can be removed too
		if (link == null) GameObject.DestroyImmediate(gameObject);
	}

	// ================================================================================================================
} }