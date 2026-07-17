using Vintagestory.API.Common;
using System;
using Vintagestory.API.MathTools;
using Vintagestory.API.Client;

namespace UsefulStuff
{
    public class ItemUSGlider : Item
    {
        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            base.OnHeldIdle(slot, byEntity);
            //Prevents gliding in caves
            if (UsefulStuffConfig.Loaded.GliderNoCaveDiving && byEntity.World.BlockAccessor.GetRainMapHeightAt(byEntity.Pos.AsBlockPos) > byEntity.Pos.Y && byEntity.World.SeaLevel > byEntity.Pos.Y) return;

            if (!byEntity.OnGround && !byEntity.FeetInLiquid && !byEntity.Controls.IsFlying && !byEntity.Controls.IsClimbing && byEntity.ApplyGravity && byEntity.Pos.Motion.Y < 0)
            {
                //Handles duration loss
                int dur = slot.Itemstack.Attributes.GetInt("glidingDur");
                if (dur > UsefulStuffConfig.Loaded.GliderDamageRate)
                {
                    DamageItem(byEntity.World, byEntity, slot);
                    dur = 0;
                }
                slot.Itemstack.Attributes.SetInt("glidingDur", dur + 1);

                //Air brake to prevent hold jump exploit
                if (byEntity.Controls.Jump) return;

                //Handler to stop animation
                long handler = slot.Itemstack.TempAttributes.GetLong("handler");
                if (handler != 0) api.World.UnregisterCallback(handler);
                slot.Itemstack.TempAttributes.SetLong("handler", api.World.RegisterCallback((dt) => CancelGlide(byEntity), 100));

                //This animation only partly works because it gets blended with falling animation. But stopping falling animation causes head to spin VERTICALLY for some reason
                byEntity.StartAnimation("glide");

                double yVector = byEntity.Pos.GetViewVector().Y; //Returns a value between -1 (looking down) and 1 (looking up)
                double prevFall = byEntity.Pos.Motion.Y; //Save our previous elevation for horizontal speed gain calculations

                if (yVector <= UsefulStuffConfig.Loaded.GliderMaxStall && yVector >= UsefulStuffConfig.Loaded.GliderMinStall)
                {
                    //Every 16x16 block columm is a heat columm
                    double windBonus = 0;
                    if ((byEntity.Pos.AsBlockPos.X / 16) % 2 == 0 && (byEntity.Pos.AsBlockPos.Z / 16) % 2 == 0)
                    {
                        windBonus = byEntity.World.BlockAccessor.GetWindSpeedAt(byEntity.Pos.AsBlockPos).X * UsefulStuffConfig.Loaded.GliderWindPushModifier;
                        if (windBonus == 1) windBonus = 1.00001;
                    }

                    //Slows down our fall
                    byEntity.Pos.Motion.Y *= Math.Max(0.10, UsefulStuffConfig.Loaded.GliderDescentRModifier + byEntity.Pos.Motion.Y);
                    byEntity.Pos.Motion.Y += byEntity.Pos.Motion.Y * (yVector < 0 ? -yVector : yVector);
                    byEntity.Pos.Motion.Y *= windBonus != 0 ? -1 - 1.8/2 : 1;

                    //Gives us a horizontal push forward or backwards
                    Vec3d newVec = byEntity.Pos.HorizontalAheadCopy((UsefulStuffConfig.Loaded.GliderBackwardsAt - yVector) * UsefulStuffConfig.Loaded.GliderThrustModifier).XYZ;
                    byEntity.Pos.Motion.Z -= byEntity.Pos.Z - newVec.Z;
                    byEntity.Pos.Motion.X -= byEntity.Pos.X - newVec.X;
                    if (windBonus > 0)
                    {
                        byEntity.Pos.Motion.Z *= 1 + windBonus / 2;
                        byEntity.Pos.Motion.X *= 1 + windBonus / 2;
                    }
                }

                //Wind push. Note the wind does not seem fully implemented, only blows postively on X axis
                //Vec3d windSpeed = byEntity.World.BlockAccessor.GetWindSpeedAt(byEntity.Pos.AsBlockPos);
                //byEntity.Pos.Motion.X += windSpeed.X * UsefulStuffConfig.Loaded.GliderWindPushModifier;
                //byEntity.Pos.Motion.Y += windSpeed.Y * UsefulStuffConfig.Loaded.GliderWindPushModifier;
                //byEntity.Pos.Motion.Z += windSpeed.Z * UsefulStuffConfig.Loaded.GliderWindPushModifier * (byEntity.Pos.Motion.X < 0 ? -1 : 1);
            }
        }

        public void CancelGlide(EntityAgent byEntity)
        {
            byEntity.StopAnimation("glide");
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    HotKeyCode = "jump",
                    ActionLangCode = "usefulstuff:itemhelp-glider-airbrake",
                    MouseButton = EnumMouseButton.None
                }
            };
        }
    }
}
