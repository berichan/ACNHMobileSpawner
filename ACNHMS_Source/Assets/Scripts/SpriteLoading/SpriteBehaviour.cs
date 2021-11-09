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

    public static string UsableImagePath { get { return imgroot; } }

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
        if (SpritesExist())
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
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
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
        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        if (success)
        {
            InitStatusLabels();
            InitParser(true);

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

    static void InitParser(bool force = false)
    {
        if (SpriteParser.CurrentInstance == null || force)
            SpriteParser.CurrentInstance = new SpriteParser(
                imgroot + Path.DirectorySeparatorChar + dumpFileName,
                imgroot + Path.DirectorySeparatorChar + dumpHeaderName,
                imgroot + Path.DirectorySeparatorChar + pointerListName);
    }

    public static bool SpritesExist() => File.Exists(imgroot + Path.DirectorySeparatorChar + dumpFileName);

    public static Texture2D ItemToTexture2D(ushort itemId, ushort count, out Color c)
    {
        var isFence = ItemInfo.GetItemKind(Convert.ToUInt16(itemId)) == ItemKind.Kind_Fence;
        Item tempItem = new Item(itemId);
        if (!isFence)
            tempItem.Count = count;
        else
            tempItem.UseCount = count;
        return ItemToTexture2D(tempItem, out c);
    }

    public static Texture2D ItemToTexture2D(Item tr, out Color c)
    {
        if (tr.IsNone)
        {
            c = Color.white;
            return null;
        }

        Item t = new Item();
        t.CopyFrom(tr);
        if (t.ItemId >= 60_000)
        {
            if (FieldItemList.Items.TryGetValue(t.ItemId, out var def))
            {
                if (def.Dig != Item.NONE)
                    t.ItemId = def.Dig;
                else if (def.Pick != Item.NONE)
                    t.ItemId = def.Pick;
            }
        }

        Texture2D toAssignImage = ResourceLoader.GetLeafImage();
        Beri.Drawing.Color itemColor = FieldItemColor.GetItemColor(t);
        c = new Color(itemColor.R / 255f, itemColor.G / 255f, itemColor.B / 255f, itemColor.A / 255f);

        if (SpritesExist())
        {
            InitParser();
            ItemKind itemKind = ItemInfo.GetItemKind(Convert.ToUInt16(t.ItemId));
            var tx = SpriteParser.CurrentInstance.GetTexture(t.ItemId, itemKind == ItemKind.Kind_Fence ? t.UseCount : t.Count);
            if (tx != null)
            {
                toAssignImage = tx;
                c = Color.white;
            }
        }

        return toAssignImage;
    }

    public static Texture2D ItemToTexture2D(ushort itemId, ushort count, out Color c, ItemFilter iF)
    {
        if (iF == ItemFilter.Items)
            return ItemToTexture2D(itemId, count, out c);
        
        ushort toFindItem = iF == ItemFilter.Fossils ? itemId : RecipeList.Recipes[itemId];
        return ItemToTexture2D(toFindItem, count, out c);
    }

    public static Texture2D PullTextureFromParser(SpriteParser parser, string itemName)
    {
        var items = parser.GetTexture(itemName);
        if (items != null)
            return items;
        return null;
    }

    // not used

    public static Texture2D ItemToTexture2DSlow(ushort itemId, ushort count, out Color c, ItemFilter iF)
    {
        if (iF == ItemFilter.Items)
            return ItemToTexture2D(itemId, count, out c);

        ushort checkValueId = UI_SearchWindow.FilterToItemId(iF, 0);
        ushort toFindItem = iF == ItemFilter.Fossils ? itemId : RecipeList.Recipes[itemId];
        Texture2D itemType = ItemToTexture2D(checkValueId, count, out c); 
        Texture2D itemMain = ItemToTexture2D(toFindItem, count, out c);
        TextureScale.Bilinear(itemType, itemMain.width / 2, itemMain.height / 2);
        return AddWatermark(itemMain, itemType, 0, 0);
    }

    public static Texture2D AddWatermark(Texture2D background, Texture2D watermark, int startX, int startY)
    {
        Texture2D newTex = new Texture2D(background.width, background.height, background.format, false);
        for (int x = 0; x < background.width; x++)
        {
            for (int y = 0; y < background.height; y++)
            {
                if (x >= startX && y >= startY && x < watermark.width && y < watermark.height)
                {
                    Color bgColor = background.GetPixel(x, y);
                    Color wmColor = watermark.GetPixel(x - startX, y - startY);
                    Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);
                    newTex.SetPixel(x, y, final_color);
                }
                else
                    newTex.SetPixel(x, y, background.GetPixel(x, y));
            }
        }

        newTex.Apply();
        return newTex;
    }
}
