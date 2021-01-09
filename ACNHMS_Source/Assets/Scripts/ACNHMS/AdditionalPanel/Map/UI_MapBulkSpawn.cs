using NHSE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NHSE.Core.ItemKind;

public class UI_MapBulkSpawn : MonoBehaviour
{
    public enum BulkSpawnPreset
    {
        Music,
        DIYRecipesAlphabetical,
        DIYRecipesSequential,
        Fossils,
        GenericMaterials,
        SeasonalMaterials,
        RealArt,
        FakeArt,
        Bugs,
        Fish,
        BugsAndFish,
        InventoryOfApp,
        CustomFile,
    }

    public enum SpawnDirection
    {
        SouthEast,
        SouthWest,
        NorthWest,
        NorthEast
    }

    [HideInInspector]
    public BulkSpawnPreset CurrentSpawnPreset { get; private set; } = 0;

    [HideInInspector]
    public SpawnDirection CurrentSpawnDirection { get; private set; } = 0;

    public Dropdown BulkSpawnPresetMode;
    public Dropdown SpawnDir;
    public Toggle OverwriteItemsToggle;
    public InputField RectWidth, RectHeight;
    public InputField ItemMultiplier;
    public Text ItemCount;

    public int Multiplier { get 
    {
            if (int.TryParse(ItemMultiplier.text, out var val))
                return val;
            else
                return 1;
    } }
    public float RectWidthDimension => float.Parse(RectWidth.text);
    public float RectHeightDimension => float.Parse(RectHeight.text);
    public bool OverwriteTiles => OverwriteItemsToggle.isOn;

    private static IReadOnlyList<ushort> allItems = null;
    public static IReadOnlyList<ushort> GetAllItems()
    {
        if (allItems == null)
        {
            var listItems = GameInfo.Strings.ItemDataSource.ToList();
            var itemsClean = listItems.Where(x => !x.Text.StartsWith("(Item #")).ToList();
            var items = new ushort[itemsClean.Count];
            for (int i = 0; i < itemsClean.Count; ++i) 
            {
                items[i] = (ushort)itemsClean[i].Value;
            }
            allItems = items;
        }

        return allItems;
    }

    private Item[] fileLoadedItems = new Item[1] { new Item(0x09C4) };

    // Start is called before the first frame update
    void Start()
    {
        BulkSpawnPresetMode.ClearOptions();
        string[] smChoices = Enum.GetNames(typeof(BulkSpawnPreset));
        foreach (string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = UI_MapItemTile.AddNewlinesAfterCapitals(sm, ' ');
            BulkSpawnPresetMode.options.Add(newVal);
        }
        BulkSpawnPresetMode.onValueChanged.AddListener(delegate { CurrentSpawnPreset = (BulkSpawnPreset)BulkSpawnPresetMode.value; updateItemCount(); });
        BulkSpawnPresetMode.value = 0;
        BulkSpawnPresetMode.RefreshShownValue();

        SpawnDir.ClearOptions();
        smChoices = new string[4] { "South-east ↘", "South-west ↙", "North-west ↖", "North-east ↗" };
        foreach (string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = sm;
            SpawnDir.options.Add(newVal);
        }
        SpawnDir.onValueChanged.AddListener(delegate { CurrentSpawnDirection = (SpawnDirection)SpawnDir.value; });
        SpawnDir.value = 0;
        SpawnDir.RefreshShownValue();

        ItemMultiplier.onValueChanged.AddListener(delegate { updateItemCount(); });
        updateItemCount();
    }

    private void updateItemCount()
    {
        ItemCount.text = GetItemsOfCurrentPreset().Length.ToString();
    }

    public void LoadItems()
    {
        UI_NFSOACNHHandler.LastInstanceOfNFSO.OpenAnyFile(setLoadedItems);
    }

    private void setLoadedItems(byte[] bytes)
    {
        try
        {
            fileLoadedItems = Item.GetArray(bytes);
            TrimTrailingNoItems(ref fileLoadedItems, Item.NONE); // remove trailing empties
            UI_Popup.CurrentInstance.CreatePopupChoice("File loaded successfully! \r\nWould you like to set the flag0 of these items to 0x20, so that they will be able to be picked up by you and other players?", "Yes", () => { Flag20LoadedItems(); }, null, "No", () => { });
            CurrentSpawnPreset = BulkSpawnPreset.CustomFile;
            BulkSpawnPresetMode.value = (int)BulkSpawnPreset.CustomFile;
            updateItemCount();
        }
        catch (Exception e)
        {
            PopupHelper.CreateError(e.Message, 3f);
        }
    }

    private void Flag20LoadedItems()
    {
        foreach (Item i in fileLoadedItems)
            i.SystemParam = 0x20;
    }

    public Item[] GetItemsOfCurrentPreset()
    {
        return GetItemsOfPreset(CurrentSpawnPreset);
    }

