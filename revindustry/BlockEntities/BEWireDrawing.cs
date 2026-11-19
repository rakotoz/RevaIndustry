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

    private WireDrawingRecipeSystem RecipeSystem => _recipes ??= Api.ModLoader.GetModSystem<WireDrawingRecipeSystem>();

    protected override MachineRecipe? FindRecipe(ItemStack? input) {
        return RecipeSystem.FindFor(input);
    }

    protected override bool AcceptsInput(ItemStack? stack) {
        return RecipeSystem.AcceptsInput(stack);
    }
}
