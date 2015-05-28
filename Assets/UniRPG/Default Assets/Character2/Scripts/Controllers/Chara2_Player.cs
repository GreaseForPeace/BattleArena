// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;

namespace UniRPG
{
	[AddComponentMenu("UniRPG/Character 2/Player")]
	public class Chara2_Player : Chara2_Base
	{

		public float minClickDistance = 1.75f;
		public bool autoPickupItems = true;
		public GameObject clickMarkerPrefab;

		// ============================================================================================================

		private LayerMask floorLayerMask = 0;
		private bool inited = false;
		private RaycastHit[] hits;

		private bool moveToInteractTarget = false;
		private Vector3 targetInteractPosition = Vector3.zero;
		private float interactDistanceMax = 0f;
		private float moveToRemoveTimer = 0f;
		private float scanTimer = 0f;

		private GameObject[] selectionRing = null; // 0:friendly, 1:neutral, 2:hostile, 3:item, 4:object (instantiated from Database.selectionRingPrefabs)
		private UniRPGGlobal.ActorType selectedCharaType = UniRPGGlobal.ActorType.Player;

		private InputManager.InputIdxCache[] inputIdx = new InputManager.InputIdxCache[5]; // 0:click-movement, 1:forward, 2:backward, 3:left, 4:right

		private GameObject clickMarker = null;

		// ============================================================================================================

		public override void Awake()
		{
			base.Awake();
			saveKey = "C2_PLAYER";
			autoSaveLoadEnabled = false;
			autoSaveDestroy = false;
		}

		public override void Start()
		{
			base.Start();
			floorLayerMask = 1 << UniRPGGlobal.DB.floorLayerMask;

			if (UniRPGGlobal.Instance.state != UniRPGGlobal.State.InMainMenu)
			{
				if (clickMarkerPrefab)
				{
					clickMarker = (GameObject)GameObject.Instantiate(clickMarkerPrefab);
					clickMarker.SetActive(false);
				}

				selectionRing = new GameObject[UniRPGGlobal.DB.selectionRingPrefabs.Length];
				for (int i = 0; i < UniRPGGlobal.DB.selectionRingPrefabs.Length; i++)
				{
					if (UniRPGGlobal.DB.selectionRingPrefabs[i])
					{
						selectionRing[i] = (GameObject)GameObject.Instantiate(UniRPGGlobal.DB.selectionRingPrefabs[i]);
						selectionRing[i].AddComponent<SimpleFollow>();
						selectionRing[i].AddComponent<SimpleAutoHide>();
						selectionRing[i].SetActive(false);
					}
					else selectionRing[i] = null;
				}
			}
		}

		private void Init()
		{
			inited = true;
			InputManager.Instance.LoadInputFromBinder(new Chara2_Player_Input());

			// get input idx caches
			inputIdx[0] = InputManager.Instance.GetInputIdx("Player", "Click-to-Move");
			inputIdx[1] = InputManager.Instance.GetInputIdx("Player", "Move Forward");
			inputIdx[2] = InputManager.Instance.GetInputIdx("Player", "Move Back");
			inputIdx[3] = InputManager.Instance.GetInputIdx("Player", "Move Left");
			inputIdx[4] = InputManager.Instance.GetInputIdx("Player", "Move Right");
		}

		protected override void SaveState()
		{
			base.SaveState();

			// only save the following if this is a true save/load and not one used between scene transitions
			if (false == UniRPGGlobal.Instance.IsAutoLoadSave)
			{
				UniRPGGlobal.SaveVector3(saveKey + "pos", _tr.position);
			}
		}

		protected override void LoadState()
		{
			base.LoadState();
			if (UniRPGGlobal.Instance.DoNotLoad) return;

			// only save the following if this is a true save/load and not one used between scene transitions
			if (false == UniRPGGlobal.Instance.IsAutoLoadSave)
			{
				_tr.position = UniRPGGlobal.LoadVector3(saveKey + "pos", _tr.position);
			}
		}

		public override UniRPGGlobal.ActorType ActorType()
		{
			return UniRPGGlobal.ActorType.Player;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (InputManager.InstanceExist) InputManager.Instance.UnloadInputBinder(new Chara2_Player_Input());
		}

