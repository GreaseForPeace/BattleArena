// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class RPGSkill : MonoBehaviour
{
	public GUID id;

	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField]private string nm = string.Empty; // rather use ScreenName to get/set the name value of this object

	public string description = string.Empty;
	public string notes = string.Empty;			// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] icon = new Texture2D[3]; // up to 3 icons. what is used depends on game gui. editor uses the 1st if avail

	public string guiHelper;					// this can be used to help the gui determine what to do with this attrib (what you use here depends on the gui theme)

	public float onUseMaxTargetDistance = 1.5f;	// how far must be from target to use this skill on it
	public UniRPGGlobal.Target validTargetsMask = (UniRPGGlobal.Target.Neutral | UniRPGGlobal.Target.Hostile);

	public float castTimeSetting = 1f;			// 0=instant, how long casting takes
	public float cooldownTimeSetting = 1f;		// how long to wait before actor can use skill again
	public bool startCooldownAfterCast = true;	// only start the cool down timer after cast timer is finished?
	public bool canBeInterrupted = false;		// FIXME: DOES NOTHING ATM. keep false if the action should not fail if an opponent hits the skill user
	public bool ownerCanInterrupt = true;		// true if the caster can interrupt the skill

	public List<Action> onUseActions = new List<Action>(0);
	public List<Action> rightActions = new List<Action>(0);

	public enum TargetingMechanic
	{
		SingleTarget = 0,	
		AroundTargeted = 1,
		AroundOwner = 2,
		AroundLocation = 3,
	}
	public TargetingMechanic targetMech = TargetingMechanic.SingleTarget;
	public float searchRadius = 5f;
	public int maxTargets = 3;
	public bool mustInclTargeted = true;
	public GameObject aoeTargetingMarkerPrefab;

	// ----------------------------------------------------------------------------------------------------------------

	// the timers are public so that the values can be read outside this class, 
	// for example when needing to show the cool down time in the gui
	public float cooldownTimer { get; set; }
	public float castTimer { get; set; }
	private bool timersRunning = false;

	private int actIdLeft = -1;		// action execution counter (-1 mean that it should not run through the actions)
	private int actIdRight = -1;	// action execution counter (-1 mean that it should not run through the actions)
	private Action.ReturnType actRetLeft = Action.ReturnType.Done;
	private Action.ReturnType actRetRight = Action.ReturnType.Done;
	private GameObject owner = null;
	private List<GameObject> targets = new List<GameObject>(0);
	private GameObject leftTarget = null;

	private GameObject aoeTargetingMarker; // this is the instance of aoeTargetingMarkerPrefab , when used

	// ================================================================================================================

	public void CopyTo(RPGSkill s)
	{
		s.id = this.id.Copy();

		s.screenName = this.screenName;
		s.description = this.description;
		s.notes = this.notes;
		s.icon = new Texture2D[3] { this.icon[0], this.icon[1], this.icon[2] };
		s.guiHelper = this.guiHelper;

		s.onUseMaxTargetDistance = this.onUseMaxTargetDistance;
		s.validTargetsMask = this.validTargetsMask;

		s.castTimeSetting = this.castTimeSetting;
		s.cooldownTimeSetting = this.cooldownTimeSetting;
		s.startCooldownAfterCast = this.startCooldownAfterCast;
		s.canBeInterrupted = this.canBeInterrupted;
		s.ownerCanInterrupt = this.ownerCanInterrupt;

		s.onUseActions = new List<Action>(0);
		GameObject g = s.gameObject;
		foreach (Action act in this.onUseActions)
		{
			Action newAct = (Action)g.AddComponent(act.GetType());
			newAct.hideFlags = HideFlags.HideInInspector;
			act.CopyTo(newAct);
			s.onUseActions.Add(newAct);
		}

		s.rightActions = new List<Action>(0);
		foreach (Action act in this.rightActions)
		{
			Action newAct = (Action)g.AddComponent(act.GetType());
			newAct.hideFlags = HideFlags.HideInInspector;
			act.CopyTo(newAct);
			s.rightActions.Add(newAct);
		}

	}

	void Reset()
	{	// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time
		id = GUID.Create(id);
	}

	void Awake()
	{
		// disable the action components since they will be manually called as needed
		for (int i = 0; i < onUseActions.Count; i++) onUseActions[i].enabled = false;
		for (int i = 0; i < rightActions.Count; i++) rightActions[i].enabled = false;

		actIdLeft = -1;
		actIdRight = -1;
		cooldownTimer = 0.0f;
		castTimer = 0.0f;

		if (aoeTargetingMarkerPrefab != null)
		{
			aoeTargetingMarker = (GameObject)GameObject.Instantiate(aoeTargetingMarkerPrefab);
			aoeTargetingMarker.name = "AOE_MARKER";
			aoeTargetingMarker.SetActive(false);
		}

		gameObject.SetActive(false); // no timers running, so disable the skill
	}

	void Update()
	{
		if (timersRunning)
		{
			castTimer -= Time.deltaTime;
			if (startCooldownAfterCast)
			{
				if (castTimer <= 0.0f)
				{
					castTimer = 0.0f;
					cooldownTimer -= Time.deltaTime;
				}
			}
			else
			{
				cooldownTimer -= Time.deltaTime;
			}

			if (cooldownTimer <= 0.0f && castTimer <= 0.0f)
			{
				timersRunning = false;
			}
		}

		UpdateLeftActions();
		UpdateRightActions();

		if (!timersRunning && actIdLeft < 0 && actIdRight < 0)
		{
			gameObject.SetActive(false);
		}
	}

	private void UpdateLeftActions()
	{
		// only run actions if id is not -1
		if (actIdLeft >= 0)
		{
			if (actRetLeft == Action.ReturnType.Done)
			{	// is a new action, so Init
				onUseActions[actIdLeft].Init(owner, leftTarget, null, null, gameObject);
			}

			actRetLeft = onUseActions[actIdLeft].Execute(owner, leftTarget, null, null, gameObject);

			if (actRetLeft == Action.ReturnType.Done)
			{
				actIdLeft++;
				if (actIdLeft >= onUseActions.Count) actRetLeft = Action.ReturnType.Exit;
			}

			else if (actRetLeft == Action.ReturnType.CallAgain)
			{
				// do nothing. will call again.
				// do not remove
			}

			else if (actRetLeft == Action.ReturnType.SkipNext)
			{
				actIdLeft += 2;
				actRetLeft = Action.ReturnType.Done;
				if (actIdLeft >= onUseActions.Count) actRetLeft = Action.ReturnType.Exit;
			}

			// ---------- note that the following should be last else if tests

			else if (actRetLeft <= Action.ReturnType.ExecuteSpecificNext && actRetLeft != Action.ReturnType.Exit)
			{
				Debug.LogError("There is no Action (0). Please specify a number higher than (0).");
				actRetLeft = Action.ReturnType.Exit;
			}

			else if (actRetLeft > Action.ReturnType.ExecuteSpecificNext)
			{
				actIdLeft = (int)actRetLeft - (int)Action.ReturnType.ExecuteSpecificNext - 1;
				actRetLeft = Action.ReturnType.Done;
				if (actIdLeft >= onUseActions.Count) actRetLeft = Action.ReturnType.Exit;
			}

			// ---------- this should not be in else-if since the above code depends on something checking for Exit

			if (actRetLeft == Action.ReturnType.Exit)
			{
				actIdLeft = -1; // done with all actions
			}
		}
	}

	private void UpdateRightActions()
	{
		if (actIdRight >= 0)
		{
			int exeId = actIdRight;
			for (int i = 0; i < targets.Count; i++)
			{
				if (i == 0)
				{
					if (actRetRight == Action.ReturnType.Done)
					{	// is a new action, so Init
						rightActions[actIdRight].Init(owner, targets[0], null, null, gameObject);
					}

					actRetRight = rightActions[actIdRight].Execute(owner, targets[0], null, null, gameObject);

					if (actRetRight == Action.ReturnType.Done)
					{
						actIdRight++;
						if (actIdRight >= rightActions.Count) actRetRight = Action.ReturnType.Exit;
					}

					else if (actRetRight == Action.ReturnType.CallAgain)
					{
						// do nothing. will call again.
						// do not remove
					}

					else if (actRetRight == Action.ReturnType.SkipNext)
					{
						actIdRight += 2;
						actRetRight = Action.ReturnType.Done;
						if (actIdRight >= rightActions.Count) actRetRight = Action.ReturnType.Exit;
					}

					// ---------- note that the following should be last else if tests

					else if (actRetRight <= Action.ReturnType.ExecuteSpecificNext && actRetRight != Action.ReturnType.Exit)
					{
						Debug.LogError("There is no Action (0). Please specify a number higher than (0).");
						actRetRight = Action.ReturnType.Exit;
					}

					else if (actRetRight > Action.ReturnType.ExecuteSpecificNext)
					{
						actIdRight = (int)actRetRight - (int)Action.ReturnType.ExecuteSpecificNext - 1;
						actRetRight = Action.ReturnType.Done;
						if (actIdRight >= rightActions.Count) actRetRight = Action.ReturnType.Exit;
					}

					// ---------- this should not be in else-if since the above code depends on something checking for Exit

					if (actRetRight == Action.ReturnType.Exit)
					{
						actIdRight = -1; // done with all actions
					}
				}

				else
				{
					rightActions[exeId].Execute(owner, targets[i], null, null, gameObject);
				}
			}
		}
	}

	/// <summary>check to see if the skill is still on cooldown or being used</summary>
	public bool IsReady
	{
		get
		{
			return (cooldownTimer <= 0.0f && castTimer <= 0.0f && actIdRight < 0 && actIdLeft < 0);
		}
	}

	/// <summary>check if the skill is being used and not just in cooldown (like IsReady does)</summary>
	public bool IsCasting
	{
		get
		{
			return (castTimer > 0.0f || actIdRight >= 0 || actIdLeft >= 0);
		}
	}

	/// <summary>call this to use the skill (OnUse actions will run and timers started)</summary>
	public void Use(GameObject owner, List<GameObject> targets, Vector3 skillExeLocation)
	{
		// the skill must be moved to the locatio nfrom where it is executed so that
		// there is some kind of physical location that can be referenced by action
		transform.position = skillExeLocation;

		gameObject.SetActive(true);
		this.owner = owner;
		this.targets = targets;
		actRetLeft = Action.ReturnType.Done;
		actRetRight = Action.ReturnType.Done;
		timersRunning = true;
		castTimer = castTimeSetting;
		cooldownTimer = cooldownTimeSetting;

		if (onUseActions.Count > 0) actIdLeft = 0; else actIdLeft = -1;
		if (rightActions.Count > 0) actIdRight = 0; else actIdRight = -1;

		leftTarget = null;
		if (targets == null) actIdRight = -1;
		else if (targets.Count == 0) actIdRight = -1;
		else leftTarget = targets[0];
	}

	/// <summary>call this to stop execution of the SKill's actions. The cool down counter will continue to run. This does not check if interrupt is allowed.</summary>
	public void Stop()
	{
		actIdLeft = -1;
		actIdRight = -1;
		castTimer = 0.0f;
	}

	/// <summary>check if this skill can be used on the target</summary>
	public bool IsValidTarget(GameObject target)
	{
		if (validTargetsMask == 0) return true; // "Nothing"
		if (((int)UniRPGGlobal.Target.Self & (int)validTargetsMask) != 0) return true;

		if (target == null)
		{
			return false;
		}
		else
		{
			if (((int)UniRPGGlobal.Target.RPGItem & (int)validTargetsMask) != 0)
			{
				RPGItem t = target.GetComponent<RPGItem>();
				if (t != null) return t.canBeTargeted;
				else return false;
			}

			if (((int)UniRPGGlobal.Target.RPGObject & (int)validTargetsMask) != 0)
			{
				RPGObject t = target.GetComponent<RPGObject>();
				if (t != null) return t.canBeTargeted;
				else return false;
			}

			Actor actor = target.GetComponent<Actor>();
			return IsValidTargetActor(actor);
		}
	}

	/// <summary>check if this skill can be used on the target</summary>
	private bool IsValidTargetActor(Actor actor)
	{
		if (((int)UniRPGGlobal.Target.Player & (int)validTargetsMask) != 0)
		{
			// player is always valid target
			if (actor != null) return actor.Character.canBeTargeted;
			else return true;
		}

		if (actor == null)
		{
			if (validTargetsMask == 0) return true; // "Nothing"
			return false;
		}
		else
		{
			if (!actor.Character.canBeTargeted) return false;
			return (((int)actor.ActorType & (int)validTargetsMask) != 0);
		}
	}

	public List<GameObject> FindTargetsAround(Vector3 pos, GameObject selectedTarget)
	{
		if (targetMech == TargetingMechanic.SingleTarget) return new List<GameObject>() { selectedTarget };
		else if (targetMech == TargetingMechanic.AroundOwner) pos = UniRPGGlobal.Player.transform.position;
		else if (targetMech == TargetingMechanic.AroundTargeted)
		{
			if (selectedTarget != null) pos = selectedTarget.transform.position;
			else 
			{
				Debug.LogError("FindTargetsAround error: selectedTarget = null");
				return null;
			}
		}

		if (searchRadius <= 0.0f)
		{
			Debug.LogError("FindTargetsAround error: searchRadius <= 0.0f");
			return null;
		}

		pos.y += 1000f;
		RaycastHit[] hits = Physics.SphereCastAll(pos, searchRadius, Vector3.down, 2000f);
		if (hits.Length > 0)
		{
			List<GameObject> ret = new List<GameObject>();
			if (mustInclTargeted && selectedTarget != null) ret.Add(selectedTarget);

			for (int i = 0; i < hits.Length; i++)
			{
				if (ret.Count >= maxTargets) break;
				if (IsValidTarget(hits[i].collider.gameObject))
				{
					if (!ret.Contains(hits[i].collider.gameObject)) ret.Add(hits[i].collider.gameObject);
				}
			}

			//Debug.Log("Found:");
			//foreach (GameObject go in ret) Debug.Log(go);

			return ret;
		}

		return null;
	}

	public void HideAOEMarker()
	{
		if (aoeTargetingMarker)
		{
			aoeTargetingMarker.SetActive(false);
		}
	}

	public void UpdateAOEMarkerPosition(Vector3 pos)
	{
		if (aoeTargetingMarker)
		{
			aoeTargetingMarker.transform.position = pos;
			if (!aoeTargetingMarker.activeSelf)
			{
				aoeTargetingMarker.SetActive(true);
			}
		}
	}

	public bool AOEMarkerIsValid { get { return aoeTargetingMarker != null; } }

	// ================================================================================================================
} }