using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using DynamicMaps.Utils;
using UnityEngine;

// THIS IS HEAVILY BASED ON DRAKIAXYZ'S SPT-QuickMoveToContainer
namespace DynamicMaps.Config
{
    internal static class Settings
    {
        private static readonly List<ConfigEntryBase> ConfigEntries = [];

        #region General

        private const string GeneralTitle = "1. General";
        public static ConfigEntry<bool> ReplaceMapScreen;
        public static ConfigEntry<KeyboardShortcut> CenterOnPlayerHotkey;
        public static ConfigEntry<KeyboardShortcut> DumpInfoHotkey;
        public static ConfigEntry<KeyboardShortcut> MoveMapUpHotkey;
        public static ConfigEntry<KeyboardShortcut> MoveMapDownHotkey;
        public static ConfigEntry<KeyboardShortcut> MoveMapLeftHotkey;
        public static ConfigEntry<KeyboardShortcut> MoveMapRightHotkey;
        public static ConfigEntry<float> MapMoveHotkeySpeed;
        public static ConfigEntry<KeyboardShortcut> ChangeMapLevelUpHotkey;
        public static ConfigEntry<KeyboardShortcut> ChangeMapLevelDownHotkey;
        public static ConfigEntry<KeyboardShortcut> ZoomMapInHotkey;
        public static ConfigEntry<KeyboardShortcut> ZoomMapOutHotkey;
        public static ConfigEntry<float> ZoomMapHotkeySpeed;

        #endregion
        
        #region Dynamic Markers

        private const string DynamicMarkerTitle = "2. Dynamic Markers";
        public static ConfigEntry<bool> ShowPlayerMarker;
        public static ConfigEntry<bool> ShowFriendlyPlayerMarkersInRaid;
        public static ConfigEntry<bool> ShowLockedDoorStatus;
        public static ConfigEntry<bool> ShowQuestsInRaid;
        public static ConfigEntry<bool> ShowExtractsInRaid;
        public static ConfigEntry<bool> ShowExtractStatusInRaid;
        public static ConfigEntry<bool> ShowTransitPointsInRaid;
        public static ConfigEntry<bool> ShowSecretPointsInRaid;
        
        #endregion

        #region IntelCenter

        private const string ProgressionTitle = "3. Progression";
        public static ConfigEntry<bool> RequireMapInInventory;
        public static ConfigEntry<int> ShowFriendlyIntelLevel;
        
        #endregion
        
        #region In Raid

        private const string InRaidTitle = "4. In-Raid";
        public static ConfigEntry<bool> ResetZoomOnCenter;
        public static ConfigEntry<float> CenteringZoomResetPoint;
        public static ConfigEntry<float> ZoomMainMap;
        public static ConfigEntry<bool> AutoCenterOnPlayerMarker;
        public static ConfigEntry<bool> RetainMapPosition;
        public static ConfigEntry<bool> AutoSelectLevel;
        public static ConfigEntry<KeyboardShortcut> PeekShortcut;
        public static ConfigEntry<bool> HoldForPeek;

        #endregion

        public static ConfigEntry<bool> MapTransitionEnabled;

        #region Colors

        private const string MarkerColors = "6. Marker Colors";
        public static ConfigEntry<Color> PlayerColor;
        public static ConfigEntry<Color> ExtractDefaultColor;
        public static ConfigEntry<Color> ExtractOpenColor;
        public static ConfigEntry<Color> ExtractClosedColor;
        public static ConfigEntry<Color> ExtractHasRequirementsColor;
        public static ConfigEntry<Color> TransPointColor;
        public static ConfigEntry<Color> SecretPointColor;
        
        #endregion

        // public static ConfigEntry<KeyboardShortcut> KeyboardShortcut;

