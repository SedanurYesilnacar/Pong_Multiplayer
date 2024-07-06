using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class LobbyBrowserCanvas : MonoBehaviour, IInitializableCanvas
    {
        [SerializeField] private MenuTransitionManager menuTransitionManager;
        [SerializeField] private Transform lobbyContainer;
        [SerializeField] private GameObject lobbyPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshButton;
        
        private List<LobbyEntryController> _displayedLobbies = new List<LobbyEntryController>();

        private const float RefreshRateTime = 1.5f;
        private WaitForSeconds _refreshDelay;

        private void Start()
        {
            _refreshDelay = new WaitForSeconds(RefreshRateTime);
            
            backButton.onClick.AddListener(BackClickHandler);
            refreshButton.onClick.AddListener(RefreshClickHandler);
        }

        public void Init()
        {
            DisplayLobbies();
        }

        private async void DisplayLobbies()
        {
            // QueryLobbiesOptions queryOption = new QueryLobbiesOptions()
            // {
            //     Count = 25,
            //     Filters = new List<QueryFilter>() { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
            //     Order = new List<QueryOrder>() { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
            // };
            ClearLobbies();

            try
            {
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
                for (int i = 0; i < queryResponse.Results.Count; i++)
                {
                    var currentLobby = queryResponse.Results[i];
                    var spawnedLobby = Instantiate(lobbyPrefab, lobbyContainer);
                    var spawnedLobbyEntryController = spawnedLobby.GetComponent<LobbyEntryController>();
                    spawnedLobbyEntryController.Init(menuTransitionManager, currentLobby);
                    _displayedLobbies.Add(spawnedLobbyEntryController);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                menuTransitionManager.ShowNotification(e.Message);
            }

            refreshButton.interactable = false;
            StartCoroutine(RefreshRoutine());
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
            yield return _refreshDelay;

            refreshButton.interactable = true;
        }

        private void BackClickHandler()
        {
            menuTransitionManager.ChangeState(MenuStates.MainMenu);
        }

        private void RefreshClickHandler()
        {
            DisplayLobbies();
        }
    }
}