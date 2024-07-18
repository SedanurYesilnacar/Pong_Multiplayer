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
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameData.Scripts.Core
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        
        public Lobby JoinedLobby
        {
            get => _joinedLobby;
            private set
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
        public bool IsOwnerHost => IsPlayerHost(JoinedLobby, PlayerId);
        private QueryLobbiesOptions QueryLobbiesOptions { get; set; }
        private JoinLobbyByIdOptions JoinLobbyByIdOptions { get; set; }
        private JoinLobbyByCodeOptions JoinLobbyByCodeOptions { get; set; }
        private QuickJoinLobbyOptions QuickJoinLobbyOptions { get; set; }
        private CreateLobbyOptions CreateLobbyOptions { get; set; }

        private bool IsGameStartAllowed
        {
            set
            {
                if (_isGameStartAllowed != value) OnGameStartPermissionChanged?.Invoke(value);
                _isGameStartAllowed = value;
            }
        }

        private string PlayerId { get; set; }
        
        private const string PlayerBaseName = "Player";
        private const string LobbyStartKey = "IsGameStarted";

        private const float HeartbeatTriggerInterval = 25f; // LobbyActiveLifespan is 30s in dashboard
        private WaitForSeconds _heartbeatTimer;
        private Coroutine _heartbeatRoutine;

        private Lobby _joinedLobby;
        private int _readyPlayerCount;
        private bool _isGameStartAllowed;
        private bool _isLobbyCreating;
        private bool _isPlayerUpdating;
        
        private LobbyEventCallbacks _lobbyEventCallbacks;
        public event Action OnPlayerKicked;
        public event Action OnLobbyPlayerDataChanged;
        public event Action OnJoinedPlayersChanged;
        public event Action<bool> OnGameStartPermissionChanged; // IsGameStartAllowed
        public event Action<List<Lobby>> OnLobbyListUpdated; // CurrentLobbyList
        public Action<MenuStates> OnMenuStateChangeRequested; // RequestedMenuState
        public event Action<string> OnNotificationPopupRequested; // NotificationMessage

        private void Awake()
        {
            InitSingleton();

            _heartbeatTimer = new WaitForSeconds(HeartbeatTriggerInterval);
        }

        private void SubscribeEvents()
        {
            AuthenticationService.Instance.SignedIn += SignedInHandler;
        }

        private void UnsubscribeEvents()
        {
            AuthenticationService.Instance.SignedIn -= SignedInHandler;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
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
            SubscribeEvents();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            CreatePlayer();
            CreateLobbyJoinOptions();
            
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
                    { PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
                    { PlayerReadyKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false") }
                }
            };
        }

        private void CreateLobbyJoinOptions()
        {
            QueryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 25,
                Filters = new List<QueryFilter>() { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder>() { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
            };

            JoinLobbyByIdOptions = new JoinLobbyByIdOptions() { Player = Player };
            JoinLobbyByCodeOptions = new JoinLobbyByCodeOptions() { Player = Player };
            QuickJoinLobbyOptions = new QuickJoinLobbyOptions() { Player = Player };
            CreateLobbyOptions = new CreateLobbyOptions() { IsPrivate = false, Player = Player };
        }

        public bool IsPlayerHost(Lobby targetLobby, string playerId)
        {
            return targetLobby.HostId == playerId;
        }

        public string GetHostName(Lobby lobby)
        {
            for (int i = 0; i < lobby.Players.Count; i++)
            {
                var currentPlayer = lobby.Players[i];
                if (IsPlayerHost(lobby, currentPlayer.Id)) return currentPlayer.Data[PlayerNameKey].Value;
            }

            return String.Empty;
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

        public async void QuickPlay()
        {
            try
            {
                JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(QuickJoinLobbyOptions);
                OnMenuStateChangeRequested?.Invoke(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                OnNotificationPopupRequested?.Invoke(e.Message);
            }
        }

        public async void JoinLobbyById(Lobby lobbyToJoin)
        {
            try
            {
                JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyToJoin.Id, JoinLobbyByIdOptions);
                OnMenuStateChangeRequested?.Invoke(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                OnNotificationPopupRequested?.Invoke(e.Message);
            }
        }

        public async void JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, JoinLobbyByCodeOptions);
                OnMenuStateChangeRequested?.Invoke(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                OnNotificationPopupRequested?.Invoke(e.Message);
            }
        }

        public async void CreateLobby(LobbyCreateOptions lobbyCreateOptions)
        {
            if (_isLobbyCreating) return;
            _isLobbyCreating = true;

            CreateLobbyOptions.IsPrivate = lobbyCreateOptions.LobbyAccessibilityType == LobbyAccessibilityType.Private;

            try
            {
                JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyCreateOptions.LobbyName, 2, CreateLobbyOptions);
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
                if (JoinedLobby != null) yield return LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
            }

            _heartbeatRoutine = null;
        }

        public async void SetPlayerReady(bool isReady)
        {
            if (_isPlayerUpdating) return;
            _isPlayerUpdating = true;
            
            try
            {
                Player.Data[PlayerReadyKey] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady.ToString().ToLower());
                UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions() { Data = Player.Data };
               
                await Lobbies.Instance.UpdatePlayerAsync(JoinedLobby.Id, PlayerId, updatePlayerOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }

            _isPlayerUpdating = false;
        }

        public async void RemovePlayerFromLobby()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, PlayerId);
                
                JoinedLobby = null;
                OnMenuStateChangeRequested?.Invoke(MenuStates.MainMenu);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                OnNotificationPopupRequested?.Invoke(e.Message);
            }
        }

        /*private async void CreateRelay()
        {
            try
            {
                await RelayService.Instance.CreateAllocationAsync(1);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }*/

        private void UpdateGameStartPermission()
        {
            if (!IsOwnerHost || JoinedLobby.Players.Count < JoinedLobby.MaxPlayers)
            {
                IsGameStartAllowed = false;
                return;
            }

            if (JoinedLobby.Players.Any(player => !bool.Parse(player.Data[PlayerReadyKey].Value)))
            {
                IsGameStartAllowed = false;
                return;
            }

            IsGameStartAllowed = true;
        }

        public async void SetGameStartData(bool isStarted)
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { LobbyStartKey, new DataObject(DataObject.VisibilityOptions.Member, isStarted.ToString().ToLower()) }
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

        private void ResetLobbyForGameStart()
        {
            for (int i = 0; i < JoinedLobby.Players.Count; i++)
            {
                JoinedLobby.Players[i].Data[PlayerReadyKey].Value = "false";
            }
            
            if (IsOwnerHost) SetGameStartData(false);
        }

        private void ResetLobbyReadyCount()
        {
            Player.Data[PlayerReadyKey].Value = "false";
        }

        private void OnLobbyChanged(ILobbyChanges lobbyChanges)
        {
            if (!lobbyChanges.LobbyDeleted)
            {
                lobbyChanges.ApplyToLobby(JoinedLobby);
            }

            if (lobbyChanges.PlayerLeft.Changed || lobbyChanges.PlayerJoined.Changed)
            {
                ResetLobbyReadyCount();
                OnJoinedPlayersChanged?.Invoke();
            }

            TryStartGame();
        }

        private void TryStartGame()
        {
            if (JoinedLobby.Data == null) return;
            if (!JoinedLobby.Data.ContainsKey(LobbyStartKey)) return;
            if (bool.Parse(JoinedLobby.Data[LobbyStartKey].Value) == false) return;

            Debug.Log("--- GAME STARTING ---");
            LoadingCanvas.Instance.Init();
            ResetLobbyForGameStart();
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
            if (IsOwnerHost) return;
            
            JoinedLobby = null;
            OnPlayerKicked?.Invoke();
        }

        private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj)
        {
            Debug.Log("OnPlayerDataChanged");
            OnLobbyPlayerDataChanged?.Invoke();
            
            UpdateGameStartPermission();
        }
    }
}