using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip[] clips;
    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }
}