// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[System.Serializable]
public class PatrolPoint
{
	public Vector3 position = Vector3.zero; // Patrol Points positions should be read as relative to the path's "center"
	public Quaternion rotation = Quaternion.identity;

	public PatrolPoint() { }
	public PatrolPoint(Vector3 position, Quaternion rotation)
	{
		this.position = position;
		this.rotation = rotation;
	}
}

[AddComponentMenu("UniRPG/Patrol Path")]
public class PatrolPath : UniqueMonoBehaviour 
{
	public List<PatrolPoint> patrolpoints = new List<PatrolPoint>();

	// ================================================================================================================
	#region vars

	public PatrolPoint this[int index]
	{
		get
		{
			if (patrolpoints == null) return null;
			if (index < 0) return null;
			if (index >= patrolpoints.Count) return null;
			return patrolpoints[index];
		}
	}

	public int Length
	{
		get
		{
			if (patrolpoints == null) return 0;
			return patrolpoints.Count;
		}
	}

	private Transform _tr;

	#endregion
	// ================================================================================================================
	#region pub

	public override void Awake()
	{
		base.Awake();
		autoSaveLoadEnabled = false;
		_tr = this.transform;
	}

	/// <summary>this will remove any patrol points that might exist for the path and init it with a 3 points</summary>
	public void CreateDefaultPoints()
	{
		patrolpoints.Add(new PatrolPoint(new Vector3(-1f, 0.1f, 0f), Quaternion.identity));
		patrolpoints.Add(new PatrolPoint(new Vector3(1f, 0.1f, 0.5f), Quaternion.identity));
		patrolpoints.Add(new PatrolPoint(new Vector3(1f, 0.1f, -0.5f), Quaternion.identity));
	}

	/// <summary>get the proper position of the patrol point</summary>
	public Vector3 GetPatrolPointPosition(int index)
	{
		if (index >= 0 && index < patrolpoints.Count)
		{
			return _tr.position + patrolpoints[index].position;
		}
		return _tr.position;
	}

	#endregion
	// ================================================================================================================
	#region Gizmo/Editor related
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		// the path's gizmo
		Gizmos.DrawLine(transform.position - Vector3.up * 0.1f, transform.position + Vector3.up * 1f);
		Gizmos.DrawCube(transform.position - Vector3.up * 0.1f, new Vector3(0.1f, 0.1f, 0.1f));
		Gizmos.DrawSphere(transform.position + Vector3.up * 1f, 0.25f);
		Gizmos.DrawIcon(transform.position + Vector3.up * 1f, "UniRPG_path.png");
	}
#endif
	#endregion
	// ================================================================================================================
} }