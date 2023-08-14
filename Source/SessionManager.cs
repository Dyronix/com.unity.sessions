using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Sessions
{
    /// <summary>
    /// This class uses a unique player ID to bind a player to a session. Once that player connects to a host, the host
    /// associates the current ClientID to the player's unique ID. If the player disconnects and reconnects to the same
    /// host, the session is preserved.
    /// </summary>
    
    /// <remarks>
    /// Using a client-generated player ID and sending it directly could be problematic, as a malicious user could
    /// intercept it and reuse it to impersonate the original user. We are currently investigating this to offer a
    /// solution that handles security better.
    /// </remarks>
    
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T> : BaseSessionManager
        where T : struct, ISessionPlayerData
    {
        //--------------------------------------------------------------------------------------
        // Fields
        /// <summary>
        /// Maps a given client player id to the data for a given client player.
        /// </summary>
        private Dictionary<string, T> _client_data;

        /// <summary>
        /// Map to allow us to cheaply map from player id to player data.
        /// </summary>
        private Dictionary<ulong, string> _client_id_to_player_id;

        /// <summary>
        /// Has the session already started?
        /// </summary>
        private bool _has_session_started;

        //--------------------------------------------------------------------------------------
        public SessionManager()
        {
            _client_data = new Dictionary<string, T>();
            _client_id_to_player_id = new Dictionary<ulong, string>();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Handles client disconnect."
        /// </summary>
        public override void DisconnectClient(ulong clientId)
        {
            if (_has_session_started)
            {
                // Mark client as disconnected, but keep their data so they can reconnect.
                if (_client_id_to_player_id.TryGetValue(clientId, out var playerId))
                {
                    if (GetPlayerData(playerId)?.ClientID == clientId)
                    {
                        var client_data = _client_data[playerId];
                        client_data.IsConnected = false;
                        _client_data[playerId] = client_data;
                    }
                }
            }
            else
            {
                // Session has not started, no need to keep their data
                if (_client_id_to_player_id.TryGetValue(clientId, out var playerId))
                {
                    _client_id_to_player_id.Remove(clientId);
                    if (GetPlayerData(playerId)?.ClientID == clientId)
                    {
                        _client_data.Remove(playerId);
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Check if the player already has a connection running
        /// </summary>
        /// <param name="playerId">This is the playerId that is unique to this client and persists across multiple logins from the same client</param>
        /// <returns>True if a player with this ID is already connected.</returns>
        public override bool IsDuplicateConnection(string playerId)
        {
            return _client_data.ContainsKey(playerId) && _client_data[playerId].IsConnected;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Adds a connecting player's session data if it is a new connection, or updates their session data in case of a reconnection.
        /// </summary>
        /// <param name="clientId">This is the clientId that Netcode assigned us on login. It does not persist across multiple logins from the same client. </param>
        /// <param name="playerId">This is the playerId that is unique to this client and persists across multiple logins from the same client</param>
        /// <param name="sessionPlayerData">The player's initial data</param>
        public void SetupConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData)
        {
            var is_reconnecting = false;

            // Test for duplicate connection
            if (IsDuplicateConnection(playerId))
            {
                Debug.LogError($"Player ID {playerId} already exists. This is a duplicate connection. Rejecting this session data.");
                return;
            }

            // If another client exists with the same playerId
            if (_client_data.ContainsKey(playerId))
            {
                if (!_client_data[playerId].IsConnected)
                {
                    // If this connecting client has the same player Id as a disconnected client, this is a reconnection.
                    is_reconnecting = true;
                }

            }

            // Reconnecting. Give data from old player to new player
            if (is_reconnecting)
            {
                // Update player session data
                sessionPlayerData = _client_data[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            //Populate our dictionaries with the SessionPlayerData
            _client_id_to_player_id[clientId] = playerId;
            _client_data[playerId] = sessionPlayerData;

            OnPlayerDataSetup?.Invoke(clientId, playerId, sessionPlayerData);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the player id
        /// </summary>
        /// <param name="clientId"> id of the client whose data is requested</param>
        /// <returns>The Player ID matching the given client ID</returns>
        public override string GetPlayerId(ulong clientId)
        {
            if (_client_id_to_player_id.TryGetValue(clientId, out string player_id))
            {
                return player_id;
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve player data from clientId
        /// </summary>
        /// <param name="clientId"> id of the client whose data is requested</param>
        /// <returns>Player data struct matching the given ID</returns>
        public T? GetPlayerData(ulong clientId)
        {
            //First see if we have a playerId matching the clientID given.
            var player_id = GetPlayerId(clientId);
            if (player_id != null)
            {
                return GetPlayerData(player_id);
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve player data from playerId
        /// </summary>
        /// <param name="playerId"> Player ID of the client whose data is requested</param>
        /// <returns>Player data struct matching the given ID</returns>
        public T? GetPlayerData(string playerId)
        {
            if (_client_data.TryGetValue(playerId, out T data))
            {
                return data;
            }

            Debug.Log($"No PlayerData of matching player ID found: {playerId}");
            return null;
        }

        /// <summary>
        /// Updates player data
        /// </summary>
        /// <param name="clientId"> id of the client whose data will be updated </param>
        /// <param name="sessionPlayerData"> new data to overwrite the old </param>
        public void SetPlayerData(ulong clientId, T sessionPlayerData)
        {
            if (_client_id_to_player_id.TryGetValue(clientId, out string playerId))
            {
                _client_data[playerId] = sessionPlayerData;

                OnPlayerDataSet?.Invoke(clientId, sessionPlayerData);
            }
            else
            {
                Debug.LogError($"No client player ID found mapped to the given client ID: {clientId}");
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Marks the current session as started, so from now on we keep the data of disconnected players.
        /// </summary>
        public override void OnSessionStarted()
        {
            _has_session_started = true;

            OnSessionStarted?.Invoke();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Reinitializes session data from connected players, and clears data from disconnected players, so that if they reconnect in the next game, they will be treated as new players
        /// </summary>
        public override void OnSessionEnded()
        {
            ClearDisconnectedPlayersData();
            ReinitializePlayersData();

            _has_session_started = false;

            OnSessionEnded?.Invoke();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Resets all our runtime state, so it is ready to be reinitialized when starting a new server
        /// </summary>
        public override void OnServerEnded()
        {
            _client_data.Clear();
            _client_id_to_player_id.Clear();

            _has_session_started = false;
        }

        //--------------------------------------------------------------------------------------
        private void ReinitializePlayersData()
        {
            foreach (var id in _client_id_to_player_id.Keys)
            {
                string player_id = _client_id_to_player_id[id];
                T session_player_data = _client_data[player_id];
                session_player_data.Reinitialize();
                _client_data[player_id] = session_player_data;
            }
        }

        //--------------------------------------------------------------------------------------
        private void ClearDisconnectedPlayersData()
        {
            List<ulong> ids_to_clear = new List<ulong>();
            foreach (var id in _client_id_to_player_id.Keys)
            {
                var data = GetPlayerData(id);
                if (data is { IsConnected: false })
                {
                    ids_to_clear.Add(id);
                }
            }

            foreach (var id in ids_to_clear)
            {
                string player_id = _client_id_to_player_id[id];
                if (GetPlayerData(player_id)?.ClientID == id)
                {
                    _client_data.Remove(player_id);
                }

                _client_id_to_player_id.Remove(id);
            }
        }
    }
}