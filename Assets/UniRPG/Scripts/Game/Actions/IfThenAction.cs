// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[System.Serializable]
public class IfThenActionTest
{
	public enum TestType
	{
		Equal=0, NotEqual=1,
		Bigger=2, Smaller=3,
		BiggerEqual=4, SmallerEqual=5
	}

	public enum VarType
	{
		Number=0, String=1, Object=2, AttributeVal=3, AttributeMin=4, AttributeMax=5, Level=6, Currency=7, CustomVar=8, Subject=9,
		Empty=10, IsActor=11, IsPlayer=12, Enabled=13, Friendly=14, Neutral=15, Hostile=16
	}

	public int combineWithPrev = 0;					// how this test must be combined with previous test, 0:and, 1:or

	public TestType testType = TestType.Equal;
	public VarType[] varType = { VarType.Number, VarType.Number };

	// helpers
	public NumericValue[] num = { new NumericValue(), new NumericValue() };
	public StringValue[] str = { new StringValue(), new StringValue() };
	public ObjectValue[] obj = { new ObjectValue(), new ObjectValue() };
	public ActionTarget[] target = { new ActionTarget(), new ActionTarget() };
	public GUID[] attribId = { new GUID(), new GUID() };
	
	public IfThenActionTest Copy()
	{
		IfThenActionTest t = new IfThenActionTest();
		t.combineWithPrev = this.combineWithPrev;
		t.testType = this.testType;
		t.varType = new VarType[] { this.varType[0], this.varType[1] };
		t.num = new NumericValue[] { this.num[0].Copy(), this.num[1].Copy() };
		t.str = new StringValue[] { this.str[0].Copy(), this.str[1].Copy() };
		t.obj = new ObjectValue[] { this.obj[0].Copy(), this.obj[1].Copy() };
		t.target = new ActionTarget[] { this.target[0].Copy(), this.target[1].Copy() };
		t.attribId = new GUID[] { this.attribId[0].Copy(), this.attribId[1].Copy() };
		return t;
	}	
}

[AddComponentMenu("")]
public class IfThenAction : Action
{

	public List<IfThenActionTest> tests = new List<IfThenActionTest>(0);
	public int doOption = 0;	// what to do when test is TRUE; 0: skip next, 1: goto, 2: exit
	public int gotoAction = 1;	// the action to goto if doOption = 1

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		IfThenAction a = act as IfThenAction;
		
		a.tests = new List<IfThenActionTest>(0);
		foreach (IfThenActionTest t in this.tests) a.tests.Add(t.Copy());

		a.doOption = this.doOption;
		a.gotoAction = this.gotoAction;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		if (tests.Count == 0)
		{
			Debug.LogError("IfThen Action Error: No Tests where created. Exiting Action Queue.");
			return ReturnType.Exit;
		}

		bool prevRes = true;
		bool res = true;

		for (int i = 0; i < tests.Count; i++)
		{
			prevRes = res;
			res = RunTest(tests[i], self, targeted, selfTargetedBy, equipTarget, helper);

			if (tests[i].combineWithPrev == 0)		// AND
			{
				if (res == false || prevRes == false) 
				{
					res = false;
					break; // can stop now since AND fails with any False result
				}
			}
			else if (tests[i].combineWithPrev == 1)	// OR
			{
				if (res == true || prevRes == true) res = true;
				// else, nothing else to do since if res was false then thete is still a chance next test might succeeed
			}
		}

		if (res == true)
		{
			if (doOption == 0) 
			{
				return ReturnType.SkipNext;
			}
			else if (doOption == 1)
			{
				if (gotoAction <= 0)
				{
					Debug.LogError("IfThen Action Error: You should enter an Action number of (1) or higher. Exiting Action Queue.");
					return ReturnType.Exit;
				}
				return (ReturnType.ExecuteSpecificNext + gotoAction);
			}
			else if (doOption == 2)
			{
				return ReturnType.Exit;
			}
		}

