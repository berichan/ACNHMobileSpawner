using Beri.Drawing;

namespace NHSE.Core
{
    public static class FieldItemColor
    {
        public static Color GetItemColor(Item item)
        {
            if (item.DisplayItemId >= Item.FieldItemMin)
                return GetItemColor60000(item);
            if (item.DisplayItemId > 13930)
            {
                //UnityEngine.Debug.Log(item.DisplayItemId);
                return Color.White;
            }
            var kind = ItemInfo.GetItemKind(item);
            return ColorUtil.GetColor((int)kind);
        }

        private static Color GetItemColor60000(Item item)
        {
            var id = item.DisplayItemId;
            if (id == Item.NONE)
                return Color.Transparent;

            if (!FieldItemList.Items.TryGetValue(id, out var def))
                return Color.DarkGreen;

            var kind = def.Kind;
            if (kind.IsTree())
                return GetTreeColor(id);
            if (kind.IsFlower())
                return Color.HotPink;
            if (kind.IsWeed())
                return Color.DarkOliveGreen;
            if (kind.IsFence())
                return Color.LightCoral;
            if (kind == FieldItemKind.UnitIconHole)
                return Color.Black;
            if (kind.IsBush())
                return Color.LightGreen;
            if (kind.IsStone())
                return Color.LightGray;

            return Color.DarkGreen; // shouldn't reach here, but ok
        }

        private static Color GetTreeColor(ushort id)
        {
            if (0xEC9C <= id && id <= 0xECA0) // money tree
                return Color.Gold;

            Color toRet;

            switch (id)
            {
                // Fruit
                case 0xEA61 : toRet = Color.Red; break;       // "PltTreeApple"
                case 0xEA62 : toRet = Color.Orange; break;    // "PltTreeOrange"
                case 0xEAC8 : toRet = Color.Lime; break;      // "PltTreePear"
                case 0xEAC9 : toRet = Color.DarkRed; break;   // "PltTreeCherry"
                case 0xEACA : toRet = Color.PeachPuff; break; // "PltTreePeach"

                // Cedar
                case 0xEA69 : toRet = Color.SaddleBrown; break; // "PltTreeCedar4"
                case 0xEAB6 : toRet = Color.SaddleBrown; break; // "PltTreeCedar2"
                case 0xEAB7 : toRet = Color.SaddleBrown; break; // "PltTreeCedar1"
                case 0xEAB8 : toRet = Color.SaddleBrown; break; // "PltTreeCedar3"

                // Palm
                case 0xEA77 : toRet = Color.LightGoldenrodYellow; break; // "PltTreePalm4"
                case 0xEAC0 : toRet = Color.LightGoldenrodYellow; break; // "PltTreePalm2"
                case 0xEAC1 : toRet = Color.LightGoldenrodYellow; break; // "PltTreePalm1"
                case 0xEAC2 : toRet = Color.LightGoldenrodYellow; break; // "PltTreePalm3"

                case 0xEA76 : toRet = Color.MediumSeaGreen; break; // "PltTreeBamboo4"
                case 0xEAC4 : toRet = Color.MediumSeaGreen; break; // "PltTreeBamboo0"
                case 0xEAC5 : toRet = Color.MediumSeaGreen; break; // "PltTreeBamboo2"
                case 0xEAC6 : toRet = Color.MediumSeaGreen; break; // "PltTreeBamboo1"
                case 0xEAC7 : toRet = Color.MediumSeaGreen; break; // "PltTreeBamboo3"

                default: toRet = Color.SandyBrown; break;
            };

            return toRet;
        }
    }
}