        public static void Init(ConfigFile config)
        {
            #region General

            ConfigEntries.Add(ReplaceMapScreen = config.Bind(
                GeneralTitle,
                "Replace Map Screen",
                true,
                new ConfigDescription(
                    "If the map should replace the BSG default map screen, requires swapping away from modded map to refresh",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(CenterOnPlayerHotkey = config.Bind(
                GeneralTitle,
                "Center on Player Hotkey",
                new KeyboardShortcut(KeyCode.Semicolon),
                new ConfigDescription(
                    "Pressed while the map is open, centers the player",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(MoveMapUpHotkey = config.Bind(
                GeneralTitle,
                "Move Map Up Hotkey",
                new KeyboardShortcut(KeyCode.UpArrow),
                new ConfigDescription(
                    "Hotkey to move the map up",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(MoveMapDownHotkey = config.Bind(
                GeneralTitle,
                "Move Map Down Hotkey",
                new KeyboardShortcut(KeyCode.DownArrow),
                new ConfigDescription(
                    "Hotkey to move the map down",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(MoveMapLeftHotkey = config.Bind(
                GeneralTitle,
                "Move Map Left Hotkey",
                new KeyboardShortcut(KeyCode.LeftArrow),
                new ConfigDescription(
                    "Hotkey to move the map left",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(MoveMapRightHotkey = config.Bind(
                GeneralTitle,
                "Move Map Right Hotkey",
                new KeyboardShortcut(KeyCode.RightArrow),
                new ConfigDescription(
                    "Hotkey to move the map right",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(MapMoveHotkeySpeed = config.Bind(
                GeneralTitle,
                "Move Map Hotkey Speed",
                0.25f,
                new ConfigDescription(
                    "How fast the map should move, units are map percent per second",
                    new AcceptableValueRange<float>(0.05f, 2f),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ChangeMapLevelUpHotkey = config.Bind(
                GeneralTitle,
                "Change Map Level Up Hotkey",
                new KeyboardShortcut(KeyCode.Period),
                new ConfigDescription(
                    "Hotkey to move the map level up (shift-scroll-up also does this in map screen)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ChangeMapLevelDownHotkey = config.Bind(
                GeneralTitle,
                "Change Map Level Down Hotkey",
                new KeyboardShortcut(KeyCode.Comma),
                new ConfigDescription(
                    "Hotkey to move the map level down (shift-scroll-down also does this in map screen)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ZoomMapInHotkey = config.Bind(
                GeneralTitle,
                "Zoom Map In Hotkey",
                new KeyboardShortcut(KeyCode.Equals),
                new ConfigDescription(
                    "Hotkey to zoom the map in (scroll-up also does this in map screen)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ZoomMapOutHotkey = config.Bind(
                GeneralTitle,
                "Zoom Map Out Hotkey",
                new KeyboardShortcut(KeyCode.Minus),
                new ConfigDescription(
                    "Hotkey to zoom the map out (scroll-down also does this in map screen)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ZoomMapHotkeySpeed = config.Bind(
                GeneralTitle,
                "Zoom Map Hotkey Speed",
                2.5f,
                new ConfigDescription(
                    "How fast the map should zoom by hotkey",
                    new AcceptableValueRange<float>(1f, 10f),
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(DumpInfoHotkey = config.Bind(
                GeneralTitle,
                "Dump Info Hotkey",
                new KeyboardShortcut(KeyCode.D, KeyCode.LeftShift, KeyCode.LeftAlt),
                new ConfigDescription(
                    "Pressed while the map is open, dumps json MarkerDefs for extracts, loot, and switches into root of plugin folder",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true })));

            #endregion

            #region Dynamic Markers

            ConfigEntries.Add(ShowPlayerMarker = config.Bind(
                DynamicMarkerTitle,
                "Show Player Marker",
                true,
                new ConfigDescription(
                    "If the player marker should be shown in raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowFriendlyPlayerMarkersInRaid = config.Bind(
                DynamicMarkerTitle,
                "Show Friendly Player Markers",
                true,
                new ConfigDescription(
                    "If friendly player markers should be shown in-raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowLockedDoorStatus = config.Bind(
                DynamicMarkerTitle,
                "Show Locked Door Status",
                true,
                new ConfigDescription(
                    "If locked door markers should be updated with status based on key acquisition (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowQuestsInRaid = config.Bind(
                DynamicMarkerTitle,
                "Show Quests In Raid",
                true,
                new ConfigDescription(
                    "If quests should be shown in raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowExtractsInRaid = config.Bind(
                DynamicMarkerTitle,
                "Show Extracts In Raid",
                true,
                new ConfigDescription(
                    "If extracts should be shown in raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowExtractStatusInRaid = config.Bind(
                DynamicMarkerTitle,
                "Show Extracts Status In Raid",
                true,
                new ConfigDescription(
                    "If extracts should be colored according to their status in raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(ShowTransitPointsInRaid = config.Bind(
                DynamicMarkerTitle,
                "Show Transit Points In Raid",
                true,
                new ConfigDescription(
                    "If transits should be shown in raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowSecretPointsInRaid = config.Bind(
                DynamicMarkerTitle,
                "Show Secret Extracts In Raid",
                true,
                new ConfigDescription(
                    "If secret extracts should be shown in raid (can be overridden by server)",
                    null,
                    new ConfigurationManagerAttributes { })));

            #endregion
            
            #region Progression

            ConfigEntries.Add(RequireMapInInventory = config.Bind(
                ProgressionTitle,
                "Require a map in your inventory",
                false,
                new ConfigDescription(
                    "Requires you to have a map in your inventory in order to view the map in raid (can be overridden by server).",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ShowFriendlyIntelLevel = config.Bind(
                ProgressionTitle,
                "Intel level required to show friendly PMCs",
                0,
                new ConfigDescription(
                    "If intel level is at or above this value it will show friendly PMCs (can be overridden by server)",
                    new AcceptableValueRange<int>(0, 3),
                    new ConfigurationManagerAttributes { })));

            #endregion
            
            #region In Raid

             ConfigEntries.Add(AutoSelectLevel = config.Bind(
                InRaidTitle,
                "Auto Select Level",
                true,
                new ConfigDescription(
                    "If the level should be automatically selected based on the players position in raid",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(AutoCenterOnPlayerMarker = config.Bind(
                InRaidTitle,
                "Auto Center On Player Marker",
                false,
                new ConfigDescription(
                    "If the player marker should be centered when showing the map in raid (Conflicts with 'Remember Map Position')",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ResetZoomOnCenter = config.Bind(
                InRaidTitle,
                "Reset Zoom On Center",
                false,
                new ConfigDescription(
                    "If the zoom level should be reset each time that the map is opened while in raid (Conflicts with 'Remember Map Position')",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(RetainMapPosition = config.Bind(
                InRaidTitle,
                "Remember Map Position",
                true,
                new ConfigDescription(
                    "Should we remember the map position (Map position memory is only maintained for the current raid) (Conflicts with 'Auto Center On Player Marker' and 'Reset Zoom On Center')",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(CenteringZoomResetPoint = config.Bind(
                InRaidTitle,
                "Centering On Player Zoom Level",
                0.15f,
                new ConfigDescription(
                    "What zoom level should be used as while centering on the player (0 is fully zoomed out, and 1 is fully zoomed in)",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(ZoomMainMap = config.Bind(
                InRaidTitle,
                "Main map zoom",
                0f,
                new ConfigDescription(
                    "What zoom level should be used for the main map. (Tab view/Peek view) (0 is fully zoomed out, and 1 is fully zoomed in)",
                    new AcceptableValueRange<float>(0f, 15f),
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(PeekShortcut = config.Bind(
                InRaidTitle,
                "Peek at Map Shortcut",
                new KeyboardShortcut(KeyCode.M),
                new ConfigDescription(
                    "The keyboard shortcut to peek at the map",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(HoldForPeek = config.Bind(
                InRaidTitle,
                "Hold for Peek",
                true,
                new ConfigDescription(
                    "If the shortcut should be held to keep it open. If disabled, button toggles",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(MapTransitionEnabled = config.Bind(
                InRaidTitle,
                "Peek Transition enabled",
                true,
                new ConfigDescription(
                    "Enable the map transition animations (When disabled everything will snap)",
                    null,
                    new ConfigurationManagerAttributes { })));

            #endregion

            AutoCenterOnPlayerMarker.SettingChanged += OnAutoOrCenterEnable;
            ResetZoomOnCenter.SettingChanged += OnAutoOrCenterEnable;
            RetainMapPosition.SettingChanged += OnPositionRetainEnable;

            #region MarkerColors

            ConfigEntries.Add(PlayerColor = config.Bind(
                MarkerColors,
                "Your player marker color",
                new Color(0, 1, 0),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(ExtractDefaultColor = config.Bind(
                MarkerColors,
                "Extract default marker color",
                new Color(1f, 0.92f, 0.01f),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(ExtractOpenColor = config.Bind(
                MarkerColors,
                "Extract open marker color",
                new Color(0f, 1f, 0f),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(ExtractClosedColor = config.Bind(
                MarkerColors,
                "Extract closed marker color",
                new Color(1f, 0f, 0f),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(ExtractHasRequirementsColor = config.Bind(
                MarkerColors,
                "Extract has requirements marker color",
                new Color(1f, 0.92f, 0.01f),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));
            
            ConfigEntries.Add(TransPointColor = config.Bind(
                MarkerColors,
                "Transit point marker color",
                new Color(1f, 0.62f, 0.20f),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(SecretPointColor = config.Bind(
                MarkerColors,
                "Secret exfil point marker color",
                new Color(0.1f, 0.6f, 0.6f),
                new ConfigDescription(
                    "Color of the marker",
                    null,
                    new ConfigurationManagerAttributes { })));

            #endregion

            RecalcOrder();
        }
        
        private static void RecalcOrder()
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            var settingOrder = ConfigEntries.Count;
            foreach (var entry in ConfigEntries)
            {
                var attributes = entry.Description.Tags[0] as ConfigurationManagerAttributes;
                if (attributes != null)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
        
        private static void OnAutoOrCenterEnable(object sender, EventArgs e)
        {
            if (AutoCenterOnPlayerMarker.Value || ResetZoomOnCenter.Value)
            {
                RetainMapPosition.Value = false;
            }
        }
        
        private static void OnPositionRetainEnable(object sender, EventArgs e)
        {
            if (RetainMapPosition.Value)
            {
                AutoCenterOnPlayerMarker.Value = false;
                ResetZoomOnCenter.Value = false;
            }
        }
    }
}
