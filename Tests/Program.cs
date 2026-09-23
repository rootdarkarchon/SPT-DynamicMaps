using _dynamicMapsServer;
using Mono.Cecil;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json;
using Path = System.IO.Path;
using System.Text.Json;

var checks = 0;
void Check(bool value, string message)
{
    if (!value) throw new Exception(message);
    checks++;
    Console.WriteLine($"PASS {message}");
}

MongoId itemA = "111111111111111111111111", itemB = "222222222222222222222222";
MongoId ordinary = "333333333333333333333333", questId = "444444444444444444444444";
MongoId conditionId = "555555555555555555555555", otherQuestId = "666666666666666666666666";
Spawnpoint Spawn(float x, params MongoId[] items) => new()
{
    Template = new SpawnpointTemplate
    {
        Position = new Vector3 { X = x, Y = 2, Z = 3 },
        Items = items.Select(id => new SptLootItem { Id = id, Template = id }).ToList()
    }
};
Location Map(string id, params Spawnpoint[] spawns) => new()
{
    Base = new LocationBase { Id = id },
    LooseLoot = new LazyLoad<LooseLoot>(() => new LooseLoot { SpawnpointsForced = spawns }, cacheValue: false)
};
var lookup = QuestItemLocationLookup.Build(
    [Map("bigmap", Spawn(1, ordinary, itemA), Spawn(5, itemA), Spawn(5, itemA), Spawn(9, itemB),
        Spawn(float.NaN, itemA), new Spawnpoint()), Map("woods", Spawn(7, itemA)),
        Map("sandbox", Spawn(11, itemB)), Map("sandbox_high", Spawn(11, itemB)), new Location()],
    new HashSet<MongoId> { itemA, itemB });
var condition = new QuestCondition
{
    Id = conditionId,
    DynamicLocale = false,
    ConditionType = "FindItem",
    Target = new ListOrT<string>([itemA, itemB, ordinary, itemA], null)
};
var templates = new Dictionary<MongoId, Quest>
{
    [questId] = new() { Id = questId, Name = "Test", Description = "Test", TraderId = otherQuestId, Location = "any", Image = "", Type = default, Restartable = false, Side = "Pmc", CanShowNotificationsInGame = true, Conditions = new() { AvailableForFinish = [condition] } }
};
QuestStatus Active() => new() { QId = questId, StartTime = 0, StatusTimers = [], Status = QuestStatusEnum.Started };
var result = lookup.GetQuestItems([Active()], templates);
Check(result.Count == 6, "All targets, maps and positions survive; duplicate and invalid spawns are excluded");
Check(result.Count(x => x.ItemId == itemA && x.MapName == "bigmap") == 2, "Secondary template item and multiple positions on one map");
Check(result.Any(x => x.ItemId == itemA && x.MapName == "woods"), "One quest item on multiple maps");
Check(result.Any(x => x.MapName == "sandbox") && result.Any(x => x.MapName == "sandbox_high"), "Map variant IDs preserved");
Check(result.All(x => x.ItemId != ordinary), "Ordinary loot excluded");
var unrelated = new QuestStatus { QId = otherQuestId, StartTime = 0, StatusTimers = [], Status = QuestStatusEnum.Started, CompletedConditions = [conditionId] };
Check(lookup.GetQuestItems([Active(), unrelated], templates).Count == 6, "Another quest's completed condition does not hide locations");
var completed = Active(); completed.CompletedConditions = [conditionId];
Check(lookup.GetQuestItems([completed], templates).Count == 0, "Completed objective excluded");
var finished = Active(); finished.Status = QuestStatusEnum.Success;
Check(lookup.GetQuestItems([finished], templates).Count == 0, "Finished quest excluded");
condition.Target = new ListOrT<string>(null, itemB);
Check(lookup.GetQuestItems([Active()], templates).Count == 3, "Scalar target supported");
Check(new QuestItemLocationLookup().GetQuestItems([Active()], templates).Count == 0, "Missing spawns produce no empty marker records");
var roundTrip = JsonSerializer.Deserialize<List<DynamicMaps.Common.ConditionData>>(JsonSerializer.Serialize(result))!;
Check(roundTrip.Count == result.Count && roundTrip.All(x => x.SpawnPoint.Length == 3 && x.QuestId == questId), "Route DTO round trip retains every position");

var install = args.Length > 0 ? args[0] : @"D:\Tarkov-SPT-4.1";
using var eft = AssemblyDefinition.ReadAssembly(Path.Combine(install, "EscapeFromTarkov_Data", "Managed", "Assembly-CSharp.dll"));
var types = eft.MainModule.Types.ToDictionary(t => t.FullName);
void Field(string type, string name, string fieldType) => Check(
    types[type].Fields.Any(f => f.Name == name && f.FieldType.FullName == fieldType), $"ABI field {type}.{name}");
void Method(string type, string name, params string[] parameters) => Check(
    types[type].Methods.Count(m => m.Name == name && m.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(parameters)) == 1,
    $"ABI method {type}.{name}");
