// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("UniRPG/RPG Item")]
public class RPGItem : Interactable
{
	[System.Serializable]
	public class Category
	{
		public string screenName = string.Empty;		// catagory name, Armour, Weapon, etc
		public List<string> types = new List<string>();	// sub-type - Light, Meadium, Heavy, Axe, Sword, etc
	}

	public GUID prefabId = new GUID();		// the prefab id is unique for each RPGItem PREFAB and a helper to find the correct prefab from the DB

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField] private string nm = string.Empty;

	public string description = string.Empty;
	public string notes = string.Empty;		// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] icon = new Texture2D[3]; // up to 3 icons. what is used depends on game gui. editor uses the 1st if avail

	public int price = 1;					// currency value of item (to buy and sell - note that shop keepers might buy at less or even sell at more depending on settings on NPC)
	public int maxStack = 1;				// how many of this can be in a stack in one bag slot
	public bool consumable = false;			// if true then the item will be consumed (destroyed) if used-from-bag

	public int categoryId = 0;				// this is used to give order to the kinds of items. See Database.itemCategories
	public int subTypeId = 0;				// type within category (0 = -1st or not-set if none defined) see class ItemCategory

	public bool equipWhenUseFromBag = false;// else, call OnUse event (Interactable)
	public bool autoPickup = true;			// can player pickup the item when running over it in scene? Note the item must have collider set to Trigger, and Database.autoPickupItems must be true

	public GameObject lootDropPrefab;		// (optional) prefab to use when creating a loot drop. if not set then item will be spawned as is.

	// when equipWhenUseFromBag
	public UniRPGGlobal.Target equipTargetMask = UniRPGGlobal.Target.Player; // what can be target when equiping
	public List<int> validEquipSlots = new List<int>();	// slots that this can be equipped on if OnUseFromBag = Equip

	// ================================================================================================================

	public override void Reset()
	{
		base.Reset();
		prefabId = GUID.Create(prefabId);
	}

	// ================================================================================================================
} }