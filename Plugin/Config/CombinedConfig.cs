using DynamicMaps.Common;
using DynamicMaps.Config;

namespace DynamicMaps.UI
{
    internal class CombinedConfig(ModConfig ServerConfig)
    {
        public bool ShowPlayerMarker => ServerConfig.AllowShowPlayerMarker && Settings.ShowPlayerMarker.Value;
        public bool ShowFriendlyPlayerMarkersInRaid => ServerConfig.AllowShowFriendlyPlayerMarkersInRaid && Settings.ShowFriendlyPlayerMarkersInRaid.Value;
        public bool ShowLockedDoorStatus => ServerConfig.AllowShowLockedDoorStatus && Settings.ShowLockedDoorStatus.Value;
        public bool ShowQuestsInRaid => ServerConfig.AllowShowQuestsInRaid && Settings.ShowQuestsInRaid.Value;
        public bool ShowExtractsInRaid => ServerConfig.AllowShowExtractsInRaid && Settings.ShowExtractsInRaid.Value;
        public bool ShowExtractsStatusInRaid => ServerConfig.AllowShowExtractStatusInRaid && Settings.ShowExtractStatusInRaid.Value;
        public bool ShowTransitPointsInRaid => ServerConfig.AllowShowTransitPointsInRaid && Settings.ShowTransitPointsInRaid.Value;
        public bool ShowSecretExtractsInRaid => ServerConfig.AllowShowSecretExtractsInRaid && Settings.ShowSecretPointsInRaid.Value;
        public bool RequireMapInInventory => ServerConfig.RequireMapInInventory || Settings.RequireMapInInventory.Value;
        public int ShowFriendlyIntelLevel => ServerConfig.ShowFriendlyIntelLevel > Settings.ShowFriendlyIntelLevel.Value ? ServerConfig.ShowFriendlyIntelLevel : Settings.ShowFriendlyIntelLevel.Value;
    }
}
