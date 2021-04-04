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
    public const ulong InventoryOffset = 0xAE19C778; // player 0 (A) 
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
    public const ulong TurnipAddress = 0xAD195B74;
    public const ulong VillagerAddress = TurnipAddress - 0x2cb0 - 0x43be1c + 0x10 - 0x90;
    public const ulong VillagerHouseAddress = TurnipAddress - 0x2cb0 - 0x43be1c + 0x43abd4 - 0x90;
    public const ulong BackupSaveDiff = 0x86D580;

    public const ulong FieldItemStart = VillagerAddress - 0x10 + 0x22e1a8;
    public const ulong LandMakingMapStart = FieldItemStart + 0xAAA00;
    public const ulong OutsideFieldStart = FieldItemStart + 0xCF998;
    public const ulong MainFieldStructurStart = FieldItemStart + 0xCF600;

    // other addresses
    public const ulong ArriverNameLocAddress = 0xB6351EA0;
    public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;

    public const ulong TextSpeedAddress = 0xBA88BC8; 
    public const ulong ChatBufferSize = 0x1E;

    public const ulong DodoAddress = 0xA98D15C;
    public const ulong OnlineSessionAddress = 0x920C740;
    public const ulong OnlineSessionVisitorAddress = 0x9D3BFB0;
    public const ulong OnlineSessionVisitorSize = 0x448; // reverse order

    public const ulong TimeAddress = 0x0BA7FCF8;

    // pointers
    public static readonly long[] PlayerCoordJumps = new long[5] { 0x39DC030L, 0x18L, 0x178L, 0xD0L, 0xD8L };
    public static readonly long[] ChatCoordJumps = new long[2] { 0x03C3EED0L, 0x40L };

    // exefs (main). Thanks to Arch9sk7 for these
    public const ulong AnimationSpeedOffset = 0x037A41E8;
    public const ulong WalkSpeedOffset = 0x01115CE0;
    public const ulong CollisionOffset = 0x01084140;
    public const ulong TimeStateAddress = 0x002704C0;
}
