using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class CreateLobbyCanvas : MonoBehaviour, IInitializableCanvas
    {
        [SerializeField] private MenuTransitionManager menuTransitionManager;
        [SerializeField] private LobbyAccessibilityToggle lobbyAccessibilityToggle;
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button backButton;

        private string _lobbyName;
        private const string DefaultLobbyName = "My Lobby";
        private Lobby _createdLobby;
        private WaitForSeconds _heartbeatTimer; // Active lifespan = 30s

        public void Init()
        {
            _heartbeatTimer = new WaitForSeconds(25f);
            createLobbyButton.onClick.AddListener(CreateLobbyClickHandler);
            backButton.onClick.AddListener(BackClickHandler);
        }

        private LobbyCreateOptions GetLobbyOptions()
        {
            if (String.IsNullOrWhiteSpace(lobbyNameText.text) || String.IsNullOrEmpty(lobbyNameText.text)) _lobbyName = DefaultLobbyName;
            else _lobbyName = lobbyNameText.text;

            LobbyCreateOptions newLobbyCreateOptions = new LobbyCreateOptions()
            {
                LobbyName = _lobbyName,
                LobbyAccessibilityType = lobbyAccessibilityToggle.CurrentAccessibilityType
            };
            
            return newLobbyCreateOptions;
        }
        
        private async void CreateLobbyClickHandler()
        {
            var userLobbyOptions = GetLobbyOptions();
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = userLobbyOptions.LobbyAccessibilityType == LobbyAccessibilityType.Private,
                Player = new Player()
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "Player1") }
                    }
                }
            };
            
            try
            {
                _createdLobby = await LobbyService.Instance.CreateLobbyAsync(userLobbyOptions.LobbyName, 2, lobbyOptions);
                StartCoroutine(HeartbeatRoutine());
                Debug.Log(_createdLobby.Name);
                Debug.Log(_createdLobby.MaxPlayers);
                menuTransitionManager.CurrentLobby = _createdLobby;
                menuTransitionManager.ChangeState(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                menuTransitionManager.ShowNotification(e.Message);
            }
        }

        private IEnumerator HeartbeatRoutine()
        {
            yield return _heartbeatTimer;
            
            if (_createdLobby == null) yield break;
            
            yield return LobbyService.Instance.SendHeartbeatPingAsync(_createdLobby.Id);
            StartCoroutine(HeartbeatRoutine());
        }

        private void BackClickHandler()
        {
            menuTransitionManager.ChangeState(MenuStates.MainMenu);
        }
    }

    public struct LobbyCreateOptions
    {
        public string LobbyName;
        public LobbyAccessibilityType LobbyAccessibilityType;
    }
}