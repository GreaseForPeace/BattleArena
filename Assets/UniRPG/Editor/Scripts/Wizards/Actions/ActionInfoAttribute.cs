// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UniRPG;

namespace UniRPGEditor {

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ActionInfoAttribute : System.Attribute
{
	public System.Type Action { get { return this.action; } }
	public string Name { get { return this.name; } }
	public string Description { get; set; }

	private readonly System.Type action;
	private readonly string name;

	public ActionInfoAttribute(System.Type action, string name)
	{
		if (typeof(Action).IsAssignableFrom(action))
		{
			this.action = action;
		}
		else
		{	// make it invalid by assigning NULL if the class did not inherit from Action
			this.action = null;
		}

		if (string.IsNullOrEmpty(name))
		{	// must be proper name, else this is again, invalid
			this.action = null;
		}
		else
		{
			this.name = name;
		}
	}

	// ================================================================================================================
} }