using System.Collections.Generic;
using System.Linq;
using revindustry.Recipes;
using Vintagestory.API.Common;

namespace revindustry.Systems;

public abstract class MachineRecipeSystem<T> : ModSystem where T : MachineRecipe {
    public List<T> Recipes { get; } = new();

    protected abstract string RecipeFolder { get; }

    public override double ExecuteOrder() => 1;

    public override void AssetsLoaded(ICoreAPI api) {
        if (api.Side != EnumAppSide.Server) {
            return;
        }

        var files = api.Assets.GetMany<T[]>(api.Logger, RecipeFolder, null);
        foreach (var (location, recipes) in files) {
            foreach (var recipe in recipes) {
                if (recipe.Resolve(api.World, RecipeFolder + " " + location)) {
                    Recipes.Add(recipe);
                }
            }
        }
        api.Logger.Notification("[revindustry] {0}: loaded {1} recipes", RecipeFolder, Recipes.Count);
    }

    public T? FindFor(ItemStack? input) {
        return Recipes.FirstOrDefault(r => r.Matches(input));
    }
}
