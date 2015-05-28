// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using UniRPG;

namespace UniRPGEditor {

public class DatabaseEdBase 
{
	protected DatabaseEditor ed; // the database editor window

	public virtual void OnEnable(DatabaseEditor ed) { this.ed = ed; }
	public virtual void OnDisable(DatabaseEditor ed) { this.ed = ed; }

	public virtual void Update(DatabaseEditor ed) { this.ed = ed;  }

	public virtual void OnGUI(DatabaseEditor ed) { this.ed = ed; }

	// ================================================================================================================
} }