		public override void Update()
		{
			if (UniRPGGlobal.Instance.state == UniRPGGlobal.State.InMainMenu) return;

			base.Update();
			if (IsLoading) return;
			if (!inited) Init();

			// *** Update selection ring
			if (TargetedCharacter != null)
			{
				// check if the targeted character changed type
				if (selectedCharaType != TargetedCharacter.ActorType())
				{
					int id = 0;  // 0:friendly, 1:neutral, 2:hostile
					if (selectedCharaType == UniRPGGlobal.ActorType.Neutral) id = 1;
					else if (selectedCharaType == UniRPGGlobal.ActorType.Hostile) id = 2;
					UpdateSelectionRing(id, false, null);
					OnCharacterTargeted(true, TargetedCharacter);
				}
			}

			// *** check if reached interact target
			if (moveToInteractTarget)
			{
				if ((TargetInteract.transform.position - _tr.position).sqrMagnitude <= interactDistanceMax)
				{					
					moveToInteractTarget = false;
					navi.Stop();
					InteractWith(TargetInteract);
				}
				else
				{
					scanTimer -= Time.deltaTime;
					if (navi.IsMovingOrPathing() == false || scanTimer <= 0.0f)
					{
						scanTimer = 1f;
						navi.MoveTo(TargetInteract.transform.position, moveSpeed, turnSpeed);
					}

					//if (navi.CurrentSpeed() == 0.0f)
					//{
					//	moveToRemoveTimer -= Time.deltaTime;
					//	if (moveToRemoveTimer <= 0.0f)
					//	{
					//		navi.Stop();
					//		moveToInteractTarget = false;
					//	}
					//}
				}
			}

			// *** check if should move in range for skill
			if (_actor.nextSkill != null)
			{
				scanTimer -= Time.deltaTime;
				if (navi.IsMovingOrPathing() == false || scanTimer <= 0.0f)
				{
					scanTimer = 1f;
					if (_actor.nextSkillTarget != null) 
					{
						_actor.nextSkillLocation = _actor.nextSkillTarget.transform.position;
					}
					if ((_actor.nextSkillLocation - _tr.position).sqrMagnitude > _actor.nextSkillMaxDistance)
					{	// move closer to target
						navi.MoveTo(_actor.nextSkillLocation, moveSpeed, turnSpeed);
					}
					else
					{	// at least look in correct direction					
						if (!IsLookingAt(_actor.nextSkillLocation))
						{
							navi.LookAt(_actor.nextSkillLocation);
						}
					}
				}
			}

			// *** update AOE targeting effect
			if (_actor.InAOESelectMode && _actor.AOESkill != null)
			{	// don't bother if the marker is null and not shown anyway
				if (_actor.AOESkill.AOEMarkerIsValid)
				{
					Ray ray = UniRPGGlobal.ActiveCamera.Camera.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 100f, floorLayerMask))
					{
						_actor.AOESkill.UpdateAOEMarkerPosition(hit.point);
					}
				}
			}
		}

		public void LateUpdate()
		{
			hits = null; // clear it for next frame
		}

		// ============================================================================================================

		public void OnInput_ClickMove()
		{
			if (!enabled) return;
			if (_actor.InAOESelectMode) return;

			// check if performing skill that can't be interrupted
			if (_actor.currSkill != null)
			{
				if (!_actor.currSkill.ownerCanInterrupt) return;
			}

			// if hits is set then some other action already collected this info in this frame
			if (hits == null)
			{
				Ray ray = UniRPGGlobal.ActiveCamera.Camera.ScreenPointToRay(Input.mousePosition);
				hits = Physics.RaycastAll(ray, 100f);
				if (hits.Length > 0) hits.Sort();
			}
		
			for (int i=0; i < hits.Length; i++)
			{
				// Ignore any hit against a trigger
				if (hits[i].collider.isTrigger)
				{
					// check if it is the trigger of an item
					RPGItem it = hits[i].transform.gameObject.GetComponent<RPGItem>();
					if (it != null) break;	// click on item, not floor
					else continue;			// else ignore the trigger
				}

				// check if floor was hit and start movement there
				if (floorLayerMask.HasLayer(hits[i].transform.gameObject.layer))
				{
					if (moveToInteractTarget)
					{
						moveToRemoveTimer -= Time.deltaTime;
						if (moveToRemoveTimer <= 0.0f) moveToInteractTarget = false;
					}
					_actor.ClearQueuedSkill();

					// do not bother to move if click is too close to player as it will cause jerky movement
					if ((hits[i].point - _tr.position).sqrMagnitude >= minClickDistance * minClickDistance)
					{
						if (clickMarker)
						{
							clickMarker.transform.position = hits[i].point;
							clickMarker.SetActive(true);
							if (clickMarker.animation) clickMarker.animation.Play();
						}

						navi.MoveTo(hits[i].point, moveSpeed, turnSpeed);
					}
					return;
				}

				// drop out cause any clicks to follow is not useful if player did 
				// not click floor by now then he obviously meant to click 
				// something else and might not want to be running towards 
				// that position right now
				break;
			}
		}

		public void OnInput_SelectTarget()
		{
			if (!enabled) return;

			// if hits is set then some other action already collected this info in this frame
			if (hits == null)
			{
				Ray ray = UniRPGGlobal.ActiveCamera.Camera.ScreenPointToRay(Input.mousePosition);
				hits = Physics.RaycastAll(ray, 100f);
				if (hits.Length > 0) hits.Sort();
			}

			// AOE targeting is also bound to OnInput_SelectTarget
			if (_actor.InAOESelectMode)
			{
				// find the floor and submit AOE
				for (int i = 0; i < hits.Length; i++)
				{
					if (floorLayerMask.HasLayer(hits[i].transform.gameObject.layer))
					{
						_actor.SubmitAOETarget(hits[i].point);
						break;
					}
				}
				return;
			}

			// Any clicks will cause following to happen no matter what
			if (hits.Length > 0)
			{
				_actor.ClearQueuedSkill();
			}

			if (_actor.IsPerformingSkill)
			{
				return; // can't interrupt
			}

			// Find the Interactable that was clicked
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].transform.gameObject == gameObject) continue;	// Ignore click on player itself

				// check what was clicked
				Interactable obj = hits[i].transform.gameObject.GetComponent<Interactable>();
				if (obj != null)
				{
					if (obj == TargetInteract) break;	// Check if the target is already selected and ignore if so

					if (hits[i].collider.isTrigger)
					{
						// only triggers on items may be clicked on
						if (!(obj is RPGItem)) continue;
					}

					if (!obj.canBeTargeted) break;		// Can it be targeted?

					if (moveToInteractTarget)
					{
						navi.Stop();
						moveToInteractTarget = false;
					}

					ClearTarget();						// Clear active target

					// now select new target
					TargetInteract = obj;
					UniRPGGameController.ExecuteActions(TargetInteract.onTargetedActions, TargetInteract.gameObject, null, gameObject, null, null, false);
					return;
				}

				if (hits[i].collider.isTrigger) continue; // Ignore any hit against a trigger

				// anything to follow would be an invalid click since current 
				// click was possibly valid and click failed otherwise
				break;
			}
		}

		public void OnInput_Interact()
		{
			if (!enabled) return;
			if (_actor.InAOESelectMode) return;

			// if hits is set then some other action already collected this info in this frame
			if (hits == null)
			{
				Ray ray = UniRPGGlobal.ActiveCamera.Camera.ScreenPointToRay(Input.mousePosition);
				hits = Physics.RaycastAll(ray, 100f);
				if (hits.Length > 0) hits.Sort();
			}

			// Any clicks will cause following to happen no matter what
			if (hits.Length > 0)
			{
				_actor.ClearQueuedSkill();
			}

			if (_actor.IsPerformingSkill)
			{
				return; // can't interrupt
			}

			// Find the Interactable that was clicked
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].transform.gameObject == gameObject) continue;	// Ignore click on player itself

				// check what was clicked
				Interactable obj = hits[i].transform.gameObject.GetComponent<Interactable>();
				if (obj != null)
				{
					if (hits[i].collider.isTrigger)
					{
						// only triggers on items may be clicked on
						if (!(obj is RPGItem)) continue;
					}

					if (!obj.canBeTargeted) break;		// Can it be targeted?

					// if not yet targeted, target it now and then interact
					if (obj != TargetInteract)
					{
						moveToInteractTarget = false;
						ClearTarget();
						TargetInteract = obj;
						UniRPGGameController.ExecuteActions(TargetInteract.onTargetedActions, TargetInteract.gameObject, null, gameObject, null, null, false);
					}

					// now start interact procedure
					if (TargetInteract.onInteractActions.Count > 0 || TargetedItem != null || TargetedLootDrop != null)
					{
						interactDistanceMax = TargetInteract.interactDistance * TargetInteract.interactDistance;
						if ((TargetInteract.transform.position - _tr.position).sqrMagnitude <= interactDistanceMax)
						{	// in range to interact with it
							navi.Stop();
							InteractWith(TargetInteract);
						}
						else
						{	// move closer
							moveToInteractTarget = true; // need to move closer so set this 
							moveToRemoveTimer = 0.3f;
							targetInteractPosition = TargetInteract.transform.position;
							navi.MoveTo(targetInteractPosition, moveSpeed, turnSpeed);
						}
					}


				}

				if (hits[i].collider.isTrigger) continue; // Ignore any hit against a trigger

				// anything to follow would be an invalid click since current 
				// click was possibly valid and click failed otherwise
				break;
			}
		}

		public void OnInput_ClearTarget()
		{
			if (_actor.InAOESelectMode)
			{	// simply remove the AOE target
				_actor.CancelAOETargeting();
			}
			else
			{	// A normal un-target
				ClearTarget();
			}
		}

		public void OnInput_UseSlot(int slot, bool isDoubleTap)
		{
			if (!enabled) return;
			navi.Stop();
			_actor.UseActionSlot(slot, (TargetInteract == null ? null : TargetInteract.gameObject));
			if (isDoubleTap) _actor.UseActionSlot(slot, (TargetInteract == null ? null : TargetInteract.gameObject)); // call again to cause queuing
		}

		public void OnInput_AWSDMovement(int param)
		{
			moveToInteractTarget = false;
			Transform _camTr = UniRPGGlobal.ActiveCamera.transform;
			switch (param)
			{
				case 1:	// Forward
				{
					if (InputManager.Instance.IsHeld(inputIdx[3])) Move(_camTr.forward - _camTr.right);				// left is held too
					else if (InputManager.Instance.IsHeld(inputIdx[4])) Move(_camTr.forward + _camTr.right);		// right is held too
					else Move(_camTr.forward);
				} break;

				case 2: // Backward
				{
					if (InputManager.Instance.IsHeld(inputIdx[3])) Move((_camTr.forward * -1) - _camTr.right);		// left is held too
					else if (InputManager.Instance.IsHeld(inputIdx[4])) Move((_camTr.forward * -1) + _camTr.right); // right is held too
					else Move(_camTr.forward * -1);
				} break;

				case 3: // Left
				{
					if (InputManager.Instance.IsHeld(inputIdx[1])) Move((_camTr.right * -1) + _camTr.forward);		// forward is held too
					else if (InputManager.Instance.IsHeld(inputIdx[2])) Move((_camTr.right + _camTr.forward) * -1); // backward is held too
					else Move(_camTr.right * -1);
				} break;

				case 4: // Right
				{
					if (InputManager.Instance.IsHeld(inputIdx[1])) Move(_camTr.right + _camTr.forward);				// forward is held too
					else if (InputManager.Instance.IsHeld(inputIdx[2])) Move(_camTr.right - _camTr.forward);		// backward is held too
					else Move(_camTr.right);
				} break;
			}
		}

		private void InteractWith(Interactable obj)
		{
			// first check what it is before calling the OnInteract actions
			//bool wasItemOrDrop = false;
			GameObject destroyItem = null;

			RPGLootDrop drop = (obj is RPGLootDrop ? obj as RPGLootDrop : null);
			
			if (drop != null)
			{	// just in case the item lands on the player before the "drop" component is removed
				if (drop.loot == null) drop = null;
			}

			if (drop != null)
			{
				//wasItemOrDrop = true;
				destroyItem = obj.gameObject;

				// find what items the drop offers and update the player as needed
				for (int i = 0; i < drop.loot.rewards.Count; i++)
				{
					if (drop.loot.rewards[i].count == 0f) continue;

					if (drop.loot.rewards[i].type == RPGLoot.Reward.RewardType.Currency)
					{
						_actor.currency += (int)drop.loot.rewards[i].count;
					}
					else if (drop.loot.rewards[i].type == RPGLoot.Reward.RewardType.Attribute)
					{
						RPGAttribute att = _actor.ActorClass.GetAttribute(drop.loot.rewards[i].guid);
						if (att != null) att.Value += drop.loot.rewards[i].count;
					}
					else if (drop.loot.rewards[i].type == RPGLoot.Reward.RewardType.Item)
					{
						RPGItem item = UniRPGGlobal.DB.GetItem(drop.loot.rewards[i].guid);
						if (item != null) _actor.AddToBag(item, (int)drop.loot.rewards[i].count);
					}
				}
			}
			else
			{
				// if an item, then attempt to pick it up
				RPGItem item = (obj is RPGItem ? obj as RPGItem : null);
				if (item != null)
				{
					//wasItemOrDrop = true;
					destroyItem = item.gameObject;

					// get a reference to the item's prefab and save that to the bag. do not save item instances in the bag!
					if (UniRPGGlobal.DB.RPGItems.ContainsKey(item.prefabId.Value))
					{
						_actor.AddToBag(UniRPGGlobal.DB.RPGItems[item.prefabId.Value], 1);
					}
				}
			}

			//if (!wasItemOrDrop) UniRPGGameController.ExecuteActions(obj.onInteractActions, obj.gameObject, null, gameObject, null, null, false);
			UniRPGGameController.ExecuteActions(obj.onInteractActions, obj.gameObject, null, gameObject, null, null, false);
			if (destroyItem != null) Destroy(destroyItem);
		}

		public override bool CanPerformSkillNow(RPGSkill skill, Vector3 skillTargetLocation) 
		{
			if (skillTargetLocation == _tr.position) return true; // standing on top of target location
			if (IsLookingAt(skillTargetLocation)) return true;
			else navi.LookAt(skillTargetLocation);
			return false;
		}

		private bool IsLookingAt(Vector3 position)
		{
			Vector3 d = (position - _tr.position);
			d.y = _tr.forward.y; d = d.normalized;
			if (Vector3.Dot(_tr.forward, d) > 0.8f) return true;
			return false;
		}

		private void Move(Vector3 direction)
		{
			// first check if a skill is being performed and if it can be interrupted by Self
			if (_actor.currSkill != null)
			{
				if (!_actor.currSkill.ownerCanInterrupt) return;
				_actor.StopAndClearAllSkill();
			}

			direction.y = 0f;
			// find point in target direction and ask navi to move player there
			navi.MoveTo(_tr.position + direction.normalized, moveSpeed, turnSpeed);
		}

		// ============================================================================================================

		void OnTriggerEnter(Collider collider)
		{
			// only bother to check if an item if the auto-pickup system is on
			if (autoPickupItems)
			{
				// check if it was a loot drop
				RPGLootDrop loot = collider.GetComponent<RPGLootDrop>();
				if (loot)
				{
					InteractWith(loot);
					return;
				}

				// check if it was an item and if it should be picked up
				RPGItem item = collider.GetComponent<RPGItem>();
				if (item)
				{
					if (item.autoPickup)
					{
						InteractWith(item);
						return;
					}
				}
			}

			// Send a message to the object to inform it that the Player has collided with it.
			// In the case of a Hostile NPC this will be how it "detects" the player but anything
			// can listen for this message. It might happened that the trigger is on an object 
			// which is a child of another object. For example the case where the designer wants
			// a player object to be able to move through an NPC object and he set those to not 
			// detect collision between each other (on layers) in that case it would not work 
			// if the trigger was on the main object and it might be placed in a child object 
			// which is on a different layer. so send the message "upward" so that the parent
			// object of the trigger receives the message if it is setup like that

			collider.gameObject.SendMessageUpwards("OnPlayerEnterTrigger", SendMessageOptions.DontRequireReceiver);
		}

		void OnTriggerExit(Collider collider)
		{
			collider.gameObject.SendMessageUpwards("OnPlayerExitTrigger", SendMessageOptions.DontRequireReceiver);
		}

		void OnTriggerStay(Collider collider)
		{
			collider.gameObject.SendMessageUpwards("OnPlayerStayTrigger", SendMessageOptions.DontRequireReceiver);
		}

		// ============================================================================================================

		private void UpdateSelectionRing(int id, bool show, Transform followThis)
		{
			if (selectionRing[id] != null)
			{
				if (show)
				{
					selectionRing[id].GetComponent<SimpleFollow>().followTr = followThis;
					selectionRing[id].GetComponent<SimpleAutoHide>().goToWatch = followThis.gameObject;
					selectionRing[id].SetActive(true);
				}
				else selectionRing[id].SetActive(false);
			}
		}

		protected override void OnCharacterTargeted(bool becomeTargeted, CharacterBase character)
		{
			int id = 0;
			if (character.ActorType() == UniRPGGlobal.ActorType.Neutral) id = 1;
			else if (character.ActorType() == UniRPGGlobal.ActorType.Hostile) id = 2;
			selectedCharaType = character.ActorType();
			UpdateSelectionRing(id, becomeTargeted, character.transform);
		}

		protected override void OnItemTargeted(bool becomeTargeted, RPGItem item)
		{
			UpdateSelectionRing(3, becomeTargeted, item.transform);
		}

		protected override void OnObjectTargeted(bool becomeTargeted, RPGObject obj)
		{
			UpdateSelectionRing(4, becomeTargeted, obj.transform);
		}

		protected override void OnLootDropTargeted(bool becomeTargeted, RPGLootDrop lootDrop)
		{
			UpdateSelectionRing(3, becomeTargeted, lootDrop.transform);
		} 

		// ============================================================================================================
	}
}