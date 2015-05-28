// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using System.Collections.Generic;

namespace UniRPG {

public abstract class InputBinderBase
{
	private List<InputManager.InputIdxCache> _idxCache = null;

	public abstract List<InputDefinition> GetInputBinds();

	/// <summary>Used to enable/ disable this Input Binder's defintions</summary>
	public void SetActive(bool active)
	{
		if (_idxCache == null) return;
		for (int i = 0; i < _idxCache.Count; i++)
		{
			InputManager.Instance.SetActive(_idxCache[i], active);
		}
	}

	/// <summary>Used internally and called by the InputManager</summary>
	public void _SaveInputIdxCache(InputManager.InputIdxCache idx)
	{
		if (_idxCache == null) _idxCache = new List<InputManager.InputIdxCache>(0);
		_idxCache.Add(idx);
	}

	// ================================================================================================================
} }