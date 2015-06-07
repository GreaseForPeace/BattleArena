using System;

using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Fury.Editor
{
	public sealed class FuryWindow : UnityEditor.EditorWindow
	{
		private static EventInfo[] Events;
		private static String[] EventNames;
		private static Type[] DefinitionTypes;
		private static String[] DefinitionTypeNames;
		private static Dictionary<String, String> Fields;

		[MenuItem("Window/Fury")]
		static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(FuryWindow), false, "Fury");
		}

		private Int32 SelectedEvent;
		private Boolean TriggerStartingState;
		private String TriggerName = "";

		private void OnGUI()
		{
			if (Events == null || EventNames == null)
			{
				Events = (typeof(Fury.Behaviors.Manager).GetEvents());
				Events = Resize<EventInfo>(Events, Events.Length + 1);
				EventNames = new String[Events.Length];

				for (Int32 i = 0; i < Events.Length; i++)
					if (Events[i] != null)
						EventNames[i] = Events[i].Name;

				EventNames[EventNames.Length - 1] = "OnUnitEnterTrigger";
			}

			if (DefinitionTypes == null || DefinitionTypeNames == null)
			{
				DefinitionTypes = new[] { typeof(Database.Ability), typeof(Database.Deposit), typeof(Database.Energy),
					typeof(Database.Map), typeof(Database.Map), typeof(Database.Status),
					typeof(Database.Token), typeof(Database.Unit), typeof(Database.Weapon)				
				};

				DefinitionTypeNames = new String[DefinitionTypes.Length];
				for (Int32 i = 0; i < DefinitionTypeNames.Length; i++)
					DefinitionTypeNames[i] = DefinitionTypes[i].Name;
			}

			if (Fields == null)
				Fields = new Dictionary<String, String>();

			GUILayout.Space(5f);

			EditorGUILayout.LabelField("Events");
			TriggerName = EditorGUILayout.TextField("Name", TriggerName);
			TriggerStartingState = EditorGUILayout.Toggle("Starting State", TriggerStartingState);
			SelectedEvent = EditorGUILayout.Popup("Event Type", SelectedEvent, EventNames);

			if (GUILayout.Button("Create"))
			{
				if (EventNames[SelectedEvent] == "OnUnitEnterTrigger")
					CreateOnUnitEnterTrigger(TriggerName, TriggerStartingState);
				else
					CreateGenericTrigger(TriggerName, TriggerStartingState, Events[SelectedEvent]);
			}
		}

		private void CreateOnUnitEnterTrigger(String name, Boolean startingState)
		{
			#region // Define the template
			var templateGeneric = @"using System;
using System.Collections.Generic;

using UnityEngine;
using Fury.Behaviors;

/// <summary>
/// This is an automatically generated trigger template for the 
/// trigger of type OnUnitEnterTrigger.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class {TRIGGER_NAME} : MonoBehaviour
{
	/// <summary>
	/// For triggers with multiple copies, this property will return all scene instances of the trigger.
	/// </summary>
	public static {TRIGGER_NAME}[] All
	{
		get
		{
			return ({TRIGGER_NAME}[])GameObject.FindSceneObjectsOfType(typeof({TRIGGER_NAME}));
		}
	}

	/// <summary>
	/// Most triggers will only have one instance active, and this static property
	/// can be used to retrieve a reference to it.
	/// </summary>
	public static {TRIGGER_NAME} Last { get; private set; }

	/// <summary>
	/// If true, the trigger will process events.
	/// </summary>
	public Boolean IsTriggerEnabled { get; set; }

	/// <summary>
	/// Called by the Unity engine when the behaviour starts up.
	/// </summary>
	private void Start()
	{
		Last = this;
		IsTriggerEnabled = {TRIGGER_STARTING_STATE};
		gameObject.GetComponent<BoxCollider>().isTrigger = true;
		if (gameObject.renderer != null)
			renderer.enabled = false;
	}

	/// <summary>
	/// Called by the Fury Framework every time the event occurs.
	/// </summary>
	private void OnTriggerEnter(Collider other)
	{
		// Only process events if the trigger is enabled
		if (!IsTriggerEnabled) return;

		// See if the a unit entered the region
		var unit = other.gameObject.GetComponent<Unit>();

		// TODO: Most triggers will need to verify some condition of the event
		if (unit != null)
		{
			// TODO: Do something!

			// Most triggers will just destroy themselves after executing
			GameObject.Destroy(this);
		}
	}
}";
			#endregion
			
			templateGeneric = templateGeneric.Replace("{TRIGGER_NAME}", name);
			templateGeneric = templateGeneric.Replace("{TRIGGER_STARTING_STATE}", startingState ? "true" : "false");

			CreateScript("Triggers", templateGeneric, name);
		}

		private void CreateGenericTrigger(String name, Boolean startingState, EventInfo eventInfo)
		{
			#region // Define the template
			var templateGeneric = @"using System;
using System.Collections.Generic;

using UnityEngine;
using Fury.Behaviors;

/// <summary>
/// This is an automatically generated trigger template for the 
/// trigger of type {EVENT_NAME}.
/// </summary>
public class {TRIGGER_NAME} : MonoBehaviour
{
	/// <summary>
	/// For triggers with multiple copies, this property will return all scene instances of the trigger.
	/// </summary>
	public static {TRIGGER_NAME}[] All
	{
		get
		{
			return ({TRIGGER_NAME}[])GameObject.FindSceneObjectsOfType(typeof({TRIGGER_NAME}));
		}
	}

	/// <summary>
	/// Most triggers will only have one instance active, and this static property
	/// can be used to retrieve a reference to it.
	/// </summary>
	public static {TRIGGER_NAME} Last { get; private set; }

	/// <summary>
	/// If true, the trigger will process events.
	/// </summary>
	public Boolean IsTriggerEnabled { get; set; }

	/// <summary>
	/// Called by the Unity engine when the behaviour starts up.
	/// </summary>
	private void Start()
	{
		Last = this;
		IsTriggerEnabled = {TRIGGER_STARTING_STATE};
		Manager.Instance.{EVENT_NAME} += {EVENT_NAME};
	}

	/// <summary>
	/// Called by the Unity engine when the behaviour is destroyed.
	/// </summary>
	private void OnDestroy()
	{
		Manager.Instance.{EVENT_NAME} -= {EVENT_NAME};
	}

	/// <summary>
	/// Called by the Fury Framework every time the event occurs.
	/// </summary>
	private void {EVENT_NAME}({EVENT_PARAMETERS})
	{
		// Only process events if the trigger is enabled
		if (!IsTriggerEnabled) return;

		// TODO: Most triggers will need to verify some condition of the event
		if (true)
		{
			// TODO: Do something!

			// Most triggers will just destroy themselves after executing
			GameObject.Destroy(this);
		}
	}
}";
			#endregion

			var args = new List<String>();
			var pInfo = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();
			for (Int32 i = 0; i < pInfo.Length; i++)
				args.Add(pInfo[i].ParameterType + " " + pInfo[i].Name);

			templateGeneric = templateGeneric.Replace("{TRIGGER_NAME}", name);
			templateGeneric = templateGeneric.Replace("{TRIGGER_STARTING_STATE}", startingState ? "true" : "false");
			templateGeneric = templateGeneric.Replace("{EVENT_NAME}", eventInfo.Name);
			templateGeneric = templateGeneric.Replace("{EVENT_PARAMETERS}", String.Join(", ", args.ToArray()));

			CreateScript("Triggers", templateGeneric, name);
		}

		private void CreateGenericDefinition(Type baseType, ParameterInfo[] args, Dictionary<String, String> fields)
		{
			#region // Definition template
			var template = @"using System;
using System.Collections.Generic;
using UnityEngine;
using Fury.Database;
			
public class {NAME} : {BASE_TYPE}
{
	public static {NAME} Instance { get; private set; }

	public {NAME}() :
		base({BASE_PARAMETERS})
	{
		Instance = this;
	}
}";
			#endregion

			var arr = new List<String>();
			foreach (var pinfo in args)
			{
				if (pinfo.ParameterType.IsPrimitive || pinfo.ParameterType == typeof(String))
				{
					var fieldVal = fields[pinfo.Name].Trim();

					try
					{
						System.Convert.ChangeType(fieldVal, pinfo.ParameterType);
					}
					catch
					{
						Debug.LogError(String.Format("Couldn't the value [{0}] of field [{1}] to its type [{2}].",
							fieldVal, pinfo.Name, pinfo.ParameterType));
						return;
					}

					if (fieldVal.Length == 0)
					{
						Debug.LogError("No field can be left empty!");
						return;
					}

					if (pinfo.ParameterType == typeof(String)) arr.Add('"' + fieldVal + '"');
					else if (pinfo.ParameterType == typeof(Single)) arr.Add(fieldVal + 'f');
					else arr.Add(fieldVal);
				}
				else
				{
					arr.Add("null");
				}
			}

			template = template.Replace("{NAME}", fields["name"]);
			template = template.Replace("{BASE_PARAMETERS}", String.Join(", ", arr.ToArray()));
			template = template.Replace("{BASE_TYPE}", baseType.Name);

			CreateScript("Definitions", template, fields["name"]);
		}

		private void CreateScript(String prefix, String content, String name)
		{
			if (name == null || name.Length == 0)
			{
				Debug.LogError("The script name is too short!");
				return;
			}

			var path = @"Assets\Scripts\" + prefix + @"\";
			if (!System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);

			var file = path + name + ".cs";

			if (System.IO.File.Exists(file))
			{
				Debug.LogError("A file with that name already exists.");
				return;
			}

			System.IO.File.WriteAllText(file, content);
			AssetDatabase.Refresh(ImportAssetOptions.Default);
		}

		/// <summary>
		/// Resize an array while retaining old values.
		/// </summary>
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="input">The input array.</param>
		/// <param name="size">The desired size of the new array.</param>
		/// <returns>A new array of the specified size containing as many elements from the old array as possible.</returns>
		private  T[] Resize<T>(T[] input, Int32 size)
		{
			if (input == null) return new T[size];
			if (input.Length == size) return input;

			T[] output = new T[size];
			if (input.Length > 0)
				for (Int32 i = 0; i < Math.Min(size, input.Length); i++)
					output[i] = input[i];

			return output;
		}
	}
}