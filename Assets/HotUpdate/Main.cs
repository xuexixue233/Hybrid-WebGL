using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Main : MonoBehaviour
{
    public TMP_Text timeText;

    private void Awake()
    {
        timeText.color=Color.red;
    }

    private void Update()
    {
        timeText.text = DateTime.Now.ToLongTimeString();
    }
}
