using System;
using _GameData.Scripts.Core;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class LobbyUserController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Button kickButton;
        [SerializeField] private GameObject readyIcon;
        [SerializeField] private GameObject hostIcon;

        private const string DefaultPlayerName = "Waiting...";
        private Player _currentPlayer;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            kickButton.onClick.AddListener(KickClickHandler);
        }

        private void UnsubscribeEvents()
        {
            kickButton.onClick.RemoveAllListeners();
        }

        public void ResetUser()
        {
            _currentPlayer = null;
            
            playerNameText.text = DefaultPlayerName;
            kickButton.gameObject.SetActive(false);
            readyIcon.SetActive(false);
            hostIcon.SetActive(false);
        }
        
        public void ChangeUserType(bool isOwnerHost, bool isUserHost)
        {
            kickButton.gameObject.SetActive(isOwnerHost && !isUserHost);
            hostIcon.SetActive(isUserHost);
        }

        public void UpdateUser(Player player)
        {
            _currentPlayer = player;
            playerNameText.text = player.Data[LobbyManager.Instance.PlayerNameKey].Value;
            readyIcon.SetActive(player.Data[LobbyManager.Instance.PlayerReadyKey].Value == "true");
        }

        private async void KickClickHandler()
        {
            kickButton.interactable = false;
            
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(LobbyManager.Instance.JoinedLobby.Id, _currentPlayer.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
            }

            kickButton.interactable = true;
        }
    }
}