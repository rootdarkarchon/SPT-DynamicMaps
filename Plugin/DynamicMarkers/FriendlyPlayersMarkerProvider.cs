using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using DynamicMaps.Data;
using DynamicMaps.Patches;
using DynamicMaps.UI;
using DynamicMaps.UI.Components;
using DynamicMaps.Utils;
using EFT;
using UnityEngine;

namespace DynamicMaps.DynamicMarkers
{
    public class FriendlyPlayersMarkerProvider : IDynamicMarkerProvider
    {
        private const string _friendlyPlayerCategory = "Friendly Player";
        private const string _friendlyPlayerImagePath = "Markers/arrow.png";
        private static readonly Color _friendlyPlayerColor = Color.Lerp(Color.blue, Color.white, 0.5f);

        private MapView _lastMapView;
        private Dictionary<Player, PlayerMapMarker> _playerMarkers = [];

        public void OnShowInRaid(MapView map)
        {
            _lastMapView = map;
            
            TryAddMarkers();
            RemoveNonActivePlayers();

            // register to event to get all the new ones while map is showing
            Singleton<GameWorld>.Instance.OnPersonAdd += TryAddMarker;

            // register to events to get when players die
            GameWorldUnregisterPlayerPatch.OnUnregisterPlayer += OnUnregisterPlayer;
            PlayerOnDeadPatch.OnDead += TryRemoveMarker;
        }

        public void OnHideInRaid(MapView map)
        {
            // unregister from events while map isn't showing
            Singleton<GameWorld>.Instance.OnPersonAdd -= TryAddMarker;
            GameWorldUnregisterPlayerPatch.OnUnregisterPlayer -= OnUnregisterPlayer;
            PlayerOnDeadPatch.OnDead -= TryRemoveMarker;
        }

        public void OnRaidEnd(MapView map)
        {
            // unregister from events since map is ending
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld is not null)
            {
                gameWorld.OnPersonAdd -= TryAddMarker;
            }

            GameWorldUnregisterPlayerPatch.OnUnregisterPlayer -= OnUnregisterPlayer;
            PlayerOnDeadPatch.OnDead -= TryRemoveMarker;

            TryRemoveMarkers();
        }

        public void OnMapChanged(MapView map, MapDef mapDef)
        {
            _lastMapView = map;

            foreach (var player in _playerMarkers.Keys.ToList())
            {
                TryRemoveMarker(player);
                TryAddMarker(player);
            }
        }

        public void OnDisable(MapView map)
        {
            // unregister from events since provider is being disabled
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld is not null)
            {
                gameWorld.OnPersonAdd -= TryAddMarker;
            }

            GameWorldUnregisterPlayerPatch.OnUnregisterPlayer -= OnUnregisterPlayer;
            PlayerOnDeadPatch.OnDead -= TryRemoveMarker;

            TryRemoveMarkers();
        }

        private void TryRemoveMarkers()
        {
            foreach (var player in _playerMarkers.Keys.ToList())
            {
                TryRemoveMarker(player);
            }

            _playerMarkers.Clear();
        }

        private void TryAddMarkers()
        {
            if (!GameUtils.IsInRaid())
            {
                return;
            }
            
            // add all players that have spawned already in raid
            var gameWorld = Singleton<GameWorld>.Instance;
            foreach (var player in gameWorld.AllAlivePlayersList)
            {
                if (player.IsYourPlayer || _playerMarkers.ContainsKey(player))
                {
                    continue;
                }

                TryAddMarker(player);
            }
        }

        private void OnUnregisterPlayer(IPlayer iPlayer)
        {
            var player = iPlayer as Player;
            if (player is null)
            {
                return;
            }

            TryRemoveMarker(player);
        }

        private void RemoveNonActivePlayers()
        {
            var alivePlayers = new HashSet<Player>(Singleton<GameWorld>.Instance.AllAlivePlayersList);
            foreach (var player in _playerMarkers.Keys.ToList())
            {
                if (player.HasCorpse() || !alivePlayers.Contains(player))
                {
                    TryRemoveMarker(player);
                }
            }
        }

        public void RefreshMarkers()
        {
            if (!GameUtils.IsInRaid()) return;

            foreach (var player in _playerMarkers.ToArray())
            {
                if (player.Key.IsYourPlayer) continue;
                
                TryRemoveMarker(player.Key);
                TryAddMarker(player.Key);
            }
        }
        
        private void TryAddMarker(IPlayer iPlayer)
        {
            var player = iPlayer as Player;
            if (player is null || player.IsYourPlayer || player.IsHeadlessClient())
            {
                return;
            }

            if (_lastMapView is null || _playerMarkers.ContainsKey(player))
            {
                return;
            }
            
            if (!player.IsGroupedWithMainPlayer()
                || !(ModdedMapScreen._config.ShowFriendlyIntelLevel <= GameUtils.GetIntelLevel()))
            {
                return;
            }

            var marker = _lastMapView.AddPlayerMarker(player, _friendlyPlayerCategory,
                _friendlyPlayerColor, _friendlyPlayerImagePath);
            _playerMarkers[player] = marker;
        }

        private void TryRemoveMarker(Player player)
        {
            if (!_playerMarkers.ContainsKey(player))
            {
                return;
            }

            _playerMarkers[player].ContainingMapView.RemoveMapMarker(_playerMarkers[player]);
            _playerMarkers.Remove(player);
        }

        public void OnShowOutOfRaid(MapView map)
        {
            // do nothing
        }

        public void OnHideOutOfRaid(MapView map)
        {
            // do nothing
        }
    }
}
