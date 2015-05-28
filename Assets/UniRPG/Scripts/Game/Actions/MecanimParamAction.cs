// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG
{
	[AddComponentMenu("")]
	public class MecanimParamAction : Action
	{
		public StringValue paramName = new StringValue();
		public int paramType = 0;	// 0:bool, 1:int, 2:float, 3:vector
		public bool paramBool = false;
		public NumericValue paramIntFloat = new NumericValue(0f);
		public Vector3 paramVector = Vector3.zero;
		public bool restoreOldValAfterCall = false;

		private int called = 0;
		private float oldNum = 0f;
		private bool oldBool = false;
		private Vector3 oldVec = Vector3.zero;

		public override void CopyTo(Action act)
		{
			base.CopyTo(act);
			MecanimParamAction a = act as MecanimParamAction;
			a.paramName = this.paramName.Copy();
			a.paramIntFloat = this.paramIntFloat.Copy();
			a.paramType = this.paramType;
			a.paramBool = this.paramBool;
			a.paramVector = this.paramVector;
			a.restoreOldValAfterCall = this.restoreOldValAfterCall;
		}

		public override void Init(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			called = 0;
		}

		public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
		{
			called++;
			string param = paramName.Value;
			if (string.IsNullOrEmpty(param))
			{
				Debug.LogError("Mecanim Param Error: Invalid parameter name.");
				return ReturnType.Done;
			}

			GameObject obj = DetermineTarget(self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj == null)
			{
				Debug.LogError("Mecanim Param Error: The subject did not exist.");
				return ReturnType.Done;
			}

			Animator ani = obj.GetComponent<Animator>();
			if (ani == null)
			{
				Debug.LogError("Mecanim Param Error: The subject does not have an Animator component.");
				return ReturnType.Done;
			}

			switch (paramType)
			{
				case 0:
				{
					if (restoreOldValAfterCall)
					{
						if (called > 1) ani.SetBool(param, oldBool);
						else
						{
							oldBool = ani.GetBool(param);
							ani.SetBool(param, paramBool);
							return ReturnType.CallAgain;
						}
					}
					else ani.SetBool(param, paramBool);

				} break;
				case 1:
				{
					if (restoreOldValAfterCall)
					{
						if (called > 1) ani.SetInteger(param, (int)oldNum);
						else
						{
							oldNum = ani.GetInteger(param);
							ani.SetInteger(param, (int)paramIntFloat.Value);
							return ReturnType.CallAgain;
						}
					}
					else ani.SetInteger(param, (int)paramIntFloat.Value);

				} break;

				case 2:
				{
					if (restoreOldValAfterCall)
					{
						if (called > 1) ani.SetFloat(param, oldNum);
						else
						{
							oldNum = ani.GetFloat(param);
							ani.SetFloat(param, paramIntFloat.Value);
							return ReturnType.CallAgain;
						}
					}
					else ani.SetFloat(param, paramIntFloat.Value);
					
				} break;

				case 3:
				{
					if (restoreOldValAfterCall)
					{
						if (called > 1) ani.SetVector(param, oldVec);
						else
						{
							oldVec = ani.GetVector(param);
							ani.SetVector(param, paramVector);
							return ReturnType.CallAgain;
						}
					}
					else ani.SetVector(param, paramVector);
				} break;
			}

			return ReturnType.Done; // this action is done
		}

		// ================================================================================================================
	}
}
