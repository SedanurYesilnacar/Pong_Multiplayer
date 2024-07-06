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
        [SerializeField] private LobbyUserController[] lobbyUserControllers;

        private Lobby _currentLobby;
        private bool _isOwnerHost;
        private string _ownerId;

        public void Init()
        {
            _ownerId = AuthenticationService.Instance.PlayerId;
            SetupLobby();
            
            leaveButton.onClick.AddListener(LeaveClickHandler);
            LobbyManager.Instance.OnLobbyPlayersUpdateRequested += OnLobbyPlayersUpdateRequestedHandler;
        }

        private void SetupLobby()
        {
            _currentLobby = LobbyManager.Instance.JoinedLobby;
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
                
                LobbyManager.Instance.JoinedLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
            
                leaveButton.onClick.RemoveAllListeners();
                LobbyManager.Instance.OnLobbyPlayersUpdateRequested -= OnLobbyPlayersUpdateRequestedHandler;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                if (e.Reason != LobbyExceptionReason.LobbyNotFound) menuTransitionManager.ShowNotification(e.Message);
            }
        }

        private void OnLobbyPlayersUpdateRequestedHandler()
        {
            UpdateLobbyPlayers();
        }
    }
}