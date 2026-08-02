using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using MedRebalance.Client.Patches;

namespace MedRebalance.Client;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInIncompatibility("com.lacyway.ch")]
public class MedRebalancePlugin : BaseUnityPlugin
{
    public const string PluginGuid = "gadjed.medrebalance";
    public const string PluginName = "gadjed-MedRebalance";
    public const string PluginVersion = "1.3.0";

    public static ConfigEntry<bool> ContinuousHealing { get; private set; } = null!;
    public static ConfigEntry<bool> HealLimbs { get; private set; } = null!;
    public static ConfigEntry<int> HealDelay { get; private set; } = null!;
    public static ConfigEntry<bool> ResetAnimation { get; private set; } = null!;
    public static ConfigEntry<bool> CancelOnDamage { get; private set; } = null!;
    public static ConfigEntry<bool> ScratchHeal { get; private set; } = null!;
    public static ConfigEntry<float> ScratchHealAmount { get; private set; } = null!;
    public static ConfigEntry<float> ScratchMaxMissingHp { get; private set; } = null!;

    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        ContinuousHealing = Config.Bind(
            "1. Continuous Healing",
            "Enabled",
            true,
            "Continue healing across body parts instead of stopping after one limb."
        );
        HealLimbs = Config.Bind(
            "1. Continuous Healing",
            "Heal Limbs",
            true,
            "Also continue with surgery kits / splints (MedicalItemClass). Animation does not loop for those."
        );
        HealDelay = Config.Bind(
            "1. Continuous Healing",
            "Heal Delay",
            0,
            new ConfigDescription(
                "Delay between limb heals. Game default is ~2; 0 is instant continue.",
                new AcceptableValueRange<int>(0, 5)
            )
        );
        ResetAnimation = Config.Bind(
            "1. Continuous Healing",
            "Reset Animations",
            true,
            "Play a fresh animation between limbs. Disable to keep the starting animation / skip resets."
        );

        CancelOnDamage = Config.Bind(
            "2. Interrupt",
            "Cancel On Damage",
            true,
            "Any HP damage during healing cancels the med animation and restores the last weapon."
        );

        ScratchHeal = Config.Bind(
            "3. Scratch Heal",
            "Enabled",
            true,
            "While healing, also top up other limbs that only miss a few HP (uses medkit resource)."
        );
        ScratchHealAmount = Config.Bind(
            "3. Scratch Heal",
            "Heal Amount",
            2.5f,
            new ConfigDescription(
                "HP restored per scratched limb per continue tick.",
                new AcceptableValueRange<float>(1f, 5f)
            )
        );
        ScratchMaxMissingHp = Config.Bind(
            "3. Scratch Heal",
            "Max Missing HP",
            8f,
            new ConfigDescription(
                "Only limbs missing at most this much HP count as scratches.",
                new AcceptableValueRange<float>(2f, 20f)
            )
        );

        new StartHealPatch().Enable();
        new EndHealPatch().Enable();
        new CancelHealPatch().Enable();
        new DamageInterruptPatch().Enable();

        Log.LogInfo($"{PluginName} {PluginVersion} loaded (SPT 4.0.13).");
    }
}
