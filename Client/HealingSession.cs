using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using UnityEngine;

namespace MedRebalance.Client;

/// <summary>
/// Shared state for continuous healing, damage interrupt, and scratch heals.
/// </summary>
internal static class HealingSession
{
    private static readonly EBodyPart[] BodyParts =
    [
        EBodyPart.Head,
        EBodyPart.Chest,
        EBodyPart.Stomach,
        EBodyPart.LeftArm,
        EBodyPart.RightArm,
        EBodyPart.LeftLeg,
        EBodyPart.RightLeg
    ];

    public static int AnimationVariant;
    public static bool CancelRequested;
    public static bool IsHealing;

    public static void Begin()
    {
        CancelRequested = false;
        AnimationVariant = 0;
        IsHealing = true;
    }

    public static void End()
    {
        IsHealing = false;
        CancelRequested = false;
    }

    public static void RequestCancel()
    {
        CancelRequested = true;
    }

    /// <summary>
    /// Soft-heal other limbs that only miss a few HP, consuming medkit resource.
    /// Bleed / fracture treatment stays on vanilla <see cref="ActiveHealthController.DoMedEffect"/>.
    /// </summary>
    public static void ApplyScratchHeals(Player player, MedsItemClass medsItem)
    {
        if (!MedRebalancePlugin.ScratchHeal.Value || player == null || medsItem?.MedKitComponent == null)
        {
            return;
        }

        // Surgery kits / splints: low MaxHpResource — skip scratch HP top-ups.
        if (medsItem.MedKitComponent.MaxHpResource < 95f)
        {
            return;
        }

        var kit = medsItem.MedKitComponent;
        var amountPerLimb = MedRebalancePlugin.ScratchHealAmount.Value;
        var maxMissing = MedRebalancePlugin.ScratchMaxMissingHp.Value;
        var health = player.ActiveHealthController;

        foreach (var part in BodyParts)
        {
            if (kit.HpResource <= 0f)
            {
                break;
            }

            var partHealth = health.GetBodyPartHealth(part);
            if (partHealth.Current <= 0f || partHealth.AtMaximum)
            {
                continue;
            }

            var missing = partHealth.Maximum - partHealth.Current;
            if (missing <= 0f || missing > maxMissing)
            {
                continue;
            }

            var heal = Mathf.Min(amountPerLimb, missing, kit.HpResource);
            if (heal <= 0f)
            {
                continue;
            }

            health.Heal(part, heal);
            kit.HpResource -= heal;
        }
    }

    public static bool HasUsableResource(MedsItemClass medsItem)
    {
        if (medsItem?.MedKitComponent == null)
        {
            return false;
        }

        // Single-use medical items (surgery/splint style) stop when resource is gone.
        if (medsItem.MedKitComponent.HpResource <= 0f)
        {
            return false;
        }

        // Match prior continuous-heal guard for tiny residual on non-medkits.
        if (medsItem.MedKitComponent.HpResource <= 1f && medsItem.MedKitComponent.MaxHpResource < 95f)
        {
            return false;
        }

        return true;
    }

    public static void InterruptAndRestoreWeapon(Player player)
    {
        if (player == null || !player.IsYourPlayer)
        {
            return;
        }

        RequestCancel();

        try
        {
            player.HealthController?.CancelApplyingItem();
        }
        catch
        {
            // ignored — cancel path may already be tearing down
        }

        if (player.HandsController is Player.MedsController meds)
        {
            try
            {
                meds.FastForwardCurrentState();
            }
            catch
            {
                // ignored
            }
        }

        try
        {
            player.TrySetLastEquippedWeapon(true);
        }
        catch
        {
            // ignored
        }

        End();
    }
}
