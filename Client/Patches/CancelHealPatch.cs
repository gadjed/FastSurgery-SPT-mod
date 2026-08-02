using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace MedRebalance.Client.Patches;

internal class CancelHealPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GClass3010).GetMethod(nameof(GClass3010.CancelApplyingItem));
    }

    [PatchPrefix]
    private static void Prefix(Player ___Player)
    {
        if (___Player != null && ___Player.IsYourPlayer)
        {
            HealingSession.RequestCancel();
        }
    }
}
