using revindustry.Recipes;
using revindustry.Systems;
using Vintagestory.API.Common;

namespace revindustry.BlockEntities;

public class BEWireDrawing : BEHandCrankedMachine {
    private WireDrawingRecipeSystem? _recipes;

    public override string InventoryClassName => "revwiredrawing";
    protected override string AnimationCode => "draw";
    protected override string TitleLangKey => "revindustry:wiredrawing-title";
    protected override string ProgressLangKey => "revindustry:wiredrawing-progress";

    protected override MachineRecipe? FindRecipe(ItemStack? input) {
        _recipes ??= Api.ModLoader.GetModSystem<WireDrawingRecipeSystem>();
        return _recipes.FindFor(input);
    }
}
