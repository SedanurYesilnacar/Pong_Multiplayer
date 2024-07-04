using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class MenuCanvas : MonoBehaviour
    {
        [SerializeField] private Button quickPlayButton;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button lobbyBrowserButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private NetworkManager _networkManager;
        private MenuTransitionManager _menuTransitionManager;

        private void OnEnable()
        {
            _networkManager = NetworkManager.Singleton;
            _menuTransitionManager = FindObjectOfType<MenuTransitionManager>();
            
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

        private void QuickPlayClickHandler() => _networkManager.StartHost();
        private void CreateLobbyClickHandler() => _menuTransitionManager.ChangeState(MenuStates.CreateLobby);
        private void LobbyBrowserClickHandler() => _menuTransitionManager.ChangeState(MenuStates.LobbyBrowser);
        private void SettingsClickHandler()
        {
            
        }

        private void QuitClickHandler() => Application.Quit();
    }
}
