using System;
using _GameData.Scripts.Core;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
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
        private bool _isOwnerHost;
        private string _ownerId;

        public void Init()
        {
            if (!_lobbyManager) _lobbyManager = LobbyManager.Instance;
            _ownerId = AuthenticationService.Instance.PlayerId;
            SetupLobby();
            
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            leaveButton.onClick.AddListener(LeaveClickHandler);
            readyButton.onClick.AddListener(ReadyClickHandler);
            _lobbyManager.OnLobbyPlayersUpdateRequested += OnLobbyPlayersUpdateRequestedHandler;
            _lobbyManager.OnPlayerKicked += OnPlayerKickedHandler;
            Debug.Log("OnLobbyPlayersUpdateRequested subscribed");
        }

        private void UnsubscribeEvents()
        {
            leaveButton.onClick.RemoveAllListeners();
            readyButton.onClick.RemoveAllListeners();
            _lobbyManager.OnLobbyPlayersUpdateRequested -= OnLobbyPlayersUpdateRequestedHandler;
            _lobbyManager.OnPlayerKicked -= OnPlayerKickedHandler;
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

            _isOwnerHost = IsPlayerHost(_ownerId);
            lobbyNameText.text = _currentLobby.Name;
            if (!_currentLobby.IsPrivate) lobbyCodeText.gameObject.SetActive(false);
            else lobbyCodeText.text = "Lobby Code: " + _currentLobby.LobbyCode;
            startGameButton.gameObject.SetActive(_isOwnerHost);

            if (lobbyUserControllers.Length < _currentLobby.MaxPlayers)
            {
                Debug.LogError("Lobby user count can not be less than " + _currentLobby.MaxPlayers);
                _currentLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
                return;
            }
            
            UpdateLobbyPlayers();
        }

        private void UpdateLobbyPlayers()
        {
            for (int i = 0; i < lobbyUserControllers.Length; i++)
            {
                if (i >= _currentLobby.Players.Count || _currentLobby.Players[i] == null) lobbyUserControllers[i].ResetUser();
                else
                {
                    var isUserHost = IsPlayerHost(_currentLobby.Players[i].Id);
                    lobbyUserControllers[i].ChangeUserType(_isOwnerHost, isUserHost);
                    lobbyUserControllers[i].UpdateUser(_currentLobby.Players[i]);
                }
            }
        }

        private bool IsPlayerHost(string playerId)
        {
            return _currentLobby.HostId == playerId;
        }

        private async void LeaveClickHandler()
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, _ownerId);
                
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
        
        private async void ReadyClickHandler()
        {
            readyButton.interactable = false;
            
            try
            {
                var currentReadyStatus = _lobbyManager.Player.Data[_lobbyManager.PlayerReadyKey].Value == "true";
                _lobbyManager.Player.Data[_lobbyManager.PlayerReadyKey] = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, currentReadyStatus ? "false" : "true");
                UpdatePlayerOptions updatePlayerOptions = new UpdatePlayerOptions()
                {
                    AllocationId = _lobbyManager.Player.AllocationId,
                    ConnectionInfo = _lobbyManager.Player.ConnectionInfo,
                    Data = _lobbyManager.Player.Data
                };
               
                Debug.Log("-----" + updatePlayerOptions.Data[_lobbyManager.PlayerReadyKey].Value);
                await Lobbies.Instance.UpdatePlayerAsync(_lobbyManager.JoinedLobby.Id, _ownerId, updatePlayerOptions);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }

            readyButton.interactable = true;
        }

        private void OnLobbyPlayersUpdateRequestedHandler()
        {
            UpdateLobbyPlayers();
        }

        private void OnPlayerKickedHandler()
        {
            UnsubscribeEvents();
        }
    }
}