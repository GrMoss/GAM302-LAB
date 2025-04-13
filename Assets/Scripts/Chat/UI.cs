using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject chatUI;
    // [SerializeField] private GameObject shopUI;

    private void Start()
    {
        if (chatUI != null)
        {
            chatUI.SetActive(false);
        }
        else
        {
            Debug.LogError("chatUI chưa được gán trong Inspector!");
        }

        // if (shopUI != null)
        // {
        //     shopUI.SetActive(false);
        // }
        // else
        // {
        //     Debug.LogError("shopUI chưa được gán trong Inspector!");
        // }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (chatUI != null)
            {
                chatUI.SetActive(!chatUI.activeSelf);
            }
            else
            {
                Debug.LogError("chatUI chưa được gán trong Inspector!");
            }
        }
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     if (shopUI != null)
        //     {
        //         shopUI.SetActive(!shopUI.activeSelf);
        //     }
        //     else
        //     {
        //         Debug.LogError("shopUI chưa được gán trong Inspector!");
        //     }
        // }
    }

    public void OnBoxChat()
    {
        chatUI.SetActive(!chatUI.activeSelf);
    }
    // public void OnBoxShop()
    // {
    //     shopUI.SetActive(!shopUI.activeSelf);
    // }

}