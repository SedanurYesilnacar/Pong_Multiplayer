using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GameData.Scripts.UI;
using _GameData.Scripts.UI.MenuUI;
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
        private QueryLobbiesOptions QueryLobbiesOptions { get; set; }
        public string PlayerNameKey { get; private set; } = "PlayerName";
        public string PlayerReadyKey { get; private set; } = "IsPlayerReady";
        public string LobbyStartKey { get; private set; } = "IsGameStarted";
        public string LobbyHostName { get; private set; } = "HostName";
        public int ReadyPlayerCount { get; private set; } = 0;
        public bool IsGameStartAllowed { get; private set; } = false;
        public string PlayerId { get; private set; }
        public bool IsOwnerHost => IsPlayerHost(PlayerId);
        
        private int _readyPlayerCount;
        private const string PlayerBaseName = "Player";

        private const float HeartbeatTriggerInterval = 25f; // LobbyActiveLifespan is 30s in dashboard
        private WaitForSeconds _heartbeatTimer;
        private Coroutine _heartbeatRoutine;

        private bool _isLobbyCreating;
        
        private LobbyEventCallbacks _lobbyEventCallbacks;
        public event Action OnPlayerKicked;
        public event Action OnLobbyPlayerDataChanged;
        public event Action OnJoinedPlayersChanged;
        public event Action OnGameStartPermissionChanged;
        public event Action<List<Lobby>> OnLobbyListUpdated; // CurrentLobbyList
        public Action<MenuStates> OnMenuStateChangeRequested; // RequestedMenuState
        public event Action<string> OnNotificationPopupRequested; // NotificationMessage

        private void Awake()
        {
            InitSingleton();

            _heartbeatTimer = new WaitForSeconds(HeartbeatTriggerInterval);
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
            CreateQueryLobbiesOptions();
            
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

        private void CreateQueryLobbiesOptions()
        {
            QueryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 25,
                Filters = new List<QueryFilter>() { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder>() { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
            };
        }
        
        public bool IsPlayerHost(string playerId)
        {
            return JoinedLobby.HostId == playerId;
        }

        private async void SubscribeLobbyEvents()
        {
            _lobbyEventCallbacks = new LobbyEventCallbacks();
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

            _lobbyEventCallbacks.LobbyChanged -= OnLobbyChanged;
            _lobbyEventCallbacks.KickedFromLobby -= OnKickedFromLobby;
            _lobbyEventCallbacks.PlayerDataChanged -= OnPlayerDataChanged;
            Debug.Log("unsubscribed");
        }

        public async void QueryLobbies()
        {
            try
            {
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(QueryLobbiesOptions);
                var uniqueQueryResponseResults = queryResponse.Results.Distinct().ToList();
          
                OnLobbyListUpdated?.Invoke(uniqueQueryResponseResults);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }
        }

        public async void CreateLobby(LobbyCreateOptions lobbyCreateOptions)
        {
            if (_isLobbyCreating) return;
            _isLobbyCreating = true;
            
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = lobbyCreateOptions.LobbyAccessibilityType == LobbyAccessibilityType.Private,
                Player = Player
            };
            
            try
            {
                JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyCreateOptions.LobbyName, 2, lobbyOptions);
                if (_heartbeatRoutine == null) _heartbeatRoutine = StartCoroutine(HeartbeatRoutine());
                OnMenuStateChangeRequested?.Invoke(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                OnNotificationPopupRequested?.Invoke(e.Message);
            }

            _isLobbyCreating = false;
        }

        private IEnumerator HeartbeatRoutine()
        {
            while (JoinedLobby != null)
            {
                yield return _heartbeatTimer;
                yield return LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
            }

            _heartbeatRoutine = null;
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

        public async void SetGameStartData(bool isStarted)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { LobbyStartKey, new DataObject(DataObject.VisibilityOptions.Member, isStarted ? "true" : "false") }
                }
            };
            
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, updateLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }

        private void ResetLobbyReadyCount()
        {
            Player.Data[PlayerReadyKey].Value = "false";
            ReadyPlayerCount = 0;
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
            
            if (lobbyChanges.Data.Value != null && lobbyChanges.Data.Value.TryGetValue(LobbyStartKey, out var gameStartData))
            {
                if (gameStartData.Value.Value == "true")
                {
                    StartGame();
                }
            }
        }

        private void StartGame()
        {
            LoadingCanvas.Instance.Init();
            Debug.Log("--- GAME STARTING ---");
            if (IsOwnerHost)
            {
                SetGameStartData(false);
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }
            
        }

        private void OnKickedFromLobby()
        {
            if (IsOwnerHost) return;
            
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