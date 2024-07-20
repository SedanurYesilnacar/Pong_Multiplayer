using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GameData.Scripts.UI;
using _GameData.Scripts.UI.MenuUI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
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
                    ResetReadyStatus();
                    UnsubscribeLobbyEvents();
                }
                else
                {
                    UnsubscribeLobbyEvents();
                    SubscribeLobbyEvents();
                }
            }
        }

        private string PlayerId { get; set; }
        public Player Player { get; private set; }
        public string PlayerNameKey => "PlayerName";
        public string PlayerReadyKey => "IsPlayerReady";
        public bool IsOwnerHost => IsPlayerHost(JoinedLobby, PlayerId);
        private QueryLobbiesOptions QueryLobbiesOptions { get; set; }
        private JoinLobbyByIdOptions JoinLobbyByIdOptions { get; set; }
        private JoinLobbyByCodeOptions JoinLobbyByCodeOptions { get; set; }
        private QuickJoinLobbyOptions QuickJoinLobbyOptions { get; set; }
        private CreateLobbyOptions CreateLobbyOptions { get; set; }

        private const string PlayerBaseName = "Player";
        private const string IsGameReadyToStartKey = "IsGameReadyToStart";
        private const string RelayCodeKey = "RelayCode";

        private const float HeartbeatTriggerInterval = 25f; // LobbyActiveLifespan is 30s in dashboard
        private WaitForSeconds _heartbeatTimer;
        private Coroutine _heartbeatRoutine;

        private Lobby _joinedLobby;
        private int _readyPlayerCount;
        private bool _isLobbyCreating;
        private bool _isPlayerUpdating;
        private bool _isGameStarting;
        
        private LobbyEventCallbacks _lobbyEventCallbacks;
        public event Action OnPlayerKicked;
        public event Action OnLobbyPlayerDataChanged;
        public event Action OnJoinedPlayersChanged;
        public event Action<bool> OnGameStartPermissionChanged; // IsGameStartAllowed
        public event Action<List<Lobby>> OnLobbyListUpdated; // CurrentLobbyList
        public event Action<string> OnNotificationPopupRequested; // NotificationMessage
        public Action<MenuStates> OnMenuStateChangeRequested; // RequestedMenuState

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
            
            (LobbyService.Instance as ILobbyServiceSDKConfiguration)?.EnableLocalPlayerLobbyEvents(true);
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
            CreateLobbyOptions = new CreateLobbyOptions() { IsPrivate = false, Player = Player, Data = new Dictionary<string, DataObject>()
            {
                { IsGameReadyToStartKey, new DataObject(DataObject.VisibilityOptions.Member, "false") },
                { RelayCodeKey, new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }};
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
        }

        private async void CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                Debug.Log("StartingHost");
                NetworkManager.Singleton.StartHost();
                SetRelayCode(joinCode);
                _isGameStarting = false;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }

        private async void JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                
                Debug.Log("StartingClient");
                NetworkManager.Singleton.StartClient();
                _isGameStarting = false;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
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
            
            Player.Data[PlayerReadyKey] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady.ToString().ToLower());
            UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions() { Data = Player.Data };
            
            try
            {
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

        private void UpdateGameStartPermission()
        {
            var isLobbyFull = JoinedLobby.Players.Count == JoinedLobby.MaxPlayers;
            var isAllPlayersReady = JoinedLobby.Players.All(player => bool.Parse(player.Data[PlayerReadyKey].Value));
            
            if (!IsOwnerHost || !isLobbyFull || !isAllPlayersReady) OnGameStartPermissionChanged?.Invoke(false);
            else OnGameStartPermissionChanged?.Invoke(true);
        }

        private async void UpdateLobbyData(LobbyData lobbyData)
        {
            Debug.Log("Updating Lobby Data: " + lobbyData.DataKey + " " + lobbyData.DataValue);
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { lobbyData.DataKey, new DataObject(lobbyData.Visibility, lobbyData.DataValue) }
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

        public void TriggerGameStart(bool isGameReadyToStart)
        {
            if (isGameReadyToStart && _isGameStarting) return;
            _isGameStarting = true;
            
            LobbyData lobbyData = new LobbyData
            {
                DataKey = IsGameReadyToStartKey,
                DataValue = isGameReadyToStart.ToString().ToLower(),
                Visibility = DataObject.VisibilityOptions.Member
            };

            UpdateLobbyData(lobbyData);
        }

        private void SetRelayCode(string relayCode)
        {
            LobbyData lobbyData = new LobbyData
            {
                DataKey = RelayCodeKey,
                DataValue = relayCode,
                Visibility = DataObject.VisibilityOptions.Member
            };

            UpdateLobbyData(lobbyData);
        }

        private void ResetLobbyForGameStart()
        {
            for (int i = 0; i < JoinedLobby.Players.Count; i++)
            {
                JoinedLobby.Players[i].Data[PlayerReadyKey].Value = "false";
            }

            if (IsOwnerHost)
            {
                TriggerGameStart(false);
                SetRelayCode("0");
            }
        }

        private void ResetReadyStatus()
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
                ResetReadyStatus();
                OnJoinedPlayersChanged?.Invoke();
            }

            if (lobbyChanges.Data.Changed)
            {
                var isGameReadyToStart = bool.Parse(JoinedLobby.Data[IsGameReadyToStartKey].Value);
                if (isGameReadyToStart)
                {
                    if (IsOwnerHost) TryStartGame();
                }

                if (!IsOwnerHost && lobbyChanges.Data.Value.ContainsKey(RelayCodeKey))
                {
                    TryJoinGame(lobbyChanges.Data.Value[RelayCodeKey].Value.Value);
                }
            }
        }

        private void PrepareGameStart()
        {
            Debug.Log("--- GAME STARTING ---");
            LoadingCanvas.Instance.Init();
            ResetLobbyForGameStart();
        }

        private void TryStartGame()
        {
            PrepareGameStart();
            CreateRelay();
        }

        private void TryJoinGame(string relayCode)
        {
            Debug.Log("Trying to join the game with relay code : " + relayCode);
            if (relayCode == "0") return;
            
            PrepareGameStart();
            JoinRelay(relayCode);
        }

        private void OnKickedFromLobby()
        {
            if (IsOwnerHost) return;
            
            JoinedLobby = null;
            OnPlayerKicked?.Invoke();
        }

        private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj)
        {
            OnLobbyPlayerDataChanged?.Invoke();
            UpdateGameStartPermission();
        }
    }

    public struct LobbyData
    {
        public string DataKey;
        public string DataValue;
        public DataObject.VisibilityOptions Visibility;
    }
}