using DynamicMaps.Common;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace _dynamicMapsServer;

public sealed class QuestItemLocationLookup
{
    private readonly Dictionary<MongoId, HashSet<SpawnLocation>> _locations = new();

    private sealed record SpawnLocation(string MapName, float X, float Y, float Z);

    public static QuestItemLocationLookup Build(
        IEnumerable<Location> locations,
        IReadOnlySet<MongoId> questItemIds,
        CancellationToken cancellationToken = default)
    {
        var result = new QuestItemLocationLookup();
        foreach (var location in locations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mapName = location?.Base?.Id;
            if (string.IsNullOrWhiteSpace(mapName)) continue;

            // As in QuestMap, inspect every template item in every forced loose-loot spawn.
            // Keep variant IDs: client map definitions group day/night and level variants.
            foreach (var point in location!.LooseLoot?.Value?.SpawnpointsForced ?? [])
            {
                var position = point.Template?.Position;
                if (position is not { } coordinates
                    || !float.IsFinite(coordinates.X) || !float.IsFinite(coordinates.Y) || !float.IsFinite(coordinates.Z)) continue;

                foreach (var item in point.Template?.Items ?? [])
                {
                    if (!questItemIds.Contains(item.Template)) continue;
                    if (!result._locations.TryGetValue(item.Template, out var spawns))
                        result._locations[item.Template] = spawns = [];
                    spawns.Add(new SpawnLocation(mapName.ToLowerInvariant(), coordinates.X, coordinates.Y, coordinates.Z));
                }
            }
        }
        return result;
    }

    public List<ConditionData> GetQuestItems(
        IEnumerable<QuestStatus> profileQuests,
        IReadOnlyDictionary<MongoId, Quest> templates)
    {
        List<ConditionData> result = [];
        foreach (var progress in profileQuests.Where(q => q.Status == QuestStatusEnum.Started))
        {
            if (!templates.TryGetValue(progress.QId, out var quest)) continue;
            foreach (var condition in quest.Conditions?.AvailableForFinish ?? [])
            {
                if (condition.ConditionType != "FindItem"
                    || (progress.CompletedConditions?.Contains(condition.Id) ?? false)
                    || condition.Target is null) continue;

                var targets = condition.Target.IsList ? condition.Target.List ?? [] : [condition.Target.Item!];
                foreach (var target in targets.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(target) || !_locations.TryGetValue(target, out var spawns)) continue;
                    foreach (var spawn in spawns.OrderBy(p => p.MapName, StringComparer.Ordinal)
                                 .ThenBy(p => p.X).ThenBy(p => p.Y).ThenBy(p => p.Z))
                    {
                        result.Add(new ConditionData(progress.QId, condition.Id, target)
                        {
                            MapName = spawn.MapName,
                            SpawnPoint = [spawn.X, spawn.Y, spawn.Z]
                        });
                    }
                }
            }
        }
        return result;
    }
}
