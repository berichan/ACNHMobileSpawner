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
    public RawImage PopImage;
    public Text ToWriteTo;
    public Button Choice1, Choice2;

    Color originalTextColor = new Color(1,1,1,1);
    Action currentB1Action, currentB2Action;

    void Start()
    {
        // there can be only one (0_0)
        CurrentInstance = this;
        originalTextColor = ToWriteTo.color;
        ClearButtons();
    }

    public void ClearButtons()
    {
        Choice1.onClick.RemoveAllListeners();
        Choice2.onClick.RemoveAllListeners();
        Choice1.gameObject.SetActive(false);
        Choice2.gameObject.SetActive(false);
    }

    public void CreatePopupChoice(string message, string buttonLabel1, Action onButton1, Color? c = null, string buttonLabel2 = null,  Action onButton2 = null)
    {
        ToWriteTo.text = message;
        MainBlocker.SetActive(true);
        Root.SetActive(true);

        if (c.HasValue)
            ToWriteTo.color = c.Value;
        else
            ToWriteTo.color = originalTextColor;

        StopAllCoroutines();
        PopImage.gameObject.SetActive(false);

        currentB1Action = onButton1;
        Choice1.gameObject.SetActive(true);
        Choice1.onClick.AddListener(delegate { currentB1Action(); cleanUp(); });
        Choice1.GetComponentInChildren<Text>().text = buttonLabel1;

        if (buttonLabel2 != null && onButton2 != null)
        {
            currentB2Action = onButton2;
            Choice2.gameObject.SetActive(true);
            Choice2.onClick.AddListener(delegate { currentB2Action(); cleanUp(); });
            Choice2.GetComponentInChildren<Text>().text = buttonLabel2;
        }
    }

    public void CreatePopupMessage(float length, string message, Action onStart, Color? c = null, bool animate = false, Texture2D imgTexture = null)
    {
        ClearButtons();
        ToWriteTo.text = message;
        MainBlocker.SetActive(true);
        Root.SetActive(true);

        if (c.HasValue)
            ToWriteTo.color = c.Value;
        else
            ToWriteTo.color = originalTextColor;

        PopImage.gameObject.SetActive(true);
        if (imgTexture != null)
            PopImage.texture = imgTexture;
        else
            PopImage.gameObject.SetActive(false);

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
        cleanUp();
    }

    private void cleanUp()
    {
        MainBlocker.SetActive(false);
        Root.SetActive(false);
    }
}
