using revindustry.BlockEntities;
using Vintagestory.API.Common;

namespace revindustry;

public class BlockWireDrawing: Block {
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWireDrawing be) {
            return be.OnPlayerRightClick(byPlayer, blockSel);
        }
        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}
