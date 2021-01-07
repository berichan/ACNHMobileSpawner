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
        DIYRecipes,
        GenericMaterials,
        SeasonalMaterials,
        RealArt,
        FakeArt,
        Bugs,
        Fish,
        BugsAndFish,
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

    public int Multiplier => int.Parse(ItemMultiplier.text);
    public int RectWidthDimension => int.Parse(RectWidth.text);
    public int RectHeightDimension => int.Parse(RectHeight.text);

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
        BulkSpawnPresetMode.value = 0;
        BulkSpawnPresetMode.RefreshShownValue();
        BulkSpawnPresetMode.onValueChanged.AddListener(delegate { CurrentSpawnPreset = (BulkSpawnPreset)BulkSpawnPresetMode.value; });

        SpawnDir.ClearOptions();
        smChoices = new string[4] { "South-east ↘", "South-west ↙", "North-west ↖", "North-east ↗" };
        foreach (string sm in smChoices)
        {
            Dropdown.OptionData newVal = new Dropdown.OptionData();
            newVal.text = sm;
            SpawnDir.options.Add(newVal);
        }
        SpawnDir.value = 0;
        SpawnDir.RefreshShownValue();
        SpawnDir.onValueChanged.AddListener(delegate { CurrentSpawnDirection = (SpawnDirection)SpawnDir.value; });
    }

    public Item[] GetItemsOfCurrentPreset()
    {
        return GetItemsOfPreset(CurrentSpawnPreset);
    }

    public Item[] GetItemsOfPreset(BulkSpawnPreset preset)
    {
        List<Item> toRet = new List<Item>();
        switch(preset)
        {
            case BulkSpawnPreset.Music:
                toRet.AddRange(GetItemsOfKind(Kind_Music));
                break;
            case BulkSpawnPreset.DIYRecipes:
                toRet.AddRange(GetDIYRecipes());
                break;
            case BulkSpawnPreset.GenericMaterials:
                toRet.AddRange(GetItemsOfKind(Kind_Ore, Kind_CraftMaterial));
                break;
            case BulkSpawnPreset.SeasonalMaterials:
                toRet.AddRange(GetItemsOfKind(Kind_Vegetable, Kind_Sakurapetal, Kind_ShellDrift, Kind_TreeSeedling, Kind_CraftMaterial, Kind_Mushroom, Kind_AutumnLeaf, Kind_SnowCrystal));
                break;
            case BulkSpawnPreset.RealArt:
                toRet.AddRange(GetItemsOfKind(Kind_Picture));
                break;
            case BulkSpawnPreset.FakeArt:
                toRet.AddRange(GetItemsOfKind(Kind_PictureFake));
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
            default:
                toRet.Add(new Item(0x09C4)); // tree branch
                break;

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
            toRet.AddRange(allItems.Where(x => ItemInfo.GetItemKind(x) == kind));
        }

        var asItems = new Item[toRet.Count];
        for (int i = 0; i < toRet.Count; ++i)
            asItems[i] = new Item(toRet[i]);

        return asItems;
    }

    private Item[] GetDIYRecipes()
    {
        var recipes = RecipeList.Recipes;
        var retRecipes = new List<Item>();
        foreach (var recipe in recipes)
        {
            var itemRecipe = new Item(Item.DIYRecipe);
            itemRecipe.Count = recipe.Key;
            retRecipes.Add(itemRecipe);
        }
        retRecipes.OrderBy(x => getRecipeName(x.ItemId, recipes)[0]);
        return retRecipes.ToArray();
    }

    private string getRecipeName(ushort count, IReadOnlyDictionary<ushort, ushort> recipes)
    {
        var currentRecipeItem = recipes[count];
        return GameInfo.Strings.itemlistdisplay[currentRecipeItem];
    }
}
