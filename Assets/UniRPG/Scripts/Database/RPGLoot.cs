// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

public class RPGLoot : ScriptableObject
{
	[System.Serializable]
	public class Reward
	{
		public enum RewardType { Item = 0, Currency = 1, Attribute = 2 }
		public RewardType type = RewardType.Item;
		public float count = 1;
		public GUID guid;			// item and attrib
		public float chance = 100f;	// 0% to 100%
		public bool lookatNpcLevel = false;
		public int npcMaxLevel = 1;
		public int group = 0;
	}

	public GUID id;

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField] private string nm = string.Empty;

	public bool dropAsBag = true;	// drop the loot as a bag or individual pieces?
	public GameObject bagPrefab;	// the bag

	public List<Reward> rewards = new List<Reward>(0);

	// ================================================================================================================

	public void CopyTo(RPGLoot l)
	{
		l.id = this.id.Copy();
		l.screenName = this.screenName;
		l.dropAsBag = this.dropAsBag;
		l.bagPrefab = this.bagPrefab;
		foreach (Reward r in this.rewards)
		{
			l.rewards.Add(new Reward()
			{
				type = r.type,
				count = r.count,
				guid = r.guid.Copy(),
				chance = r.chance,
				lookatNpcLevel = r.lookatNpcLevel,
				npcMaxLevel = r.npcMaxLevel,
				group = r.group
			});
		}
	}

	void OnEnable()
	{	// This function is called when the object is loaded (used for similar reasons to MonoBehaviour.Reset)
		id = GUID.Create(id);
	}

	public class RewardGroup
	{
		public List<Reward> rewards = new List<Reward>();
		public Reward selected=null;
	}

	public static void CreateLootDrops(RPGLoot loot, Vector3 fromPoint, int npcLevel)
	{
		// ** Figure out which rewards will be selected from groups
		Dictionary<int, RewardGroup> groups = new Dictionary<int, RewardGroup>();

		for (int i = 0; i < loot.rewards.Count; i++)
		{
			if (loot.rewards[i].group != 0)
			{
				if (!groups.ContainsKey(loot.rewards[i].group))
				{
					RewardGroup rg = new RewardGroup();
					rg.rewards.Add(loot.rewards[i]);
					groups.Add(loot.rewards[i].group, rg);
				}
				else
				{
					groups[loot.rewards[i].group].rewards.Add(loot.rewards[i]);
				}
			}
		}

		if (groups.Count > 0)
		{
			foreach (RewardGroup rg in groups.Values)
			{
				int r = rg.rewards.Count > 1 ? Random.Range(0, rg.rewards.Count) : 0;
				rg.selected = rg.rewards[r];
			}
		}

		// ** Drop a Loot Bag
		if (loot.dropAsBag)
		{
			if (loot.bagPrefab == null)
			{
				Debug.LogError("Create Loot Action Error: The Bag Prefab must be set.");
				return;
			}

			GameObject obj = (GameObject)GameObject.Instantiate(loot.bagPrefab);
			obj.transform.position = fromPoint;

			// make sure the prefab has a LootDrop component on it
			RPGLootDrop drop = obj.GetComponent<RPGLootDrop>();
			if (drop == null) drop = obj.AddComponent<RPGLootDrop>();

			// also make sure it has a trigger so that the player can actually interact with it
			Collider coll = obj.GetComponent<Collider>();
			if (coll == null) coll = obj.AddComponent<BoxCollider>();
			coll.isTrigger = true;

			RPGLoot lt = ScriptableObject.CreateInstance<RPGLoot>();	// create new loot bag which holds the rewards
			for (int i = 0; i < loot.rewards.Count; i++)
			{
				// first check if reward in a group and if it is the one chosen to be used from the group
				if (loot.rewards[i].group != 0)
				{
					if (groups[loot.rewards[i].group].selected != loot.rewards[i]) continue;
				}

				// check if the reward is allowed for specified NPC level
				if (loot.rewards[i].lookatNpcLevel && npcLevel < loot.rewards[i].npcMaxLevel) continue;

				// check the chance of spawning this loot
				if (!CanSpawnOnChance(loot.rewards[i].chance)) continue;

				// add the reward
				lt.rewards.Add(loot.rewards[i]);
			}
			drop.DoDrop(lt);
		}

		// ** Drop individual Loot Items
		else
		{
			for (int i = 0; i < loot.rewards.Count; i++)
			{
				// first check if reward in a group and if it is the one chosen to be used from the group
				if (loot.rewards[i].group != 0)
				{
					if (groups[loot.rewards[i].group].selected != loot.rewards[i]) continue;
				}

				// check if the reward is allowed for specified NPC level
				if (loot.rewards[i].lookatNpcLevel && npcLevel < loot.rewards[i].npcMaxLevel) continue;

				// check the chance of spawning this loot
				if (!CanSpawnOnChance(loot.rewards[i].chance)) continue;

				// CURRENCY
				if (loot.rewards[i].type == Reward.RewardType.Currency)
				{
					if (UniRPGGlobal.DB.currencyDropPrefab == null)
					{
						Debug.LogError("Could not create Currency Loot. The Currency LootDrop Prefab is not set.");
						continue;
					}

					GameObject obj = (GameObject)GameObject.Instantiate(UniRPGGlobal.DB.currencyDropPrefab);
					obj.transform.position = fromPoint;

					// make sure the prefab has a LootDrop component on it
					RPGLootDrop drop = obj.GetComponent<RPGLootDrop>();
					if (drop == null) drop = obj.AddComponent<RPGLootDrop>();

					// also make sure it has a trigger so that the player can actually interact with it
					Collider coll = drop.gameObject.GetComponent<Collider>();
					if (coll == null) coll = drop.gameObject.AddComponent<BoxCollider>();
					coll.isTrigger = true;

					RPGLoot lt = ScriptableObject.CreateInstance<RPGLoot>();	// create new loot
					lt.rewards.Add(loot.rewards[i]);							// which holds the one reward
					drop.DoDrop(lt);
				}

				// ATTRIBUTE
				else if (loot.rewards[i].type == Reward.RewardType.Attribute)
				{
					RPGAttribute att = UniRPGGlobal.DB.GetAttribute(loot.rewards[i].guid);
					if (att == null)
					{
						Debug.LogError("Could not create Attribute Loot. The specified Attribute could not be found.");
						continue;
					}

					if (att.lootDropPrefab == null)
					{
						Debug.LogError("Could not create Attribute Loot. The Attribute's LootDrop Prefab is not set.");
						continue;
					}

					GameObject obj = (GameObject)GameObject.Instantiate(att.lootDropPrefab);
					obj.transform.position = fromPoint;

					// make sure the prefab has a LootDrop component on it
					RPGLootDrop drop = obj.GetComponent<RPGLootDrop>();
					if (drop == null) drop = obj.AddComponent<RPGLootDrop>();

					// also make sure it has a trigger so that the player can actually interact with it
					Collider coll = drop.gameObject.GetComponent<Collider>();
					if (coll == null) coll = drop.gameObject.AddComponent<BoxCollider>();
					coll.isTrigger = true;

					RPGLoot lt = ScriptableObject.CreateInstance<RPGLoot>();	// create new loot
					lt.rewards.Add(loot.rewards[i]);							// which holds the one reward
					drop.DoDrop(lt);
				}

				// ITEM
				else if (loot.rewards[i].type == Reward.RewardType.Item)
				{
					RPGItem item = UniRPGGlobal.DB.GetItem(loot.rewards[i].guid);
					if (item == null)
					{
						Debug.LogError("Could not create Item Loot. The specified Item could not be found.");
						continue;
					}

					if (item.lootDropPrefab == null)
					{	// no loot prefab, spawn item normal
						GameObject obj = (GameObject)GameObject.Instantiate(item.gameObject);
						obj.transform.position = fromPoint;

						RPGItem it = obj.GetComponent<RPGItem>();
						it.IsPersistent = false;
		
						RPGLootDrop drop = obj.AddComponent<RPGLootDrop>();
						drop.DoDrop(null); // no reward link. just play drop animation on the item
					}
					else
					{
						// in the case of item one of each copy must be created and dropped
						for (int j = 0; j < (int)loot.rewards[i].count; j++)
						{
							GameObject obj = (GameObject)GameObject.Instantiate(item.lootDropPrefab);
							obj.transform.position = fromPoint;

							// make sure the prefab has a LootDrop component on it
							RPGLootDrop drop = obj.GetComponent<RPGLootDrop>();
							if (drop == null) drop = obj.AddComponent<RPGLootDrop>();

							// also make sure it has a trigger so that the player can actually interact with it
							Collider coll = drop.gameObject.GetComponent<Collider>();
							if (coll == null) coll = drop.gameObject.AddComponent<BoxCollider>();
							coll.isTrigger = true;

							RPGLoot lt = ScriptableObject.CreateInstance<RPGLoot>();	// create new loot
							lt.rewards.Add(loot.rewards[i]);							// which holds the one reward
							drop.DoDrop(lt);
						}
					}
				}
			}
		}
	}

	private static bool CanSpawnOnChance(float chance)
	{
		if (chance == 0f) return false;
		if (chance == 100f) return true;
		if (Random.Range(0f, 100f) <= chance) return true;
		return false;
	}

	// ================================================================================================================
} }
