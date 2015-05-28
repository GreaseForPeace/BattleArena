// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNIRPG_CORE
using UniRPG;
#endif

namespace DiaQ
{
	[AddComponentMenu("")]
	public class DiaQEngine : MonoBehaviour
	{
		// ============================================================================================================
		#region vars

		/// <summary>The loaded DiaQ Asset. You don't normally work with this directly.</summary>
		public DiaQAsset Asset { get { return asset; } }

		private DiaQAsset asset = null;			// the container of all DiaQ data
		private DiaQGraph activeGraph = null;	// is set after a call to StartGraph(). Should be ended by call to EndGraph(), but not compulsory

		/// <summary>All the Quests that the Player has according to the Quest Engine</summary>
		public List<DiaQuest> acceptedQuests = new List<DiaQuest>(0);

		#endregion
		// ============================================================================================================
		#region loading

		/// <summary>
		/// Call this to load the DiaQ assets and get it ready for use. You must specify the path to the asset, as set in 
		/// the DiaQ editor settings, excluding the specific asset file. For example assetPath = *Assets/DiaQ/Sample/Resources/*. 
		/// Will return false if it failed to load.
		/// </summary>
		public bool Load()
		{
			Object res = Resources.Load("DiaQ", typeof(DiaQAsset));
			if (res == null)
			{
#if !UNIRPG_CORE
				// silently fail in UniRPG
				Debug.LogError("Error: DiaQ failed to locate and load (DiaQ.asset)");
#endif
				return false;
			}

			asset = (DiaQAsset)Instantiate(res);
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ failed to locate and load (DiaQ.asset)");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Start a Dialogue Graph of the specified name. This will use the first graph that has the name. It will start 
		/// walking the graph and pass back a *DiaQConversation* object when a Dialogue node is reached, or null on error 
		/// or when the graph has reached its end.
		/// </summary>
		public bool Load(string assetPath)
		{
			if (!assetPath.EndsWith("/") && !assetPath.EndsWith("\\")) assetPath += "/";
			assetPath += "DiaQ.asset";
			asset = (DiaQAsset)Instantiate(Resources.Load(assetPath, typeof(DiaQAsset)));
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ failed to load (" + assetPath + ")");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Returns True if the asset was successfully loaded, else you can expect *DiaQEngine.Instance.Asset* to be null and nothing in the engine to work. 
		/// </summary>
		public bool IsLoaded { get { return asset != null; } }

		#endregion
		// ============================================================================================================
		#region graph/dialogue

		/// <summary>
		/// Start a Dialogue Graph of the specified name. This will use the first graph that has the name.
		/// </summary>
		public DiaQConversation StartGraph(string byName, bool ignoreCase)
		{
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ assets are not loaded.");
				return null;
			}

			DiaQGraph graph = null;
			if (ignoreCase) graph = asset.graphs.FirstOrDefault(g => g.name.Equals(byName, System.StringComparison.OrdinalIgnoreCase));
			else graph = asset.graphs.FirstOrDefault(g => g.name.Equals(byName));

			if (graph == null)
			{
				Debug.LogError(string.Format("Error: Could not find graph by name ({0}).", byName));
				return null;
			}

			activeGraph = graph;
			return activeGraph.GetData(true);
		}

		/// <summary>
		/// Start a Dialogue Graph with the specified custom id. This will use the first graph that has the custom ident.
		/// </summary>
		public DiaQConversation StartGraphByCustomIdent(string ident, bool ignoreCase)
		{
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ assets are not loaded.");
				return null;
			}

			DiaQGraph graph = null;
			if (ignoreCase) graph = asset.graphs.FirstOrDefault(g => g.customIdent.Equals(ident, System.StringComparison.OrdinalIgnoreCase));
			else graph = asset.graphs.FirstOrDefault(g => g.customIdent.Equals(ident));

			if (graph == null)
			{
				Debug.LogError(string.Format("Error: Could not find graph by customID ({0}).", ident));
				return null;
			}

			activeGraph = graph;
			return activeGraph.GetData(true);
		}

		/// <summary>
		/// Start a Dialogue Graph of the specified Ident.
		/// </summary>
		public DiaQConversation StartGraph(string byIdent)
		{
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ assets are not loaded.");
				return null;
			}

			DiaQGraph graph = asset.graphs.FirstOrDefault(g => g.IdentString.Equals(byIdent));

			if (graph == null)
			{
				Debug.LogError(string.Format("Error: Could not find graph by Ident ({0}).", byIdent));
				return null;
			}

			activeGraph = graph;
			return activeGraph.GetData(true);
		}

