using _GameData.Scripts.Core;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class LobbyEntryController : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyOwnerNameText;
        [SerializeField] private Button lobbyJoinButton;

        private Lobby _currentLobby;

        public void Init(Lobby lobbyInfo)
        {
            _currentLobby = lobbyInfo;
            
            lobbyNameText.text = _currentLobby.Name;
            lobbyOwnerNameText.text = _currentLobby.Players[0].Id;
            
            lobbyJoinButton.onClick.AddListener(LobbyJoinClickHandler);
        }

        private void LobbyJoinClickHandler()
        {
            LobbyManager.Instance.JoinLobbyById(_currentLobby);
        }
    }
}