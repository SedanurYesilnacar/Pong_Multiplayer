using System;
using System.Collections;
using System.Collections.Generic;
using _GameData.Scripts.Core;
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
        [SerializeField] private TMP_InputField lobbyNameText;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button backButton;

        private string _lobbyName;
        private const string DefaultLobbyName = "My Lobby";
        private Lobby _createdLobby;
        private WaitForSeconds _heartbeatTimer; // Active lifespan = 30s

        private void Start()
        {
            backButton.onClick.AddListener(BackClickHandler);
        }

        public void Init()
        {
            _heartbeatTimer = new WaitForSeconds(25f);
            
            createLobbyButton.onClick.RemoveAllListeners();
            createLobbyButton.onClick.AddListener(CreateLobbyClickHandler);
        }

        private LobbyCreateOptions GetLobbyOptions()
        {
            if (string.IsNullOrWhiteSpace(lobbyNameText.text) || string.IsNullOrEmpty(lobbyNameText.text)) _lobbyName = DefaultLobbyName;
            else _lobbyName = lobbyNameText.text;

            LobbyCreateOptions newLobbyCreateOptions = new LobbyCreateOptions()
            {
                LobbyName = _lobbyName,
                LobbyAccessibilityType = lobbyAccessibilityToggle.CurrentAccessibilityType
            };
            
            return newLobbyCreateOptions;
        }

        private IEnumerator HeartbeatRoutine()
        {
            yield return _heartbeatTimer;
            
            if (_createdLobby == null) yield break;
            
            yield return LobbyService.Instance.SendHeartbeatPingAsync(_createdLobby.Id);
            StartCoroutine(HeartbeatRoutine());
        }

        private async void CreateLobbyClickHandler()
        {
            var userLobbyOptions = GetLobbyOptions();
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = userLobbyOptions.LobbyAccessibilityType == LobbyAccessibilityType.Private,
                Player = LobbyManager.Instance.Player
            };
            
            try
            {
                _createdLobby = await LobbyService.Instance.CreateLobbyAsync(userLobbyOptions.LobbyName, 2, lobbyOptions);
                StartCoroutine(HeartbeatRoutine());
                LobbyManager.Instance.JoinedLobby = _createdLobby;
                menuTransitionManager.ChangeState(MenuStates.Lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
                menuTransitionManager.ShowNotification(e.Message);
            }
        }

        private void BackClickHandler()
        {
            menuTransitionManager.ChangeState(MenuStates.MainMenu);
        }

        private void OnDisable()
        {
            createLobbyButton.onClick.RemoveAllListeners();
            backButton.onClick.RemoveAllListeners();
        }
    }

    public struct LobbyCreateOptions
    {
        public string LobbyName;
        public LobbyAccessibilityType LobbyAccessibilityType;
    }
}