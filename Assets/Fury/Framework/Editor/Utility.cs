using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

internal class Utility : MonoBehaviour
{
	[MenuItem("Assets/Create Definition/Map")]
	static void CreateMap() { Create<Fury.Database.Map>(); }

	[MenuItem("Assets/Create Definition/Deposit")]
	static void CreateDeposit() { Create<Fury.Database.Deposit>(); }

	[MenuItem("Assets/Create Definition/Energy")]
	static void CreateEnergy() { Create<Fury.Database.Energy>(); }

	[MenuItem("Assets/Create Definition/Generator")]
	static void CreateGenerator() { Create<Fury.Database.Generator>(); }

	[MenuItem("Assets/Create Definition/Token")]
	static void CreateToken() { Create<Fury.Database.Token>(); }

	[MenuItem("Assets/Create Definition/Unit")] 
	static void CreateUnit() { Create<Fury.Database.Unit>(); }

	[MenuItem("Assets/Create Definition/Weapon")]
	static void CreateWeapon() { Create<Fury.Database.Weapon>(); }

	[MenuItem("Assets/Create Definition/Custom")]
	static void CreateCustomDefinition()
	{
		Type defType = typeof(Fury.Database.Definition);
		Boolean flag = false;

		foreach (var obj in Selection.objects)
			if (obj is MonoScript)
			{
				var underlyingType = (obj as MonoScript).GetClass();
				if (defType.IsAssignableFrom(underlyingType))
				{
					Create(underlyingType);
					flag = true;
				}
			}

		if (!flag)
		{
			EditorUtility.DisplayDialog("Couldn't create definition",
				"You must select a script that derives from Fury.Database.Definition", "OK");
		}
	}

	private static void Create<T1>() where T1 : Fury.Database.Definition
	{
		Create(typeof(T1));
	}

	private static void Create(Type type)
	{
		if (!System.IO.Directory.Exists("Assets/Definitions"))
			AssetDatabase.CreateFolder("Assets", "Definitions");
		var asset = ScriptableObject.CreateInstance(type);
		var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Definitions/" + type.Name + ".asset");
		AssetDatabase.CreateAsset(asset, path);
	}
}