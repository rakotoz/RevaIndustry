using System;
using revindustry.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace revindustry;

public class BlockHandCrankedMachine : BlockMPBase {
    private const int CrankBoxIndex = 1;
    private static readonly string[] Sides = { "north", "east", "south", "west" };

    public BlockFacing? ShaftFace { get; private set; }

    public override void OnLoaded(ICoreAPI api) {
        base.OnLoaded(api);

        string? face = Attributes?["shaftFace"].AsString();
        if (face == null) {
            return;
        }

        BlockFacing facing = BlockFacing.FromCode(face);
        int turns = Math.Max(0, Array.IndexOf(Sides, Variant["side"]));
        for (int i = 0; i < turns; i++) {
            facing = facing.GetCW();
        }
        ShaftFace = facing;
    }

    public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face,
        BlockMPBase forBlock) {
        return face == ShaftFace;
    }

    public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face) {
    }

    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack,
        BlockSelection blockSel, ref string failureCode) {
        if (!base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode)) {
            return false;
        }

        if (world.BlockAccessor.GetBlock(blockSel.Position) is BlockHandCrankedMachine { ShaftFace: { } face } placed) {
            placed.tryConnect(world, byPlayer, blockSel.Position, face);
        }
        return true;
    }

    public bool IsShaftAttached(IWorldAccessor world, BlockPos pos) {
        if (ShaftFace == null) {
            return false;
        }

        BlockPos shaftPos = pos.AddCopy(ShaftFace);
        return world.BlockAccessor.GetBlock(shaftPos) is IMechanicalPowerBlock shaft
               && shaft.HasMechPowerConnectorAt(world, shaftPos, ShaftFace.Opposite, this);
    }

    public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos) {
        if (world.BlockAccessor.GetBlockEntity(pos) is BEHandCrankedMachine { IsBuilt: true } machine) {
            return machine.Title;
        }
        return base.GetPlacedBlockName(world, pos);
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BEHandCrankedMachine machine) {
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        if (!machine.IsBuilt) {
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        if (blockSel.SelectionBoxIndex == CrankBoxIndex && !machine.ShaftAttached) {
            machine.Crank();
            return true;
        }

        return machine.OnPlayerRightClick(byPlayer, blockSel);
    }

    public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer,
        BlockSelection blockSel) {
        if (blockSel.SelectionBoxIndex == CrankBoxIndex
            && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHandCrankedMachine
                { IsBuilt: true, ShaftAttached: false } machine) {
            machine.Crank();
            return true;
        }
        return false;
    }
}
