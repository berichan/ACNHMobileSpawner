using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OffsetHelper
{
    // some helpers
    public const ulong PlayerSize = 0x10E500;
    public const ulong PlayerOtherStartPadding = 0x36A50;

    // player other 
    public const ulong InventoryOffset = 0xAE19C778; // player 0 (A) done
    private const ulong playerOtherStart = InventoryOffset - 0x10; // helps to get other values, unused 

    public const ulong WalletAddress = InventoryOffset + 0xB8;
    public const ulong MilesAddress = InventoryOffset - 0x25590;
    public const ulong BankAddress = InventoryOffset + 0x224CC;

    // main player offsets functions
    private static ulong getPlayerStart(ulong invOffset) => invOffset - 0x10 - PlayerOtherStartPadding + 0x110;
    public static ulong getPlayerIdAddress(ulong invOffset) => getPlayerStart(invOffset) + 0xAFA8;
    public static ulong getPlayerProfileMainAddress(ulong invOffset) => getPlayerStart(invOffset) + 0x116A0;
    public static ulong getManpu(ulong invOffset) => invOffset - 0x10 + 0xAF7C + 72;
    public static ulong getTownNameAddress(ulong invOffset) => getPlayerIdAddress(invOffset) - 0xB8 + 0x04;

    // main save offsets
    public const ulong TurnipAddress = 0xAD195B74; // done
    public const ulong VillagerAddress = TurnipAddress - 0x2cb0 - 0x43be1c + 0x10 - 0x90; // done
    public const ulong VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x43be1c + 0x43abd4 - 0x90; // done
    public const ulong BackupSaveDiff = 0x849C50;

    public const ulong FieldItemStart = VillagerAddress - 0x10 + 0x22e1a8; // done
    public const ulong LandMakingMapStart = FieldItemStart + 0xAAA00; // done
    public const ulong OutsideFieldStart = FieldItemStart + 0xCF998; // done
    public const ulong MainFieldStructurStart = FieldItemStart + 0xCF600; // done

    // other addresses
    public const ulong ArriverNameLocAddress = 0xB66F8208;
    public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;

    public const ulong TextSpeedAddress = 0xBA88BC8; // done
                                                     //public const ulong ChatBufferAddressEUen = 0x42F216C8;
                                                     //public const ulong ChatBufferAddressUSen = ChatBufferAddressEUen - 0x80;
    public const ulong ChatBufferSize = 0x1E;

    public const ulong DodoAddress = 0x09F52E3C;
    public const ulong OnlineSessionAddress = 0x920C740; // done
    public const ulong OnlineSessionVisitorAddress = 0x9D2FFB0;
    public const ulong OnlineSessionVisitorSize = 0x448; // reverse order

    // pointers
    public static readonly long[] PlayerCoordJumps = new long[5] { 0x39DC030L, 0x18L, 0x178L, 0xD0L, 0xD8L }; // done
    public static readonly long[] ChatCoordJumps = new long[2] { 0x03C3EED0L, 0x40L };
}