		/// <summary>
		/// Start a Dialogue Graph of the specified Ident.
		/// </summary>
		public DiaQConversation StartGraph(System.Guid byIdent)
		{
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ assets are not loaded.");
				return null;
			}

			DiaQGraph graph = asset.graphs.FirstOrDefault(g => g.Ident == byIdent);

			if (graph == null)
			{
				Debug.LogError(string.Format("Error: Could not find graph by Ident ({0}).", byIdent));
				return null;
			}

			activeGraph = graph;
			return activeGraph.GetData(true);
		}

		/// <summary>
		/// End the current Dialogue Graph. This is an optional call. Any call to StartGraph will overwrite what
		/// the currently "active" graph is, as used by SubmitChoice() and others
		/// </summary>
		public void EndGraph()
		{
			activeGraph = null;
		}

		/// <summary>
		/// Submit a choice for the active Dialogue Graph (as started by a call to StartGraph). Will return null 
		/// if the end of a path in the graph was reached else a *DiaQConversation* object with conversation data 
		/// and choices to present to the player.
		/// </summary>
		public DiaQConversation SubmitChoice(int choiceNumber)
		{
			if (asset == null)
			{
				Debug.LogError("Error: DiaQ assets are not loaded.");
				return null;
			}

			if (activeGraph == null)
			{
				Debug.LogError("Error: Call to SubmitChoice() while there is no active graph. Did you forget to call StartGraph()?");
				return null;
			}

			return activeGraph.GetData(choiceNumber);
		}

		#endregion
		// ============================================================================================================
		#region quest

		/// <summary>
		/// Give the payer the quest. Quest will be set as not completed.
		/// Nothing is done if the quest is already in he list of accepted quests.
		/// </summary>
		public void AcceptQuest(DiaQuest quest)
		{
			if (quest == null) return;
			quest.Accept(); // .Accept will add the quest to acceptedQuests if needed
		}

		/// <summary>
		/// Remove the quest from list of accepted quests. Completed quests can't be dropped
		/// </summary>
		public void DropQuest(DiaQuest quest)
		{
			DropQuest(quest, false);
		}

		/// <summary>
		/// Remove the quest from list of accepted quests. Completed quests can be dropped if allowDropCompleted = true, else not
		/// </summary>
		public void DropQuest(DiaQuest quest, bool allowDropCompleted)
		{
			if (quest == null) return;
			if (allowDropCompleted == false && quest.IsCompleted) return;
			acceptedQuests.Remove(quest);
		}

		/// <summary>
		/// Find an accepted quest by custom ident, as specified byt designer when quest was defined
		/// </summary>
		public DiaQuest FindAcceptedQuest(string customIdent, bool ignoreCase)
		{
			if (string.IsNullOrEmpty(customIdent)) return null;
			if (ignoreCase) return acceptedQuests.FirstOrDefault(q => q.customIdent.Equals(customIdent, System.StringComparison.OrdinalIgnoreCase));
			return acceptedQuests.FirstOrDefault(q => q.customIdent.Equals(customIdent));
		}

		/// <summary>
		/// Find an accepted quest by build-in ident, as generated when quest was defined
		/// </summary>
		public DiaQuest FindAcceptedQuest(string ident)
		{
			if (string.IsNullOrEmpty(ident)) return null;
			return acceptedQuests.FirstOrDefault(q => q.IdentString.Equals(ident));
		}

		/// <summary>
		/// Find an accepted quest by build-in ident, as generated when quest was defined
		/// </summary>
		public DiaQuest FindAcceptedQuest(System.Guid ident)
		{
			return acceptedQuests.FirstOrDefault(q => q.Ident == ident);
		}

		/// <summary>
		/// This will update all conditions, with specified ident, in all accepted (uncompleted) quests
		/// If you want to update only a specific quest then use FindAcceptedQuest() to first get the
		/// quest and then update the condition for the specific quest
		/// </summary>
		public void UpdateQuestConditions(string conditionIdent, int val)
		{
			if (string.IsNullOrEmpty(conditionIdent)) return;
			foreach (DiaQuest q in acceptedQuests)
			{
				if (q.IsCompleted == false)
				{
					q.UpdateCondition(conditionIdent, val);
				}
			}
		}

		// ------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Find defined quest by custom ident, as specified by designer when quest was defined
		/// </summary>
		public DiaQuest FindDefinedQuest(string customIdent, bool ignoreCase)
		{
			if (ignoreCase) return asset.quests.FirstOrDefault(q => q.customIdent.Equals(customIdent, System.StringComparison.OrdinalIgnoreCase));
			return asset.quests.FirstOrDefault(q => q.customIdent.Equals(customIdent));
		}

		/// <summary>
		/// Find defined quest by build-in ident, as generated when quest was defined
		/// </summary>
		public DiaQuest FindDefinedQuest(string ident)
		{
			return asset.quests.FirstOrDefault(q => q.IdentString.Equals(ident));
		}

		/// <summary>
		/// Find defined quest by build-in ident, as generated when quest was defined
		/// </summary>
		public DiaQuest FindDefinedQuest(System.Guid ident)
		{
			return asset.quests.FirstOrDefault(q => q.Ident == ident);
		}

		#endregion
		// ============================================================================================================
		#region diaq vars manip

		/// <summary>
		/// Will set the variable to val. Will create var if not exist.
		/// </summary>
		public void SetDiaQVarValue(string name, string val)
		{
			if (string.IsNullOrEmpty(name)) return;
			DiaQVar v= asset.vars.FirstOrDefault(vr => vr.name.Equals(name));
			if (v == null)
			{
				v = new DiaQVar() { name = name, val = val };
				asset.vars.Add(v);
			}
			else v.val = val;
		}

		/// <summary>
		/// Return value of given Diag var. Return null if not found.
		/// </summary>
		public string GetDiaQVarValue(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			DiaQVar v = asset.vars.FirstOrDefault(vr => vr.name.Equals(name));
			if (v != null) return v.val;
			return null;
		}

		#endregion
		// ============================================================================================================
		#region saving & loading support

		/// <summary>
		/// Returns a string that can be saved. This should be passed back to RestoreViaSaveData() 
		/// when you want DiaQ to restore its state as it was when GetSaveData() was called
		/// Do not use the 3 characters "@|@" like that in variable names or variable data as
		/// it is used as a seperator in save data
		/// </summary>
		public string GetSaveData()
		{
			if (asset == null) return null;
			System.Text.StringBuilder data = new System.Text.StringBuilder();

			// first save the state of all variables
			data.Append(asset.vars.Count).Append("@|@");
			foreach(DiaQVar v in asset.vars) 
			{
				data.Append(v.name).Append("@|@").Append(v.val).Append("@|@");
			}

			// save the state of quests (only those that changed)
			foreach (DiaQuest q in asset.quests)
			{
				if (q.IsAccepted)
				{
					if (q.IsCompleted)
					{
						data.Append(q.IdentString).Append(",").Append(q.IsCompleted ? "1," : "0,").Append(q.HandedIn ? "1" : "0").Append(";");
					}
					else
					{
						data.Append(q.IdentString).Append(",").Append(q.IsCompleted ? "1," : "0,").Append(q.HandedIn ? "1," : "0,");
						foreach (DiaQuest.Condition con in q.conditions) data.Append(con.value).Append(",");
						data.Append(";");
					}
				}
			}

			// there is nothing to save about graphs
			// ...

			// return data to save
			return data.ToString();
		}

		/// <summary>
		/// Restores DiaQ's state from save data. This data should be in the format as given when 
		/// you made a call to GetSaveData() Do not use the 3 characters "@|@" like that in variable 
		/// names or variable data as it is used as a seperator in save data
		/// Always test with a fresh save if you are still making changes to quests
		/// </summary>
		public void RestoreViaSaveData(string data)
		{
			if (asset == null) return;
			if (string.IsNullOrEmpty(data)) return;
			string[] d = data.Split(new string[] { "@|@" }, System.StringSplitOptions.None);
			if (d.Length == 0) 
			{
				Debug.LogError("DiaQ Save Data not in correct format (1).");
				return;
			}

			// restore variables
			int varCount = 0;
			int.TryParse(d[0], out varCount);
			if (varCount > 0)
			{
				if (d.Length > (2 * varCount))
				{
					for (int i = 1; i < (2 * varCount); i += 2)
					{
						SetDiaQVarValue(d[i], d[i + 1]);
					}
				}
				else
				{
					Debug.LogError("DiaQ Save Data not in correct format (2).");
					return;
				}
			}

			// restore quests
			if (d.Length >= (2 * varCount) + 1)
			{
				string[] questsData = d[(2 * varCount) + 1].Split(';');
				foreach (string qds in questsData)
				{
					if (string.IsNullOrEmpty(qds)) continue;
					string[] qd = qds.Split(',');
					DiaQuest q = FindDefinedQuest(qd[0]);
					if (q != null)
					{
						AcceptQuest(q);
						if (qd[1] == "1") q.SetCompleted(true);
						if (qd[2] == "1") q.HandIn(true);

						if (!q.IsCompleted)
						{	// expect condition data now
							for (int i = 0; i < q.conditions.Count; i++)
							{
								if (3 + i < qd.Length)
								{
									int parsedVal = 0;
									if (int.TryParse(qd[3 + i], out parsedVal))
									{
										q.UpdateCondition(i, parsedVal);
									}
									else Debug.LogError("DiaQ Save Data not in correct format (6).");
								}
								else Debug.LogError("DiaQ Save Data not in correct format (5).");
							}
						}
					}
					else Debug.LogError("DiaQ Save Data not in correct format (4).");
				}
			}
			else
			{
				Debug.LogError("DiaQ Save Data not in correct format (3).");
				return;
			}

		}

		#endregion
		// ============================================================================================================
		#region internal

		void Awake()
		{
			_instance = this;
		}

		#endregion
		// ============================================================================================================
		#region UniRPG specific

