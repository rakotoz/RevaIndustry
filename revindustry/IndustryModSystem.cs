using Vintagestory.API.Common;

namespace revindustry;

public class IndustryModSystem : ModSystem {
    public override void Start(ICoreAPI api) {
        api.Logger.Notification("[revindustry] Mod loaded on: " + api.Side);
    }
}
