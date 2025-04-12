using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLoginUI : MonoBehaviour
{
    [SerializeField] private GameObject panelBXH;
    [SerializeField] Button btnBXH;
    private bool isPanelBXHActive = false;

    void Start()
    {
        btnBXH.onClick.AddListener(OnButtonBXHClicked);
    }

    public void OnButtonBXHClicked()
    {
        isPanelBXHActive = !isPanelBXHActive;
        panelBXH.SetActive(isPanelBXHActive);

        if (isPanelBXHActive)
        {
            FirebaseWebGL.Instance.LoadScores();
        }
    }
}
