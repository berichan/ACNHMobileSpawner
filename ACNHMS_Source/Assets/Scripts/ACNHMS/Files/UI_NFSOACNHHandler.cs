using UnityEngine;
using UnityEngine.UI;
using Keiwando.NFSO;
using System;
using System.IO;
using NHSE.Core;

public class UI_NFSOACNHHandler : MonoBehaviour
{
    public static UI_NFSOACNHHandler LastInstanceOfNFSO;

    private static string TempNHIPath { get => Application.temporaryCachePath + Path.DirectorySeparatorChar + "Inventory.nhi"; }
    private SupportedFileType[] supportedFileTypes = {
        SupportedFileType.Any,
        SupportedFileType.NHI,
        SupportedFileType.NHV,
        SupportedFileType.NHV2,
        SupportedFileType.NHVH};

    public Toggle EmptySpacesOnly;

    private void Start()
    {
        LastInstanceOfNFSO = this;
    }

    public void SaveFile(string filenameNoPath, byte[] bytes)
    {
        try
        {
            string tempPath = Application.temporaryCachePath + Path.DirectorySeparatorChar + filenameNoPath;

            if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));

            if (File.Exists(tempPath))
                File.Delete(tempPath);

            File.WriteAllBytes(tempPath, bytes);

            saveFile(tempPath, filenameNoPath);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f, true);
        }
    }

    public void OpenFile(string aType, Action<byte[]> handleFile, int expectedFileSize = -1)
    {
        try
        {
            openFile(aType, handleFile, expectedFileSize);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f, true);
        }
    }

    public void OpenAnyFile(Action<byte[]> handleFile, int expectedFileSize = -1)
    {
        try
        {
            openFileAnyType(handleFile, expectedFileSize);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f, true);
        }
    }

    public void SaveInventoryNHI()
    {
        try
        {
            saveItemArray();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f, true);
        }
    }

    public void OpenFileNHI()
    {
        OpenFile("nhi", parseItemDataArray, Item.SIZE * 40);
    }

    private void saveItemArray()
    {
        if (UI_ACItemGrid.LastInstanceOfItemGrid == null)
            throw new Exception("Item grid connection is non-existent.");

        ItemArrayEditor<Item> ItemArray = new ItemArrayEditor<Item>(UI_ACItemGrid.LastInstanceOfItemGrid.Items);
        var bytes = ItemArray.Write();

        SaveFile(DateTime.Now.ToString("yyyyddMM_HHmmss") + ".nhi", bytes);
    }

    private void parseItemDataArray(byte[] bytes)
    {
        if (UI_ACItemGrid.LastInstanceOfItemGrid == null)
            throw new Exception("Item grid connection is non-existent.");

        ItemArrayEditor<Item> ItemArray = new ItemArrayEditor<Item>(UI_ACItemGrid.LastInstanceOfItemGrid.Items);
        ItemArray.ImportItemDataX(bytes, EmptySpacesOnly.isOn, 0);

        for (int i = 0; i < ItemArray.Items.Count; ++i)
        {
            UI_ACItemGrid.LastInstanceOfItemGrid.SetItemAt(ItemArray.Items[i], i, i == (ItemArray.Items.Count - 1));
        }
    }

    private void saveFile(string filePath, string newFileName)
    {
        FileToSave file = new FileToSave(filePath, newFileName, SupportedFileType.Any);
        
        NativeFileSO.shared.SaveFile(file);
    }
    
    private void openFile(string aType, Action<byte[]> handleFile, int expectedFileSize = -1)
    {
        NativeFileSO.shared.OpenFile(supportedFileTypes,
          delegate (bool fileWasOpened, OpenedFile file)
          {
              if (fileWasOpened)
              {
                  // Process the loaded contents of "file"
                  if (file.Extension != "." + aType)
                      PopupHelper.CreateError(string.Format("Not a *.{0} file.", aType), 2f, true);
                  else if (file.Data.Length != expectedFileSize && expectedFileSize != -1)
                      PopupHelper.CreateError(string.Format("Selected file is not the correct size for a *.{0} file.", aType), 2f, true);
                  else
                      handleFile(file.Data);
              }
              else
              {
                  // The file selection was cancelled.	
              }
          });
    }

    private void openFileAnyType(Action<byte[]> handleFile, int expectedFileSize = -1)
    {
        NativeFileSO.shared.OpenFile(supportedFileTypes,
          delegate (bool fileWasOpened, OpenedFile file)
          {
              if (fileWasOpened)
              {
                  // Process the loaded contents of "file"
                  if (file.Data.Length != expectedFileSize && expectedFileSize != -1)
                      PopupHelper.CreateError(string.Format("Selected file is not the expected size."), 2f, true);
                  else
                      handleFile(file.Data);
              }
              else
              {
                  // The file selection was cancelled.	
              }
          });
    }
}
