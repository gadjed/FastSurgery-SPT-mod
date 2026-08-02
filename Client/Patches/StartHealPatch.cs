using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace MedRebalance.Client.Patches;

internal class StartHealPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player.MedsController).GetMethod(nameof(Player.MedsController.Spawn));
    }

    [PatchPrefix]
    private static void Prefix(Player ____player)
    {
        if (____player != null && ____player.IsYourPlayer)
        {
            HealingSession.Begin();
        }
    }
}
