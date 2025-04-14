using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = s.mixerGroup;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            // Đặt clip mặc định nếu có sẵn
            s.source.clip = s.GetRandomClip();
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // Chọn clip ngẫu nhiên nếu sound có mảng clips
        AudioClip clipToPlay = s.GetRandomClip();
        if (clipToPlay != null)
        {
            s.source.clip = clipToPlay;
        }

        s.source.Play();
    }

    // Hàm để dừng phát một âm thanh cụ thể
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    // FindObjectOfType<AudioManager>().Play("NameSound");
    // FindObjectOfType<AudioManager>().Stop("NameSound");
    // ---------------------------------------------------
    // private AudioManager audioManager;
    // audioManager = FindObjectOfType<AudioManager>();
    // audioManager.Play("NameSound"); 
    // ---------------------------------------------------
    // audioManager.Stop("NameSound");
}