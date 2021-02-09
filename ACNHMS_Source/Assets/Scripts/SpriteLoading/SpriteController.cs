using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using NHSE.Core;
using System.Data;
using System.Linq;

public class SpriteController : MonoBehaviour
{
    private static string host = "https://github.com/berichan/ACNHMobileSpawner/releases/download/0.1-a/hosts_ignore.txt";
    private static string fileroot { get { return Application.persistentDataPath + Path.DirectorySeparatorChar + "Sprites"; } }
    private static string imgroot { get { return fileroot + Path.DirectorySeparatorChar + "img"; } }
    private static string filename = "img.zip";

    private static string flowercsv = "SpriteLoading/flowers";
    private static string itemcsv = "SpriteLoading/items";
    private static string recipecsv = "SpriteLoading/recipe";
    private static string variationcsv = "SpriteLoading/variation";

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
            FileInfoText.text = "Downloading: " + (www.downloadProgress*100f).ToString("0.00") + "%";
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

        while(!downloadAndUnzipSuccess)
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
                FileInfoText.text = "Downloading: " + (wwwHost.downloadProgress*100f).ToString("0.00") + "%";
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

    /*
     * Adapted version of kinglycosa's csv and item loader code, with thanks to myshilingstar for variation item loading.
     * Licensed under BSD-2-Clause with full credit to kinglycosa and myshilingstar, mine is just the unity/loading code
     */

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

        Texture2D toAssignImage = ResourceLoader.GetLeafImage();
        Beri.Drawing.Color itemColor = ItemColor.GetItemColor(t);
        c = new Color(itemColor.R / 255f, itemColor.G / 255f, itemColor.B / 255f, itemColor.A / 255f);

        string path = GetImagePathFromItem(t);
        if (path != "")
        {
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                toAssignImage = new Texture2D(2, 2);
                toAssignImage.LoadImage(bytes);
                c = Color.white;
            }
        }

        return toAssignImage;
    }

    public static string GetImagePathFromItem(ushort itemId, ushort count)
    {
        Item tempItem = new Item(itemId);
        tempItem.Count = count;
        return GetImagePathFromItem(tempItem);
    }

    public static string GetImagePathFromItem(Item t)
    {
        if (!Directory.Exists(fileroot))
            return "";
        if (itemSource == null)
            GenerateAssetHashes();

        string pathRet = "";
        string itemidAsHex = t.ItemId.ToString("X4");
        string countAsHex = t.Count.ToString("X4");
        uint count = Convert.ToUInt32(t.Count);

        string hexToUse;
        if (t.ItemId == Convert.ToUInt16(UI_SearchWindow.MESSAGEBOTTLEITEM) || t.ItemId == Convert.ToUInt16(UI_SearchWindow.RECIPEITEM))
            hexToUse = countAsHex;
        else
            hexToUse = itemidAsHex;


        if (ItemInfo.GetItemKind(t).IsFlower()) 
        {
            pathRet = GetImagePathFromID(hexToUse, flowerSource);
            if (pathRet == "")
                pathRet = GetImagePathFromID(hexToUse, itemSource);
        }
        else if (t.ItemId == Convert.ToUInt16(UI_SearchWindow.MESSAGEBOTTLEITEM) || t.ItemId == Convert.ToUInt16(UI_SearchWindow.RECIPEITEM))
            pathRet = GetImagePathFromID(hexToUse, recipeSource);
        else
            pathRet = GetImagePathFromID(hexToUse, itemSource, count);

        return pathRet;
    }

    public static void GenerateAssetHashes()
    {
        itemSource = getDataTableFromRaw((Resources.Load(itemcsv) as TextAsset).text);
        recipeSource = getDataTableFromRaw((Resources.Load(recipecsv) as TextAsset).text);
        variationSource = getDataTableFromRaw((Resources.Load(variationcsv) as TextAsset).text);
        flowerSource = getDataTableFromRaw((Resources.Load(flowercsv) as TextAsset).text);
    }

    
    private static DataTable getDataTableFromRaw(string rawVal)
    {
        var dt = new DataTable();
        List<string> rawValList = new List<string>(rawVal.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));

        rawValList.Take(1)
            .SelectMany(x => x.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            .ToList()
            .ForEach(x => dt.Columns.Add(x.Trim()));

        rawValList.Skip(1)
            .Select(x => x.Split(','))
            .ToList()
            .ForEach(line => dt.Rows.Add(line));

        if (dt.Columns.Contains("ID"))
            dt.PrimaryKey = new DataColumn[1] {dt.Columns["ID"]};

        return dt;
    }

    public static string GetImagePathFromID(string itemID, DataTable source, uint count = 0u)
    {
        if (source == null)
            return "";

        DataRow sourceRow = source.Rows.Find(itemID);
        DataRow variationRow = variationSource.Rows.Find(itemID);

        if (sourceRow == null)
            return "";

        string pathMain;
        if (variationRow != null)
        {
            pathMain = imgroot + Path.DirectorySeparatorChar + "variation" + Path.DirectorySeparatorChar + variationRow[1]?.ToString() + Path.DirectorySeparatorChar + variationRow[3]?.ToString() + ".png";
            if (File.Exists(pathMain))
                return pathMain;

            string bodyVal = (count & 0xF).ToString();
            string fabricVal = (((count & 0xFF) - (count & 0xF)) / 32u).ToString();

            pathMain = imgroot + Path.DirectorySeparatorChar + "variation" + Path.DirectorySeparatorChar + variationRow[1]?.ToString() + Path.DirectorySeparatorChar + variationRow[3]?.ToString() + "_" + bodyVal + "_" + fabricVal + ".png";
            if (File.Exists(pathMain))
                return pathMain;
        }

        pathMain = imgroot + Path.DirectorySeparatorChar + sourceRow[1]?.ToString() + Path.DirectorySeparatorChar + sourceRow[0]?.ToString() + ".png";

        if (File.Exists(pathMain))
            return pathMain;

        // no image, sorry
        return "";
    }
}
