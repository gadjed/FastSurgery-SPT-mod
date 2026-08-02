using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MedRebalance.Client.Patches;

internal class EndHealPatch : ModulePatch
{
    private static FieldInfo? _playerField;

    protected override MethodBase GetTargetMethod()
    {
        _playerField = AccessTools.Field(typeof(Player.MedsController), "_player");
        return typeof(Player.MedsController.ObservedMedsControllerClass).GetMethod("method_8");
    }

    [PatchPrefix]
    private static bool Prefix(Player.MedsController.ObservedMedsControllerClass __instance, IEffect effect)
    {
        if (HealingSession.CancelRequested)
        {
            __instance.ClearQueue();
            __instance.method_9();
            HealingSession.End();
            return true;
        }

        if (!MedRebalancePlugin.ContinuousHealing.Value)
        {
            HealingSession.End();
            return true;
        }

        // Only continue after a real med effect finishes (keeps bleed/fracture vanilla flow on DoMedEffect).
        if (effect is not GInterface376)
        {
            return false;
        }

        var player = (Player?)_playerField?.GetValue(__instance.MedsController_0);
        if (player == null || !player.IsYourPlayer)
        {
            return true;
        }

        var item = __instance.MedsController_0.Item;
        var allowMedical = MedRebalancePlugin.HealLimbs.Value && item is MedicalItemClass;
        if (item is not MedKitItemClass && !allowMedical)
        {
            HealingSession.End();
            return true;
        }

        if (item is not MedsItemClass medsItem)
        {
            HealingSession.End();
            return true;
        }

        HealingSession.ApplyScratchHeals(player, medsItem);

        if (!HealingSession.HasUsableResource(medsItem))
        {
            HealingSession.End();
            return true;
        }

        if (!player.ActiveHealthController.CanApplyItem(__instance.MedsController_0.Item, EBodyPart.Common))
        {
            HealingSession.End();
            return true;
        }

        player.HealthController.EffectRemovedEvent -= __instance.method_8;
        var originalDelay = ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay;
        ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay =
            MedRebalancePlugin.HealDelay.Value;

        var newEffect = player.ActiveHealthController.DoMedEffect(
            __instance.MedsController_0.Item,
            EBodyPart.Common,
            1f
        );

        if (newEffect == null)
        {
            __instance.State = Player.EOperationState.Finished;
            __instance.MedsController_0.FailedToApply = true;
            var callback = __instance.Callback_0;
            __instance.Callback_0 = null;
            callback(__instance.MedsController_0);
            ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay = originalDelay;
            HealingSession.End();
            return false;
        }

        player.HealthController.EffectRemovedEvent += __instance.method_8;
        ActiveHealthController.GClass3008.GClass3019_0.MedEffect.MedKitStartDelay = originalDelay;

        if (MedRebalancePlugin.ResetAnimation.Value && item is not MedicalItemClass)
        {
            HealingSession.AnimationVariant++;
            var variantCount = 0;
            if (__instance.MedsController_0.Item.TryGetItemComponent(out AnimationVariantsComponent animationVariants))
            {
                variantCount = animationVariants.VariantsNumber;
            }

            var newAnim = (int)Mathf.Repeat(HealingSession.AnimationVariant, Mathf.Max(variantCount, 1));
            var animator = __instance.MedsController_0.FirearmsAnimator;
            if (animator != null)
            {
                var mult = player.Skills.SurgerySpeed.Value / 100f;
                animator.SetUseTimeMultiplier(1f + mult);
                if (animator.HasNextLimb())
                {
                    animator.SetNextLimb(true);
                    animator.SetActiveParam(false, false);
                }

                animator.SetAnimationVariant(newAnim);
            }
        }

        return false;
    }
}
