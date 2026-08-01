using System.Reflection;
using Spectre.Console;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Path = System.IO.Path;

namespace FastSurgery;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "gadjed.fastsurgery";
    public string Name { get; init; } = "Fast Surgery";
    public string Author { get; init; } = "gadjed";
    public List<string>? Contributors { get; init; } = null;
    public SemanticVersioning.Version Version { get; init; } = new("1.2.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; } = null;
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = null;
    public string? Url { get; init; } = "https://github.com/gadjed/FastSurgery-SPT-mod";
    public string License { get; init; } = "MIT";
}

public class ModConfig
{
    public double UseTimeSeconds { get; set; } = 5;

    /// <summary>
    /// Friendly item name (or template id) -> enabled.
    /// </summary>
    public Dictionary<string, bool> Items { get; set; } = new();
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class FastSurgeryMod(
    ISptLogger<FastSurgeryMod> logger,
    ModHelper modHelper,
    TemplateTable templateTable
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configPath = Path.Combine(pathToMod, "config.json");
        var config = File.Exists(configPath)
            ? modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config.json")
            : new ModConfig { Items = new Dictionary<string, bool>(ItemCatalog.DefaultEnabledItems) };

        var useTime = config.UseTimeSeconds <= 0 ? 5 : config.UseTimeSeconds;
        var items = templateTable.Items;
        var changed = 0;

        foreach (var (key, enabled) in config.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                Color.Cyan
            );
        }

        logger.Success($"[FastSurgery] Updated use time on {changed} medical item(s).");
        return Task.CompletedTask;
    }
}
