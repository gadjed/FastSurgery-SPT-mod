using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using Path = System.IO.Path;

namespace FastSurgery;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "gadjed.fastsurgery";
    public override string Name { get; init; } = "Fast Surgery";
    public override string Author { get; init; } = "gadjed";
    public override List<string>? Contributors { get; init; } = null;
    public override SemanticVersioning.Version Version { get; init; } = new("1.1.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; } = null;
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public override string? Url { get; init; } = "https://github.com/gadjed/FastSurgery-SPT-mod";
    public override bool? IsBundleMod { get; init; } = false;
    public override string? License { get; init; } = "MIT";
}

public class ModConfig
{
    public double UseTimeSeconds { get; set; } = 5;

    /// <summary>
    /// Friendly item name (or template id) -> enabled.
    /// </summary>
    public Dictionary<string, bool> Items { get; set; } = new();
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class FastSurgeryMod(
    ISptLogger<FastSurgeryMod> logger,
    ModHelper modHelper,
    DatabaseService databaseService
) : IOnLoad
{
    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configPath = Path.Combine(pathToMod, "config.json");
        var config = File.Exists(configPath)
            ? modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.json")
            : new ModConfig { Items = new Dictionary<string, bool>(ItemCatalog.DefaultEnabledItems) };

        var useTime = config.UseTimeSeconds <= 0 ? 5 : config.UseTimeSeconds;
        var items = databaseService.GetItems();
        var changed = 0;

        foreach (var (key, enabled) in config.Items)
        {
            if (!enabled)
            {
                continue;
            }

            if (!ItemCatalog.TryResolve(key, out var tpl, out var displayName))
            {
                logger.Warning($"[FastSurgery] Unknown item key: {key}");
                continue;
            }

            if (!MongoId.IsValidMongoId(tpl) || !items.TryGetValue(tpl, out var item) || item is null)
            {
                logger.Warning($"[FastSurgery] Item not found in database: {displayName} ({tpl})");
                continue;
            }

            if (item.Properties is null)
            {
                logger.Warning($"[FastSurgery] Item has no properties: {displayName} ({tpl})");
                continue;
            }

            var previous = item.Properties.MedUseTime;
            item.Properties.MedUseTime = useTime;
            changed++;

            logger.LogWithColor(
                $"[FastSurgery] {displayName}: medUseTime {previous} -> {useTime}s",
                LogTextColor.Cyan
            );
        }

        logger.Success($"[FastSurgery] Updated use time on {changed} medical item(s).");
        return Task.CompletedTask;
    }
}
