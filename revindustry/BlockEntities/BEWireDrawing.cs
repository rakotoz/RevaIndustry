using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace revindustry.BlockEntities;

public class BEWireDrawing : BlockEntity {
    public int pulls;

    public bool OnInteract(IPlayer byPlayer) {
        if (Api.Side == EnumAppSide.Server) {
            pulls++;
            MarkDirty(true);
            (byPlayer as IServerPlayer)?.SendMessage(GlobalConstants.GeneralChatGroup, "Протяжек: " + pulls,
                EnumChatType.Notification);
        }

        return true;
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetInt("pulls", pulls);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        base.FromTreeAttributes(tree, worldForResolving);
        pulls = tree.GetInt("pulls");
    }
}
