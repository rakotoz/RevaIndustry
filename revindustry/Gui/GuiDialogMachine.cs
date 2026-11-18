using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace revindustry.Gui;

public class GuiDialogMachine : GuiDialogBlockEntity {
    protected static readonly double[] ProgressColor = { 0.85, 0.55, 0.25, 1 };

    public GuiDialogMachine(string title, InventoryBase inventory, BlockPos pos, ICoreClientAPI capi)
        : base(title, inventory, pos, capi) {
        if (IsDuplicate) {
            return;
        }
        SetupDialog();
    }

    protected virtual void SetupDialog() {
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
            .WithAlignment(EnumDialogArea.RightMiddle)
            .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

        ElementBounds inputBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 30, 1, 1);
        ElementBounds outputBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 176, 30, 1, 1);
        ElementBounds progressBounds = ElementBounds.Fixed(64, 52, 100, 10);

        ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
        bgBounds.BothSizing = ElementSizing.FitToChildren;
        bgBounds.WithChildren(inputBounds, outputBounds, progressBounds);

        GuiComposer composer = capi.Gui.CreateCompo("revmachine-" + BlockEntityPosition, dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
            .BeginChildElements(bgBounds)
            .AddItemSlotGrid(Inventory, DoSendPacket, 1, new[] { 0 }, inputBounds, "inputSlot")
            .AddItemSlotGrid(Inventory, DoSendPacket, 1, new[] { 1 }, outputBounds, "outputSlot")
            .AddStatbar(progressBounds, ProgressColor, "progress");
        ComposeExtras(composer, bgBounds);
        SingleComposer = composer.EndChildElements().Compose();
    }

    protected virtual void ComposeExtras(GuiComposer composer, ElementBounds bgBounds) {
    }

    protected void OnTitleBarClose() {
        TryClose();
    }

    public void UpdateProgress(float progress, float workTime) {
        SingleComposer?.GetStatbar("progress")?.SetValues(progress, 0, workTime > 0 ? workTime : 1);
    }
}
