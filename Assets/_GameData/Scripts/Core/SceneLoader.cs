using _GameData.Scripts.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _GameData.Scripts.Core
{
    public class SceneLoader : NetworkBehaviour
    {
        private Scene _currentGameScene;

        private const string MainMenuSceneName = "MainMenu";
        private const string GameplaySceneName = "Gameplay";

        private bool _isSessionInitialized;

        public static SceneLoader Instance { get; private set; }

        public virtual void Awake()
        {
            InitSingleton();
        }

        private void Start()
        {
            InitStartUp();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.Singleton.OnServerStarted += InitNetworkSession;
            NetworkManager.Singleton.OnClientStarted += InitNetworkSession;
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEventHandler;
            
            LoadingCanvas.Instance.Hide();
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnServerStarted -= InitNetworkSession;
            NetworkManager.Singleton.OnServerStarted -= InitNetworkSession;
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEventHandler;
            
            base.OnNetworkDespawn();
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

        private void InitStartUp()
        {
            LoadScene(MainMenuSceneName, false, LoadSceneMode.Single);
        }

        private void InitNetworkSession()
        {
            Debug.Log("IsSessionInitialized: " + _isSessionInitialized);
            if (_isSessionInitialized) return;

            LoadScene(GameplaySceneName, true, LoadSceneMode.Single);
            _isSessionInitialized = true;
            Debug.Log(IsHost ? "HOST" : "CLIENT");
            
            LoadingCanvas.Instance.Hide();
        }

        private void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode)
        {
            Debug.Log(sceneName + " load requested");
            if (useNetworkSceneManager && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, loadSceneMode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }

        private void OnConnectionEventHandler(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
            {
                Debug.Log("client disconnected");
                networkManager.Shutdown();
                _isSessionInitialized = false;
                InitStartUp();
            }
        }
    }
}