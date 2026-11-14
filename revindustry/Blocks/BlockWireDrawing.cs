using revindustry.BlockEntities;
using Vintagestory.API.Common;

namespace revindustry;

public class BlockWireDrawing: Block {
    private const int CrankBoxIndex = 1;
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BEWireDrawing be) {
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        if (blockSel.SelectionBoxIndex == CrankBoxIndex) {
            be.Crank();
            return true;
        }

        return be.OnPlayerRightClick(byPlayer, blockSel);
    }

    public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (blockSel.SelectionBoxIndex == CrankBoxIndex
            && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWireDrawing be) {
            be.Crank();
            return true;
        }
        return false;
    }
}
