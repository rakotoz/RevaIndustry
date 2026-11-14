using revindustry.Gui;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace revindustry.BlockEntities;

public class BEWireDrawing : BlockEntityOpenableContainer {
    private readonly InventoryGeneric _inventory;

    public override InventoryBase Inventory => _inventory;
    public override string InventoryClassName => "revwiredrawing";

    public BEWireDrawing() {
        _inventory = new InventoryGeneric(2, null, null,
            (id, inv) => id == 1 ? new ItemSlotOutput(inv) : new ItemSlot(inv));
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel) {
        if (Api.Side == EnumAppSide.Client) {
            toggleInventoryDialogClient(byPlayer, () => new GuiDialogWireDrawing(
                Lang.Get("revindustry:wiredrawing-title"), Inventory, Pos, (ICoreClientAPI)Api));
        }

        return true;
    }
}
