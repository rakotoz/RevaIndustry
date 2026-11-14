using Vintagestory.API.Common;

namespace revindustry.Recipes;

public class WireDrawingRecipe {
    public string Code = "";
    public CraftingRecipeIngredient Ingredient = null!;
    public JsonItemStack Output = null!;
    public float DrawTime = 5;

    public bool Resolve(IWorldAccessor world, string source) {
        if (Ingredient == null || Output == null) {
            world.Logger.Warning("{0}: missing ingredient or output, skipped", source);
            return false;
        }
        return Ingredient.Resolve(world, source) && Output.Resolve(world, source);
    }

    public bool Matches(ItemStack? stack) {
        return stack != null && Ingredient.SatisfiesAsIngredient(stack, true);
    }
}
