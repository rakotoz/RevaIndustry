using Vintagestory.API.Common;

namespace revindustry.Inventory;

public class ItemSlotMachineInput : ItemSlot {
    private readonly System.Func<ItemStack?, bool> _accepts;

    public ItemSlotMachineInput(InventoryBase inventory, System.Func<ItemStack?, bool> accepts) : base(inventory) {
        _accepts = accepts;
    }

    public override bool CanHold(ItemSlot sourceSlot) {
        return _accepts(sourceSlot.Itemstack) && base.CanHold(sourceSlot);
    }

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
        return _accepts(sourceSlot.Itemstack) && base.CanTakeFrom(sourceSlot, priority);
    }
}
