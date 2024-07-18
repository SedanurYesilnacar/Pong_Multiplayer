using _GameData.Scripts.Core;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class LobbyCanvas : MonoBehaviour, IInitializableCanvas
    {
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyCodeText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private LobbyUserController[] lobbyUserControllers;

        private LobbyManager _lobbyManager;
        private Lobby _currentLobby;
        private int _readyUserCount;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        public void Init()
        {
            SetupLobby();
        }

        private void SubscribeEvents()
        {
            if (!_lobbyManager) _lobbyManager = LobbyManager.Instance;
            
            leaveButton.onClick.AddListener(LeaveClickHandler);
            readyButton.onClick.AddListener(ReadyClickHandler);
            startGameButton.onClick.AddListener(StartGameClickHandler);
            _lobbyManager.OnLobbyPlayerDataChanged += OnLobbyPlayerDataChangedHandler;
            _lobbyManager.OnJoinedPlayersChanged += OnJoinedPlayersChangedHandler;
            _lobbyManager.OnGameStartPermissionChanged += OnGameStartPermissionChangedHandler;
        }

        private void UnsubscribeEvents()
        {
            leaveButton.onClick.RemoveAllListeners();
            readyButton.onClick.RemoveAllListeners();
            startGameButton.onClick.RemoveAllListeners();
            if (_lobbyManager == null) return;
            _lobbyManager.OnLobbyPlayerDataChanged -= OnLobbyPlayerDataChangedHandler;
            _lobbyManager.OnJoinedPlayersChanged -= OnJoinedPlayersChangedHandler;
            _lobbyManager.OnGameStartPermissionChanged -= OnGameStartPermissionChangedHandler;
        }
        
        private void SetupLobby()
        {
            _currentLobby = _lobbyManager.JoinedLobby;
            
            if (_currentLobby == null) HandleLobbySetupFail();
            else
            {
                lobbyNameText.text = _currentLobby.Name;
                SetLobbyCode(_currentLobby.LobbyCode, _currentLobby.IsPrivate);
                startGameButton.interactable = false;
            
                UpdateLobby();
                UpdateLobbyPlayers();
            }
        }

        private void UpdateLobby()
        {
            startGameButton.gameObject.SetActive(_lobbyManager.IsOwnerHost);
        }

        private void UpdateLobbyPlayers()
        {
            for (int i = 0; i < lobbyUserControllers.Length; i++)
            {
                if (i >= _currentLobby.Players.Count || _currentLobby.Players[i] == null) lobbyUserControllers[i].ResetUser();
                else
                {
                    var isUserHost = _lobbyManager.IsPlayerHost(_lobbyManager.JoinedLobby, _currentLobby.Players[i].Id);
                    lobbyUserControllers[i].ChangeUserType(_lobbyManager.IsOwnerHost, isUserHost);
                    lobbyUserControllers[i].UpdateUser(_currentLobby.Players[i]);
                }
            }
        }

        private void SetLobbyCode(string lobbyCode, bool isVisible)
        {
            lobbyCodeText.text = lobbyCode;
            lobbyCodeText.gameObject.SetActive(isVisible);
        }

        private void HandleLobbySetupFail()
        {
            Debug.LogError("Lobby setup failed");
            _currentLobby = null;
            _lobbyManager.OnMenuStateChangeRequested?.Invoke(MenuStates.MainMenu);
        }

        private void ChangeReadyStatus(bool isReady)
        {
            _lobbyManager.SetPlayerReady(isReady);
        }

        private void LeaveClickHandler()
        {
            _lobbyManager.RemovePlayerFromLobby();
        }
        
        private void ReadyClickHandler()
        {
            var currentReadyStatus = bool.Parse(_lobbyManager.Player.Data[_lobbyManager.PlayerReadyKey].Value);
            ChangeReadyStatus(!currentReadyStatus);
        }

        private void StartGameClickHandler()
        {
            _lobbyManager.SetGameStartData(true);
        }

        private void OnLobbyPlayerDataChangedHandler()
        {
            UpdateLobbyPlayers();
        }

        private void OnJoinedPlayersChangedHandler()
        {
            ChangeReadyStatus(false);
            UpdateLobby();
            UpdateLobbyPlayers();
        }

        private void OnGameStartPermissionChangedHandler(bool isGameStartAllowed)
        {
            startGameButton.interactable = isGameStartAllowed;
        }
    }
}