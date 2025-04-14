using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingSound : MonoBehaviour
{
    [SerializeField] private string soundTheme;
    [SerializeField] private string soundButton;
    [SerializeField] private string soundSelect;
    [SerializeField] private string soundInputField;
    [SerializeField] private string buySoundButton;
    private AudioManager audioManager;

    [Obsolete]
    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play(soundTheme);
    }

    public void SelectSound()
    {
        audioManager.Play(soundSelect);
    }
    
    public void ButtonSound()
    {
        audioManager.Play(soundButton);
    }

    public void BuySound()
    {
        audioManager.Play(buySoundButton);
    }

    public void SoundFx(string nameSound)
    {
        audioManager.Play(nameSound);
    }

    public void PlaySound(string nameSound)
    {
        audioManager.Play(nameSound);
    }

    public void InputFieldSound()
    {
        audioManager.Play(soundInputField);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)|| Input.GetKeyDown(KeyCode.E))
        {
            audioManager.Play(soundButton);
        }
    }
}
