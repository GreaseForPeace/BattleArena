// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPGEditor {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DatabaseEditorAttribute : System.Attribute
{
	public string Name { get { return this.name; } }
	public int Priority = 999; // lower value will be shown 1st in editor window's toolbar

	private readonly string name;

	// ================================================================================================================

	public DatabaseEditorAttribute(string name)
	{
		this.name = name;
	}

	// ================================================================================================================
} }