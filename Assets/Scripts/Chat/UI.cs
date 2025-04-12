using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject chatUI;

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
    }

    public void OnBoxChat()
    {
        chatUI.SetActive(!chatUI.activeSelf);
    }

}