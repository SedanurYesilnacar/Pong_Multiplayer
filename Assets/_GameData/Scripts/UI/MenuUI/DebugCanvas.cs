using _GameData.Scripts.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI.MenuUI
{
    public class DebugCanvas : MonoBehaviour
    {
        [SerializeField] private Canvas debugCanvas;
        [SerializeField] private Toggle isLocalClientToggle;
        [SerializeField] private string debugRemoteClientIp = "127.0.0.2";
        [SerializeField] private string debugLocalClientIp = "127.0.0.1";

        private bool IsLocalClient => isLocalClientToggle.isOn;
        private UnityTransport _unityTransport;

        private void Awake()
        {
            _unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }
        
        private void OnEnable()
        {
            InputManager.Instance.OnDebugToggled += OnDebugToggledHandler;
        }

        private void OnDisable()
        {
            InputManager.Instance.OnDebugToggled -= OnDebugToggledHandler;
        }

        private void ToggleDebugCanvasVisibility()
        {
            debugCanvas.enabled = !debugCanvas.enabled;
        }

        private void OnDebugToggledHandler()
        {
            ToggleDebugCanvasVisibility();
        }

        public void SetConnectionData()
        {
            _unityTransport.SetConnectionData(IsLocalClient ? debugLocalClientIp : debugRemoteClientIp, 777);
        }
    }
}