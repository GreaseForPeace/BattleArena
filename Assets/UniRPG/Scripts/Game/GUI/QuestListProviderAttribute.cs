// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPG {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class QuestListProviderAttribute : System.Attribute
{
	public string Name { get { return this._name; } }

	public QuestListProviderAttribute(string name)
	{
		this._name = name;
	}

	private readonly string _name;

	// ================================================================================================================
} }