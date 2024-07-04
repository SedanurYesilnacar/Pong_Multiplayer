using System;
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
        [SerializeField] private LobbyUserController[] lobbyUserControllers;

        private Lobby _currentLobby;

        private void OnValidate()
        {
            menuTransitionManager = FindObjectOfType<MenuTransitionManager>();
        }

        public void Init()
        {
            SetupLobby();
        }

        public void SetupLobby()
        {
            _currentLobby = menuTransitionManager.CurrentLobby;
            if (_currentLobby == null)
            {
                Debug.LogError("Lobby null");
                _currentLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
                return;
            }

            var isOwnerHost = IsPlayerHost(AuthenticationService.Instance.PlayerId);
            lobbyNameText.text = _currentLobby.Name;
            if (!_currentLobby.IsPrivate) lobbyCodeText.gameObject.SetActive(false);
            else lobbyCodeText.text = "Lobby Code: " + _currentLobby.LobbyCode;
            startGameButton.gameObject.SetActive(isOwnerHost);

            if (lobbyUserControllers.Length < _currentLobby.MaxPlayers)
            {
                Debug.LogError("Lobby user count can not be less than " + _currentLobby.MaxPlayers);
                _currentLobby = null;
                menuTransitionManager.ChangeState(MenuStates.MainMenu);
                return;
            }

            for (int i = 0; i < _currentLobby.Players.Count; i++)
            {
                var isUserHost = IsPlayerHost(_currentLobby.Players[i].Id);
                lobbyUserControllers[i].ChangeUserType(isOwnerHost, isUserHost);
                lobbyUserControllers[i].SetPlayerCredentials(_currentLobby.Players[i].Data["PlayerName"].Value);
            }
        }

        private bool IsPlayerHost(string playerId)
        {
            return _currentLobby.HostId == playerId;
        }
    }
}