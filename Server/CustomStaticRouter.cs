using DynamicMaps.Common;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Text.Json;

namespace _dynamicMapsServer;

[Injectable(InjectionType.Singleton)]
public class CustomStaticRouter : StaticRouter
{
    private static ModConfig? _modConfig;

    public CustomStaticRouter(JsonUtil jsonUtil, TemplateTable templates, SaveServer saveServer,
        DynamicMapsPreload preload, ISptLogger<CustomStaticRouter> logger)
        : base(jsonUtil,
        [
            new RouteAction<EmptyRequestData>(Routes.LoadConfigRoute,
                (url, info, session, output, cancellationToken) => new ValueTask<string>(JsonSerializer.Serialize(_modConfig))),
            new RouteAction<EmptyRequestData>(Routes.GetQuestItemsForMap,
                (url, info, session, output, cancellationToken) => HandleMapData(session, templates, saveServer, preload, logger))
        ])
    {
    }

    public void PassConfig(ModConfig config) => _modConfig = config;

    private static ValueTask<string> HandleMapData(MongoId session, TemplateTable templates,
        SaveServer saveServer, DynamicMapsPreload preload, ISptLogger<CustomStaticRouter> logger)
    {
        try
        {
            var quests = saveServer.GetProfile(session).CharacterData?.PmcData?.Quests ?? [];
            return new ValueTask<string>(JsonSerializer.Serialize(preload.QuestItems.GetQuestItems(quests, templates.Quests)));
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
            return new ValueTask<string>("[]");
        }
    }
}
