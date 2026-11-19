using System.Text;
using revindustry.Gui;
using revindustry.Recipes;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace revindustry.BlockEntities;

public abstract class BEHandCrankedMachine : BlockEntityOpenableContainer {
    private const long CrankTimeoutMs = 300;
    private const float AnimationFps = 30f;

    private readonly InventoryGeneric _inventory;
    private bool _working;
    private float _progress;
    private float _workTime;
    private string? _progressRecipe;
    private long _lastCrankMs;
    private Shape? _shape;

    public override InventoryBase Inventory => _inventory;
    protected ItemSlot InputSlot => _inventory[0];
    protected ItemSlot OutputSlot => _inventory[1];
    private BlockEntityAnimationUtil? AnimUtil => GetBehavior<BEBehaviorAnimatable>()?.animUtil;
    public bool IsBuilt => GetBehavior<BEBehaviorRightClickConstructable>()?.IsComplete ?? true;
    public string Title => Lang.Get(TitleLangKey);

    protected abstract string AnimationCode { get; }
    protected virtual int AnimationFrames => 150;
    protected abstract string TitleLangKey { get; }
    protected abstract string ProgressLangKey { get; }
    protected abstract MachineRecipe? FindRecipe(ItemStack? input);

    protected BEHandCrankedMachine() {
        _inventory = new InventoryGeneric(2, null, null,
            (id, inv) => id == 1 ? new ItemSlotOutput(inv) : new ItemSlot(inv));
    }

    public override void Initialize(ICoreAPI api) {
        base.Initialize(api);
        if (api.Side == EnumAppSide.Server) {
            RegisterGameTickListener(OnServerTick, 100);
        }

        if (api is ICoreClientAPI capi) {
            _shape = Shape.TryGet(api, Block.Shape.Base.CopyWithPathPrefixAndAppendixOnce("shapes/", ".json"));
            AnimUtil?.InitializeAnimator(InventoryClassName + "-" + Block.Code.ToShortString(), _shape,
                capi.Tesselator.GetTextureSource(Block), new Vec3f(0, Block.Shape.rotateY, 0));
            UpdateAnimation(_progress);
        }
    }

    protected virtual GuiDialogMachine CreateDialog(ICoreClientAPI capi) {
        return new GuiDialogMachine(Title, Inventory, Pos, capi);
    }

    public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel) {
        if (Api.Side == EnumAppSide.Client) {
            toggleInventoryDialogClient(byPlayer, () => CreateDialog((ICoreClientAPI)Api));
            (invDialog as GuiDialogMachine)?.UpdateProgress(_progress, _workTime);
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
        MachineRecipe? recipe = FindRecipe(InputSlot.Itemstack);
        bool working = cranking && recipe != null && HasRoomFor(recipe);

        if (working != _working) {
            _working = working;
            MarkDirty(true);
        }

        if (_progress > 0 && (recipe == null || recipe.Code != _progressRecipe)) {
            ResetProgress();
        }

        if (!working) {
            return;
        }

        if (_progressRecipe != recipe!.Code) {
            _progressRecipe = recipe.Code;
            _progress = 0;
        }

        _workTime = recipe.Time;
        _progress += dt;
        if (_progress >= recipe.Time) {
            Produce(recipe);
            _progress = 0;

            MachineRecipe? next = FindRecipe(InputSlot.Itemstack);
            if (next == null || !HasRoomFor(next)) {
                _working = false;
                _progressRecipe = null;
                MarkDirty(true);
                return;
            }
        }
        MarkDirty();
    }

    protected void ResetProgress() {
        if (_progress == 0) {
            return;
        }
        _progress = 0;
        _progressRecipe = null;
        MarkDirty(true);
    }

    private bool HasRoomFor(MachineRecipe recipe) {
        if (OutputSlot.Empty) {
            return true;
        }
        ItemStack result = recipe.Output.ResolvedItemstack!;
        ItemStack current = OutputSlot.Itemstack!;
        return current.Equals(Api.World, result, GlobalConstants.IgnoredStackAttributes)
               && current.StackSize + result.StackSize <= current.Collectible.MaxStackSize;
    }

    private void Produce(MachineRecipe recipe) {
        InputSlot.TakeOut(recipe.Ingredient.Quantity);
        InputSlot.MarkDirty();

        ItemStack result = recipe.Output.ResolvedItemstack!.Clone();
        if (OutputSlot.Empty) {
            OutputSlot.Itemstack = result;
        } else {
            OutputSlot.Itemstack!.StackSize += result.StackSize;
        }
        OutputSlot.MarkDirty();
    }

    public override void ToTreeAttributes(ITreeAttribute tree) {
        base.ToTreeAttributes(tree);
        tree.SetFloat("progress", _progress);
        tree.SetFloat("workTime", _workTime);
        tree.SetBool("working", _working);
        if (_progressRecipe != null) {
            tree.SetString("progressRecipe", _progressRecipe);
        }
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving) {
        float oldProgress = _progress;

        _working = tree.GetBool("working");
        base.FromTreeAttributes(tree, worldForResolving);
        _progress = tree.GetFloat("progress");
        _workTime = tree.GetFloat("workTime");
        _progressRecipe = tree.GetString("progressRecipe");

        (invDialog as GuiDialogMachine)?.UpdateProgress(_progress, _workTime);
        UpdateAnimation(oldProgress);
    }

    private void UpdateAnimation(float oldProgress) {
        if (Api?.Side != EnumAppSide.Client || AnimUtil == null) {
            return;
        }

        if (!_working && _progress <= 0) {
            if (AnimUtil.activeAnimationsByAnimCode.ContainsKey(AnimationCode)) {
                AnimUtil.StopAnimation(AnimationCode);
            }
            return;
        }

        float time = _workTime > 0 ? _workTime : AnimationFrames / AnimationFps;
        float speed = _working ? AnimationFrames / AnimationFps / time : 0;

        if (AnimUtil.activeAnimationsByAnimCode.TryGetValue(AnimationCode, out AnimationMetaData? meta)) {
            meta.AnimationSpeed = speed;

            if (_working && _progress < oldProgress) {
                RunningAnimation? state = AnimUtil.animator?.GetAnimationState(AnimationCode);
                if (state != null) {
                    state.CurrentFrame = 0;
                }
            }
        } else if (AnimUtil.animator?.GetAnimationState(AnimationCode) != null) {
            AnimUtil.StartAnimation(new AnimationMetaData {
                Animation = AnimationCode,
                Code = AnimationCode,
                AnimationSpeed = speed,
                StartFrameOnce = _progress / time * AnimationFrames,
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
        if (_progress > 0 && _workTime > 0) {
            dsc.AppendLine(Lang.Get(ProgressLangKey, (int)(_progress / _workTime * 100)));
        }
    }
}
