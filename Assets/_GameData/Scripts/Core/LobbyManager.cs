using System;
using System.Collections.Generic;
using _GameData.Scripts.UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameData.Scripts.Core
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        private Lobby _joinedLobby;

        public Lobby JoinedLobby
        {
            get => _joinedLobby;
            set
            {
                _joinedLobby = value;
                if (_joinedLobby == null)
                {
                    ResetLobbyReadyCount();
                    UnsubscribeLobbyEvents();
                }
                else
                {
                    SubscribeLobbyEvents();
                }
            }
        }

        public Player Player { get; private set; }
        public string PlayerNameKey { get; private set; } = "PlayerName";
        public string PlayerReadyKey { get; private set; } = "IsPlayerReady";
        public string LobbyStartKey { get; private set; } = "IsGameStarted";
        public int ReadyPlayerCount { get; private set; } = 0;
        public bool IsGameStartAllowed { get; private set; } = false;
        public string PlayerId { get; private set; }
        public bool IsOwnerHost => IsPlayerHost(PlayerId);
        
        private int _readyPlayerCount;
        private const string PlayerBaseName = "Player";
        
        private LobbyEventCallbacks _lobbyEventCallbacks;
        public event Action OnPlayerKicked;
        public event Action OnLobbyPlayerDataChanged;
        public event Action OnJoinedPlayersChanged;
        public event Action OnGameStartPermissionChanged;

        private void Awake()
        {
            InitSingleton();
        }
        
        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        private async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += SignedInHandler;
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            CreatePlayer();
            
            (LobbyService.Instance as ILobbyServiceSDKConfiguration).EnableLocalPlayerLobbyEvents(true);
        }

        private void SignedInHandler()
        {
            PlayerId = AuthenticationService.Instance.PlayerId;
        }

        private void CreatePlayer()
        {
            var playerName = PlayerBaseName + Random.Range(1, 101);
            
            Player = new Player()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    { PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                    { PlayerReadyKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                }
            };
        }
        
        public bool IsPlayerHost(string playerId)
        {
            return JoinedLobby.HostId == playerId;
        }

        private async void SubscribeLobbyEvents()
        {
            _lobbyEventCallbacks = new LobbyEventCallbacks();
            _lobbyEventCallbacks.DataAdded += OnDataAdded;
            _lobbyEventCallbacks.LobbyChanged += OnLobbyChanged;
            _lobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;
            _lobbyEventCallbacks.PlayerDataChanged += OnPlayerDataChanged;

            try
            {
                await Lobbies.Instance.SubscribeToLobbyEventsAsync(JoinedLobby.Id, _lobbyEventCallbacks);
            }
            catch (LobbyServiceException e)
            {
                switch (e.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{JoinedLobby.Id}]. We did not need to try and subscribe again. Exception Message: {e.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {e.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}"); throw;
                }
            }
        }

        private void UnsubscribeLobbyEvents()
        {
            if (_lobbyEventCallbacks == null) return;

            _lobbyEventCallbacks.DataAdded -= OnDataAdded;
            _lobbyEventCallbacks.LobbyChanged -= OnLobbyChanged;
            _lobbyEventCallbacks.KickedFromLobby -= OnKickedFromLobby;
            _lobbyEventCallbacks.PlayerDataChanged -= OnPlayerDataChanged;
            Debug.Log("unsubscribed");
        }

        private async void CreateRelay()
        {
            try
            {
                await RelayService.Instance.CreateAllocationAsync(1);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
        
        private void CheckGameStartAllowed()
        {
            if (!IsOwnerHost || JoinedLobby.Players.Count < JoinedLobby.MaxPlayers)
            {
                IsGameStartAllowed = false;
                OnGameStartPermissionChanged?.Invoke();
                return;
            }

            var playersInLobby = JoinedLobby.Players;
            for (int i = 0; i < playersInLobby.Count; i++)
            {
                if (playersInLobby[i].Data[PlayerReadyKey].Value == "false")
                {
                    IsGameStartAllowed = false;
                    OnGameStartPermissionChanged?.Invoke();
                    return;
                }
            }

            IsGameStartAllowed = true;
            OnGameStartPermissionChanged?.Invoke();
        }

        private void ResetLobbyReadyCount()
        {
            Player.Data[PlayerReadyKey].Value = "false";
            ReadyPlayerCount = 0;
        }

        private void OnDataAdded(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changedOrRemovedLobbyValues)
        {
            if (changedOrRemovedLobbyValues.ContainsKey(LobbyStartKey))
            {
                StartGame();
            }
        }

        private void OnLobbyChanged(ILobbyChanges lobbyChanges)
        {
            if (lobbyChanges.LobbyDeleted)
            {
                
            }
            else
            {
                lobbyChanges.ApplyToLobby(JoinedLobby);
            }

            if (lobbyChanges.PlayerLeft.Changed || lobbyChanges.PlayerJoined.Changed)
            {
                ResetLobbyReadyCount();
                OnJoinedPlayersChanged?.Invoke();
            }
        }

        private void StartGame()
        {
            LoadingCanvas.Instance.Init();
            Debug.Log("--- GAME STARTING ---");
            if (IsOwnerHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
        }

        private void OnKickedFromLobby()
        {
            JoinedLobby = null;
            OnPlayerKicked?.Invoke();
        }

        private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj)
        {
            Debug.Log("player data changed");
            OnLobbyPlayerDataChanged?.Invoke();
            
            CheckGameStartAllowed();
        }
    }
}