		return ReturnType.Done; // was False, simply go on as normal
	}

	private bool RunTest(IfThenActionTest t, GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		int doTest = -1; // 0:numbers, 1:text, 2:object
		
		float[] n = { 0f, 0f };
		string[] s = { null, null };
		Object[] o = { null, null };

		// *** Find the Type and Value for Param 1
		// "Numeric", "Text", "Object", "Attribute", "AttribMin", "AttribMax", "Level", "Currency", "CustomVar", "Subject"

		if (t.varType[0] == IfThenActionTest.VarType.Number)
		{
			doTest = 0;	n[0] = t.num[0].Value;
		}

		else if (t.varType[0] == IfThenActionTest.VarType.AttributeVal || 
				t.varType[0] == IfThenActionTest.VarType.AttributeMin || 
				t.varType[0] == IfThenActionTest.VarType.AttributeMax ||
				t.varType[0] == IfThenActionTest.VarType.Level || 
				t.varType[0] == IfThenActionTest.VarType.Currency ||
				t.varType[0] == IfThenActionTest.VarType.CustomVar)
		{
			GameObject go = DetermineTarget(t.target[0], self, targeted, selfTargetedBy, equipTarget, helper);
			if (go)
			{
				if (t.varType[0] == IfThenActionTest.VarType.CustomVar)
				{
					UniqueMonoBehaviour b = go.GetComponent<UniqueMonoBehaviour>();
					if (b)
					{
						if (b.HasCustomVariable(t.str[0].Value))
						{
							doTest = 1; // string test
							s[0] = b.GetCustomVariable(t.str[0].Value);
						} else { Debug.LogError("IfThen Action Error: The custom variable could not be found (param1)."); return false; }
					} else { Debug.LogError("IfThen Action Error: The Subject does not support custom variables (param1)."); return false; }
				}
				else
				{
					Actor a = go.GetComponent<Actor>();
					if (a)
					{
						doTest = 0; // number test
						if (t.varType[0] == IfThenActionTest.VarType.Level) n[0] = a.ActorClass.Level;
						else if (t.varType[0] == IfThenActionTest.VarType.Currency) n[0] = a.currency;
						else
						{
							RPGAttribute att = a.ActorClass.GetAttribute(t.attribId[0]);
							if (att != null)
							{
								if (t.varType[0] == IfThenActionTest.VarType.AttributeVal) n[0] = att.Value;
								else if (t.varType[0] == IfThenActionTest.VarType.AttributeMin) n[0] = att.MinValue;
								else if (t.varType[0] == IfThenActionTest.VarType.AttributeMax) n[0] = att.MaxValue;
							}
							else { Debug.LogError("IfThen Action Error: The Attribute could not be found for the specific Actor (param1)."); return false; }
						}
					} else { Debug.LogError("IfThen Action Error: The Subject was not an Actor (param1)."); return false; }
				}
			} else { Debug.LogError("IfThen Action Error: The Subject was not found (param1)."); return false; }
		}

		else if (t.varType[0] == IfThenActionTest.VarType.String)
		{
			doTest = 1; s[0] = t.str[0].Value;
		}

		else if (t.varType[0] == IfThenActionTest.VarType.Object)
		{
			doTest = 2; o[0] = t.obj[0].Value;
		}

		else if (t.varType[0] == IfThenActionTest.VarType.Subject)
		{
			doTest = 2; o[0] = DetermineTarget(t.target[0], self, targeted, selfTargetedBy, equipTarget, helper);
		}

		// *** Get Param 2 if number related

		if (t.varType[1] == IfThenActionTest.VarType.Number)
		{
			n[1] = t.num[1].Value;
		}

		else if (
			t.varType[1] == IfThenActionTest.VarType.AttributeVal ||
			t.varType[1] == IfThenActionTest.VarType.AttributeMin ||
			t.varType[1] == IfThenActionTest.VarType.AttributeMax ||
			t.varType[1] == IfThenActionTest.VarType.Level ||
			t.varType[1] == IfThenActionTest.VarType.Currency ||
			t.varType[1] == IfThenActionTest.VarType.CustomVar)
		{
			GameObject go = DetermineTarget(t.target[1], self, targeted, selfTargetedBy, equipTarget, helper);
			if (go)
			{
				if (t.varType[1] == IfThenActionTest.VarType.CustomVar)
				{
					UniqueMonoBehaviour b = go.GetComponent<UniqueMonoBehaviour>();
					if (b)
					{
						if (b.HasCustomVariable(t.str[1].Value))
						{
							doTest = 1; // string test
							s[1] = b.GetCustomVariable(t.str[1].Value);
						} else { Debug.LogError("IfThen Action Error: The custom variable could not be found (param2)."); return false; }
					} else { Debug.LogError("IfThen Action Error: The Subject does not support custom variables (param2)."); return false; }
				}
				else
				{
					Actor a = go.GetComponent<Actor>();
					if (a)
					{
						doTest = 0; // number test
						if (t.varType[1] == IfThenActionTest.VarType.Level) n[1] = a.ActorClass.Level;
						else if (t.varType[1] == IfThenActionTest.VarType.Currency) n[1] = a.currency;
						else
						{
							RPGAttribute att = a.ActorClass.GetAttribute(t.attribId[1]);
							if (att != null)
							{
								if (t.varType[1] == IfThenActionTest.VarType.AttributeVal) n[1] = att.Value;
								else if (t.varType[1] == IfThenActionTest.VarType.AttributeMin) n[1] = att.MinValue;
								else if (t.varType[1] == IfThenActionTest.VarType.AttributeMax) n[1] = att.MaxValue;
							} else { Debug.LogError("IfThen Action Error: The Attribute could not be found for the specific Actor (param2)."); return false; }
						}
					} else { Debug.LogError("IfThen Action Error: The Subject was not an Actor (param2)."); return false; }
				}
			} else { Debug.LogError("IfThen Action Error: The Subject was not found (param2)."); return false; }
		}

		// *** Do Number Test

		if (doTest == 0)	
		{
			if (t.varType[1] == IfThenActionTest.VarType.Number ||
				t.varType[1] == IfThenActionTest.VarType.AttributeVal ||
				t.varType[1] == IfThenActionTest.VarType.AttributeMin ||
				t.varType[1] == IfThenActionTest.VarType.AttributeMax ||
				t.varType[1] == IfThenActionTest.VarType.Level ||
				t.varType[1] == IfThenActionTest.VarType.Currency)
			{
				if (t.testType == IfThenActionTest.TestType.Equal) return n[0] == n[1];
				else if (t.testType == IfThenActionTest.TestType.NotEqual) return n[0] != n[1];
				else if (t.testType == IfThenActionTest.TestType.Bigger) return n[0] > n[1];
				else if (t.testType == IfThenActionTest.TestType.Smaller) return n[0] < n[1];
				else if (t.testType == IfThenActionTest.TestType.BiggerEqual) return n[0] >= n[1];
				else if (t.testType == IfThenActionTest.TestType.SmallerEqual) return n[0] <= n[1];
			}

			// also allowed to test against a string
			else if (t.varType[1] == IfThenActionTest.VarType.String)
			{
				s[0] = n[0].ToString();
				s[1] = t.str[1].Value;
				if (t.testType == IfThenActionTest.TestType.Equal) return s[0].Equals(s[1]);
				else if (t.testType == IfThenActionTest.TestType.NotEqual) return !s[0].Equals(s[1]);
				else { Debug.LogError("IfThen Action Error: Can't perform test with selected Operator."); return false; }
			}

			else
			{
				Debug.LogError("IfThen Action Error: Can't compare the selected Params.");
				return false;
			}
		}

		// *** Do String Test

		else if (doTest == 1)
		{
			if (t.varType[1] == IfThenActionTest.VarType.String)
			{
				s[1] = t.str[1].Value;
				bool r = false;

				if (string.IsNullOrEmpty(s[0]) || string.IsNullOrEmpty(s[1]))
				{
					r = (string.IsNullOrEmpty(s[0]) && string.IsNullOrEmpty(s[1]));
				}
				else r = s[0].Equals(s[1]);

				if (t.testType == IfThenActionTest.TestType.Equal) return r;
				else if (t.testType == IfThenActionTest.TestType.NotEqual) return !r;
				else { Debug.LogError("IfThen Action Error: Can't perform test with selected Operator."); return false; }
			}

			else if (t.varType[1] == IfThenActionTest.VarType.Empty)
			{
				return string.IsNullOrEmpty(s[0]);
			}

			// also allowed to test a number with the string (just convert it)
			else if (t.varType[1] == IfThenActionTest.VarType.Number ||
					t.varType[1] == IfThenActionTest.VarType.AttributeVal ||
					t.varType[1] == IfThenActionTest.VarType.AttributeMin ||
					t.varType[1] == IfThenActionTest.VarType.AttributeMax ||
					t.varType[1] == IfThenActionTest.VarType.Level ||
					t.varType[1] == IfThenActionTest.VarType.Currency)
			{
				if (string.IsNullOrEmpty(s[0])) return false;
				s[1] = n[1].ToString();
				return s[0].Equals(s[1]);
			}

			else
			{
				Debug.LogError("IfThen Action Error: Can't compare the selected Params.");
				return false;
			}
		}

		// *** Object Test

		else if (doTest == 2)
		{
			if (t.testType == IfThenActionTest.TestType.Equal || t.testType == IfThenActionTest.TestType.NotEqual)
			{
				// "Object", "Subject", "Empty (null)", "IsActor", "IsPlayer", "Enabled", "Friendly", "Neutral", "Hostile"

				if (t.varType[1] == IfThenActionTest.VarType.Object)
				{
					o[1] = t.obj[1].Value;
					if (o[0] == null || o[1]==null) { Debug.LogError("IfThen Action Error: One or both Params are Null. Can't compare."); return false; }
					if (t.testType == IfThenActionTest.TestType.Equal) return (o[0] == o[1]);
					else if (t.testType == IfThenActionTest.TestType.NotEqual) return (o[0] != o[1]);
				}

				else if (t.varType[1] == IfThenActionTest.VarType.Subject)
				{
					o[1] = DetermineTarget(t.target[1], self, targeted, selfTargetedBy, equipTarget, helper);
					if (o[0] == null || o[1]==null) { Debug.LogError("IfThen Action Error: One or both Params are Null. Can't compare."); return false; }
					if (t.testType == IfThenActionTest.TestType.Equal) return (o[0] == o[1]);
					else if (t.testType == IfThenActionTest.TestType.NotEqual) return (o[0] != o[1]);
				}

				else if (t.varType[1] == IfThenActionTest.VarType.Empty)
				{
					if (t.testType == IfThenActionTest.TestType.Equal) return (o[0] == null);
					else if (t.testType == IfThenActionTest.TestType.NotEqual) return (o[0] != null);
				}

				else if (t.varType[1] == IfThenActionTest.VarType.Enabled)
				{
					if (o[0] == null) { Debug.LogError("IfThen Action Error: The Param is Null. Can't check if enabled."); return false; }
					
					GameObject go = o[0] as GameObject;
					bool r = false;
					if (go) r = go.activeSelf;
					else
					{
						MonoBehaviour b = o[0] as MonoBehaviour;
						if (b) r = b.enabled;
						else
						{
							Debug.LogError("IfThen Action Error: The Param is not a type that can be checked if it is active/enabled.");
							return false;
						}
					}

					if (t.testType == IfThenActionTest.TestType.Equal) return r;
					else if (t.testType == IfThenActionTest.TestType.NotEqual) return !r;
				}

				else if (t.varType[1] == IfThenActionTest.VarType.IsActor ||
						t.varType[1] == IfThenActionTest.VarType.IsPlayer ||
						t.varType[1] == IfThenActionTest.VarType.Friendly ||
						t.varType[1] == IfThenActionTest.VarType.Neutral ||
						t.varType[1] == IfThenActionTest.VarType.Hostile)
				{
					GameObject go = o[0] as GameObject;
					if (go)
					{
						Actor a = go.GetComponent<Actor>();
						if (a)
						{
							bool r = false;
							if (t.varType[1] == IfThenActionTest.VarType.IsActor) r = true;
							else if (t.varType[1] == IfThenActionTest.VarType.IsPlayer) r = (a.ActorType == UniRPGGlobal.ActorType.Player);
							else if (t.varType[1] == IfThenActionTest.VarType.Friendly) r = (a.ActorType == UniRPGGlobal.ActorType.Friendly);
							else if (t.varType[1] == IfThenActionTest.VarType.Neutral) r = (a.ActorType == UniRPGGlobal.ActorType.Neutral);
							else if (t.varType[1] == IfThenActionTest.VarType.Hostile) r = (a.ActorType == UniRPGGlobal.ActorType.Hostile);

							if (t.testType == IfThenActionTest.TestType.Equal) return r;
							else if (t.testType == IfThenActionTest.TestType.NotEqual) return !r;
						}
					}

					if (t.varType[1] == IfThenActionTest.VarType.IsActor || t.varType[1] == IfThenActionTest.VarType.IsPlayer)
					{
						if (t.testType == IfThenActionTest.TestType.Equal) return false;
						else if (t.testType == IfThenActionTest.TestType.NotEqual) return true;
					}

					Debug.LogError("IfThen Action Error: Could not perform test. Param 1 must be an Actor");
					return false;
				}

				else
				{
					Debug.LogError("IfThen Action Error: Can't compare the selected Params.");
					return false;
				}
			}
			else 
			{ 
				Debug.LogError("IfThen Action Error: Can't perform test with selected Operator.");
				return false;
			}
		}

		Debug.LogError("IfThen Action Error: Invalid Params or Operator.");
		return false;
	}

	// ================================================================================================================
} }