using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SpriteController : MonoBehaviour
{
    private static string host = "https://github.com/berichan/ACNHMobileSpawner/releases/download/0.1-a/hosts_ignore.txt";

    public Text FileInfoText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator tryDownloadSprites()
    {
        FileInfoText.text = "Attempting to get mirror list...";
        UnityWebRequest www = UnityWebRequest.Get(host);
        www.SendWebRequest();
        while (!www.isDone)
        {
            FileInfoText.text = "Downloading: " + www.downloadProgress.ToString() + "%";
            yield return null;
        }
        FileInfoText.text = "Downloading: 100%";


    }
}
