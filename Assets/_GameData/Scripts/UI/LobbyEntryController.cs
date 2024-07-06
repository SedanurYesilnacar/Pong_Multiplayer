using System;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class LobbyEntryController : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyOwnerNameText;
        [SerializeField] private Button lobbyJoinButton;

        private MenuTransitionManager _menuTransitionManager;
        private Lobby _currentLobby;

        public void Init(MenuTransitionManager menuTransitionManager, Lobby lobbyInfo)
        {
            _menuTransitionManager = menuTransitionManager;
            _currentLobby = lobbyInfo;
            
            lobbyNameText.text = _currentLobby.Name;
            lobbyOwnerNameText.text = _currentLobby.Players[0].Id;
            
            lobbyJoinButton.onClick.AddListener(LobbyJoinClickHandler);
        }

        private async void LobbyJoinClickHandler()
        {
            try
            {
                _menuTransitionManager.CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(_currentLobby.Id);
                _menuTransitionManager.ChangeState(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                _menuTransitionManager.ShowNotification(e.Message);
            }
        }
    }
}