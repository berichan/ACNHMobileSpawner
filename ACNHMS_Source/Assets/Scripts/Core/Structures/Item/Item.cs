﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NHSE.Core
{
    [StructLayout(LayoutKind.Explicit, Size = SIZE, Pack = 1)]
    public class Item : ICopyableItem<Item>
    {
        public static readonly Item NO_ITEM = new Item {ItemId = NONE};
        public const ushort NONE = 0xFFFE;
        public const ushort EXTENSION = 0xFFFD;
        public const ushort FieldItemMin = 60_000;
        public const ushort LLOYD = 63_005;

        public const ushort MessageBottle = 0x16A1;
        public const ushort DIYRecipe = 0x16A2;
        public const ushort MessageBottleEgg = 0x3100;
        public const int SIZE = 8;

        [field: FieldOffset(0)] public ulong RawValue { get; set; }

        [field: FieldOffset(0)] public ushort ItemId { get; set; }
        [field: FieldOffset(2)] public byte SystemParam { get; set; }
        [field: FieldOffset(3)] public byte AdditionalParam { get; set; }
        [field: FieldOffset(4)] public int FreeParam { get; set; }

        public int Rotation => SystemParam & 3;
        public bool IsBuried => (SystemParam & 4) != 0;
        public bool Is_08 => (SystemParam & 0x08) != 0;
        public bool Is_10 => (SystemParam & 0x10) != 0;
        public bool IsDropped => (SystemParam & 0x20) != 0;
        public bool Is_40 => (SystemParam & 0x40) != 0;
        public bool Is_80 => (SystemParam & 0x80) != 0;

        #region Flag1 (Wrapping / Etc)
        public bool IsWrapped
        {
            get
            {
                if (AdditionalParam == 0)
                    return false;
                var id = DisplayItemId;
                return id != MessageBottle && id != MessageBottleEgg;
            }
        }

        public ItemWrapping WrappingType
        {
            get => (ItemWrapping) (AdditionalParam & 3);
            set => AdditionalParam = (byte)((AdditionalParam & ~3) | ((byte)value & 3));
        }

        public ItemWrappingPaper WrappingPaper
        {
            get => (ItemWrappingPaper) ((AdditionalParam >> 2) & 0xF);
            set => AdditionalParam = (byte)((AdditionalParam & 3) | ((byte)value & 0xF) << 2);
        }

        public void SetWrapping(ItemWrapping wrap, ItemWrappingPaper color, bool showItem = false, bool item80 = false)
        {
            if (wrap == ItemWrapping.Nothing || wrap > ItemWrapping.Delivery)
            {
                AdditionalParam = 0;
                return;
            }
            WrappingType = wrap;
            WrappingPaper = wrap == ItemWrapping.WrappingPaper ? color : 0;
            WrappingShowItem = showItem;
            Wrapping80 = item80;
        }

        public bool WrappingShowItem
        {
            get => (AdditionalParam & 0x40) != 0;
            set => AdditionalParam = (byte)((AdditionalParam & ~0x40) | (value ? 1 << 6 : 0));
        }

        public bool Wrapping80
        {
            get => (AdditionalParam & 0x80) != 0;
            set => AdditionalParam = (byte)((AdditionalParam & ~0x80) | (value ? 1 << 7 : 0));
        }
        #endregion

        #region Stackable Items
        [field: FieldOffset(4)] public ushort Count { get; set; }
        [field: FieldOffset(6)] public ushort UseCount { get; set; }
        #endregion

        #region Customizable Items
        public int BodyType
        {
            get => FreeParam & 7;
            set => FreeParam = (FreeParam & ~7) | (value & 7);
        }

        public int PatternSource // see RemakeDesignSource
        {
            get => (FreeParam >> 3) & 3;
            set => FreeParam = (FreeParam & ~0x18) | ((value & 3) << 3);
        }

        public int PatternChoice
        {
            get => FreeParam >> 5;
            set => FreeParam = (FreeParam & 0x1F) | ((value & 0x7FF) << 5);
        }
        #endregion

        #region Flowers

        // flowers only -- this is really just a packed u32
        [field: FieldOffset(4)] public FlowerGene Genes { get; set; }
        [field: FieldOffset(5)] private ushort Watered { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        [field: FieldOffset(7)] private byte Watered2 { get; set; }
        // offset 7 is unused bits

        public int DaysWatered
        {
            get => Watered & 0x1F;
            set => Watered = (ushort)((Watered & 0xFFE0) | (value & 0x1F));
        }

        public bool GetIsWateredByVisitor(int visitor)
        {
            if ((uint)visitor >= 10)
                throw new ArgumentOutOfRangeException(nameof(visitor));

            var shift = 5 + visitor;
            return (Watered & (1 << shift)) != 0;
        }

        public void SetIsWateredByVisitor(int visitor, bool value = true)
        {
            if ((uint)visitor >= 10)
                throw new ArgumentOutOfRangeException(nameof(visitor));

            var shift = 5 + visitor;
            var bit = (1 << shift);
            var mask = ~bit;

            Watered = (ushort)((Watered & mask) | (value ? bit : 0));
        }

        public bool IsWatered
        {
            get => ((Watered >> 15) & 1) == 1;
            set => Watered = (ushort)((Watered & 0x7FFF) | (value ? 0x8000 : 0));
        }

        public bool IsWateredGold
        {
            get => (Watered2 & 1) == 1;
            set => Watered2 = (byte)((Watered2 & 0xFE) | (value ? 1 : 0));
        }

        public void Water(bool all = false)
        {
            if (all)
            {
                Watered = 0xFFFF;
                Watered2 = 1;
                return;
            }

            DaysWatered = 31;
            IsWatered = true;
        }

        #endregion

        #region Item Extensions
        public ushort DisplayItemId => IsExtension ? ExtensionItemId : ItemId;
        public bool IsNone => ItemId == NONE;
        public bool IsExtension => ItemId == EXTENSION;
        public bool IsRoot => ItemId < EXTENSION;
        public bool IsFieldItem => IsRoot && ItemId >= 60_000;
        [field: FieldOffset(4)] public ushort ExtensionItemId { get; set; }
        [field: FieldOffset(6)] public byte ExtensionX { get; set; }
        [field: FieldOffset(7)] public byte ExtensionY { get; set; }

        public void SetAsExtension(Item tile, byte x, byte y)
        {
            ItemId = EXTENSION;
            SystemParam = 0;
            AdditionalParam = 0;
            ExtensionX = x;
            ExtensionY = y;
            ExtensionItemId = tile.ItemId;
        }
        #endregion

        public Item() { } // marshalling

        public Item(ushort itemId = NONE)
        {
            ItemId = itemId;
        }

        public Item(ulong raw)
        {
            RawValue = raw;
        }

        public void Delete()
        {
            ItemId = NONE;
            SystemParam = AdditionalParam = 0;
            FreeParam = 0;
        }

        public virtual int Size => SIZE;

        public void CopyFrom(Item item)
        {
            ItemId = item.ItemId;
            SystemParam = item.SystemParam;
            AdditionalParam = item.AdditionalParam;
            FreeParam = item.FreeParam;
        }

        public static Item[] GetArray(byte[] data) => data.GetArray<Item>(SIZE);
        public static byte[] SetArray(IReadOnlyList<Item> data) => data.SetArray(SIZE);

        public ushort GetWrappedItemName()
        {
            ushort returnValue = ItemId;
            switch (WrappingType)
            {
                case ItemWrapping.WrappingPaper:
                    returnValue = (ushort)(0x1E13 + (ushort)WrappingPaper);
                    break;
                case ItemWrapping.Present:
                    returnValue = 0x1180;
                    break;
                case ItemWrapping.Delivery:
                    returnValue = 0x1225;
                    break;
                default:
                    returnValue = ItemId;
                    break;
            }
            return returnValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDifferentTo(Item i) // doesn't look like it, but fastest for map acre comparison for writes
        {
            if (ItemId != i.ItemId)
                return true;
            if (SystemParam != i.SystemParam)
                return true;
            if (AdditionalParam != i.AdditionalParam)
                return true;
            if (FreeParam != i.FreeParam)
                return true;

            return false;
        }
    }
}
