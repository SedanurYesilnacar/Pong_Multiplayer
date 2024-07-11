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
            NetworkManager.Singleton.OnServerStarted += OnServerStartedHandler;
            NetworkManager.Singleton.OnServerStopped += OnServerStoppedHandler;
        }

        private void UnsubscribeEvents()
        {
            backButton.onClick.RemoveAllListeners();
            NetworkManager.Singleton.OnServerStarted -= OnServerStartedHandler;
            NetworkManager.Singleton.OnServerStopped -= OnServerStoppedHandler;
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