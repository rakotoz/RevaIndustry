using revindustry.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace revindustry;

public class BlockWireDrawing : Block {
    private const int CrankBoxIndex = 1;

    public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos) {
        if (world.BlockAccessor.GetBlockEntity(pos) is BEWireDrawing { IsBuilt: true }) {
            return Lang.Get("revindustry:wiredrawing-title");
        }
        return base.GetPlacedBlockName(world, pos);
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BEWireDrawing be) {
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        if (!be.IsBuilt) {
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        if (blockSel.SelectionBoxIndex == CrankBoxIndex) {
            be.Crank();
            return true;
        }

        return be.OnPlayerRightClick(byPlayer, blockSel);
    }

    public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer,
        BlockSelection blockSel) {
        if (blockSel.SelectionBoxIndex == CrankBoxIndex
            && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWireDrawing be && be.IsBuilt) {
            be.Crank();
            return true;
        }

        return false;
    }
}
