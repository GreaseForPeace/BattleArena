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
	public class DiaQDecisionTest
	{
		public enum TestType
		{
			Equal = 0, NotEqual = 1,
			Bigger = 2, Smaller = 3,
			BiggerEqual = 4, SmallerEqual = 5
		}

		public enum VarType
		{
			HasSeenPlayer = 0,
			AcceptedQuest = 1,
			CompletedQuest = 2,
			HandedInQuest = 3,
			DiaQVariable = 4,
		}

		public enum ValType
		{
			True = 0, False = 1, 
			Empty = 2, String = 3, DiaQVar = 4
		}

		public int combineWithPrev = 0; // how this test must be combined with previous test, 0:and, 1:or

		public TestType testType = TestType.Equal;
		public VarType varType = VarType.HasSeenPlayer;
		public ValType valType = ValType.True;

		// -- extra fields, used as needed
		public string[] s_opt1 = new string[0];
		public string[] s_opt2 = new string[0];

		// ------------------------------------------------------------------------------------------------------------

		public DiaQDecisionTest Copy()
		{
			DiaQDecisionTest t = new DiaQDecisionTest();
			t.combineWithPrev = this.combineWithPrev;
			t.testType = this.testType;
			t.varType = this.varType;
			t.valType = this.valType;

			if (this.s_opt1.Length > 0)
			{
				t.s_opt1 = new string[this.s_opt1.Length];
				this.s_opt1.CopyTo(t.s_opt1, 0);
			}
			if (this.s_opt2.Length > 0)
			{
				t.s_opt2 = new string[this.s_opt2.Length];
				this.s_opt2.CopyTo(t.s_opt2, 0);
			}

			return t;
		}

		// ------------------------------------------------------------------------------------------------------------

		public bool Evaluate(DiaQGraph graph)
		{
			bool ret = true;

			switch (varType)
			{
				case VarType.HasSeenPlayer:
				{
					if (graph == null)
					{
						Debug.LogError("DiaQ Decision: Invalid Graph was provided for test (Has Seen Player)");
						return true;
					}
					else
					{
						bool b_val = (valType == ValType.False ? false : true);
						if (testType == TestType.Equal) ret = graph.HasSeenPlayer == b_val;
						else if (testType == TestType.NotEqual) ret = graph.HasSeenPlayer != b_val;
					}
				} break;

				case VarType.AcceptedQuest:
				case VarType.CompletedQuest:
				case VarType.HandedInQuest:
				{
					ret = QuestTest();
				} break;

				case VarType.DiaQVariable:
				{
					ret = DiaQVariableTest();
				} break;

				default: Debug.LogError("Decision Node encountered unknown test."); break;
			}

			return ret;
		}

		private bool DiaQVariableTest()
		{
			string v = DiaQEngine.Instance.GetDiaQVarValue(s_opt1[0]);
			bool b_val = (valType == ValType.False ? false : true);
			if (valType == ValType.Empty)
			{
				if (testType == TestType.Equal) return string.IsNullOrEmpty(v) == b_val;
				else if (testType == TestType.NotEqual) return string.IsNullOrEmpty(v) != b_val;
			}
			
			string v2 = (valType == ValType.DiaQVar ? DiaQEngine.Instance.GetDiaQVarValue(s_opt2[0]) : s_opt2[0]);

			if (string.IsNullOrEmpty(v))
			{
				if (testType == TestType.Equal) return string.IsNullOrEmpty(v2) == b_val;
				else if (testType == TestType.NotEqual) return string.IsNullOrEmpty(v2) != b_val;
			}
			else
			{
				if (testType == TestType.Equal) return v.Equals(v2) == b_val;
				else if (testType == TestType.NotEqual) return v.Equals(v2) != b_val;
			}

			return true;
		}

		private bool QuestTest()
		{
			if (s_opt1.Length > 0)
			{
				if (!string.IsNullOrEmpty(s_opt1[0]))
				{
					DiaQuest q = DiaQEngine.Instance.FindAcceptedQuest(s_opt1[0]);
					bool b_val = (valType == ValType.False ? false : true);

					if (varType == VarType.AcceptedQuest)
					{
						bool accepted = (q != null);
						if (testType == TestType.Equal) return (accepted == b_val);
						else if (testType == TestType.NotEqual) return (accepted != b_val);
					}
					else if (varType == VarType.CompletedQuest)
					{
						bool completed = (q != null ? q.IsCompleted : false);
						if (testType == TestType.Equal) return (completed == b_val);
						else if (testType == TestType.NotEqual) return (completed != b_val);
					}
					else if (varType == VarType.HandedInQuest)
					{
						bool handedin = (q != null ? q.HandedIn : false);
						if (testType == TestType.Equal) return (handedin == b_val);
						else if (testType == TestType.NotEqual) return (handedin != b_val);
					}

				} else Debug.LogError("Decision Node error: No quest specified.");
			} else Debug.LogError("Decision Node error: No quest specified.");
			return true;
		}

		// ============================================================================================================
	}

	[System.Serializable]
	public class DiaQDecision
	{
		public List<DiaQDecisionTest> tests = new List<DiaQDecisionTest>(0);

		public DiaQDecision Copy()
		{
			DiaQDecision d = new DiaQDecision();
			d.tests = new List<DiaQDecisionTest>(0);
			foreach (DiaQDecisionTest t in this.tests) d.tests.Add(t.Copy());
			return d;
		}

		public bool Evaluate(DiaQGraph graph)
		{
			bool ret = true;
			foreach (DiaQDecisionTest t in tests)
			{
				bool res = t.Evaluate(graph);
				if (t.combineWithPrev == 0) ret = ret && res;
				else ret = ret || res;
			}
			return ret;
		}

		// ============================================================================================================
	}
}