using revindustry.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace revindustry;

public class BlockHandCrankedMachine : Block {
    private const int CrankBoxIndex = 1;

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

        if (blockSel.SelectionBoxIndex == CrankBoxIndex) {
            machine.Crank();
            return true;
        }

        return machine.OnPlayerRightClick(byPlayer, blockSel);
    }

    public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer,
        BlockSelection blockSel) {
        if (blockSel.SelectionBoxIndex == CrankBoxIndex
            && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEHandCrankedMachine { IsBuilt: true } machine) {
            machine.Crank();
            return true;
        }
        return false;
    }
}
