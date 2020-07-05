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

    void Start()
    {
        // there can be only one (0_0)
        CurrentInstance = this;
    }

    public void CreatePopupMessage(float length, string message, Action onStart, bool animate = false)
    {
        ToWriteTo.text = message;
        MainBlocker.SetActive(true);
        Root.SetActive(true);
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
