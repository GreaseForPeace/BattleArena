// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPGEditor {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GameCameraAttribute : System.Attribute
{
	public string Name { get { return this._name; } }
	public System.Type cameraType { get { return this._cameraType; } }

	public GameCameraAttribute(string name, System.Type cameraType)
	{
		this._name = name;
		this._cameraType = cameraType;
	}

	private readonly string _name;
	private readonly System.Type _cameraType;

	// ================================================================================================================
} }