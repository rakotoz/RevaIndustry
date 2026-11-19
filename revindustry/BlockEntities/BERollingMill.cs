using System.Text;
using revindustry.Gui;
using revindustry.Recipes;
using revindustry.Systems;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace revindustry.BlockEntities;

public class BERollingMill : BEHandCrankedMachine {
    private const int PacketSetMode = 1500;

    private RollingRecipeSystem? _recipes;

    public string Mode { get; private set; } = RollingRecipe.ModeSheet;

    public override string InventoryClassName => "revrollingmill";
    protected override string AnimationCode => "roll";
    protected override int AnimationFrames => 180;
    protected override string TitleLangKey => "revindustry:rollingmill-title";
    protected override string ProgressLangKey => "revindustry:rollingmill-progress";

    private RollingRecipeSystem RecipeSystem => _recipes ??= Api.ModLoader.GetModSystem<RollingRecipeSystem>();

    protected override MachineRecipe? FindRecipe(ItemStack? input) {
        return RecipeSystem.FindFor(input, Mode);
    }

    protected override bool AcceptsInput(ItemStack? stack) {
        return RecipeSystem.AcceptsInput(stack);
    }

    protected override GuiDialogMachine CreateDialog(ICoreClientAPI capi) {
        GuiDialogRollingMill dialog = new(Title, Inventory, Pos, capi);
        dialog.UpdateMode(Mode);
        return dialog;
    }

    public void RequestMode(string mode) {
        if (Api is ICoreClientAPI capi) {
            capi.Network.SendBlockEntityPacket(Pos, PacketSetMode, Encoding.UTF8.GetBytes(mode));
        }
    }

    public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data) {
        if (packetid != PacketSetMode) {
            base.OnReceivedClientPacket(player, packetid, data);
            return;
        }

        string mode = Encoding.UTF8.GetString(data);
        if (mode != RollingRecipe.ModeSheet && mode != RollingRecipe.ModeRod) {
            return;
        }
        Mode = mode;
        MarkDirty(true);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetString("mode", Mode);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        Mode = tree.GetString("mode", RollingRecipe.ModeSheet);
        base.FromTreeAttributes(tree, worldForResolving);
        (invDialog as GuiDialogRollingMill)?.UpdateMode(Mode);
    }
}
