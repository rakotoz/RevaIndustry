using revindustry.BlockEntities;
using revindustry.Recipes;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace revindustry.Gui;

public class GuiDialogRollingMill : GuiDialogMachine {
    private const string SheetKey = "modeSheet";
    private const string RodKey = "modeRod";

    public GuiDialogRollingMill(string title, InventoryBase inventory, BlockPos pos, ICoreClientAPI capi)
        : base(title, inventory, pos, capi) {
    }

    protected override void ComposeExtras(GuiComposer composer, ElementBounds bgBounds) {
        ElementBounds sheetBounds = ElementBounds.Fixed(0, 100, 110, 26);
        ElementBounds rodBounds = ElementBounds.Fixed(114, 100, 110, 26);
        CairoFont font = CairoFont.SmallButtonText();
        bgBounds.WithChildren(sheetBounds, rodBounds);

        composer
            .AddToggleButton(Lang.Get("revindustry:rollingmill-mode-sheet"), font,
                on => OnModeToggle(on, RollingRecipe.ModeSheet), sheetBounds, SheetKey)
            .AddToggleButton(Lang.Get("revindustry:rollingmill-mode-rod"), font,
                on => OnModeToggle(on, RollingRecipe.ModeRod), rodBounds, RodKey);
    }

    private void OnModeToggle(bool on, string mode) {
        if (capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) is BERollingMill mill) {
            mill.RequestMode(mode);
        }
        UpdateMode(mode);
    }

    public void UpdateMode(string mode) {
        SingleComposer?.GetToggleButton(SheetKey)?.SetValue(mode == RollingRecipe.ModeSheet);
        SingleComposer?.GetToggleButton(RodKey)?.SetValue(mode == RollingRecipe.ModeRod);
    }
}
