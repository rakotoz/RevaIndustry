using System.Linq;
using revindustry.Recipes;
using Vintagestory.API.Common;

namespace revindustry.Systems;

public class RollingRecipeSystem : MachineRecipeSystem<RollingRecipe> {
    protected override string RecipeFolder => "recipes/rolling";

    public RollingRecipe? FindFor(ItemStack? input, string mode) {
        return Recipes.FirstOrDefault(r => r.Mode == mode && r.Matches(input));
    }
}
