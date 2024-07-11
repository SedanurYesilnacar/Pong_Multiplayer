using System.Collections.Generic;
using _GameData.Scripts.Core;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class LobbyCanvas : MonoBehaviour, IInitializableCanvas
    {
        [SerializeField] private MenuTransitionManager menuTransitionManager;
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyCodeText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private LobbyUserController[] lobbyUserControllers;

        private LobbyManager _lobbyManager;
        private Lobby _currentLobby;
        private int _readyUserCount;

        public void Init()
        {
            if (!_lobbyManager) _lobbyManager = LobbyManager.Instance;
            SetupLobby();
            
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            leaveButton.onClick.AddListener(LeaveClickHandler);
            readyButton.onClick.AddListener(ReadyClickHandler);
            startGameButton.onClick.AddListener(StartGameClickHandler);
            _lobbyManager.OnLobbyPlayerDataChanged += OnLobbyPlayerDataChangedHandler;
            _lobbyManager.OnJoinedPlayersChanged += OnJoinedPlayersChangedHandler;
            _lobbyManager.OnPlayerKicked += OnPlayerKickedHandler;
            _lobbyManager.OnGameStartPermissionChanged += OnGameStartPermissionChangedHandler;
            Debug.Log("OnLobbyPlayersUpdateRequested subscribed");
        }

        private void UnsubscribeEvents()
        {
            leaveButton.onClick.RemoveAllListeners();
            readyButton.onClick.RemoveAllListeners();
            startGameButton.onClick.RemoveAllListeners();
            _lobbyManager.OnLobbyPlayerDataChanged -= OnLobbyPlayerDataChangedHandler;
            _lobbyManager.OnJoinedPlayersChanged -= OnJoinedPlayersChangedHandler;
            _lobbyManager.OnPlayerKicked -= OnPlayerKickedHandler;
            _lobbyManager.OnGameStartPermissionChanged -= OnGameStartPermissionChangedHandler;
            Debug.Log("OnLobbyPlayersUpdateRequested unsubscribed");
        }
        
        private void SetupLobby()
        {
            _currentLobby = _lobbyManager.JoinedLobby;
            if (_currentLobby == null)
            {
                Debug.LogError("Lobby null");
                _currentLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
                return;
            }

            lobbyNameText.text = _currentLobby.Name;
            if (!_currentLobby.IsPrivate) lobbyCodeText.gameObject.SetActive(false);
            else lobbyCodeText.text = "Lobby Code: " + _currentLobby.LobbyCode;
            startGameButton.interactable = false;
            UpdateLobby();
            
            if (lobbyUserControllers.Length < _currentLobby.MaxPlayers)
            {
                Debug.LogError("Lobby user count can not be less than " + _currentLobby.MaxPlayers);
                _currentLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
                return;
            }
            
            UpdateLobbyPlayers();
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
                    var isUserHost = _lobbyManager.IsPlayerHost(_currentLobby.Players[i].Id);
                    lobbyUserControllers[i].ChangeUserType(_lobbyManager.IsOwnerHost, isUserHost);
                    lobbyUserControllers[i].UpdateUser(_currentLobby.Players[i]);
                }
            }
        }

        private async void ChangeReadyStatus(bool isReady)
        {
            readyButton.interactable = false;
            
            try
            {
                _lobbyManager.Player.Data[_lobbyManager.PlayerReadyKey] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady ? "true" : "false");
                UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
                {
                    AllocationId = _lobbyManager.Player.AllocationId,
                    ConnectionInfo = _lobbyManager.Player.ConnectionInfo,
                    Data = _lobbyManager.Player.Data
                };
               
                Debug.Log("-----" + updatePlayerOptions.Data[_lobbyManager.PlayerReadyKey].Value);
                await Lobbies.Instance.UpdatePlayerAsync(_lobbyManager.JoinedLobby.Id, _lobbyManager.PlayerId, updatePlayerOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }

            readyButton.interactable = true;
        }

        private async void LeaveClickHandler()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, _lobbyManager.PlayerId);
                
                _lobbyManager.JoinedLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
            
                UnsubscribeEvents();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                if (e.Reason != LobbyExceptionReason.LobbyNotFound) menuTransitionManager.ShowNotification(e.Message);
            }
        }
        
        private void ReadyClickHandler()
        {
            var currentReadyStatus = _lobbyManager.Player.Data[_lobbyManager.PlayerReadyKey].Value == "true";
            ChangeReadyStatus(!currentReadyStatus);
        }

        private async void StartGameClickHandler()
        {
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { _lobbyManager.LobbyStartKey, new DataObject(DataObject.VisibilityOptions.Member, "true") }
                }
            };
            
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(_lobbyManager.JoinedLobby.Id, updateLobbyOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
            }
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

        private void OnPlayerKickedHandler()
        {
            UnsubscribeEvents();
        }

        private void OnGameStartPermissionChangedHandler()
        {
            Debug.Log("IsGameStartAllowed " + _lobbyManager.IsGameStartAllowed);
            startGameButton.interactable = _lobbyManager.IsGameStartAllowed;
        }
    }
}