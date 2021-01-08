using System;
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
        if (!layer.GetTile(x, y).IsNone)
        {
            if (!layer.GetTile(x, y).IsRoot)
                x--;
            if (!layer.GetTile(x, y).IsRoot)
            {
                x++;
                y--;
            }
            if (!layer.GetTile(x, y).IsRoot)
                x--;
            if (!layer.GetTile(x, y).IsRoot)
            {
                x++; y++;
            }
        }
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
            if (SelectedItem.IsExtension)
            {
                var tileToDelete = SelectedItem;
                var l = Layer;
                var rx = Math.Max(0, Math.Min(l.MaxWidth - 1, X - tileToDelete.ExtensionX));
                var ry = Math.Max(0, Math.Min(l.MaxHeight - 1, Y - tileToDelete.ExtensionY));
                var redir = l.GetTile(rx, ry);
                if (redir.IsRoot && redir.ItemId == tileToDelete.ExtensionItemId)
                    tileToDelete = redir;
                
                l.DeleteExtensionTiles(tileToDelete, rx, ry);
                tileToDelete.Delete();
            }
            else
            {
                Layer.DeleteExtensionTiles(SelectedItem, X, Y);
                SelectedItem.Delete();
            }
        }
    }
}
