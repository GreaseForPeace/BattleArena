// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

public class RPGActorClass : ScriptableObject
{
	public GUID id;								// unique identifier of this class definition

	//use screenName to get/set the visible name for this definition. do not depend on .name to be valid
	public string screenName { get { return nm; } set { nm = value; name = value; } }
	[SerializeField]private string nm = string.Empty;

	public string description = string.Empty;	
	public string notes = string.Empty;			// additional notes (used by designer, should not be something used in the game itself)
	public Texture2D[] icon = new Texture2D[3]; // up to 3 icons. what is used depends on game gui. editor uses the 1st if avail

	public string guiHelper;					// this can be used to help the gui determine what to do with this attrib (what you use here depends on the gui theme)

	public bool availAtStart = false;			// is this class available for selection at startup?

	public List<RPGAttributeData> attribDataFabs; // attributes (dont use directly at runtime, rather use ActorClass.attributes)

	// leveling settings
	public GUID xpAttribId;						// id of the attribute that represents Experience on the Actor (the attribute must also be rpesent in attribDataFabs)
	public int maxXP = 1000;					// the max that the XP Attribute's value can reach
	public int maxLevel = 10;					// the Level at max xp
	public AnimationCurve levelCurve;			// determines what level the actor is on depending on the xp on the curve (Time is Level, and Value is XP)

	// ----------------------------------------------------------------------------------------------------------------
	// Runtime ...

	public List<RPGAttribute> Attributes { get; set; }						// the runtime/instantiated attr
	private List<RPGAttribute> _RegenAttribs = new List<RPGAttribute>(0);	// helper that keeps tract of only the attributs that need to be called by Update()

	private RPGAttribute xpAttrib = null;		// the attribute that is used as XP. Leveling system not active if this is NULL.
	public int Level { get; private set; }		// the actor's current level (available at runtime)

	private bool loadingState = false;

	// ================================================================================================================
	#region init and update 

	public void CopyTo(RPGActorClass ac)
	{
		ac.id = this.id.Copy();
		ac.screenName = this.screenName;
		ac.description = this.description;
		ac.notes = this.notes;
		ac.icon = new Texture2D[3] { this.icon[0], this.icon[1], this.icon[2] };
		ac.guiHelper = this.guiHelper;
		ac.availAtStart = this.availAtStart;
		ac.xpAttribId = this.xpAttribId;
		ac.maxXP = this.maxXP;
		ac.maxLevel = this.maxLevel;
		ac.levelCurve = AnimationCurve.Linear(1, 1, ac.maxXP, ac.maxLevel);

		if (this.attribDataFabs != null)
		{
			ac.attribDataFabs = new List<RPGAttributeData>();
			foreach (RPGAttributeData dat in this.attribDataFabs)
			{
				ac.attribDataFabs.Add(dat.Copy(this.maxLevel));
			}
		}
		else ac.attribDataFabs = null;
	}

	void OnEnable()
	{	// This function is called when the object is loaded (used for similar reasons to MonoBehaviour.Reset)
		id = GUID.Create(id);
		if (attribDataFabs == null) attribDataFabs = new List<RPGAttributeData>(0);
		if (levelCurve == null) levelCurve = AnimationCurve.Linear(1, 1, maxXP, maxLevel);
		if (xpAttribId == null) xpAttribId = new GUID();
	}

	public void Init(GameObject owningActor, int startXP)
	{
		// init the attributes
		Attributes = new List<RPGAttribute>(attribDataFabs.Count);
		for (int i = 0; i < attribDataFabs.Count; i++)
		{
			RPGAttribute attribFab = RPGAttribute.GetAttribByGuid(UniRPGGlobal.DB.attributes, attribDataFabs[i].attribId);
			if (attribFab)
			{
				RPGAttribute a = (RPGAttribute)ScriptableObject.Instantiate(attribFab);
				a.data = attribDataFabs[i];
				Attributes.Add(a);
				if (a.data.canRegen) _RegenAttribs.Add(a);

				if (a.id == xpAttribId) a.Init(owningActor, startXP); // this attrib is the one used for XP, so init it with the starting XP
				else a.Init(owningActor); // else, typical attribute
			}
			else
			{
				Debug.LogError("The Attribute definition could not be found. You might have removed it from the Database and not updated the ActorClass definition.");
			}
		}

		// init the XP attribute
		Level = 1;
		xpAttrib = GetAttribute(xpAttribId);
		if (xpAttrib != null)
		{
			xpAttrib.onValueChange += OnXPValueChange;
			OnXPValueChange(xpAttrib);
		}
	}

