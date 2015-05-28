// ====================================================================================================================
// -== UniRPG ==-
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using System.Collections.Generic;

namespace UniRPG {

[AddComponentMenu("")]
public class SoundAction : Action
{
	public int doWhat = 0; // 0: Play Sound, 1: Play Music, 2: Play Target Sound, 3: Stop Target Sound
	
	// the rest is only used if doWhat == 0 or 1
	public List<AudioClip> clips = new List<AudioClip>(0); // for chosing a random clip
	public bool loop = false;
	public bool destroyWhenDone = true;		// only if loop = false
	public int useVolume = 0;				// (not used if doWhat==1) 0:Custom, 1:Effect, 2:Environment, 3:GUI, 4:Music
	public float customVolume = 1f;
	public Vector3 position = Vector3.zero; // if 3D sound
	public bool useParent = true;			// then subject will be used as parent and position as offset from that position
	public string setGlobalObjectVar = null;// if set then this var will contain a reference to the new object that was created

	public override void CopyTo(Action act)
	{
		base.CopyTo(act);
		SoundAction a = act as SoundAction;
		a.doWhat = this.doWhat;
		a.clips = new List<AudioClip>(0);
		foreach (AudioClip c in this.clips) a.clips.Add(c);
		a.loop = this.loop;
		a.destroyWhenDone = this.destroyWhenDone;
		a.useVolume = this.useVolume;
		a.customVolume = this.customVolume;
		a.position = this.position;
		a.useParent = this.useParent;
		a.setGlobalObjectVar = this.setGlobalObjectVar;
	}

	public override ReturnType Execute(GameObject self, GameObject targeted, GameObject selfTargetedBy, GameObject equipTarget, GameObject helper)
	{
		// play new sound/music
		if (doWhat == 0)
		{
			if (clips.Count == 0)
			{
				Debug.LogError("Sound Action Error: No sound clip(s) specified.");
				return ReturnType.Done;
			}

			int idx = 0; if (clips.Count > 1) idx = Random.Range(0, clips.Count);

			if (clips[idx] == null)
			{
				Debug.LogError("Sound Action Error: The clip was not set to a valid Audio Clip.");
				return ReturnType.Done;
			}

			float vol = customVolume;
			if (useVolume == 1) vol = UniRPGGlobal.DB.fxAudioVolume;
			else if (useVolume == 2) vol = UniRPGGlobal.DB.enviroAudioVolume;
			else if (useVolume == 3) vol = UniRPGGlobal.DB.guiAudioVolume;
			else if (useVolume == 4) vol = UniRPGGlobal.DB.musicVolume;

			GameObject go = new GameObject("sound");
			AudioSource au = go.AddComponent<AudioSource>();
			if (!loop && destroyWhenDone) go.AddComponent<AutoDestroyAudio>();
			au.playOnAwake = false;
			au.clip = clips[idx];
			au.loop = loop;
			au.volume = vol;

			go.transform.position = position;
			if (useParent)
			{
				GameObject obj = DetermineTarget(subject, self, targeted, selfTargetedBy, equipTarget, helper);
				if (obj != null)
				{
					go.transform.parent = obj.transform;
					go.transform.localPosition = position;
				}
				else Debug.LogError("Sound Action Error: The parent object could not be found.");
			}

			if (!string.IsNullOrEmpty(setGlobalObjectVar) && !destroyWhenDone)
			{
				UniRPGGlobal.DB.SetGlobalObject(setGlobalObjectVar, go);
			}

			au.Play();
		}

		// play/stop target sound
		else
		{
			GameObject obj = DetermineTarget(subject, self, targeted, selfTargetedBy, equipTarget, helper);
			if (obj != null)
			{
				AudioSource au = obj.GetComponent<AudioSource>();
				if (au)
				{
					// play sound
					if (doWhat == 1)
					{
						if (!string.IsNullOrEmpty(setGlobalObjectVar) && !destroyWhenDone)
						{
							UniRPGGlobal.DB.SetGlobalObject(setGlobalObjectVar, obj);
						}

						if (!loop && destroyWhenDone)
						{
							AutoDestroyAudio d = obj.GetComponent<AutoDestroyAudio>();
							if (d == null) obj.AddComponent<AutoDestroyAudio>();
						}

						float vol = customVolume;
						if (useVolume == 1) vol = UniRPGGlobal.DB.fxAudioVolume;
						else if (useVolume == 2) vol = UniRPGGlobal.DB.enviroAudioVolume;
						else if (useVolume == 3) vol = UniRPGGlobal.DB.guiAudioVolume;
						else if (useVolume == 4) vol = UniRPGGlobal.DB.musicVolume;

						au.volume = vol;
						au.loop = loop;
						au.Play();
					}

					// stop sound
					else
					{
						au.Stop();
					}

				} else Debug.LogError("Sound Action Error: The target object does not have an audio source.");
			} else Debug.LogError("Sound Action Error: The sound object could not be found.");
		}

		return ReturnType.Done;
	}

	// ================================================================================================================
} }