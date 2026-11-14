using revindustry.Gui;
using revindustry.Recipes;
using revindustry.Systems;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using System.Text;

namespace revindustry.BlockEntities;

public class BEWireDrawing : BlockEntityOpenableContainer {
    private const long CrankTimeoutMs = 300;
    private readonly InventoryGeneric _inventory;
    private WireDrawingRecipeSystem? _recipes;
    private bool _drawing;

    private float _drawProgress;
    private float _drawTime;
    private string? _progressRecipe;
    private long _lastCrankMs;
    private Shape? _shape;

    public override InventoryBase Inventory => _inventory;
    public override string InventoryClassName => "revwiredrawing";
    private ItemSlot InputSlot => _inventory[0];
    private ItemSlot OutputSlot => _inventory[1];
    private BlockEntityAnimationUtil? AnimUtil => GetBehavior<BEBehaviorAnimatable>()?.animUtil;
    public bool IsBuilt => GetBehavior<BEBehaviorRightClickConstructable>()?.IsComplete ?? true;

    public BEWireDrawing() {
        _inventory = new InventoryGeneric(2, null, null,
            (id, inv) => id == 1 ? new ItemSlotOutput(inv) : new ItemSlot(inv));
    }

    public override void Initialize(ICoreAPI api) {
        base.Initialize(api);
        _recipes = api.ModLoader.GetModSystem<WireDrawingRecipeSystem>();
        if (api.Side == EnumAppSide.Server) {
            RegisterGameTickListener(OnServerTick, 100);
        }
        if (api is ICoreClientAPI capi) {
            _shape = Shape.TryGet(api, Block.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json"));
            AnimUtil?.InitializeAnimator("revwiredrawing-" + Block.Code.ToShortString(), _shape,
                capi.Tesselator.GetTextureSource(Block), new Vec3f(0, Block.Shape.rotateY, 0));
            UpdateAnimation(_drawProgress);
        }
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel) {
        if (Api.Side == EnumAppSide.Client) {
            toggleInventoryDialogClient(byPlayer, () => new GuiDialogWireDrawing(
                Lang.Get("revindustry:wiredrawing-title"), Inventory, Pos, (ICoreClientAPI)Api));
            (invDialog as GuiDialogWireDrawing)?.UpdateProgress(_drawProgress, _drawTime);
        }
        return true;
    }

    public void Crank() {
        _lastCrankMs = Api.World.ElapsedMilliseconds;
    }

    private void OnServerTick(float dt) {
        if (!IsBuilt) {
            return;
        }
        bool cranking = Api.World.ElapsedMilliseconds - _lastCrankMs < CrankTimeoutMs;
        WireDrawingRecipe? recipe = _recipes?.FindFor(InputSlot.Itemstack);
        bool drawing = cranking && recipe != null && HasRoomFor(recipe);

        if (drawing != _drawing) {
            _drawing = drawing;
            MarkDirty(true);
        }

        if (_drawProgress > 0 && (recipe == null || recipe.Code != _progressRecipe)) {
            ResetProgress();
        }

        if (!drawing) {
            return;
        }

        if (_progressRecipe != recipe!.Code) {
            _progressRecipe = recipe.Code;
            _drawProgress = 0;
        }

        _drawTime = recipe.DrawTime;
        _drawProgress += dt;
        if (_drawProgress >= recipe.DrawTime) {
            Produce(recipe);
            _drawProgress = 0;

            WireDrawingRecipe? next = _recipes?.FindFor(InputSlot.Itemstack);
            if (next == null || !HasRoomFor(next)) {
                _drawing = false;
                _progressRecipe = null;
                MarkDirty(true);
                return;
            }
        }
        MarkDirty();
    }

    private bool HasRoomFor(WireDrawingRecipe recipe) {
        if (OutputSlot.Empty) {
            return true;
        }
        ItemStack result = recipe.Output.ResolvedItemstack!;
        ItemStack current = OutputSlot.Itemstack;
        return current.Equals(Api.World, result, GlobalConstants.IgnoredStackAttributes)
               && current.StackSize + result.StackSize <= current.Collectible.MaxStackSize;
    }

    private void Produce(WireDrawingRecipe recipe) {
        InputSlot.TakeOut(recipe.Ingredient.Quantity);
        InputSlot.MarkDirty();

        ItemStack result = recipe.Output.ResolvedItemstack!.Clone();
        if (OutputSlot.Empty) {
            OutputSlot.Itemstack = result;
        } else {
            OutputSlot.Itemstack.StackSize += result.StackSize;
        }
        OutputSlot.MarkDirty();
    }

    private void ResetProgress() {
        if (_drawProgress == 0) {
            return;
        }
        _drawProgress = 0;
        _progressRecipe = null;
        MarkDirty(true);
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetFloat("drawProgress", _drawProgress);
        tree.SetFloat("drawTime", _drawTime);
        tree.SetBool("drawing", _drawing);
        if (_progressRecipe != null) {
            tree.SetString("progressRecipe", _progressRecipe);
        }
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        float oldProgress = _drawProgress;

        _drawing = tree.GetBool("drawing");
        base.FromTreeAttributes(tree, worldForResolving);
        _drawProgress = tree.GetFloat("drawProgress");
        _drawTime = tree.GetFloat("drawTime");
        _progressRecipe = tree.GetString("progressRecipe");

        (invDialog as GuiDialogWireDrawing)?.UpdateProgress(_drawProgress, _drawTime);
        UpdateAnimation(oldProgress);
    }

    private void UpdateAnimation(float oldProgress) {
        if (Api?.Side != EnumAppSide.Client || AnimUtil == null) {
            return;
        }

        if (!_drawing && _drawProgress <= 0) {
            if (AnimUtil.activeAnimationsByAnimCode.ContainsKey("draw")) {
                AnimUtil.StopAnimation("draw");
            }
            return;
        }

        float time = _drawTime > 0 ? _drawTime : 5;
        float speed = _drawing ? 5f / time : 0;

        if (AnimUtil.activeAnimationsByAnimCode.TryGetValue("draw", out AnimationMetaData? meta)) {
            meta.AnimationSpeed = speed;

            if (_drawing && _drawProgress < oldProgress) {
                RunningAnimation? state = AnimUtil.animator?.GetAnimationState("draw");
                if (state != null) {
                    state.CurrentFrame = 0;
                }
            }
        } else {
            AnimUtil.StartAnimation(new AnimationMetaData {
                Animation = "draw",
                Code = "draw",
                AnimationSpeed = speed,
                StartFrameOnce = _drawProgress / time * 150,
                EaseInSpeed = 10,
                EaseOutSpeed = 2
            });
        }
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator) {
        if (AnimUtil?.activeAnimationsByAnimCode.Count > 0) {
            return true;
        }

        CompositeShape? stageShape = GetBehavior<BEBehaviorRightClickConstructable>()?.shape;
        if (stageShape == null) {
            return base.OnTesselation(mesher, tessThreadTesselator);
        }

        _shape ??= Shape.TryGet(Api, stageShape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json"));
        if (_shape == null) {
            return false;
        }

        tessThreadTesselator.TesselateShape(Block, _shape, out MeshData mesh,
            new Vec3f(0, stageShape.rotateY, 0), null, stageShape.SelectiveElements);
        mesher.AddMeshData(mesh);
        return true;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc) {
        if (!IsBuilt) {
            GetBehavior<BEBehaviorRightClickConstructable>()?.GetBlockInfo(forPlayer, dsc);
            return;
        }
        if (_drawProgress > 0 && _drawTime > 0) {
            dsc.AppendLine(Lang.Get("revindustry:wiredrawing-progress", (int)(_drawProgress / _drawTime * 100)));
        }
    }
}
