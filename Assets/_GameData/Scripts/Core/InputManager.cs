using System;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private PlayerControls _playerControls;
        public float VerticalValue { get; private set; }
        public Action OnSettingsToggled;

        private void Awake()
        {
            InitSingleton();
        }
        
        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnClientStarted += OnClientStartedHandler;
            NetworkManager.Singleton.OnClientStopped += OnClientStoppedHandler;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.OnClientStarted -= OnClientStartedHandler;
            NetworkManager.Singleton.OnClientStopped -= OnClientStoppedHandler;
        }

        private void Start()
        {
            SetupInputSystem();
        }

        private void SetupInputSystem()
        {
            _playerControls = new PlayerControls();
            _playerControls.Movement.Enable();
        }

        private void Update()
        {
            VerticalValue = _playerControls.Movement.Move.ReadValue<float>();

            if (_playerControls.UI.ToggleSettings.WasPressedThisFrame())
            {
                OnSettingsToggled?.Invoke();
            }
        }

        private void OnClientStartedHandler()
        {
            _playerControls.UI.Enable();
        }

        private void OnClientStoppedHandler(bool isHostClient)
        {
            _playerControls.UI.Disable();
        }
    }
}