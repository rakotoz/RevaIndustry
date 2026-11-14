using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace revindustry.Gui;

public class GuiDialogWireDrawing : GuiDialogBlockEntity {
    public GuiDialogWireDrawing(string title, InventoryBase inventory, BlockPos pos, ICoreClientAPI capi) : base(title,
        inventory, pos, capi) {
        if (IsDuplicate) {
            return;
        }

        SetupDialog();
    }

    private void SetupDialog() {
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(EnumDialogArea.RightMiddle)
            .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

        ElementBounds inputBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 30, 1, 1);
        ElementBounds outputBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 120, 30, 1, 1);

        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(inputBounds, outputBounds);

        SingleComposer = capi.Gui.CreateCompo("revwiredrawing-" + BlockEntityPosition, dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
            .BeginChildElements(bgBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 1, new[] { 0 }, inputBounds, "inputSlot")
            .AddItemSlotGrid(Inventory, DoSendPacket, 1, new[] { 1 }, outputBounds, "outputSlot")
            .EndChildElements()
            .Compose();
    }

    private void OnTitleBarClose() {
        TryClose();
    }
}
