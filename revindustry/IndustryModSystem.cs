using revindustry.BlockEntities;
using Vintagestory.API.Common;

namespace revindustry;

public class IndustryModSystem : ModSystem {
    public override void Start(ICoreAPI api) {
        api.Logger.Notification("[revindustry] Mod loaded on: " + api.Side);

        api.RegisterBlockClass("RevWireDrawing", typeof(BlockWireDrawing));
        api.RegisterBlockEntityClass("RevWireDrawing", typeof(BEWireDrawing));
    }
}