Field("EFT.TarkovApplication", "_menuOperation", "EFT.MainMenuShowOperation");
Field("EFT.UI.InventoryScreen", "_mapScreen", "EFT.UI.Map.MapScreen");
Field("EFT.Airdrop.ClientAirDrop", "_syncObject", "EFT.SynchronizableObjects.AirdropSynchronizableObject");
Field("EFT.UI.ItemContextInteractionsSwitcher", "_item", "EFT.InventoryLogic.Item");
Field("EFT.Player", "Corpse", "EFT.Interactive.Corpse");
Field("EFT.Player", "LastAggressor", "EFT.IPlayer");
Method("EFT.Airdrop.ClientAirDrop", "CloseParachute");
Method("EFT.UI.ItemContextInteractionsSwitcher", "IsActive", "EFT.InventoryLogic.EItemInfoButton");
Method("EFT.UI.Map.MapScreen", "Show", "EFT.InventoryLogic.InventoryController", "EFT.InventoryLogic.CompoundItem");
Method("EFT.UI.Map.MapScreen", "Close");
Method("EFT.Version", "Create", "System.String", "System.String", "System.String", "System.String");
Method("EFT.GameWorld", "OnGameStarted");
Method("EFT.GameWorld", "OnDestroy");
Method("EFT.GameWorld", "UnregisterPlayer", "EFT.IPlayer");
Method("EFT.GameWorld", "DestroyLoot", "IKillable");
Method("EFT.Player", "OnDead", "EFT.EDamageType");
Method("EFT.UI.CommonUI", "Awake");
Method("EFT.UI.BattleUIScreen`2", "Show", "EFT.GamePlayerOwner");
Method("EFT.Interactive.LootItem", "Init", "EFT.InventoryLogic.Item", "System.String", "EFT.GameWorld", "System.Boolean", "EFT.MongoID[]", "System.String", "System.Boolean");
var playerInventory = types["EFT.Player"].NestedTypes.Single(t => t.Name == "PlayerInventoryController");
Check(playerInventory.Methods.Count(m => m.Name == "ThrowItem") == 1
      && playerInventory.Fields.Any(f => f.Name == "Player" && f.FieldType.FullName == "EFT.Player"), "ABI player inventory throw hook");

if (args.Contains("--database"))
{
    var database = Path.Combine(install, "SPT_Runtime", "SPT_Data", "database");
    using var itemDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(database, "templates", "items.json")));
    var questItems = itemDocument.RootElement.EnumerateObject()
        .Where(p => p.Value.TryGetProperty("_props", out var props)
                    && props.TryGetProperty("QuestItem", out var flag) && flag.ValueKind == JsonValueKind.True)
        .Select(p => new MongoId(p.Name)).ToHashSet();
    List<Location> locations = [];
    foreach (var file in Directory.EnumerateFiles(Path.Combine(database, "locations"), "looseLoot.json", SearchOption.AllDirectories))
    {
        using var loot = JsonDocument.Parse(File.ReadAllText(file));
        List<Spawnpoint> points = [];
        foreach (var point in loot.RootElement.GetProperty("spawnpointsForced").EnumerateArray())
        {
            var template = point.GetProperty("template");
            var position = template.GetProperty("Position");
            points.Add(new Spawnpoint
            {
                Template = new SpawnpointTemplate
                {
                    Position = new Vector3 { X = position.GetProperty("x").GetSingle(), Y = position.GetProperty("y").GetSingle(), Z = position.GetProperty("z").GetSingle() },
                    Items = template.GetProperty("Items").EnumerateArray().Select(i => new SptLootItem
                    {
                        Id = new MongoId(i.GetProperty("_id").GetString()!),
                        Template = new MongoId(i.GetProperty("_tpl").GetString()!)
                    }).ToList()
                }
            });
        }
        using var mapBase = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(file)!, "base.json")));
        locations.Add(Map(mapBase.RootElement.GetProperty("Id").GetString()!, points.ToArray()));
    }
    var realLookup = QuestItemLocationLookup.Build(locations, questItems);
    condition.Target = new ListOrT<string>(questItems.Select(id => id.ToString()).ToList(), null);
    var allLocations = realLookup.GetQuestItems([Active()], templates);
    Check(allLocations.Count > 0 && allLocations.All(x => questItems.Contains(x.ItemId)), "Installed database produces only quest-item locations");
    using var questDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(database, "templates", "quests.json")));
    using var locale = JsonDocument.Parse(File.ReadAllText(Path.Combine(database, "locales", "global", "en.json")));
    var verifiedQuests = 0;
    foreach (var quest in questDocument.RootElement.EnumerateObject())
    {
        if (!quest.Value.TryGetProperty("name", out var nameKey)
            || !locale.RootElement.TryGetProperty(nameKey.GetString()!, out var localized)) continue;
        var name = localized.GetString();
        var expected = name switch { "You've Got Mail" => 4, "Getting Acquainted" => 23, _ => 0 };
        if (expected == 0) continue;
        var targets = quest.Value.GetProperty("conditions").GetProperty("AvailableForFinish").EnumerateArray()
            .Where(c => c.GetProperty("conditionType").GetString() == "FindItem")
            .SelectMany(c => c.GetProperty("target").ValueKind == JsonValueKind.Array
                ? c.GetProperty("target").EnumerateArray().Select(t => t.GetString()!)
                : [c.GetProperty("target").GetString()!]).ToList();
        condition.Target = new ListOrT<string>(targets, null);
        Check(realLookup.GetQuestItems([Active()], templates).Count == expected, $"Installed database: {name} retains all {expected} positions");
        verifiedQuests++;
    }
    Check(verifiedQuests == 2, "Both installed-database quest regressions exercised");
    Console.WriteLine($"Installed database: {questItems.Count} quest-item templates, {allLocations.Count} distinct item/map/position records.");
}
Console.WriteLine($"{checks} regression and client ABI checks passed.");
