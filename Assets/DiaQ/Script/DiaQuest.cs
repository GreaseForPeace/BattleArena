// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DiaQ
{
	[System.Serializable]
	public class DiaQuest
	{
		[System.Serializable]
		public class Condition
		{
			// example. ident: wolf_pelts. text: "Collect 5 wolf pelts". target: 5. 

			public string ident = "";	// the key that this condition id identified by. a quest should not have conditions with dupicate idents.
			public string text="";		// the description of this condition. normally used as the text presened to the player
			public int target = 1;		// the target value. how many times this condition should be met before it is considered finished. must be 1 or more.
			
			// -- runtime
			public bool IsMet { get { return value >= target; } }	// will be true if this condition is met/completed
			[System.NonSerialized] public int value = 0;			// the value towards target so far
		}

		[System.Serializable]
		public class Reward
		{

#if UNIRPG_CORE
			// in UniRPG there are specific types that can be used.
			// the rest are removed so as not to conuse UniRPG users
			public enum Type
			{
				Item = 0,
				Currency = 1,
				Attribute = 2,
			}
#else
			public enum Type
			{
				Custom = 0,		// something that does not fall in one of below types
				Item = 1,		// an item like, sword, postion, etc
				Currency = 2,	// some kind of currency reward, like gold or coins
				Experience = 3,	// experience
				Attribute = 4,	// typically something like Health, Intelligence, Experience, etc.
			}
#endif

			// How you use these following values in your code is up to you but you would typically
			// use a combination of type and ident to identify what the reward is. For example, 
			// type = Item would tell you it is an item reward and then ident = 2 would tell you 
			// it is the 2nd defined item. or you could make ident the name of the item and 
			// look for the item by name.

			public Type type = Type.Currency;	// helper to identify reward type. how you use this is up to you.
			public string ident = "";			// custom key to identify reward by. how you use this is up to you.
			public int value = 1;				// how many of reward to give

			// helper
			[System.NonSerialized] public string CachedName = "-";
		}

		// ============================================================================================================
		#region properties

		public string name;				// name if quest
		public string text;				// text/description of the quest
		public string customIdent;		// optional

		// the conditions to meet before this quest will be considered completed
		public List<Condition> conditions = new List<Condition>(0);

		// the rewards that this quest gives
		public List<Reward> rewards = new List<Reward>(0);

		// ------------------------------------------------------------------------------------------------------------

		public string IdentString { get { return savedIdent; } }

		public System.Guid Ident
		{
			get
			{
				if (_ident == System.Guid.Empty)
				{
					if (!string.IsNullOrEmpty(savedIdent))
					{
						try { _ident = new System.Guid(savedIdent); }
						catch { _ident = System.Guid.Empty; savedIdent = _ident.ToString("N"); }
					}
				}
				return _ident;
			}
		}

		[SerializeField]		private string savedIdent = string.Empty;
		[System.NonSerialized]	private System.Guid _ident = System.Guid.Empty;

		#endregion
		// ============================================================================================================
		#region runtime

		/// <summary>tells if quest was accepted. should also be in list if DiaQEngine.acceptedQuests</summary>
		public bool IsAccepted { get { return _isAccepted; } }
		[System.NonSerialized] private bool _isAccepted = false;

		/// <summary>tells if quest is completed. can't be set directly as conditions must be met to complete a quest</summary>
		public bool IsCompleted { get { return _isCompleted; } }
		[System.NonSerialized] private bool _isCompleted = false;

		/// <summary>tells if quest was completed and handed in. will be set when HandIn is called.</summary>
		public bool HandedIn { get { return _handedIn; } }
		[System.NonSerialized] private bool _handedIn = false;

		#endregion
		// ============================================================================================================
		#region pub

		/// <summary>
		/// Call to inform this quest that it has just been accepted and
		/// should make sure its conditions are reset and ready for use
		/// </summary>
		public void Accept()
		{
			if (!DiaQEngine.Instance.acceptedQuests.Contains(this))
			{
				DiaQEngine.Instance.acceptedQuests.Add(this);
			}

			_isAccepted = true;
			_isCompleted = false;
			conditions.ForEach(c => c.value = 0);
		}

		/// <summary>
		/// Update a condition by passing a value. the condition's value towards its target value will be updated and
		/// checked against the target. if the target is reache, true will be returned, else false. if the condition
		/// was not then false will be returned. The "ident" check is case sensitive. The return value does not tell 
		/// if the whole quest is completed or not. Use, IsCompleted, to check for that. You should never define more
		/// that one conditions with the same ident per quest. This will return emmedietly with true if quest is
		/// allready completed.
		/// </summary>
		public bool UpdateCondition(string ident, int val)
		{
			return UpdateCondition(ident, val, false);
		}

		/// <summary>
		/// Update a condition by passing a value. the condition's value towards its target value will be updated and
		/// checked against the target. if the target is reached, true will be returned, else false. if the condition
		/// was not then false will be returned. The return value does not tell if the whole quest is completed or not. 
		/// Use, IsCompleted, to check for that. This will return emmedietly with true if quest is allready completed.
		/// This will update the first condition with the given ident. You should never define more that one
		/// conditions with the same ident per quest.
		/// </summary>
		public bool UpdateCondition(string ident, int val, bool ignoreCaseWithIdentTest)
		{
			if (IsCompleted) return true;
			if (string.IsNullOrEmpty(ident)) return false;

			Condition c = null;
			if (ignoreCaseWithIdentTest) c = conditions.FirstOrDefault(con => con.ident.Equals(ident, System.StringComparison.OrdinalIgnoreCase));
			else c = conditions.FirstOrDefault(con => con.ident.Equals(ident));

			if (c != null)
			{
				c.value += val;
				if (c.value >= c.target)
				{
					c.value = c.target;
					CheckQuestCompletion(); // possible that quest might be completed, check now
					return true;			// condition was met
				}
			}
			return false;
		}

		/// <summary>
		/// Updates the condition at specific list index
		/// </summary>
		public bool UpdateCondition(int idx, int val)
		{
			if (IsCompleted) return true;
			Condition c = conditions[idx];
			if (c != null)
			{
				c.value += val;
				if (c.value >= c.target)
				{
					c.value = c.target;
					CheckQuestCompletion(); // possible that quest might be completed, check now
					return true;			// condition was met
				}
			}
			return false;
		}

		/// <summary>
		/// Will set all conditions as met/unmet and the quest as completed/incomplete
		/// </summary>
		public void SetCompleted(bool completed)
		{
			if (!_isAccepted)
			{
				if (!DiaQEngine.Instance.acceptedQuests.Contains(this))
				{
					DiaQEngine.Instance.acceptedQuests.Add(this);
				}

				_isAccepted = true;
			}

			if (completed)
			{
				_isCompleted = true;
				conditions.ForEach(c => c.value = c.target);
			}
			else
			{
				_isCompleted = false;
				conditions.ForEach(c => c.value = 0);
			}
		}

		/// <summary>
		/// Call this to inform the quest that is has been handed. This is typically when the player
		/// hands a completed quest in and received his rewrd. HandedIn will be set so that you
		/// can later check if the player recieved reward(s) yet. Will return false if HandedIn
		/// was not updated => when Quest not Completed or HandedIn is allready true
		/// </summary>
		public bool HandIn()
		{
			if (_handedIn == true || _isCompleted == false) return false;
			_isCompleted = true;
			_handedIn = true;
			return true;
		}

		/// <summary>
		/// Call this to inform the quest that is has been handed. This is typically when the player
		/// hands a completed quest in and received his rewrd. HandedIn will be set so that you
		/// can later check if the player recieved reward(s) yet. Will return false if HandedIn
		/// was not updated => when Quest not Completed or HandedIn is allready true
		/// This also allows you to specify that IsCompleted must be forced to true 
		/// even if it was false. So this will only return false then if HandedIn was
		/// alrleady set to true
		/// </summary>
		public bool HandIn(bool forceAsCompleted)
		{
			if (false == forceAsCompleted)
			{
				if (_handedIn == true || _isCompleted == false) return false;
			}
			else
			{
				if (_handedIn == true) return false;
			}

			SetCompleted(true);
			
			_handedIn = true;
			return true;
		}

		#endregion
		// ============================================================================================================
		#region internal

		public DiaQuest()
		{
			_ident = System.Guid.NewGuid();
			savedIdent = _ident.ToString("N");
		}

		private void CheckQuestCompletion()
		{
			if (_isCompleted) return; // no need to check if alrleady set as done

			foreach (Condition c in conditions)
			{	// fail as soon as a condition is found that is not completed
				if (c.value < c.target) return;
			}

			// reached this point without fail, so all conditions must be met
			_isCompleted = true;
		}

		#endregion
		// ============================================================================================================
	}
}