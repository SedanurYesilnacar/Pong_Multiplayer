using System;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts.UI.InGameUI
{
    public class PingCanvas : NetworkBehaviour
    {
        [SerializeField] private Canvas pingCanvas;
        [SerializeField] private TMP_Text pingText;

        private bool IsDisplayingPing => _networkTransport && pingCanvas.enabled;

        private InGameSettingsCanvas _settingsCanvas;
        private NetworkTransport _networkTransport;
        private StringBuilder _stringBuilder = new StringBuilder();
        private float _pingDisplayTimer;
        
        private const string PingPostfix = " MS";
        private const float PingDisplayInterval = 1f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _networkTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        }

        public override void OnNetworkDespawn()
        {
            _networkTransport = null;
            
            base.OnNetworkDespawn();
        }

        private void OnEnable()
        {
            _settingsCanvas = FindObjectOfType<InGameSettingsCanvas>();
            _settingsCanvas.OnPingToggled += OnPingToggledHandler;
        }

        private void OnDisable()
        {
            _settingsCanvas.OnPingToggled -= OnPingToggledHandler;
        }

        private void Update()
        {
            if (!IsDisplayingPing) return;
            
            if (_pingDisplayTimer < PingDisplayInterval)
            {
                _pingDisplayTimer += Time.deltaTime;
            }
            else
            {
                _pingDisplayTimer = 0f;
                DisplayPing();
            }
        }

        private void DisplayPing()
        {
            var currentPing = _networkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
                
            _stringBuilder.Clear();
            _stringBuilder.Append(currentPing.ToString());
            _stringBuilder.Append(PingPostfix);
            pingText.text = _stringBuilder.ToString();
        }

        private void SetCanvasVisibility(bool isVisible)
        {
            pingCanvas.enabled = isVisible;
        }

        private void OnPingToggledHandler(bool isActivated)
        {
            SetCanvasVisibility(isActivated);
        }
    }
}