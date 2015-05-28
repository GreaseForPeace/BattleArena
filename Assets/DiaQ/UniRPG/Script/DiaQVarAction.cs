// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

#if UNIRPG_CORE

using UnityEngine;
using UniRPG;

namespace DiaQ
{
	[AddComponentMenu("")]
	public class DiaQVarAction : Action
	{
		public string varName = "";
		public StringValue toVal = new StringValue();

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			DiaQVarAction a = act as DiaQVarAction;
			a.varName = this.varName;
			a.toVal = this.toVal.Copy();
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			if (!string.IsNullOrEmpty(varName))
			{
				DiaQEngine.Instance.SetDiaQVarValue(varName, toVal.Value);
			}
			else Debug.LogError("DiaQ Set Variable Action Error: No variable name provided.");
			return ReturnType.Done;
		}

		// ================================================================================================================
	}
}

#endif