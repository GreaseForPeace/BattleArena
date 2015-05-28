// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UniRPG;

namespace UniRPGEditor {

public class GUIEditorBase
{
	public virtual void InitDefaults(UnityEngine.GameObject menuGUIDataPrefab, UnityEngine.GameObject gameGUIDataPrefab) { }
	public virtual InputBinderBase GetInputBinder(UnityEngine.GameObject menuGUIDataPrefab, UnityEngine.GameObject gameGUIDataPrefab) { return null; }
	public virtual void OnEnable(UnityEngine.GameObject menuGUIDataPrefab, UnityEngine.GameObject gameGUIDataPrefab) { }
	public virtual void OnDisable(UnityEngine.GameObject menuGUIDataPrefab, UnityEngine.GameObject gameGUIDataPrefab) { }
	public virtual void Update(UnityEngine.GameObject menuGUIDataPrefab, UnityEngine.GameObject gameGUIDataPrefab) { }
	public virtual void OnGUI(DatabaseEditor ed, UnityEngine.GameObject menuGUIDataPrefab, UnityEngine.GameObject gameGUIDataPrefab) { }

	// ================================================================================================================
} }