	public void SaveState(string key)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			UniRPGGlobal.SaveString(key + "att_" + i, Attributes[i].id.ToString() + "|" + Attributes[i].MinValue.ToString() + "|" + Attributes[i].MaxValue.ToString() + "|" + Attributes[i].NoBonusValue.ToString() + "|" + Attributes[i].Bonus.ToString());
		}
	}

	public void LoadState(string key)
	{
		loadingState = true;
		bool failed = true; // helper

		for (int i = 0; i < Attributes.Count; i++)
		{
			failed = true;
			string v = UniRPGGlobal.LoadString(key + "att_" + i, null);
			if (!string.IsNullOrEmpty(v))
			{
				string[] vs = v.Split('|');
				if (vs.Length >= 4)
				{
					GUID id = new GUID(vs[0]);
					for (int j = 0; j < Attributes.Count; j++)
					{
						if (Attributes[j].id == id)
						{
							bool good = true;
							float val = 0f, min = 0f, max = 0f, bonus = 0f;
							if (!float.TryParse(vs[1], out min)) good = false;
							if (!float.TryParse(vs[2], out max)) good = false;
							if (!float.TryParse(vs[3], out val)) good = false;
							if (vs.Length > 4)
							{
								if (!float.TryParse(vs[4], out bonus)) good = false;
							}
							if (good)
							{
								failed = false;
								Attributes[j].SetWithoutTriggeringEvents(val, min, max, bonus);
								if (xpAttrib != null)
								{
									if (xpAttrib.id == id) OnXPValueChange(xpAttrib);
								}
							}
							break;
						}
					}
				}

				if (failed)
				{
					Debug.LogWarning("ActorClass LoadState Attribute failed: Key (" + key + "att_" + i + "), Value (" + v + ")");
				}
			}
		}
		loadingState = false;
	}

	public void Update()
	{
		if (_RegenAttribs.Count > 0)
		{
			for (int i = 0; i < _RegenAttribs.Count; i++) _RegenAttribs[i].Update();
		}
	}

	private void OnXPValueChange(RPGAttribute att)
	{
		if (xpAttrib != null)
		{
			Level = (int)levelCurve.Evaluate(xpAttrib.Value);
		}
		if (Level > maxLevel) Level = maxLevel;
		if (Level < 1) Level = 1;

		if (!loadingState)
		{	// check if any attributes must be updated when the level changes
			for (int i = 0; i < Attributes.Count; i++)
			{
				if (Attributes[i].data.levelAffectMax)
				{
					Attributes[i].MaxValue = Attributes[i].data.maxAffectCurve.Evaluate(Level);
					if (Attributes[i].Value > Attributes[i].MaxValue) Attributes[i].Value = Attributes[i].MaxValue;
				}

				if (Attributes[i].data.levelAffectVal)
				{
					Attributes[i].Value = Attributes[i].data.valAffectCurve.Evaluate(Level);
				}
			}
		}
	}

	#endregion
	// ================================================================================================================
	#region pub

	/// <summary>get reference to an instance of an attribute of this class</summary>
	public RPGAttribute GetAttribute(GUID guid)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			if (Attributes[i].id == guid) return Attributes[i];
		}
		return null;
	}

	/// <summary>
	/// return the level for the amount of xp when using this class' xp curve. 
	/// fromXP and returned level is clamped to the maxXP and maxLevel values of this class
	/// </summary>
	public int CalculateLevel(int fromXP)
	{
		if (fromXP > maxXP) fromXP = maxXP; 
		if (fromXP < 0) fromXP = 0;
		int lv = (int)levelCurve.Evaluate(fromXP);
		if (lv > maxLevel) lv = maxLevel;
		if (lv < 1) lv = 1;
		return lv;
	}

	/// <summary>Increase the XP Attribute by so much. Only if such an Attribute was set. Pass a negative value to subtract.</summary>
	public void AddXP(int xp)
	{
		if (xpAttrib != null)
		{
			xpAttrib.Value += xp;
		}
	}

	/// <summary>Set the XP Attribute to this value. Only if such an Attribute was set.</summary>
	public void SetXP(int xp)
	{
		if (xpAttrib != null)
		{
			xpAttrib.Value = xp;
		}
	}

	#endregion
	// ================================================================================================================
} }