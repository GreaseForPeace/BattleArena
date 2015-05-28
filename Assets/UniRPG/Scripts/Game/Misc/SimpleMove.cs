// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class SimpleMove : MonoBehaviour 
{
	public enum OrientTo { Forward, Right, Up };

	public Transform follow = null;			// if null then finalPos will be used alone (else finalPos is offset from follow.position)
	public Vector3 finalPos = Vector3.zero;
	public float speed = 1f;
	public bool destroyAtEnd = false;
	public bool faceMoveDirection = false;
	public bool doOrient = false;
	public SimpleMove.OrientTo orientTo = SimpleMove.OrientTo.Forward;
	public float orientOffset = 1f;
	public Transform targetTransform;

	private Vector3 startMarker;
	private Vector3 offset = Vector3.zero;
	private float startTime;
	private float journeyLength;

	void Start()
	{
		startTime = Time.time;
		startMarker = transform.position;
		if (follow != null) offset = finalPos;
	}

	private void UpdateJourney()
	{
		if (follow != null)
		{
			finalPos = follow.position + offset;
		}

		journeyLength = Vector3.Distance(startMarker, finalPos);
	}

	void Update() 
	{
		UpdateJourney();
		float distCovered = (Time.time - startTime) * speed;
		float fracJourney = distCovered / journeyLength;
		transform.position = Vector3.Lerp(startMarker, finalPos, fracJourney);
		if (faceMoveDirection) transform.LookAt(finalPos);
		if (fracJourney >= 1f)
		{
			if (follow == null) enabled = false;

			if (destroyAtEnd) GameObject.Destroy(gameObject);
			else
			{
				if (doOrient && targetTransform!=null)
				{
					if (orientTo == SimpleMove.OrientTo.Forward) transform.forward = targetTransform.forward * orientOffset;
					else if (orientTo == SimpleMove.OrientTo.Right) transform.right = targetTransform.right * orientOffset;
					else if (orientTo == SimpleMove.OrientTo.Up) transform.up = targetTransform.up * orientOffset;
				}

			}
		}
	}

	// ================================================================================================================
} }

