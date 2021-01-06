using System.Collections;
using System.Collections.Generic;
using NHSE.Core;

public class FieldItemBlock
{
    private readonly FieldItemLayer Layer;
    private readonly int X, Y;

    public Item SelectedItem { get => Layer.GetTile(X, Y); }

    public FieldItemBlock(FieldItemLayer layer, int x, int y)
    {
        Layer = layer;
        X = x; Y = y;
    }

    public void UpdateItem(Item refItem)
    {
        if (refItem == null)
            return;
        if (!refItem.IsNone)
        {
            SelectedItem.CopyFrom(refItem);
            Layer.SetExtensionTiles(SelectedItem, X, Y);
        }
        else
        {
            SelectedItem.Delete();
            Layer.DeleteExtensionTiles(SelectedItem, X, Y);
        }
    }
}
