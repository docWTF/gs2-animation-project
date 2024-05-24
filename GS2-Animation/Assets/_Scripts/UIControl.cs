using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    public TextMeshProUGUI uiElement;
    public bool enableUI = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (enableUI == true)
            {
                enableUI = false;
                uiElement.gameObject.SetActive(false);
            }
            else if (enableUI == false)
            {
                enableUI = true;
                uiElement.gameObject.SetActive(true);
            }
        }
    }
}
