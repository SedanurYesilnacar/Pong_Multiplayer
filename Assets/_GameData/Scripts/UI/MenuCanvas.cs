using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public class MenuCanvas : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button quitButton;

        private NetworkManager _networkManager;

        private void OnEnable()
        {
            _networkManager = NetworkManager.Singleton;
            
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            hostButton.onClick.AddListener(HostClickHandler);
            clientButton.onClick.AddListener(ClientClickHandler);
            quitButton.onClick.AddListener(QuitClickHandler);
        }

        private void UnsubscribeEvents()
        {
            hostButton.onClick.RemoveAllListeners();
            clientButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }

        private void HostClickHandler() => _networkManager.StartHost();
        private void ClientClickHandler() => _networkManager.StartClient();
        private void QuitClickHandler() => Application.Quit();
    }
}
