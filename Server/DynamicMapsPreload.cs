using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Services;

namespace _dynamicMapsServer;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 0)]
public class DynamicMapsPreload(DatabaseService databaseService) : IOnLoad
{
    public Dictionary<string, List<Spawnpoint>> SpawnPointsDict { get; private set; }

    public Task OnLoad()
    {
        SpawnPointsDict = PopulateSpawnPoints(databaseService);
        return Task.CompletedTask;
    }

    private static Dictionary<string, List<Spawnpoint>> PopulateSpawnPoints(DatabaseService database)
    {
        var locations = database.GetLocations();
        return new Dictionary<string, List<Spawnpoint>>()
        {
            ["bigmap"] = [.. locations.Bigmap.LooseLoot!.Value!.SpawnpointsForced!],
            ["interchange"] = [.. locations.Interchange.LooseLoot!.Value!.SpawnpointsForced!],
            ["laboratory"] = [.. locations.Laboratory.LooseLoot!.Value!.SpawnpointsForced!],
            ["lighthouse"] = [.. locations.Lighthouse.LooseLoot!.Value!.SpawnpointsForced!],
            ["rezervbase"] = [.. locations.RezervBase.LooseLoot!.Value!.SpawnpointsForced!],
            ["shoreline"] = [.. locations.Shoreline.LooseLoot!.Value!.SpawnpointsForced!],
            ["tarkovstreets"] = [.. locations.TarkovStreets.LooseLoot!.Value!.SpawnpointsForced!],
            ["labyrinth"] = [.. locations.Labyrinth.LooseLoot!.Value!.SpawnpointsForced!],
            ["woods"] = [.. locations.Woods.LooseLoot!.Value!.SpawnpointsForced!],
            ["factory4_day"] = [.. locations.Factory4Day.LooseLoot!.Value!.SpawnpointsForced!],
            ["factory4_night"] = [.. locations.Factory4Night.LooseLoot!.Value!.SpawnpointsForced!],
            ["sandbox"] = [.. locations.Sandbox.LooseLoot!.Value!.SpawnpointsForced!],
            ["sandbox_high"] = [.. locations.SandboxHigh.LooseLoot!.Value!.SpawnpointsForced!]
        };
    }
}
