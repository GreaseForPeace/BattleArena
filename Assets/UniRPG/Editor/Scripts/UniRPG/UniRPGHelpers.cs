// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

namespace UniRPGEditor
{

	public class UniRPGDBEdInfo
	{
		public int priority;
		public string name;
		public DatabaseEdBase editor;
	}

	public class UniRPGActionEdInfo
	{
		public string name;
		public string descr;
		public System.Type actionType;
		public ActionsEdBase editor;
	}

	public class UniRPGGUIEdInfo
	{
		public string name;
		public GUIEditorBase editor;
		public string menuGUIPath;
		public string gameGUIPath;
		public System.Type menuGUIDataType;
		public System.Type gameGUIDataType;
	}

	public class UniRPGCameraEdInfo
	{
		public string name;
		public GameCameraEditorBase editor;
		public System.Type cameraType;
	}

	public class LoadSaveEdInfo
	{
		public string name;
		public LoadSaveProviderEdBase editor;
		public System.Type providerType;
	}
	
}