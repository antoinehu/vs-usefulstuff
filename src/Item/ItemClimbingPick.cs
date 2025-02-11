﻿using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace UsefulStuff
{
    public class ItemClimbingPick : Item
    {
        EntityPartitioning entityUtil;

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            base.OnHeldIdle(slot, byEntity);

            if (!(byEntity.LeftHandItemSlot.Itemstack?.Collectible is ItemClimbingPick && byEntity.RightHandItemSlot.Itemstack?.Collectible is ItemClimbingPick)) return;

            if (UsefulStuffConfig.Loaded.ClimbingPickDisabledInProtected)
            {
                bool bossnear = false;
                entityUtil?.WalkInteractableEntities(byEntity.SidedPos.XYZ, 30, (e) =>
                {
                    if (e?.Properties?.Attributes?.IsTrue("isBoss") == true)
                    {
                        bossnear = true;
                        return false;
                    }

                    return true;
                });

                if (bossnear) return;
            }

            int dur = slot.Itemstack.Attributes.GetInt("climbingDur");


            if (byEntity.Properties.CanClimbAnywhere && byEntity.Controls.IsClimbing)
            {
                if (dur > UsefulStuffConfig.Loaded.ClimbingPickDamageRate)
                {
                    DamageItem(byEntity.World, byEntity, slot);
                    dur = 0;
                }
                slot.Itemstack.Attributes.SetInt("climbingDur", dur + 1);
            }


            long handler = byEntity.LeftHandItemSlot.Itemstack.TempAttributes.GetLong("handler");
            if (handler != 0) api.World.UnregisterCallback(handler);
            byEntity.LeftHandItemSlot.Itemstack.TempAttributes.SetLong("handler", api.World.RegisterCallback((dt) => byEntity.Properties.CanClimbAnywhere = false, 100));
            byEntity.Properties.CanClimbAnywhere = true;
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            entityUtil = api.ModLoader.GetModSystem<EntityPartitioning>();
        }
    }
}
