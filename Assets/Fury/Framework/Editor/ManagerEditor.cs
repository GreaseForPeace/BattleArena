using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Fury.Editor
{
	[UnityEditor.CustomEditor(typeof(Fury.Behaviors.Manager))]
	public sealed class ManagerEditor : UnityEditor.Editor
	{
		private Fury.Behaviors.Manager Target { get { return target as Fury.Behaviors.Manager; } }

		private WWW _Request;

		public override void OnInspectorGUI()
		{
			var msg = "Downloading update data from http://dreadware.com";
			var msgType = MessageType.Info;

			if (_Request == null)
			{
				_Request = new WWW("http://services.dreadware.com/AssetStore.svc/json/GetProductRevision?productId=1");
			}
			else if (_Request.isDone)
			{
				try
				{
					var json = Json.ParseJSON(_Request.text);
					var revision = Int32.Parse(json["Payload"].ToString());

					if (revision > Info.Revision)
					{
						msg =  "You are using an outdated version of the Fury Framework! " + 
							"Download new binaries from http://dreadware.com";
						msgType = MessageType.Error;
					}
					else
					{
						msg = "Your version of the Fury Framework is up to date! " + 
							"Download additional content from http://dreadware.com";
						msgType = MessageType.Info;
					}
				}
				catch { }
			}

			msg = String.Format("Revision: {0}, Licensed: {1} \n", 
				Info.Revision, !Info.IsTrial) + msg;
			
			EditorGUILayout.HelpBox(msg, msgType);
			base.OnInspectorGUI();
		}
	}
}