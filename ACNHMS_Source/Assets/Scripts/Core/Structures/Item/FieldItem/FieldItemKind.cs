﻿using static NHSE.Core.FieldItemKind;

namespace NHSE.Core
{
    public enum FieldItemKind : byte
    {
        FenceBamboo,
        FenceBarbedWire,
        FenceChinese,
        FenceConcreteBlock,
        FenceCorrugatedIron,
        FenceCrossedBamboo,
        FenceDriedStraw,
        FenceEasterEgg,
        FenceGardenPegRope,
        FenceHalloween,
        FenceHorizontalLog,
        FenceHorizontalWood,
        FenceIce,
        FenceIkegaki,
        FenceIronAndStone,
        FenceJapanese,
        FenceJuneBride,
        FenceLattice,
        FenceLatticeBig,
        FenceLog,
        FenceLogWall,
        FenceMermaid,
        FencePark,
        FencePegRope,
        FenceSandProtection,
        FenceSharply,
        FenceSteel,
        FenceStone,
        FenceVerticalWood,
        FenceWallRenga,
        FenceWoodWhite,
        LadderKitA,
        LadderKitB,
        LadderKitC,
        LadderKitD,
        PltBushAzalea,
        PltBushCamellia,
        PltBushHibiscus,
        PltBushHolly,
        PltBushHydrangea,
        PltBushOsmanthus,
        PltBushPlumeria,
        PltFlwAnemone,
        PltFlwCosmos,
        PltFlwHyacinth,
        PltFlwLily,
        PltFlwMum,
        PltFlwPansy,
        PltFlwRose,
        PltFlwRoseGold,
        PltFlwTulip,
        PltFlwYuri,
        PltTreeBamboo,
        PltTreeCedar,
        PltTreeCedarDeco,
        PltTreeOak,
        PltTreePalm,
        PltVgtCarrot,
        PltVgtPotato,
        PltVgtPumpkin,
        PltVgtSugarcane,
        PltVgtTomato,
        PltVgtWheat,
        PltVine,
        PltWeedAut0,
        PltWeedAut1,
        PltWeedAut2,
        PltWeedLight,
        PltWeedSmr,
        PltWeedSpr,
        PltWeedWin0,
        PltWeedWin1,
        StoneA,
        StoneB,
        StoneC,
        StoneD,
        StoneE,
        UnitIconHole,
    }

    public static class FieldItemKindExtensions
    {
        public static bool IsWeed(this FieldItemKind type) => PltWeedAut0 <= type && type <= PltWeedWin1;
        public static bool IsPlant(this FieldItemKind type) => PltFlwAnemone <= type && type <= PltWeedWin1;
        public static bool IsFence(this FieldItemKind type) => FenceBamboo <= type && type <= FenceWoodWhite;
        public static bool IsBush(this FieldItemKind type) => PltBushAzalea <= type && type <= PltBushOsmanthus;
        public static bool IsFlower(this FieldItemKind type) => PltFlwAnemone <= type && type <= PltFlwYuri;
        public static bool IsTree(this FieldItemKind type) => PltTreeBamboo <= type && type <= PltTreePalm;
        public static bool IsStone(this FieldItemKind type) => StoneA <= type && type <= StoneE;

        public static ItemKind ToItemKind(this FieldItemKind type)
        {
            if (type.IsTree())
                return ItemKind.Kind_Tree;
            if (type.IsFlower())
                return ItemKind.Kind_Flower;
            if (type.IsWeed())
                return ItemKind.Kind_Weed;
            return ItemKind.Unknown;
        }
    }
}
