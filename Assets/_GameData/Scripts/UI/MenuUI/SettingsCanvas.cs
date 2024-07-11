using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class SettingsCanvas : MonoBehaviour
    {
        [SerializeField] private MenuTransitionManager menuTransitionManager;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button backButton;

        private bool _isInGame;
        private NetworkManager _networkManager;

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
            if (!_networkManager) _networkManager = NetworkManager.Singleton;
            
            backButton.onClick.AddListener(BackClickHandler);
            _networkManager.OnServerStarted += OnServerStartedHandler;
            _networkManager.OnServerStopped += OnServerStoppedHandler;
        }

        private void UnsubscribeEvents()
        {
            backButton.onClick.RemoveAllListeners();
            
            if (!_networkManager) return;
            _networkManager.OnServerStarted -= OnServerStartedHandler;
            _networkManager.OnServerStopped -= OnServerStoppedHandler;
        }

        private void BackClickHandler()
        {
            menuTransitionManager.ChangeState(_isInGame ? MenuStates.None : MenuStates.MainMenu);
        }

        private void OnServerStartedHandler()
        {
            _isInGame = true;
        }

        private void OnServerStoppedHandler(bool isHostStopped)
        {
            _isInGame = false;
        }
    }
}