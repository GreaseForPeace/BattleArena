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
	public class SimpleFollow : MonoBehaviour
	{
		public Transform followTr; // will follow this transform around
		private Transform _tr;

		void Start()
		{
			_tr = transform;
		}

		void LateUpdate()
		{
			if (followTr != null)
			{
				_tr.position = followTr.position;
			}
		}

		// ================================================================================================================
	}
}
