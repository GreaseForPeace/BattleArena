// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class BagAction : Action
{
	public bool exitWhenFail = true;
	public int doWhat = 0; // 0:add an item from equipslot, 1:add specified item, 2:remove item from a slot
	public NumericValue numberOfItems = new NumericValue(1);

	// for adding item rom equip slot
	public int equipSlotOption = 0; // 0:named, 1:number
	public NumericValue equipSlotId = new NumericValue(0);

	// for adding specific item
	public RPGItem specifiedItem = null;

	// for removing items
	public NumericValue bagSlotId = new NumericValue(0);

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		BagAction a = act as BagAction;
		a.exitWhenFail = this.exitWhenFail;
		a.doWhat = this.doWhat;
		a.numberOfItems = this.numberOfItems.Copy();
		a.equipSlotOption = this.equipSlotOption;
		a.equipSlotId = this.equipSlotId.Copy();
		a.specifiedItem = this.specifiedItem;
		a.bagSlotId = this.bagSlotId.Copy();
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
		if (obj != null)
		{
			Actor actor = obj.GetComponent<Actor>();
			if (actor)
			{
				switch (doWhat)
				{
					case 0: // add item from equip slot
					{
						RPGItem item = actor.GetEquippedItem((int)equipSlotId.Value);
						if (item != null)
						{
							if (!actor.AddToBag(item, 1))
							{
#if UNITY_EDITOR
								Debug.LogError(string.Format("Bag Action Error: Failed to add 1 copy of {0} from equip slot {1}", item.screenName, UniRPGGlobal.DB.equipSlots[(int)equipSlotId.Value]));
#endif
								return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
							}
						}


					} break;
					
					case 1: // add item
					{
						if (specifiedItem != null)
						{
							if (!actor.AddToBag(specifiedItem, (int)numberOfItems.Value))
							{
#if UNITY_EDITOR
								Debug.LogError(string.Format("Bag Action Error: Failed to add {0} copie(s) of {1}", (int)numberOfItems.Value, specifiedItem.screenName));
#endif
								return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
							}
						}
						else
						{
							Debug.LogError("Bag Action Error: The RPG Item was null.");
							return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
						}

					} break;

					case 2: // remove item from slot
					{
						actor.RemoveFromBag((int)bagSlotId.Value, (int)numberOfItems.Value);
					} break;
				}
			}
			else
			{
				Debug.LogError("Bag Action Error: The subject must be an Actor.");
				return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
			}
		}
		else
		{
			Debug.LogError("Bag Action Error: The subject did not exist.");
			return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
		}

		return ReturnType.Done;
	}

	// ================================================================================================================
} }