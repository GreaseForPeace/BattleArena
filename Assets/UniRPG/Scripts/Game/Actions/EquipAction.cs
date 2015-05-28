// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG {

[AddComponentMenu("")]
public class EquipAction : Action
{
	public bool exitWhenFail = false;
	public int doWhat = 0; // 0:add an item from bag, 1:add specified item, 2:remove item from a slot
	
	public int equipSlotOption = 0; // 0:named, 1:number
	public NumericValue equipSlotId = new NumericValue(0);

	// for adding item from bag slot
	public NumericValue bagSlotId = new NumericValue(0);

	// for adding specific item
	public RPGItem specifiedItem = null;

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		EquipAction a = act as EquipAction;
		a.exitWhenFail = this.exitWhenFail;
		a.doWhat = this.doWhat;
		a.equipSlotOption = this.equipSlotOption;
		a.equipSlotId = this.equipSlotId.Copy();
		a.bagSlotId = this.bagSlotId.Copy();
		a.specifiedItem = this.specifiedItem;
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
					case 0: // add item from bag slot
					{
						RPGItem item = actor.GetBagItem((int)bagSlotId.Value);
						if (item != null)
						{
							actor.Equip((int)equipSlotId.Value, item);
						}
						else
						{
							Debug.LogError("Equip Action Error: The RPG Item was null.");
							return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
						}
					} break;

					case 1: // add item
					{
							if (specifiedItem != null)
							{
								actor.Equip((int)equipSlotId.Value, specifiedItem);
							}
							else
							{
								Debug.LogError("Equip Action Error: The RPG Item was null.");
								return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
							}

					} break;

					case 2: // remove item from slot
					{
						actor.UnEquip((int)equipSlotId.Value);
					} break;
				}
			}
			else
			{
				Debug.LogError("Equip Action Error: The subject must be an Actor.");
				return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
			}
		}
		else
		{
			Debug.LogError("Equip Action Error: The subject did not exist.");
			return (exitWhenFail ? ReturnType.Exit : ReturnType.Done);
		}

		return ReturnType.Done;
	}
	

	// ================================================================================================================
} }