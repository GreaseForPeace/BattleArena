// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPGEditor {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class LoadSaveProviderAttribute : System.Attribute
{
	public string Name { get { return this._name; } }
	public System.Type providerType { get { return this._providerType; } }

	public LoadSaveProviderAttribute(string name, System.Type providerType)
	{
		this._name = name;
		this._providerType = providerType;
	}

	private readonly string _name;
	private readonly System.Type _providerType;

	// ================================================================================================================
} }