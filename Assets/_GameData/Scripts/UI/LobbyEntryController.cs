using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class LobbyEntryController : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyOwnerNameText;
        [SerializeField] private Button lobbyJoinButton;

        public void Init(Lobby lobbyInfo)
        {
            lobbyNameText.text = lobbyInfo.Name;
            lobbyOwnerNameText.text = lobbyInfo.Players[0].Id;
        }
    }
}