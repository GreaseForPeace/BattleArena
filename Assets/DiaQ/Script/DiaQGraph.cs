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
	[System.Serializable]
	public class DiaQGraph
	{
		public string name;
		public List<DiaQNode> nodes = new List<DiaQNode>(0);
		public string customIdent; // optional

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

		// --- These are not used at runtime and is data used by the editor only

		#pragma warning disable 0414 // dont want to see the wanring that the following are not used, when building game
		[SerializeField] private int _nextNodeId = 0;
		[SerializeField] private Vector2 _drawOffset = Vector2.zero;
		#pragma warning restore 0414

		// --- Runtime

		// the node that the graph is at currently
		[System.NonSerialized] private DiaQNode currNode = null;

		// set as soon as this graph is done or stops walking the nodes
		public bool HasSeenPlayer { get { return _hasSeenPlayer; } set { _hasSeenPlayer = value; }  }
		[System.NonSerialized] private bool _hasSeenPlayer = false;

		// ============================================================================================================

		/// <summary>
		/// Get the node with (id) in the graph
		/// </summary>
		public DiaQNode GetNode(int id)
		{
			return nodes.FirstOrDefault(n => n.id == id);
		}

		/// <summary>
		/// Remove all links/outputs in all nodes with the specified nodeId
		/// </summary>
		public void RemoveLinksWith(int nodeId)
		{
			foreach (DiaQNode n in nodes)
			{
				n.outputs.RemoveAll(nl => nl.targetNodeId == nodeId);
			}
		}

		/// <summary>
		/// Get the Dialogue data for active node (normally from Dialogue type node). Null means there was error or Graph is done
		/// Pass start=true to have the graph determine what to send by walking the nodes from the Start node again
		/// </summary>
		public DiaQConversation GetData(bool start)
		{
			if (start) currNode = GetNode(0); // go to first node
			WalkGraph();
			HasSeenPlayer = true;
			return GetData();
		}

		/// <summary>
		/// Get the Dialogue data for active node (normally from Dialogue type node). Null means there was error or Graph is done
		/// This expects that the Graph was waiting at a Dialogue type and will nto look at what choice was given to continue
		/// walking the graph nodes to the next result. If the current node has node choices the graph will just continue now
		/// </summary>
		public DiaQConversation GetData(int choise)
		{
			if (currNode == null) return null;
			currNode = currNode.GetLinkedNode(this, choise);
			WalkGraph();
			HasSeenPlayer = true;
			return GetData();
		}

		private DiaQConversation GetData()
		{	// builds the data object to return. based on "current" node
			if (currNode == null) return null;

			if (currNode.type == DiaQNode.Type.Dialogue)
			{
				DiaQConversation d = new DiaQConversation();
				
				if (!string.IsNullOrEmpty(currNode.data[0])) 
				{
					// todo: parse the text for variable value replacement?
					d.conversationText = currNode.data[0];

					// append quest text?
					if (currNode.toggle[0] == true)
					{
						DiaQuest q = DiaQEngine.Instance.FindDefinedQuest(currNode.data[1]);
						if (q != null)
						{
							// add the whole quest object for later reference by whatever wants to look at it
							d.linkedQuest = q;

							// append quest text?
							if (currNode.toggle[1] == true)
							{
								// insert a line break if there is allready text present
								if (!string.IsNullOrEmpty(d.conversationText)) d.conversationText += "\n\n";

								// todo: parse the text for variable value replacement?
								d.conversationText += q.text;
							}
						}
					}
				}
				
				if (currNode.choices.Count > 0)
				{
					d.choices = new string[currNode.choices.Count];
					for (int i = 0; i < currNode.choices.Count; i++)
					{
						d.choices[i] = currNode.choices[i];
					}
				}

				return d;
			}

			return null;
		}

		private bool WalkGraph()
		{	// recursively walks the nodes of the graph up to where player interaction is 
			// needed or the graph exits (return true when must stop walking)

			// no currNode? exit.
			if (currNode == null) return true;

			// check what to do next.
			switch (currNode.type)
			{
				case DiaQNode.Type.Dialogue:
				{
					return true; // stop walking now
				}

				case DiaQNode.Type.Decision:
				{
					currNode = currNode.ExecuteDecision(this);
				} break;

				case DiaQNode.Type.GiveQuest:
				{
					HandleGiveQuest();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

				case DiaQNode.Type.UpdateCondition: 
				{
					HandleUpdateCondition();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

				case DiaQNode.Type.QuestCheck:
				{
					currNode = HandleQuestCheck();
				} break;

				case DiaQNode.Type.SendMessage:
				{
					HandleSendMessage();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

				case DiaQNode.Type.SetVariable:
				{
					HandleSetVar();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

				case DiaQNode.Type.Random:
				{
					if (currNode.i_data[0] <= 1) currNode = currNode.GetLinkedNode(this, 0);
					else currNode = currNode.GetLinkedNode(this, Random.Range(0, currNode.i_data[0]));
				} break;

				case DiaQNode.Type.DebugLog:
				{
					HandleDebugLog();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

				case DiaQNode.Type.End:
				{
					currNode = null;
					return true; // stop walking now
				} 

				case DiaQNode.Type.Start:
				{
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

#if UNIRPG_CORE
				case DiaQNode.Type.GiveReward:
				{
					HandleGiveReward();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;

				case DiaQNode.Type.UniRPGEvent:
				{
					HandleUniRPGEventCall();
					currNode = currNode.GetLinkedNode(this, 0);
				} break;
#endif

				default:
				{
					Debug.LogError("Encountered unhandled Node Type: " + currNode.type);
					return true;
				}
			}

			// nothing more to do? stop walking now.
			if (currNode == null) return true;

			// see if should walk further.
			return WalkGraph();
		}

		private void HandleGiveQuest()
		{
			if (currNode.i_data[0] == 0)
			{
				DiaQuest q = DiaQEngine.Instance.FindDefinedQuest(currNode.data[0]);
				if (q == null)
				{
					Debug.LogError("Quest Node error: The quest does not exist.");
					return;
				}

				DiaQEngine.Instance.AcceptQuest(q);
			}
			else
			{
				string ident = DiaQEngine.Instance.GetDiaQVarValue(currNode.data[0]);
				DiaQuest q = DiaQEngine.Instance.FindDefinedQuest(ident);
				if (q == null)
				{
					Debug.LogError("Quest Node error: The quest does not exist.");
					return;
				}

				DiaQEngine.Instance.AcceptQuest(q);
			}
		}

		private void HandleDebugLog()
		{
			// todo: parse text here too? Could be usefull to then be able to print almost any kind of variable

			if (currNode.i_data[0] == 0)			// text
			{
				Debug.Log(currNode.data[0]);
			}
			else if (currNode.i_data[0] == 1)		// DiaQ var value
			{
				string val = DiaQEngine.Instance.GetDiaQVarValue(currNode.data[0]);
				Debug.Log("DiaQ Variable: " + currNode.data[0] + " = " + val);
			}
			else									// DiaQ var value
			{
				DiaQuest q = DiaQEngine.Instance.FindAcceptedQuest(currNode.data[0]);
				bool notAnAcceptedQuest = false;
				if (q == null)
				{
					notAnAcceptedQuest = true;
					q = DiaQEngine.Instance.FindDefinedQuest(currNode.data[0]);
				}

				if (q == null)
				{
					Debug.LogError("DebugLog node error: No quest selected.");
				}
				else
				{
					if (currNode.i_data[0] == 2)		// quest accepted?
					{
						if (notAnAcceptedQuest) Debug.Log(q.name + " not accepted.");
						else Debug.Log(q.name + " was accepted.");
					}

					else if (currNode.i_data[0] == 3)	// quest completed?
					{
						if (notAnAcceptedQuest) Debug.Log(q.name + " completed? No, it is not even accepted yet.");
						else
						{
							if (q.IsCompleted) Debug.Log(q.name + " is completed.");
							else Debug.Log(q.name + " is not completed yet.");
						}
					}

					else if (currNode.i_data[0] == 4)	// quest handed in?
					{
						if (notAnAcceptedQuest) Debug.Log(q.name + " handed-in? No, it is not even accepted yet.");
						else
						{
							if (q.HandedIn) Debug.Log(q.name + " was handed in.");
							else Debug.Log(q.name + " is not yet handed in.");
						}
					}

				}
			}

		}

		private void HandleSendMessage()
		{
			if (string.IsNullOrEmpty(currNode.data[0]))
			{
				Debug.LogError("SendMessage Node Error: No tag/name/type provided.");
				return;
			}

			if (string.IsNullOrEmpty(currNode.data[1]))
			{
				Debug.LogError("SendMessage Node Error: No Call/ Function name provided.");
				return;
			}

			Object[] objs = null;
			if (currNode.i_data[0] == 0)		// find object(s) by tag
			{
				objs = GameObject.FindGameObjectsWithTag(currNode.data[0]);
			}
			else if (currNode.i_data[0] == 1)	// find object by name
			{
				objs = new Object[1];
				objs[0] = GameObject.Find(currNode.data[0]);
			}
			else if (currNode.i_data[0] == 2)	// find object by type
			{
				try
				{
					System.Type t = System.Type.GetType(currNode.data[0]);
					objs = GameObject.FindObjectsOfType(t);
				}
				catch
				{
					Debug.LogError("SendMessage Node Error: Type is not valid [" + currNode.data[0] + "]");
					return;
				}
			}

			if (objs == null) return;
			if (objs.Length == 0) return;

			object val = null;

			if (currNode.i_data[1] == 1) val = currNode.data[2];			// string
			else if (currNode.i_data[1] == 2) val = currNode.f_data[0];		// number
			else if (currNode.i_data[1] == 3) val = currNode.o_data[0];		// prefab
			else if (currNode.i_data[1] == 4) val = currNode.data[2];		// quest ident
			else if (currNode.i_data[1] == 5) val = DiaQEngine.Instance.GetDiaQVarValue(currNode.data[2]); // diaq var

			if (currNode.i_data[0] == 2)
			{	// will be Components
				foreach (Component o in objs)
				{
					if (o == null) continue;
					if (currNode.i_data[1] == 0) o.SendMessage(currNode.data[1]);
					else o.SendMessage(currNode.data[1], val);
				}
			}
			else
			{	// will be GameObjects
				foreach (GameObject o in objs)
				{
					if (o == null) continue;
					if (currNode.i_data[1] == 0) o.SendMessage(currNode.data[1]);
					else o.SendMessage(currNode.data[1], val);
				}
			}

		}

		private void HandleSetVar()
		{
			if (string.IsNullOrEmpty(currNode.data[0]))
			{
				Debug.LogError("SetVariable node error: No variable name specified.");
				return;
			}

			if (currNode.i_data[0] == 0)		// value
			{
				DiaQEngine.Instance.SetDiaQVarValue(currNode.data[0], currNode.data[1]);
			}
			else if (currNode.i_data[0] == 1)	// value of other var
			{
				if (string.IsNullOrEmpty(currNode.data[1]))
				{
					Debug.LogError("SetVariable node error: No variable specified to read value from.");
					return;
				}

				DiaQEngine.Instance.SetDiaQVarValue(currNode.data[0], DiaQEngine.Instance.GetDiaQVarValue(currNode.data[1]));
			}
			else if (currNode.i_data[0] == 2)	// ident of a quest
			{
				if (string.IsNullOrEmpty(currNode.data[1]))
				{
					Debug.LogError("SetVariable node error: No Quest specified.");
					return;
				}

				DiaQEngine.Instance.SetDiaQVarValue(currNode.data[0], currNode.data[1]);
			}

		}

		private void HandleUpdateCondition()
		{
			if (!string.IsNullOrEmpty(currNode.data[1]))
			{
				DiaQuest q = DiaQEngine.Instance.FindAcceptedQuest(currNode.data[1]);
				if (q != null) q.UpdateCondition(currNode.data[0], currNode.i_data[0]);
			}
			else
			{
				DiaQEngine.Instance.UpdateQuestConditions(currNode.data[0], currNode.i_data[0]);
			}
		}

		private DiaQNode HandleQuestCheck()
		{
			DiaQuest q = null;
			if (currNode.i_data[0] == 0)
			{
				q = DiaQEngine.Instance.FindDefinedQuest(currNode.data[0]);
				if (q == null)
				{
					Debug.LogError("QuestCheck Node error: The quest does not exist.");
					return null;
				}
			}
			else
			{
				string ident = DiaQEngine.Instance.GetDiaQVarValue(currNode.data[0]);
				q = DiaQEngine.Instance.FindDefinedQuest(ident);
				if (q == null)
				{
					Debug.LogError("QuestCheck Node error: The quest does not exist.");
					return null;
				}
			}

			if (q.HandedIn) return currNode.GetLinkedNode(this, 3);
			else if (q.IsCompleted) return currNode.GetLinkedNode(this, 2);
			else if (q.IsAccepted) return currNode.GetLinkedNode(this, 1);
			return currNode.GetLinkedNode(this, 0);
		}

#if UNIRPG_CORE
		private void HandleGiveReward()
		{
			DiaQuest q = null;
			if (currNode.i_data[0] == 0)
			{
				q = DiaQEngine.Instance.FindDefinedQuest(currNode.data[0]);
			}
			else
			{
				string ident = DiaQEngine.Instance.GetDiaQVarValue(currNode.data[0]);
				q = DiaQEngine.Instance.FindDefinedQuest(ident);
			}

			if (q == null)
			{
				Debug.LogError("Reward Node error: The quest does not exist.");
				return;
			}

			// check if quest was accepted?
			if (currNode.i_data[1] == 1)
			{
				DiaQuest aq = DiaQEngine.Instance.FindAcceptedQuest(currNode.data[0]);
				if (aq == null) return; // cant get rewards
			}

			// check if the quest is completed?
			if (currNode.i_data[2] == 1)
			{
				if (false == q.IsCompleted) return;
			}

			// check if quest was previously handed in?
			if (currNode.i_data[3] == 1)
			{
				if (true == q.HandedIn) return;
			}

			// first set that quest was handed-in
			q.HandIn(true);

			// now hand out reward(s)
			foreach (DiaQuest.Reward r in q.rewards)
			{
				if (r.type == DiaQuest.Reward.Type.Currency)
				{
					UniRPGGlobal.Player.Actor.currency += r.value;
					if (UniRPGGlobal.Player.Actor.currency < 0) UniRPGGlobal.Player.Actor.currency = 0;
				}

				else if (r.type == DiaQuest.Reward.Type.Attribute)
				{
					if (!string.IsNullOrEmpty(r.ident))
					{
						RPGAttribute a = UniRPGGlobal.Player.Actor.ActorClass.GetAttribute(new GUID(r.ident));
						if (a != null)
						{
							a.Value += r.value;
						} else Debug.LogError("Reward Node error: The Player does not have the Attribute.");
					} else Debug.LogError("Reward Node error: No Attribute was specified.");
				}

				else if (r.type == DiaQuest.Reward.Type.Item)
				{
					if (r.value < 1)
					{
						Debug.LogError("Reward Node error: You must specify a number of (1) or higher for Item type rewards.");
					}
					else
					{
						if (!string.IsNullOrEmpty(r.ident))
						{
							RPGItem it = UniRPGGlobal.DB.GetItem(new GUID(r.ident));
							if (it != null)
							{
								UniRPGGlobal.Player.Actor.AddToBag(it, r.value);
							} else Debug.LogError("Reward Node error: The specified Item was not found in the Database.");
						} else Debug.LogError("Reward Node error: No Item was specified.");
					}
				}
			}
		}

		private void HandleUniRPGEventCall()
		{
			if (!string.IsNullOrEmpty(currNode.data[0]))
			{
				RPGEvent ev = UniRPGGlobal.DB.GetEvent(new GUID(currNode.data[0]));
				if (ev != null)
				{
					ev.Execute(UniRPGGlobal.Player.gameObject);
				}
				else Debug.LogError("UniRPG Event Node error: You did not specify any Event to call.");
			}
			else Debug.LogError("UniRPG Event Node error: You did not specify any Event to call.");
		}
#endif

		// ============================================================================================================
		#region internal

		public DiaQGraph()
		{
			_ident = System.Guid.NewGuid();
			savedIdent = _ident.ToString("N");
		}

		#endregion
		// ============================================================================================================
		#region Editor helpers - do not access these at runtime

#if UNITY_EDITOR

		public void CreateNode(DiaQNode.Type type, Vector2 pos)
		{
			DiaQNode n = new DiaQNode();
			n.Init(_nextNodeId++, type);
			n.SetRectInGraph(this, DiaQNode.GetNodeDefaultRect(n, pos));
			nodes.Add(n);
		}

		public Vector2 DrawOffset { get { return _drawOffset; } set { _drawOffset = value; } }

#endif

		#endregion
		// ============================================================================================================
	}
}