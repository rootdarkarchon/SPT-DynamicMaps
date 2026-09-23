using SPTarkov.Server.Core.Models.Spt.Mod;

namespace _dynamicMapsServer;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.mpstark.dynamicmaps";
    public string Name { get; init; } = "Dynamic Maps";
    public string Author { get; init; } = "mpstark";
    public List<string>? Contributors { get; init; } = [" dirtbikercj, acidphantasm"];
    public SemanticVersioning.Version Version { get; init; } = new("1.0.4");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public bool HasPrepatcher { get; init; }
    public string License { get; init; } = "MIT";
}