    public Item[] GetItemsOfPreset(BulkSpawnPreset preset, byte flag0 = 0x20)
    {
        List<Item> toRet = new List<Item>();
        switch(preset)
        {
            case BulkSpawnPreset.Music:
                toRet.AddRange(GetItemsOfKind(Kind_Music));
                break;
            case BulkSpawnPreset.DIYRecipesAlphabetical:
                toRet.AddRange(GetDIYRecipes());
                break;
            case BulkSpawnPreset.DIYRecipesSequential:
                toRet.AddRange(GetDIYRecipes(false));
                break;
            case BulkSpawnPreset.Fossils:
                toRet.AddRange(GetItemsOfKind(Kind_Fossil));
                break;
            case BulkSpawnPreset.GenericMaterials:
                toRet.AddRange(GetItemsOfKind(Kind_Ore, Kind_CraftMaterial));
                break;
            case BulkSpawnPreset.SeasonalMaterials:
                toRet.AddRange(GetItemsOfKind(Kind_Vegetable, Kind_Sakurapetal, Kind_ShellDrift, Kind_TreeSeedling, Kind_CraftMaterial, Kind_Mushroom, Kind_AutumnLeaf, Kind_SnowCrystal));
                break;
            case BulkSpawnPreset.RealArt:
                toRet.AddRange(GetItemsOfKind(Kind_Picture, Kind_Sculpture));
                break;
            case BulkSpawnPreset.FakeArt:
                toRet.AddRange(GetItemsOfKind(Kind_PictureFake, Kind_SculptureFake));
                break;
            case BulkSpawnPreset.Bugs:
                toRet.AddRange(GetItemsOfKind(Kind_Insect));
                break;
            case BulkSpawnPreset.Fish:
                toRet.AddRange(GetItemsOfKind(Kind_Fish, Kind_ShellFish, Kind_DiveFish));
                break;
            case BulkSpawnPreset.BugsAndFish:
                toRet.AddRange(GetItemsOfKind(Kind_Fish, Kind_ShellFish, Kind_DiveFish));
                toRet.AddRange(GetItemsOfKind(Kind_Insect));
                break;
            case BulkSpawnPreset.InventoryOfApp:
                toRet.AddRange(GetInventoryClone());
                break;
            case BulkSpawnPreset.CustomFile:
                toRet.AddRange(fileLoadedItems);
                break;
            default:
                toRet.Add(new Item(0x09C4)); // tree branch
                break;

        }

        if (preset != BulkSpawnPreset.CustomFile)
        {
            foreach (Item i in toRet)
            {
                i.SystemParam = flag0;

                // try stacking to max
                var kind = ItemInfo.GetItemKind(i);
                if (kind != Kind_DIYRecipe && kind != Kind_MessageBottle && kind != Kind_Fossil)
                    if (ItemInfo.TryGetMaxStackCount(i, out var max))
                        i.Count = --max;
            }
        }

        int mul = Multiplier;
        if (mul != 1)
        {
            List<Item> multipliedItemList = new List<Item>();
            foreach (var item in toRet)
                for (int i = 0; i < mul; ++i)
                    multipliedItemList.Add(item); // references are fine, should be copied from
            toRet = multipliedItemList;
        }

        return toRet.ToArray();
    }

    private Item[] GetItemsOfKind(params ItemKind[] ik)
    {
        var toRet = new List<ushort>();
        foreach (var kind in ik)
        {
            toRet.AddRange(GetAllItems().Where(x => ItemInfo.GetItemKind(x) == kind));
        }

        var asItems = new Item[toRet.Count];
        for (int i = 0; i < toRet.Count; ++i)
            asItems[i] = new Item(toRet[i]);

        return asItems;
    }

    private Item[] GetDIYRecipes(bool alphabetical = true)
    {
        var recipes = RecipeList.Recipes;
        var retRecipes = new List<Item>();
        foreach (var recipe in recipes)
        {
            var itemRecipe = new Item(Item.DIYRecipe);
            itemRecipe.Count = recipe.Key;
            retRecipes.Add(itemRecipe);
        }
        if (alphabetical)
        {
            var ordered = retRecipes.OrderBy(x => getRecipeName(x.Count, recipes));
            retRecipes = ordered.ToList();
        }
        return retRecipes.ToArray();
    }

    private Item[] GetInventoryClone()
    {
        var invItems = UI_ACItemGrid.LastInstanceOfItemGrid.Items.ToArray();
        var cloneArray = UI_MapTerrain.CloneItemArray(invItems);
        TrimTrailingNoItems(ref cloneArray, Item.NONE);
        return cloneArray;
    }

    private string getRecipeName(ushort count, IReadOnlyDictionary<ushort, ushort> recipes)
    {
        var currentRecipeItem = recipes[count];
        return GameInfo.Strings.itemlistdisplay[currentRecipeItem].ToLower();
    }

    public static void TrimTrailingNoItems(ref Item[] buffer, ushort trimValue)
    {
        int i = buffer.Length;
        while (i > 0 && buffer[--i].ItemId == trimValue)
        {
            ; // no-op by design
        }
        Array.Resize(ref buffer, i + 1);
        return;
    }
}
