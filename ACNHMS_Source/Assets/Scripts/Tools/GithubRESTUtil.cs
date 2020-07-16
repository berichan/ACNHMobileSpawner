using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable]
public struct GitRelease
{
    public string tag_name;
    public bool draft;
}

public class GithubRESTUtil : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    public static string LatestUri = "https://api.github.com/repos/berichan/ACNHMobileSpawner/releases/latest";

    [HideInInspector]
    public static string ReleasePage = "https://github.com/berichan/ACNHMobileSpawner/releases";

    public Text UpdateOrErrorWriter;

    private bool tapThrough = false;
    private bool alreadyChecked = false;

    private long timeAtLastCheck = -1;
    private bool updateAvailable = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(doRequest());
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused)
            OnApplicationFocus(true);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !updateAvailable)
        {
            long timeDifference = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - timeAtLastCheck;
            if (timeDifference > 3600000)
            {
                StopAllCoroutines();
                StartCoroutine(doRequest());
            }
        }
    }

    IEnumerator doRequest()
    {
        timeAtLastCheck = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(LatestUri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
                UpdateOrErrorWriter.text = "No network. Couldn't check for update.";
                UpdateOrErrorWriter.color = Color.magenta;
                yield break;
            }

            var json = JsonUtility.FromJson<GitRelease>(webRequest.downloadHandler.text);

            if (!json.draft)
            {
                string rootRelease = json.tag_name.Split('-')[0];
                double currentVersion = double.Parse(Application.version);
                double gitVersion = double.Parse(rootRelease);

                if (gitVersion > currentVersion)
                {
                    UpdateOrErrorWriter.text = string.Format("A new update is available. Tap here to get version {0}.", json.tag_name);
                    UpdateOrErrorWriter.color = Color.red;
                    tapThrough = true;
                    updateAvailable = true;
                }
                else
                {
                    UpdateOrErrorWriter.text = "You are using the latest version, and looking pretty fine doing it!";
                    UpdateOrErrorWriter.color = new Color(1, 1, 1, 0.4f);
                }
#if UNITY_EDITOR
                if (gitVersion >= currentVersion)
                {
                    var upVal = (gitVersion + 0.01).ToString();
                    UpdateOrErrorWriter.text = string.Format("Warning!! Editor version should be at least {0}.", upVal);
                    UpdateOrErrorWriter.color = Color.red;
                }
#endif
            }
            else
                UpdateOrErrorWriter.text = "A new release is being prepped!";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!tapThrough)
            return;

        Application.OpenURL(ReleasePage);
    }
}
