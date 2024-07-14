using _GameData.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class MenuCanvas : MonoBehaviour
    {
        [SerializeField] private Button quickPlayButton;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button lobbyBrowserButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button leaveGameButton;
        [SerializeField] private Button quitButton;

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
            quickPlayButton.onClick.AddListener(QuickPlayClickHandler);
            createLobbyButton.onClick.AddListener(CreateLobbyClickHandler);
            lobbyBrowserButton.onClick.AddListener(LobbyBrowserClickHandler);
            settingsButton.onClick.AddListener(SettingsClickHandler);
            resumeButton.onClick.AddListener(ResumeClickHandler);
            leaveGameButton.onClick.AddListener(LeaveGameClickHandler);
            quitButton.onClick.AddListener(QuitClickHandler);
        }

        private void UnsubscribeEvents()
        {
            quickPlayButton.onClick.RemoveAllListeners();
            createLobbyButton.onClick.RemoveAllListeners();
            lobbyBrowserButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            resumeButton.onClick.RemoveAllListeners();
            leaveGameButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }

        private void QuickPlayClickHandler()
        {
            LobbyManager.Instance.QuickPlay();
        }

        private void CreateLobbyClickHandler() => LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.CreateLobby);
        private void LobbyBrowserClickHandler() => LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.LobbyBrowser);
        private void SettingsClickHandler() => LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.Settings);
        private void ResumeClickHandler() => LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.None);
        private void QuitClickHandler() => Application.Quit();

        private void LeaveGameClickHandler()
        {
            
        }
    }
}
