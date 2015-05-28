// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNITY_EDITOR
using UnityEngine;

namespace UniRPG {

public static class UniRPGSettings
{
	// these are settings used by the editor mainly but some runtime side
	// scripts that uses OnGizmo() needs access to them too, so they are here

	public static bool autoLoad3DPreviews = true;
	public static int floorLayeMask = 0;

	// ================================================================================================================
} }
#endif