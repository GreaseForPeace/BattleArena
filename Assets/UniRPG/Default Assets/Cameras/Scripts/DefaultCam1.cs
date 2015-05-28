// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class DefaultCam1 : GameCameraBase 
{
	public bool autoRotateToPlayer = false;		// auto rotate cam to look at player's forward direction? (note that global setting in DB can override this to false)
	public bool allowRotation = true;			// allow rotation of the camera around the target. (note that global setting in DB can override this to false)
	public bool allowZooming = true;			// allow zooming the camera torward/from the target. (note that global setting in DB can override this to false)

	public float distance = 8f;					// distance of camera from character (influenced by zoom)
	public float backDistance = 0.85f;			// how far back from the charcater the cam should be in relation to "distabne"
	public float angle = 45f;					// angle of camera around character in world space

	// offset to center of character (this could be anything, for example the head's position could be the center)
	// if not set then the system will attempt to auto calc an offset of 1/4 from top of character controller
	public Vector3 targetCenterOffset = Vector3.zero;

	public float zoomSpeed = 1f;
	public float minZoomDistance = 1.5f;
	public float maxZoomDistance = 10f;

	public float rotationStepping = 5f;			// how much to multiply axis value with
	public float rotationSpeed = 3f;
	public float autoRotationSpeed = 1f;

	// ================================================================================================================

	private Transform playerTransform;			// the player character's transform
	private Transform _tr;						// the camera's transform
	private float scrollWheel = 0.0f;
	private float zoomVelocity = 1f;
	private float targetDistance;
	private float targetAngle;
	private float autoRotateTimout = 0f;
	private bool init = false;
	private DefaultCam1InputBinder inputBinder = null;
	private Vector3 dampSpeed = Vector3.zero;
	private Vector3 tc = Vector3.zero;

	// ================================================================================================================

	public override void CopyTo(GameCameraBase targetCam)
	{
		base.CopyTo(targetCam);

		DefaultCam1 cam = targetCam as DefaultCam1;

		cam.autoRotateToPlayer = this.autoRotateToPlayer;
		cam.allowRotation = this.allowRotation;
		cam.allowZooming = this.allowZooming;

		cam.distance = this.distance;
		cam.backDistance = this.backDistance;
		cam.angle = this.angle;
		cam.targetCenterOffset = this.targetCenterOffset;

		cam.zoomSpeed = this.zoomSpeed;
		cam.minZoomDistance = this.minZoomDistance;
		cam.maxZoomDistance = this.maxZoomDistance;

		cam.rotationStepping = this.rotationStepping;
		cam.rotationSpeed = this.rotationSpeed;
		cam.autoRotationSpeed = this.autoRotationSpeed;
	}

	void Awake()
	{
		_tr = gameObject.transform;
	}

	public override void CreatedByUniRPG()
	{
		base.CreatedByUniRPG();

		// load the input binder that defines keys for camera rotation
		inputBinder = new DefaultCam1InputBinder();
		InputManager.Instance.LoadInputFromBinder(inputBinder);
		inputBinder.SetActive(false); // camera will be disabled after being created
	}

	public override void ActivatedByUniRPG() 
	{
		base.ActivatedByUniRPG();
		inputBinder.SetActive(true);

		if (UniRPGGlobal.Player != null)
		{
			playerTransform = UniRPGGlobal.Player.transform;
			tc = playerTransform.position;
		}
		else
		{
			Debug.LogError("Error: No Player Character was found.");
			enabled = false;
			return;
		}

		// if still not assigned then error out
		if (!playerTransform)
		{
			Debug.LogError("No Player Character found.");
			enabled = false;
			return;
		}
		else
		{
			// check if the targetCenterOffset is set, else auto calc one
			if (targetCenterOffset == Vector3.zero)
			{
				CharacterController chara = playerTransform.GetComponent<CharacterController>();
				if (chara) targetCenterOffset.y = chara.height * 0.75f; // 1/4 from top as the offset
			}
		}

		angle = ClampAngle(angle, 0f, 360f);
		targetDistance = distance;
		targetAngle = angle;

		init = true;
	}

	public override void DeactivatedByUniRPG()
	{
		base.DeactivatedByUniRPG();
		inputBinder.SetActive(false);
		init = false;
	}

	// ================================================================================================================

	void LateUpdate()
	{
		if (!init) return;

		HandleZooming();

		if (autoRotateToPlayer)
		{
			if (autoRotateTimout <= 0.0f)
			{
				// if the player is idle, then do not update the angle just yet
				if (UniRPGGlobal.Player)
				{
					if (!UniRPGGlobal.Player.IsIdle()) targetAngle = playerTransform.eulerAngles.y;
				}
				else targetAngle = playerTransform.eulerAngles.y;
				angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * autoRotationSpeed);
			}
			else
			{
				autoRotateTimout -= Time.deltaTime;
				angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * rotationSpeed);
			}
		}
		else
		{
			angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * rotationSpeed);
		}

		distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, 0.5f);

		tc = Vector3.SmoothDamp(tc, playerTransform.position, ref dampSpeed, 0.1f);
		Vector3 targetCenter = tc + targetCenterOffset;
		Vector3 pos = (Quaternion.Euler(0f, angle, 0f) * Vector3.back * backDistance * distance) + targetCenter;

		pos.y = tc.y + distance;
		_tr.position = pos;
		_tr.LookAt(targetCenter);


		//Vector3 targetCenter = playerTransform.position + targetCenterOffset;
		//Vector3 pos = (Quaternion.Euler(0f, angle, 0f) * Vector3.back * backDistance * distance) + targetCenter;

		//pos.y = playerTransform.position.y + distance;
		//_tr.position = pos;
		//_tr.LookAt(targetCenter);
	}

	private void HandleZooming()
	{
		if (allowZooming)
		{
			scrollWheel = Input.GetAxis("Mouse ScrollWheel");
			if (scrollWheel > 0.0f) targetDistance -= zoomSpeed;
			else if (scrollWheel < 0.0f) targetDistance += zoomSpeed;
			targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
		}
	}

	public void OnInput_UpdateRotation(int dir)
	{
		if (allowRotation)
		{
			autoRotateTimout = 0.5f;
			if (dir == 0)
			{	// use mouse-x
				targetAngle += Input.GetAxis("Mouse X") * rotationStepping;
			}
			else
			{
				targetAngle += dir * (rotationStepping / 3f);
			}
			targetAngle = ClampAngle(targetAngle, 0f, 360f);
		}
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < 0f) angle += 360f;
		if (angle > 360f) angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}

	// ================================================================================================================
} }