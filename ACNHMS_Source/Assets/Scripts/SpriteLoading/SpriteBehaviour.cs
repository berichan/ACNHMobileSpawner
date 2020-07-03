using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using NH_CreationEngine;
using NHSE.Core;

public class SpriteBehaviour : MonoBehaviour
{
    private static string host = "https://github.com/berichan/ACNHMobileSpawner/releases/download/0.1-a/hosts_ignore_new.txt";
    private static string fileroot { get { return Application.persistentDataPath + Path.DirectorySeparatorChar + "Sprites"; } }
    private static string imgroot { get { return fileroot; } }
    private static string filename = "SpriteDump.zip";

    private static string dumpFileName = "imagedump.dmp";
    private static string dumpHeaderName = dumpFileName + ".header";
    private static string pointerListName = "SpritePointer.txt";

    private static float longwaittime = 3f, shortwaittime = 1f;

    public Text FileInfoText, SpriteStatusText, ButtonText;
    public GameObject RootBlocker;
    public GameObject DownloadingAnim;
    public Button DownloadButton;

    private string lastText;
    private static DataTable itemSource, recipeSource, flowerSource, variationSource;

    // Start is called before the first frame update
    void Start()
    {
        //fileroot = Application.persistentDataPath + Path.DirectorySeparatorChar + "Sprites";
        //imgroot = fileroot + Path.DirectorySeparatorChar + "img";
        lastText = FileInfoText.text;
        InitStatusLabels();
    }

    public void InitStatusLabels()
    {
        if (Directory.Exists(imgroot))
        {
            SpriteStatusText.text = "Sprites loaded";
            ButtonText.text = "Repair sprites";
        }
        else
        {
            SpriteStatusText.text = "Not downloaded";
            ButtonText.text = "Download sprites";
        }
    }


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        // various debug
        if (FileInfoText.text != lastText)
        {
            lastText = FileInfoText.text;
            Debug.Log(lastText);
        }
#endif
    }

    public void DownloadSprites()
    {
        RootBlocker.SetActive(true);
        DownloadButton.interactable = false;
        DownloadingAnim.SetActive(true);
        StartCoroutine(tryDownloadSprites());
    }

    IEnumerator tryDownloadSprites()
    {
        FileInfoText.text = "Attempting to get mirror list...";
        yield return new WaitForSeconds(longwaittime);
        UnityWebRequest www = UnityWebRequest.Get(host);
        www.SendWebRequest();
        while (!www.isDone)
        {
            FileInfoText.text = "Downloading: " + (www.downloadProgress * 100f).ToString("0.00") + "%";
            yield return null;
        }

        if (www.isNetworkError || www.isHttpError)
        {
            FileInfoText.text = "Net Error: " + www.error;
            completeDownload(false);
            yield break;
        }

        FileInfoText.text = "Downloading: 100%";
        yield return new WaitForSeconds(shortwaittime);
        FileInfoText.text = "Resolving hosts...";

        string[] hostLines = www.downloadHandler.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        www.Dispose();
        int currentHost = 0;
        bool downloadAndUnzipSuccess = false;
        yield return null; // give us a frame before creating new request

        while (!downloadAndUnzipSuccess)
        {
            yield return new WaitForSeconds(longwaittime);
            if (currentHost >= hostLines.Length)
            {
                FileInfoText.text = "Error: Ran out of hosts";
                completeDownload(false);
                yield break;
            }

            string hostToTry = hostLines[currentHost];
            FileInfoText.text = "Attempting to download sprites from: " + Environment.NewLine + ExtractDomainNameFromURL(hostToTry);
            yield return new WaitForSeconds(longwaittime);
            UnityWebRequest wwwHost = UnityWebRequest.Get(hostToTry);
            wwwHost.SendWebRequest();

            while (!wwwHost.isDone)
            {
                FileInfoText.text = "Downloading: " + (wwwHost.downloadProgress * 100f).ToString("0.00") + "%";
                yield return null;
            }

            if (wwwHost.isNetworkError || wwwHost.isHttpError)
            {
                FileInfoText.text = "Net Error: " + wwwHost.error;
                wwwHost.Dispose();
                yield return new WaitForSeconds(longwaittime);
                currentHost++;
                continue;
            }
            else
            {
                //unzip
                FileInfoText.text = "Download complete. Saving file (lockup is ok)...";
                yield return new WaitForSeconds(shortwaittime);
                //save to root
                string rootItemPath = Application.persistentDataPath + Path.DirectorySeparatorChar + filename;
                try
                {
                    if (!Directory.Exists(fileroot))
                        Directory.CreateDirectory(fileroot);
                    else
                    {
                        FileInfoText.text = "WARNING: Sprites found. Deleting directory...";
                        Directory.Delete(fileroot, true);
                    }

                    File.WriteAllBytes(rootItemPath, wwwHost.downloadHandler.data);
                    wwwHost.Dispose();
                }
                catch (Exception e)
                {
                    FileInfoText.text = "File Error: " + e.Message;
                    completeDownload(false);
                    yield break;
                }

                FileInfoText.text = "Attempting to unzip sprites (lockup is ok)...";
                yield return new WaitForSeconds(shortwaittime);
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(rootItemPath, fileroot);
                    File.Delete(rootItemPath);
                }
                catch (Exception e)
                {
                    FileInfoText.text = "Zip Error: " + e.Message;
                    currentHost++;
                    continue;
                }

                downloadAndUnzipSuccess = true;
                FileInfoText.text = "Sprites created successfully";
                completeDownload(true);
            }
        }

    }

    void completeDownload(bool success)
    {
        RootBlocker.gameObject.SetActive(false);
        DownloadingAnim.SetActive(false);
        DownloadButton.interactable = true;

        if (success)
        {
            InitStatusLabels();

            //reset with sprites
            if (UI_ACItemGrid.LastInstanceOfItemGrid != null)
                UI_ACItemGrid.LastInstanceOfItemGrid.ResetAllItems();
        }
    }

    public static string ExtractDomainNameFromURL(string Url)
    {
        if (Url.Contains(@"://"))
            Url = Url.Split(new string[] { "://" }, 2, StringSplitOptions.None)[1];

        return Url.Split('/')[0];
    }

    // NH_CreationEngine: https://github.com/berichan/NH_CreationEngine

    static void InitParser()
    {
        if (SpriteParser.CurrentInstance == null)
            SpriteParser.CurrentInstance = new SpriteParser(
                imgroot + Path.DirectorySeparatorChar + dumpFileName,
                imgroot + Path.DirectorySeparatorChar + dumpHeaderName,
                imgroot + Path.DirectorySeparatorChar + pointerListName);
    }

    public static Texture2D ItemToTexture2D(ushort itemId, ushort count, out Color c)
    {
        Item tempItem = new Item(itemId);
        tempItem.Count = count;
        return ItemToTexture2D(tempItem, out c);
    }

    public static Texture2D ItemToTexture2D(Item t, out Color c)
    {
        if (t.IsNone)
        {
            c = Color.white;
            return null;
        }

        InitParser();

        Texture2D toAssignImage = ResourceLoader.GetLeafImage();
        System.Drawing.Color itemColor = ItemColor.GetItemColor(t);
        c = new Color(itemColor.R / 255f, itemColor.G / 255f, itemColor.B / 255f, itemColor.A / 255f);
        
        byte[] bytes = SpriteParser.CurrentInstance.GetPng(t.ItemId, (byte)t.Count);
        toAssignImage = new Texture2D(2, 2);
        toAssignImage.LoadImage(bytes);
        c = Color.white;

        return toAssignImage;
    }
}
