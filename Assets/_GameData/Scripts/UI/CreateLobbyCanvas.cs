using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class CreateLobbyCanvas : MonoBehaviour
    {
        [SerializeField] private LobbyAccessibilityToggle lobbyAccessibilityToggle;
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private Button createLobbyButton;

        private string _lobbyName;
        private const string DefaultLobbyName = "My Lobby";

        public void Init(UnityAction lobbyCreateAction)
        {
            createLobbyButton.onClick.AddListener(lobbyCreateAction);
        }

        public LobbyCreateOptions GetLobbyOptions()
        {
            _lobbyName = lobbyNameText.text;
            if (String.IsNullOrWhiteSpace(_lobbyName)) _lobbyName = DefaultLobbyName;

            LobbyCreateOptions newLobbyCreateOptions = new LobbyCreateOptions()
            {
                LobbyName = _lobbyName,
                LobbyAccessibilityType = lobbyAccessibilityToggle.CurrentAccessibilityType
            };

            return newLobbyCreateOptions;
        }
    }

    public struct LobbyCreateOptions
    {
        public string LobbyName;
        public LobbyAccessibilityType LobbyAccessibilityType;
    }
}