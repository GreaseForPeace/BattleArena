// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

// ====================================================================================================================

public static class UniRPGClassExtensions
{
	// ===============================================================================================================
	// LayerMask

	public static bool HasLayer(this LayerMask layerMask, int layer)
	{
		return (layerMask & (1 << layer)) != 0;
	}

	// ===============================================================================================================
	// RaycastHit[] (Array)

	public static void Sort(this RaycastHit[] hits)
	{
		if (hits == null) return;
		if (hits.Length <= 1) return;

		RaycastHit temp;
		for (int i = 0; i < hits.Length; i++)
		{
			for (int j = 0; j < hits.Length - 1; j++)
			{
				if (hits[j].distance > hits[j + 1].distance)
				{
					temp = hits[j + 1];
					hits[j + 1] = hits[j];
					hits[j] = temp;
				}
			}
		}
	}
 
}

// ================================================================================================================

//public static class UniRPGTransformExtensions
//{
//	public static void positionX(this Transform transform, float x)
//	{
//		Vector3 pos = new Vector3(x, transform.position.y, transform.position.z);
//		transform.position = pos;
//	}

//	public static void positionY(this Transform transform, float y)
//	{
//		Vector3 pos = new Vector3(transform.position.x, y, transform.position.z);
//		transform.position = pos;
//	}

//	public static void positionZ(this Transform transform, float z)
//	{
//		Vector3 pos = new Vector3(transform.position.x, transform.position.y, z);
//		transform.position = pos;
//	}
//}

// ================================================================================================================
}