using System.Collections.Generic;
using UnityEngine;
using NHSE.Core;
using System.Linq;
using static NHSE.Core.ItemKind;

public class UI_MapBulkSpawn : MonoBehaviour
{
    public enum BulkSpawnPreset
    {
        DIYRecipes,
        Materials,
        RealArt,
        FakeArt,
        Bugs,
        Fish,
        BugsAndFish,
    }

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Item[] GetItemsOfPreset(BulkSpawnPreset preset)
    {
        List<Item> toRet = new List<Item>();
        switch(preset)
        {
            case BulkSpawnPreset.DIYRecipes:
                toRet.AddRange(GetDIYRecipes());
                break;
            case BulkSpawnPreset.Materials:
                toRet.AddRange(GetItemsOfKind(Kind_Ore, Kind_CraftMaterial));
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
