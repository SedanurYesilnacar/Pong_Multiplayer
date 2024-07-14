using System.Collections;
using System.Collections.Generic;
using _GameData.Scripts.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class LobbyBrowserCanvas : MonoBehaviour, IInitializableCanvas
    {
        [SerializeField] private Transform lobbyContainer;
        [SerializeField] private GameObject lobbyPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshButton;
        
        private List<LobbyEntryController> _displayedLobbies = new List<LobbyEntryController>();

        private const float RefreshRateTime = 1.5f;
        private WaitForSeconds _refreshDelay;

        private void Awake()
        {
            _refreshDelay = new WaitForSeconds(RefreshRateTime);
        }

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
            backButton.onClick.AddListener(BackClickHandler);
            refreshButton.onClick.AddListener(RefreshClickHandler);
            LobbyManager.Instance.OnLobbyListUpdated += OnLobbyListUpdatedHandler;
        }

        private void UnsubscribeEvents()
        {
            backButton.onClick.RemoveAllListeners();
            refreshButton.onClick.RemoveAllListeners();
            if (LobbyManager.Instance == null) return;
            LobbyManager.Instance.OnLobbyListUpdated -= OnLobbyListUpdatedHandler;
        }

        public void Init()
        {
            RefreshClickHandler();
        }

        private void DisplayLobbies(List<Lobby> lobbies)
        {
            StartCoroutine(RefreshRoutine());
            
            ClearLobbies();
            
            for (int i = 0; i < lobbies.Count; i++)
            {
                var currentLobby = lobbies[i];
                var spawnedLobby = Instantiate(lobbyPrefab, lobbyContainer);
                var spawnedLobbyEntryController = spawnedLobby.GetComponent<LobbyEntryController>();
                spawnedLobbyEntryController.Init(currentLobby);
                _displayedLobbies.Add(spawnedLobbyEntryController);
            }
        }

        private void ClearLobbies()
        {
            for (int i = 0; i < _displayedLobbies.Count; i++)
            {
                Destroy(_displayedLobbies[i].gameObject);
            }
            
            _displayedLobbies.Clear();
        }

        private IEnumerator RefreshRoutine()
        {
            refreshButton.interactable = false;
            yield return _refreshDelay;
            refreshButton.interactable = true;
        }

        private void BackClickHandler()
        {
            LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.MainMenu);
        }

        private void RefreshClickHandler()
        {
            LobbyManager.Instance.QueryLobbies();
        }

        private void OnLobbyListUpdatedHandler(List<Lobby> lobbies)
        {
            DisplayLobbies(lobbies);
        }
    }
}