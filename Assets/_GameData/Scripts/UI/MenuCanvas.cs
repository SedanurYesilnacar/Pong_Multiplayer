using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class MenuCanvas : MonoBehaviour
    {
        [SerializeField] private MenuTransitionManager menuTransitionManager;
        [SerializeField] private Button quickPlayButton;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button lobbyBrowserButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private NetworkManager _networkManager;

        private void OnValidate()
        {
            menuTransitionManager = FindObjectOfType<MenuTransitionManager>();
        }

        private void OnEnable()
        {
            _networkManager = NetworkManager.Singleton;
            
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            quickPlayButton.onClick.AddListener(QuickPlayClickHandler);
            createLobbyButton.onClick.AddListener(CreateLobbyClickHandler);
            lobbyBrowserButton.onClick.AddListener(LobbyBrowserClickHandler);
            settingsButton.onClick.AddListener(SettingsClickHandler);
            quitButton.onClick.AddListener(QuitClickHandler);
        }

        private void UnsubscribeEvents()
        {
            quickPlayButton.onClick.RemoveAllListeners();
            createLobbyButton.onClick.RemoveAllListeners();
            lobbyBrowserButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }

        private async void QuickPlayClickHandler()
        {
            try
            {
                QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions()
                {
                    Player = new Player()
                    {
                        Data = new Dictionary<string, PlayerDataObject>()
                        {
                            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Player2") }
                        }
                    }
                };
                
                var joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
                menuTransitionManager.CurrentLobby = joinedLobby;
                menuTransitionManager.ChangeState(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                menuTransitionManager.ShowNotification(e.Message);
            }
        }
        
        private void CreateLobbyClickHandler() => menuTransitionManager.ChangeState(MenuStates.CreateLobby);
        private void LobbyBrowserClickHandler() => menuTransitionManager.ChangeState(MenuStates.LobbyBrowser);
        private void SettingsClickHandler()
        {
            
        }

        private void QuitClickHandler() => Application.Quit();
    }
}
