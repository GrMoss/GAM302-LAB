using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class ShowUI : NetworkBehaviour
{
    [SerializeField] private GameObject uiPlayer;

    void Start()
    {
        if (uiPlayer != null)
        {
            uiPlayer.SetActive(false);
        }
        else
        {
            Debug.LogError("uiPlayer chưa được gán trong Inspector!");
        }
        if(!HasStateAuthority) return;
        uiPlayer.SetActive(true);
    }

}
