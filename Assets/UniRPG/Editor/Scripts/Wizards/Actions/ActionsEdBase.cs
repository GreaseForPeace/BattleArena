// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UniRPG;

namespace UniRPGEditor {

public class ActionsEdBase
{
	public const string TargetTypeHelp = "Please look in the documentation to learn what the specific subject type means for the queue that this action is used in as the types available and the meaning of each will differ depending on the queue it is used in.";

	public bool showAcceptButton = true;
	public EditorWindow ed = null; // the wizard editor/ window

	public virtual string ActionShortNfo(Object actionObj)
	{
		return this.ToString();
	}

	public virtual void OnGUI(Object actionObj)
	{
	}

	// ================================================================================================================
} }