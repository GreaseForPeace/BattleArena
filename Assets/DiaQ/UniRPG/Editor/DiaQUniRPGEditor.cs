// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UnityEditor;
using UniRPGEditor;

namespace DiaQEditor
{
	[InitializeOnLoad]
	public class DiaQUniRPGEditor
	{
		static DiaQUniRPGEditor()
		{
			// register the toolbar button
			UniRPGEditorGlobal.AddToolbarButton 
			(
				new UniRPGEditorToolbar.ToolbarButton() { order = 10, callback = DiaQEditorWindow.ShowDiaQEditor, gui = new GUIContent(null, null, "DiaQ Editor"), iconPath = DiaQEdGUI.EditorResourcePath + "Toolbar/diaq" + (EditorGUIUtility.isProSkin ? "" : "_l") + ".png" }
			);

			// register autocall
			UniRPGEditorGlobal.RegisterAutoCall("DiaQUniRPG", false);
		}

		// ============================================================================================================
	}
}

#endif