using System.Collections.Generic;
using System.Linq;
using revindustry.Recipes;
using Vintagestory.API.Common;

namespace revindustry.Systems;

public class WireDrawingRecipeSystem : ModSystem {
    public List<WireDrawingRecipe> Recipes { get; } = new();

    public override double ExecuteOrder() => 1;

    public override void AssetsLoaded(ICoreAPI api) {
        if (api.Side != EnumAppSide.Server) {
            return;
        }

        var files = api.Assets.GetMany<WireDrawingRecipe[]>(api.Logger, "recipes/wiredrawing", null);
        foreach (var (location, recipes) in files) {
            foreach (var recipe in recipes) {
                if (recipe.Resolve(api.World, "wire-drawing recipe " + location)) {
                    Recipes.Add(recipe);
                }
            }
        }
        api.Logger.Notification("[revindustry] Loaded {0} wire-drawing recipes", Recipes.Count);
    }

    public WireDrawingRecipe? FindFor(ItemStack? input) {
        return Recipes.FirstOrDefault(r => r.Matches(input));
    }
}
