using System;
using System.Collections.Generic;
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

        private Lobby _joinedLobby;

        public Lobby JoinedLobby
        {
            get => _joinedLobby;
            set
            {
                _joinedLobby = value;
                if (_joinedLobby == null)
                {
                    UnsubscribeLobbyEvents();
                }
                else
                {
                    SubscribeLobbyEvents();
                }
            }
        }

        public Player Player { get; private set; }
        private LobbyEventCallbacks _lobbyEventCallbacks;
        public event Action OnLobbyPlayersUpdateRequested;
        public event Action OnPlayerKicked;

        public string PlayerNameKey { get; private set; } = "PlayerName";
        private const string PlayerBaseName = "Player";

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
        }

        private void SignedInHandler()
        {
            Debug.Log(AuthenticationService.Instance.PlayerId);
        }

        private void CreatePlayer()
        {
            var playerName = PlayerBaseName + Random.Range(1, 101);
            
            Player = new Player()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    { PlayerNameKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            };
        }

        private async void SubscribeLobbyEvents()
        {
            _lobbyEventCallbacks = new LobbyEventCallbacks();
            _lobbyEventCallbacks.LobbyChanged += OnLobbyChanged;
            _lobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;

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
                OnLobbyPlayersUpdateRequested?.Invoke();
            }
        }

        private void OnKickedFromLobby()
        {
            JoinedLobby = null;
            OnPlayerKicked?.Invoke();
        }
    }
}