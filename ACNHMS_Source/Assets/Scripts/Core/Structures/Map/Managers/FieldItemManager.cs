using System.Collections.Generic;
using System.Diagnostics;

namespace NHSE.Core
{
    /// <summary>
    /// Manages the <see cref="Item"/> data for the player's outside overworld.
    /// </summary>
    public class FieldItemManager
    {
        /// <summary>
        /// Base layer of items
        /// </summary>
        public readonly FieldItemLayer Layer1;

        /// <summary>
        /// Layer of items that are supported by <see cref="Layer1"/>
        /// </summary>
        public readonly FieldItemLayer Layer2;

        public FieldItemManager(Item[] l1, Item[] l2)
        {
            Layer1 = new FieldItemLayer(l1);
            Layer2 = new FieldItemLayer(l2);
        }

        /// <summary>
        /// Lists out all coordinates of tiles present in <see cref="Layer2"/> that don't have anything underneath in <see cref="Layer1"/> to support them.
        /// </summary>
        /// <returns></returns>
        public List<string> GetUnsupportedTiles()
        {
            var result = new List<string>();
            for (int x = 0; x < FieldItemLayer.FieldItemWidth; x++)
            {
                for (int y = 0; y < FieldItemLayer.FieldItemHeight; y++)
                {
                    var tile = Layer2.GetTile(x, y);
                    if (tile.IsNone)
                        continue;

                    var support = Layer1.GetTile(x, y);
                    if (!support.IsNone)
                        continue; // dunno how to check if the tile can actually have an item put on top of it...

                    result.Add($"{x:000},{y:000}");
                }
            }
            return result;
        }
        
        public bool IsOccupied(int x, int y) => !Layer1.GetTile(x, y).IsNone || !Layer2.GetTile(x, y).IsNone;
    }
}