#if UNIRPG_CORE

		/// <summary>
		/// This is the callback when a player made a choice in the open dialogue panel. -1 is when plaeyr closed the panel
		/// The GUIDialogueData that was used is send as a param so that it can be reused if needed.
		/// </summary>
		public void OnUniRPGDialogueCallback(int selectedChoice, GUIDialogueData data)
		{
			if (selectedChoice == -1)
			{	// dialogue was closed, deselect any targeted object
				UniRPGGlobal.Player.ClearTarget();
				return; // nothing to do
			}

			DiaQConversation diaq = SubmitChoice(selectedChoice);

			if (diaq == null)
			{	// null means there is no conversation to show, so close hide the dialogue panel now
				UniRPGGlobal.GameGUIObject.SendMessage("HideDialogue");
				
				// deselect any selected object now (most likely that an NPC was selected)
				UniRPGGlobal.Player.ClearTarget();
			}
			else
			{	// update the unirpg dialogue data and send it to be shown to the player
				data.conversationText = diaq.conversationText;
				data.options = diaq.choices;
				UpdateUniRPGDialogueRewards(diaq, data);
				UniRPGGlobal.GameGUIObject.SendMessage("ShowDialogue", data);
			}
			
		}

		public void UpdateUniRPGDialogueRewards(DiaQConversation diaq, GUIDialogueData data)
		{
			// first make sure the rewards are cleared in case this is a data being reused
			data.showRewards = false;
			data.currencyReward = 0;
			data.itemRewards = null;
			data.attributeRewards = null; 

			if (diaq.linkedQuest != null)
			{
				if (diaq.linkedQuest.rewards.Count > 0)
				{
					data.showRewards = true;

					foreach (DiaQuest.Reward r in diaq.linkedQuest.rewards)
					{
						if (r.type == DiaQuest.Reward.Type.Currency)
						{
							data.currencyReward += r.value;
						}

						else if (r.type == DiaQuest.Reward.Type.Attribute)
						{
							if (!string.IsNullOrEmpty(r.ident))
							{
								RPGAttribute a = UniRPGGlobal.DB.GetAttribute(new GUID(r.ident));
								if (a != null)
								{
									data.attributeRewards = data.attributeRewards ?? new List<GUIDialogueData.AttribReward>(0);
									GUIDialogueData.AttribReward ar = data.attributeRewards.FirstOrDefault(x => x.attrib == a);
									if (ar == null) data.attributeRewards.Add(new GUIDialogueData.AttribReward() { attrib = a, valueAdd = r.value });
									else ar.valueAdd += r.value;
								}
						} } // else if (r.type == DiaQuest.Reward.Type.Attribute)

						else if (r.type == DiaQuest.Reward.Type.Item)
						{
							if (!string.IsNullOrEmpty(r.ident))
							{
								if (r.value > 0)
								{
									RPGItem it = UniRPGGlobal.DB.GetItem(new GUID(r.ident));
									if (it != null)
									{
										data.itemRewards = data.itemRewards ?? new List<GUIDialogueData.ItemReward>(0);
										GUIDialogueData.ItemReward ir = data.itemRewards.FirstOrDefault(x => x.item == it);
										if (ir == null) data.itemRewards.Add(new GUIDialogueData.ItemReward() { item = it, count = r.value });
										else ir.count += r.value;
									}
						} } } // else if (r.type == DiaQuest.Reward.Type.Item)
					}

				}
			}
		}

#endif

		#endregion
		// ============================================================================================================
		#region instance

		private static DiaQEngine _instance;
		public static DiaQEngine Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = (DiaQEngine)FindObjectOfType(typeof(DiaQEngine));
					if (_instance == null)
					{
						GameObject go = new GameObject("DiaQEngine");
						_instance = go.AddComponent<DiaQEngine>();

#if UNIRPG_CORE
						// want the DiaQ object to love over scene loads, and be destroyed when loading back to menu scene
						UniRPGGlobal.RegisterGlobalObject(go);
						_instance.Load();
#endif

					}
				}
				return _instance;
			}
		}

		#endregion
		// ============================================================================================================
	}
}