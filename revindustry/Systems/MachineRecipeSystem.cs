using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using revindustry.Recipes;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace revindustry.Systems;

[ProtoContract]
public class MachineInputsPacket {
    [ProtoMember(1)]
    public byte[] Data = Array.Empty<byte>();
}

public abstract class MachineRecipeSystem<T> : ModSystem where T : MachineRecipe {
    private readonly List<CraftingRecipeIngredient> _inputs = new();

    public List<T> Recipes { get; } = new();

    protected abstract string RecipeFolder { get; }
    private string ChannelName => "revindustry-" + RecipeFolder.Replace('/', '-');

    public override double ExecuteOrder() => 1;

    public override void Start(ICoreAPI api) {
        api.Network.RegisterChannel(ChannelName).RegisterMessageType<MachineInputsPacket>();
    }

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
        _inputs.Clear();
        _inputs.AddRange(Recipes.Select(r => r.Ingredient));
        api.Logger.Notification("[revindustry] {0}: loaded {1} recipes", RecipeFolder, Recipes.Count);
    }

    public override void StartServerSide(ICoreServerAPI api) {
        IServerNetworkChannel channel = api.Network.GetChannel(ChannelName);
        api.Event.PlayerJoin += player => channel.SendPacket(WriteInputs(), player);
    }

    public override void StartClientSide(ICoreClientAPI api) {
        api.Network.GetChannel(ChannelName)
            .SetMessageHandler<MachineInputsPacket>(packet => ReadInputs(packet, api.World));
    }

    public T? FindFor(ItemStack? input) {
        return Recipes.FirstOrDefault(r => r.Matches(input));
    }

    public bool AcceptsInput(ItemStack? stack) {
        return stack != null && _inputs.Any(i => i.SatisfiesAsIngredient(stack, false));
    }

    private MachineInputsPacket WriteInputs() {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream)) {
            writer.Write(_inputs.Count);
            foreach (CraftingRecipeIngredient input in _inputs) {
                input.ToBytes(writer);
            }
        }
        return new MachineInputsPacket { Data = stream.ToArray() };
    }

    private void ReadInputs(MachineInputsPacket packet, IWorldAccessor world) {
        using BinaryReader reader = new(new MemoryStream(packet.Data));
        int count = reader.ReadInt32();
        _inputs.Clear();
        for (int i = 0; i < count; i++) {
            CraftingRecipeIngredient input = new();
            input.FromBytes(reader, world);
            _inputs.Add(input);
        }
    }
}
