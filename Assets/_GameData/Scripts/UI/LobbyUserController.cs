using TMPro;
using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class LobbyUserController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private GameObject readyIcon;
        [SerializeField] private GameObject hostIcon;
        [SerializeField] private GameObject kickIcon;
        
        public void ChangeUserType(bool isOwnerHost, bool isUserHost)
        {
            kickIcon.SetActive(isOwnerHost);
            hostIcon.SetActive(isUserHost);
        }

        public void SetPlayerCredentials(string playerName)
        {
            playerNameText.text = playerName;
        }
    }
}