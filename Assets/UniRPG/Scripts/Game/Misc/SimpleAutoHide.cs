// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG
{
	[AddComponentMenu("")]
	public class SimpleAutoHide : MonoBehaviour
	{
		// will watch this object and if it becomes null then SimpleAutoHide will set its own gameobject as inactive
		public GameObject goToWatch;

		void LateUpdate()
		{
			if (goToWatch == null)
			{
				gameObject.SetActive(false);
			}
		}

		// ================================================================================================================
	}
}
