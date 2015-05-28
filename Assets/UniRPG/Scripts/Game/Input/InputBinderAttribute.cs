// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPG {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InputBinderAttribute : System.Attribute
{
	public bool EditorAutoLoad = false;

	// ================================================================================================================
} }