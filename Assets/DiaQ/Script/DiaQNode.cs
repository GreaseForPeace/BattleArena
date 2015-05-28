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
	public class DiaQNode
	{
		[System.Serializable]
		public class NodeLink
		{
			public int outSlot;
			public int targetNodeId;
		}

		public enum Type
		{
			Start = 0,
			End = 1,
			Dialogue = 2,
			Decision = 3,
			GiveQuest = 4,
			DebugLog = 5,
			SendMessage = 6,
			SetVariable = 7,
			Random = 9,		
			UpdateCondition = 10,
			QuestCheck = 12,

#if UNIRPG_CORE
			GiveReward = 8,
			UniRPGEvent = 11,
#endif
		}

		// common data
		public Type type;
		public int id;
		public List<NodeLink> outputs = new List<NodeLink>(0);
		public string comment = "";

		// data used in various ways by nodes
		public string[] data = new string[0];
		public bool[] toggle = new bool[0];
		public int[] i_data = new int[0];
		public float[] f_data = new float[0];
		public UnityEngine.Object[] o_data = new UnityEngine.Object[0];

		// used by Dialogue
		public List<string> choices = new List<string>(0);
		
		// used by Decision node type
		public DiaQDecision decision = null;

		// --- These are not used at runtime and is data used by the editor only

		public Rect position;

		// --- helpers and runtime (not serialised)

		public string CachedString {get;set;}

		/// <summary>return the linked quest, if any is set</summary>
		public DiaQuest LinkedQuest
		{
			get {
				if (_linkedQuest == null)
				{
					if (toggle.Length > 0 && data.Length > 1)
					{
						if (toggle[0] == true)
						{
							_linkedQuest = DiaQEngine.Instance.FindDefinedQuest(data[1]);
						}
					}
				}
				return _linkedQuest;
			}
		}
		[System.NonSerialized] private DiaQuest _linkedQuest = null;

		// ============================================================================================================

		/// <summary>
		/// return the node that is linked to the given slot
		/// </summary>
		public DiaQNode GetLinkedNode(DiaQGraph graph, int onOutSlot)
		{
			NodeLink link = outputs.FirstOrDefault(l => l.outSlot == onOutSlot);
			if (link != null) return graph.GetNode(link.targetNodeId);
			return null;
		}

		/// <summary>
		/// Called on a Decision (Tru/False) type node. It will execute the test(s) and then return the 
		/// node that was linked to either true or false, depending on result
		/// </summary>
		public DiaQNode ExecuteDecision(DiaQGraph graph)
		{
			int slot = 0;
			if (decision != null)
			{
				if (false == decision.Evaluate(graph)) slot = 1;
			}
			return GetLinkedNode(graph, slot);
		}

		// ============================================================================================================
		#region Editor helpers - do not access these at runtime

#if UNITY_EDITOR

		public void Init(int id, Type type)
		{
			this.id = id;
			this.type = type;
			this.outputs = new List<NodeLink>();
			CachedString = "-";

			switch (type)
			{
				case Type.Dialogue:
				{
					data = new string[] { "", "" };
					toggle = new bool[] { false, false };
					choices.Add("");
				} break;

				case Type.Decision:
				{
					decision = new DiaQDecision();
				} break;

				case Type.GiveQuest:
				{
					data = new string[] { "" };
					i_data = new int[] { 0 };
				} break;

				case Type.DebugLog:
				{
					data = new string[] { "" };
					i_data = new int[] { 0 };
				} break;

				case Type.SendMessage:
				{
					data = new string[] { "", "", "" };
					i_data = new int[] { 0, 0 };
					f_data = new float[] { 0f };
					o_data = new Object[] { null };
				} break;

				case Type.SetVariable:
				{
					data = new string[] { "", "" };
					i_data = new int[] { 0 };
				} break;

				case Type.Random:
				{
					i_data = new int[] { 3 };
				} break;

				case Type.UpdateCondition:
				{
					data = new string[] { "", "" };
					i_data = new int[] { 0 };
				} break;

				case Type.QuestCheck:
				{
					data = new string[] { "" };
					i_data = new int[] { 0 };
				} break;

#if UNIRPG_CORE
				case Type.GiveReward:
				{
					data = new string[] { "" };
					i_data = new int[] { 0, 1, 1, 1 };
				} break;
				case Type.UniRPGEvent:
				{
					data = new string[] { "" };
				} break;
#endif

			}
		}

		public void LinkWith(DiaQNode targetNode, int onOutputSlot, bool removePrevLink)
		{
			// don't allow linking self
			if (targetNode == this) return;

			if (!removePrevLink)
			{
				// already a link on the slot? Only one per out slot allowed
				if (outputs.Any(link => link.outSlot == onOutputSlot)) return;
			}
			else
			{
				// remove any existing link on the out slot
				outputs.RemoveAll(link => link.outSlot == onOutputSlot);
			}

			// create new link
			outputs.Add(new NodeLink() { outSlot = onOutputSlot, targetNodeId = targetNode.id });
		}

		// ------------------------------------------------------------------------------------------------------------

		public static Rect GetNodeDefaultRect(DiaQNode node, Vector2 pos)
		{
			Rect r = new Rect(pos.x, pos.y, 60f, 40f);

			switch (node.type)
			{
				case Type.Dialogue: { r.width = 200; if (node.choices.Count > 0) r.height += (node.choices.Count-1) * 20f; } break;
				case Type.GiveQuest: r.width = 150f; break;
				case Type.Decision: r.width = 100f; r.height = 60f; break;
				case Type.DebugLog: r.width = 150f; break;
				case Type.SendMessage: r.width = 150f; break;
				case Type.SetVariable: r.width = 150f; break;
				case Type.Random: r.width = 85f; r.height = 80f; break;
				case Type.UpdateCondition: r.width = 150f; break;
				case Type.QuestCheck: r.width = 120f; r.height = 120f; break;

#if UNIRPG_CORE
				case Type.GiveReward: r.width = 150; break;
				case Type.UniRPGEvent: r.width = 150; break;
#endif
			}

			r.x -= r.width * 0.5f;
			r.y -= r.height * 0.5f;
			return r;
		}

		public Rect GetRectInGraph(DiaQGraph graph)
		{
			Rect p = position;
			p.x += graph.DrawOffset.x;
			p.y += graph.DrawOffset.y;
			return p;
		}

		public void SetRectInGraph(DiaQGraph graph, Rect r)
		{
			position = r;
			position.x -= graph.DrawOffset.x;
			position.y -= graph.DrawOffset.y;
		}

		public Vector2 GetPositionInGraph(DiaQGraph graph)
		{
			return new Vector2 (position.x + graph.DrawOffset.x,position.y + graph.DrawOffset.y);
		}

		public Vector2 GetCenterPositionInGraph(DiaQGraph graph)
		{
			Vector2 p = GetPositionInGraph(graph);
			return new Vector2(p.x + position.width * 0.5f, p.y + position.height * 0.5f);
		}

		public Vector2 OutputPosition(int slot)
		{
			Vector2 v = new Vector2(position.width, 26);

			v.y += (slot * 20);

			return v;
		}

		public Vector2 OutputPositionInGraph(DiaQGraph graph, int slot)
		{
			return GetPositionInGraph(graph) + OutputPosition(slot);
		}

		public Vector2 InputPositionInGraph(DiaQGraph graph)
		{
			Vector2 v = GetPositionInGraph(graph);
			v.y += 8;
			return v;
		}

#endif // UNITY_EDITOR

		#endregion
		// ============================================================================================================
	}
}