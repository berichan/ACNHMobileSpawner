using UnityEngine;
using UnityEngine.UI;
using Keiwando.NFSO;
using System;
using System.IO;
using NHSE.Core;

public class UI_NFSOACNHHandler : MonoBehaviour
{
    private static string TempNHIPath { get => Application.persistentDataPath + Path.DirectorySeparatorChar + "Inventory.nhi"; }
    private SupportedFileType[] supportedFileTypes = { SupportedFileType.Any };

    public Toggle EmptySpacesOnly;

    public void SaveInventoryNHI()
    {
        try
        {
            saveItemArray();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    public void OpenFileNHI()
    {
        OpenFile("nhi", parseItemDataArray);
    }

    public void OpenFile(string aType, Action<byte[]> handleFile)
    {
        try
        {
            openFile(aType, handleFile);
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 2f);
        }
    }

    private void saveItemArray()
    {
        if (UI_ACItemGrid.LastInstanceOfItemGrid == null)
            throw new Exception("Item grid connection is non-existent.");

        ItemArrayEditor<Item> ItemArray = new ItemArrayEditor<Item>(UI_ACItemGrid.LastInstanceOfItemGrid.Items);
        var bytes = ItemArray.Write();

        if (!Directory.Exists(Path.GetDirectoryName(TempNHIPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(TempNHIPath));

        if (File.Exists(TempNHIPath))
            File.Delete(TempNHIPath);

        File.WriteAllBytes(TempNHIPath, bytes);

        saveFile(TempNHIPath, DateTime.Now.ToString("yyyyddMMHHmmss") + ".nhi");
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
    
    private void openFile(string aType, Action<byte[]> handleFile)
    {
        NativeFileSO.shared.OpenFile(supportedFileTypes,
          delegate (bool fileWasOpened, OpenedFile file)
          {
              if (fileWasOpened)
              {
                  // Process the loaded contents of "file"
                  if (file.Extension != "." + aType)
                      throw new Exception(string.Format("Not an *.{0} file.", aType));
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
