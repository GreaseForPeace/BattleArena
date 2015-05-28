// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPGEditor {

public class InspectorBase<T> : UnityEditor.Editor where T : UnityEngine.Object
{
	protected T Target { get { return (T)target; } }
	protected T[] Targets { get { return System.Array.ConvertAll<UnityEngine.Object, T>(targets, item => (T)item); } }

	// ================================================================================================================
} }
