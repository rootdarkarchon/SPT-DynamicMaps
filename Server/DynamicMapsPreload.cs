using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace _dynamicMapsServer;

// Follow QuestMap's final-database preload so other mods can finish adding loot first.
[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 1)]
public class DynamicMapsPreload(TemplateTable templates, LocationTable locations) : IOnLoad
{
    public QuestItemLocationLookup QuestItems { get; private set; } = new();

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        QuestItems = QuestItemLocationLookup.Build(
            locations.GetDictionary().Values,
            templates.Items.Where(pair => pair.Value.IsQuestItem()).Select(pair => pair.Key).ToHashSet(),
            cancellationToken);
        return Task.CompletedTask;
    }
}
