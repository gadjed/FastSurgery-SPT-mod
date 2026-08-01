namespace FastSurgery;

/// <summary>
/// Maps friendly config keys to Tarkov item template IDs.
/// Config can use either the display name or the raw template ID.
/// </summary>
public static class ItemCatalog
{
    public const string ImmobilizingSplint = "Immobilizing splint";
    public const string AluminumSplint = "Aluminum splint";
    public const string CmsSurgicalKit = "CMS surgical kit";
    public const string Surv12FieldSurgicalKit = "Surv12 field surgical kit";

    private static readonly Dictionary<string, string> NameToTpl = new(StringComparer.OrdinalIgnoreCase)
    {
        [ImmobilizingSplint] = "544fb3364bdc2d34748b456a",
        [AluminumSplint] = "5af0454c86f7746bf20992e8",
        [CmsSurgicalKit] = "5d02778e86f774203e7dedbe",
        [Surv12FieldSurgicalKit] = "5d02797c86f774203f38e30a",
    };

    private static readonly Dictionary<string, string> TplToName =
        NameToTpl.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, bool> DefaultEnabledItems { get; } =
        NameToTpl.Keys.ToDictionary(name => name, _ => true, StringComparer.OrdinalIgnoreCase);

    public static bool TryResolve(string key, out string tpl, out string displayName)
    {
        if (NameToTpl.TryGetValue(key, out var byName))
        {
            tpl = byName;
            displayName = TplToName[byName];
            return true;
        }

        if (TplToName.TryGetValue(key, out var byTpl))
        {
            tpl = key;
            displayName = byTpl;
            return true;
        }

        tpl = key;
        displayName = key;
        return MongoIdLike(key);
    }

    private static bool MongoIdLike(string value) =>
        value.Length == 24 && value.All(Uri.IsHexDigit);
}
