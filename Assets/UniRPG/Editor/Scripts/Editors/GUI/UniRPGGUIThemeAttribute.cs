// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPGEditor {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class UniRPGGUIThemeAttribute : System.Attribute
{
	public string Name { get { return this._name; } }
	public string menuGUIPath { get { return this._menuGUIPath; } }
	public string gameGUIPath { get { return this._gameGUIPath; } }
	public System.Type menuGUIDataType { get { return this._menuGUIDataType; } }
	public System.Type gameGUIDataType { get { return this._gameGUIDataType; } }

	public UniRPGGUIThemeAttribute(string name, string menuGUIPath, string gameGUIPath, System.Type menuGUIDataType, System.Type gameGUIDataType)
	{
		this._name = name;
		this._menuGUIPath = menuGUIPath;
		this._gameGUIPath = gameGUIPath;
		this._menuGUIDataType = menuGUIDataType;
		this._gameGUIDataType = gameGUIDataType;
	}

	private readonly string _name;
	private readonly string _menuGUIPath;
	private readonly string _gameGUIPath;
	private readonly System.Type _menuGUIDataType;
	private readonly System.Type _gameGUIDataType;

	// ================================================================================================================
} }