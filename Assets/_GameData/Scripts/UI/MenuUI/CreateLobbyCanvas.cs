using _GameData.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class CreateLobbyCanvas : MonoBehaviour
    {
        [SerializeField] private LobbyAccessibilityToggle lobbyAccessibilityToggle;
        [SerializeField] private TMP_InputField lobbyNameText;
        [SerializeField] private Button createLobbyButton;
        [SerializeField] private Button backButton;

        private string _lobbyName;
        private const string DefaultLobbyName = "My Lobby";

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
            createLobbyButton.onClick.AddListener(CreateLobbyClickHandler);
        }

        private void UnsubscribeEvents()
        {
            backButton.onClick.RemoveAllListeners();
            createLobbyButton.onClick.RemoveAllListeners();
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

        private void CreateLobbyClickHandler()
        {
            var userLobbyOptions = GetLobbyOptions();
            LobbyManager.Instance.CreateLobby(userLobbyOptions);
        }

        private void BackClickHandler()
        {
            LobbyManager.Instance.OnMenuStateChangeRequested?.Invoke(MenuStates.MainMenu);
        }
    }

    public struct LobbyCreateOptions
    {
        public string LobbyName;
        public LobbyAccessibilityType LobbyAccessibilityType;
    }
}