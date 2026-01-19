using NHSE.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OffsetHelper
{
    // some helpers
    public const ulong PlayerSize = 0x131F70;
    public const ulong PlayerOtherStartPadding = 0x37BE0;

    // player other 
    public const ulong InventoryOffset = 0xB27BB758; // player 0 (A) 
    private const ulong playerOtherStart = InventoryOffset - 0x10; // helps to get other values, unused 

    public const ulong WalletAddress = InventoryOffset + 0xB8;
    public const ulong MilesAddress = InventoryOffset - 0x25590;
    public const ulong BankAddress = InventoryOffset + 0x2D5D4;

    // main player offsets functions
    private static ulong getPlayerStart(ulong invOffset) => invOffset - 0x10 - PlayerOtherStartPadding + 0x110;
    public static ulong getPlayerIdAddress(ulong invOffset) => getPlayerStart(invOffset) + 0xC138;
    public static ulong getPlayerProfileMainAddress(ulong invOffset) => getPlayerStart(invOffset) + 0x12830;
    public static ulong getManpu(ulong invOffset) => invOffset - 0x10 + 0x12C7C + 72;
    public static ulong getTownNameAddress(ulong invOffset) => getPlayerIdAddress(invOffset) - 0xB8 + 0x04;

    // main save offsets
    public const ulong TurnipAddress = 0xB14DBB30;
    public const ulong VillagerAddress = TurnipAddress - 0x2d40 - 0x48d920 + 0x10;
    public const ulong VillagerHouseAddress = TurnipAddress - 0x2d40 - 0x48d920 + 0x481c10;
    public const ulong BackupSaveDiff = 0x9B0EB0; 

    private const ulong FieldItemStart = VillagerAddress - 0x10 + 0x22f3f0;
    private const ulong FieldBufferSize = MapGrid.AcreHeight * 32 * 32 * (MapGrid.AcreWidth + 1);
    public const ulong FieldSize = MapGrid.MapTileCount32x32 * Item.SIZE;
    public const ulong FieldItemStartLayer1 = FieldItemStart + FieldBufferSize;
    public const ulong FieldItemStartLayer2 = (FieldItemStart + FieldSize) + (FieldBufferSize * 3); // 2 for layer 1 + 1 buffer for this layer

    public const ulong LandMakingMapStart = FieldItemStart + 0xdb600;
    public const ulong OutsideFieldStart = FieldItemStart + 0x1005ac;
    public const ulong MainFieldStructurStart = FieldItemStart + 0x100200;

    // other addresses
    public const ulong ArriverNameLocAddress = 0xBACD2ED8;
    public const ulong ArriverVillageLocAddress = ArriverNameLocAddress - 0x1C;

    public const ulong TextSpeedAddress = 0xBD9A9FC;
    public const ulong ChatBufferSize = 0x1E;

    public const ulong DodoAddress = 0xAC1A164;
    public const ulong OnlineSessionAddress = 0x9499748;
    public const ulong OnlineSessionVisitorAddress = 0xA2CE644;
    public const ulong OnlineSessionVisitorSize = 0x78;

    public const ulong TimeAddress = 0x0BD91B00;

    // pointers
    public static readonly long[] PlayerCoordJumps = new long[5] { 0x4BF9E30L, 0x18L, 0x178L, 0xD0L, 0xD8L };
    public static readonly long[] ChatCoordJumps = new long[2] { 0x5254A40L, 0x40L };

    // exefs (main)
    public const ulong AnimationSpeedOffset = 0x043BC3C0; // Unknown for 3.0.0 at this time
    public const ulong WalkSpeedOffset = 0x02318428;
    public const ulong CollisionStateOffset = 0x02217B30;
    public const ulong TimeStateAddress = 0x00981B28;

    public const ulong ArriverVillageId = ArriverVillageLocAddress - 0x4;
    public const ulong ArriverNID = ArriverNameLocAddress - 0x1D688;

    // dlc
    public const ulong PokiAddress = 0xB449E6C8;
}
