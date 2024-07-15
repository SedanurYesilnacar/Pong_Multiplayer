using _GameData.Scripts.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.InGameUI
{
    public class InGameSettingsCanvas : BaseSettingsCanvas
    {
        [SerializeField] private Canvas settingsCanvas;
        [SerializeField] private Button leaveGameButton;

        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();
            leaveGameButton.onClick.AddListener(LeaveGameClickHandler);
            InputManager.Instance.OnSettingsToggled += OnSettingsToggledHandler;
        }

        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            leaveGameButton.onClick.RemoveAllListeners();
            InputManager.Instance.OnSettingsToggled -= OnSettingsToggledHandler;
        }

        protected override void BackClickHandler()
        {
            settingsCanvas.enabled = false;
        }

        private void LeaveGameClickHandler()
        {
            NetworkManager.Singleton.Shutdown();
        }

        private void OnSettingsToggledHandler()
        {
            settingsCanvas.enabled = !settingsCanvas.enabled;
        }
    }
}