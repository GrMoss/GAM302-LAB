using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
  [SerializeField] private GameObject chatUI;

  private void Start()
  {
    chatUI.SetActive(false);
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.LeftShift))
    {
      chatUI.SetActive(!chatUI.activeSelf);
    }
  }
}
