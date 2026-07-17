using Vintagestory.API.Common;
using System;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace UsefulStuff
{
    public class ItemGasMask : Item
    {
        public override int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack, EnumMergePriority priority)
        {
            if (priority == EnumMergePriority.DirectMerge)
            {
                if (sinkStack.Attributes.GetInt("durability", Durability) >= Durability || !sourceStack.Collectible.Code.Path.Contains("charcoal")) return base.GetMergableQuantity(sinkStack, sourceStack, priority);

                return 1;
            }


            return base.GetMergableQuantity(sinkStack, sourceStack, priority);
        }

        public override void TryMergeStacks(ItemStackMergeOperation op)
        {
            if (op.CurrentPriority == EnumMergePriority.DirectMerge)
            {
                int durability; 
                if ((durability = op.SinkSlot.Itemstack.Attributes.GetInt("durability", Durability)) < Durability && op.SourceSlot.Itemstack.Collectible.Code.Path.Contains("charcoal"))
                {
                    op.MovedQuantity = 1;
                    op.SourceSlot.TakeOut(1);
                    op.SinkSlot.Itemstack.Attributes.SetInt("durability", Math.Min(durability + 300, Durability));
                }
            }

            base.TryMergeStacks(op);
        }

        public override void DamageItem(IWorldAccessor world, Entity byEntity, ItemSlot itemSlot, int amount = 1, bool destroyOnZeroDurability = true)
        {
            {
                ItemStack itemstack = itemSlot.Itemstack;

                int leftDurability = itemstack.Attributes.GetInt("durability", GetMaxDurability(itemstack));
                leftDurability = GameMath.Clamp(leftDurability - amount, 0, Durability);
                itemstack.Attributes.SetInt("durability", leftDurability);

                itemSlot.MarkDirty();
            }
        }
    }
}
