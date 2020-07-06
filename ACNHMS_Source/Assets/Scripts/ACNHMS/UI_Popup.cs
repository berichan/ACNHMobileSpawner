using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_Popup : MonoBehaviour
{
    public static UI_Popup CurrentInstance;

    public GameObject Root;
    public GameObject MainBlocker;
    public Text ToWriteTo;

    Color originalTextColor = new Color(1,1,1,1);

    void Start()
    {
        // there can be only one (0_0)
        CurrentInstance = this;
        originalTextColor = ToWriteTo.color;
    }

    public void CreatePopupMessage(float length, string message, Action onStart, Color? c = null, bool animate = false)
    {
        ToWriteTo.text = message;
        MainBlocker.SetActive(true);
        Root.SetActive(true);

        if (c.HasValue)
            ToWriteTo.color = c.Value;
        else
            ToWriteTo.color = originalTextColor;

        StopAllCoroutines();
        StartCoroutine(waitPop(length, onStart, animate));
    }

    IEnumerator waitPop(float length, Action onStart, bool anim)
    {
        if (onStart != null)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            onStart();
        }

        yield return new WaitForSeconds(length);
        MainBlocker.SetActive(false);
        Root.SetActive(false);
    }
}
