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
    public RawImage PopImage, LoaderImage;
    public GameObject LoaderPosStart, LoaderPosEnd;
    public SlicedFilledImage ProgressBar;
    public Text ToWriteTo;
    public Button Choice1, Choice2;

    Color originalTextColor = new Color(1,1,1,1);
    Action currentB1Action, currentB2Action;
    string lastText = string.Empty;

    void Start()
    {
        // there can be only one (0_0)
        CurrentInstance = this;
        originalTextColor = ToWriteTo.color;
        cleanUp();
    }

    public void ClearButtons()
    {
        Choice1.onClick.RemoveAllListeners();
        Choice2.onClick.RemoveAllListeners();
        Choice1.gameObject.SetActive(false);
        Choice2.gameObject.SetActive(false);
    }

    public void UpdateText(string txt)
    {
        lastText = ToWriteTo.text;
        ToWriteTo.text = txt;
    }

    public void ResetText()
    {
        ToWriteTo.text = lastText;
    }

    public void CreateProgressBar(string message, ReferenceContainer<float> progress, Texture2D progressMovingTexture = null, Vector3? pmtRot = null, Texture2D imgTexture = null, string buttonLabel1 = null, Action onButton1 = null, Color? c = null)
    {
        CreatePopupChoice(message, buttonLabel1, onButton1, c);

        ProgressBar.gameObject.SetActive(true);
        ProgressBar.transform.parent.gameObject.SetActive(true);

        PopImage.gameObject.SetActive(true);
        if (imgTexture != null)
            PopImage.texture = imgTexture;
        else
            PopImage.gameObject.SetActive(false);

        LoaderImage.gameObject.SetActive(true);
        if (progressMovingTexture != null)
            LoaderImage.texture = progressMovingTexture;
        else
            LoaderImage.gameObject.SetActive(false);
        LoaderImage.transform.position = LoaderPosStart.transform.position;

        if (pmtRot.HasValue)
            LoaderImage.transform.rotation = Quaternion.Euler(pmtRot.Value);
        else
            LoaderImage.transform.rotation = Quaternion.identity;

        StopAllCoroutines();
        StartCoroutine(progressBar(progress));
    }

    public void CreatePopupChoice(string message, string buttonLabel1, Action onButton1, Color? c = null, string buttonLabel2 = null,  Action onButton2 = null)
    {
        cleanUp();
        ToWriteTo.text = message;
        MainBlocker.SetActive(true);
        Root.SetActive(true);

        if (c.HasValue)
            ToWriteTo.color = c.Value;
        else
            ToWriteTo.color = originalTextColor;

        StopAllCoroutines();
        PopImage.gameObject.SetActive(false);

        if (buttonLabel1 != null && onButton1 != null)
        {
            currentB1Action = onButton1;
            Choice1.gameObject.SetActive(true);
            Choice1.onClick.AddListener(delegate { cleanUp(); currentB1Action(); });
            Choice1.GetComponentInChildren<Text>().text = buttonLabel1;
        }

        if (buttonLabel2 != null && onButton2 != null)
        {
            currentB2Action = onButton2;
            Choice2.gameObject.SetActive(true);
            Choice2.onClick.AddListener(delegate { cleanUp(); currentB2Action(); });
            Choice2.GetComponentInChildren<Text>().text = buttonLabel2;
        }
    }

    public void CreatePopupMessage(float length, string message, Action onStart, Color? c = null, bool animate = false, Texture2D imgTexture = null)
    {
        cleanUp();
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

    IEnumerator progressBar(ReferenceContainer<float> progress)
    {
        while (progress.Value < 1)
        {
            ProgressBar.fillAmount = progress.Value;

            Vector3 newLoaderPos = Vector3.Lerp(LoaderPosStart.transform.position, LoaderPosEnd.transform.position, progress.Value);
            LoaderImage.transform.position = newLoaderPos;

            yield return null;
        }

        cleanUp();
    }

    private void cleanUp()
    {
        ClearButtons();
        MainBlocker.SetActive(false);
        Root.SetActive(false);
        PopImage.gameObject.SetActive(false);
        LoaderImage.gameObject.SetActive(false);

        ProgressBar.gameObject.SetActive(false);
        ProgressBar.transform.parent.gameObject.SetActive(false);
    }
}
