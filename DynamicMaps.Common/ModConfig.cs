namespace DynamicMaps.Common
{

    public class ModConfig
    {
        public bool AllowShowPlayerMarker { get; set; } = true;
        public bool AllowShowFriendlyPlayerMarkersInRaid { get; set; } = true;
        public bool AllowShowLockedDoorStatus { get; set; } = true;
        public bool AllowShowQuestsInRaid { get; set; } = true;
        public bool AllowShowExtractsInRaid { get; set; } = true;
        public bool AllowShowExtractStatusInRaid { get; set; } = true;
        public bool AllowShowTransitPointsInRaid { get; set; } = true;
        public bool AllowShowSecretExtractsInRaid { get; set; } = true;
        public bool RequireMapInInventory { get; set; } = false;
        public int ShowFriendlyIntelLevel { get; set; } = 0;
    }
}