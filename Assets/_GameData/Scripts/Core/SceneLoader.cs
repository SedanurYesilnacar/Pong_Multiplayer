using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _GameData.Scripts.Core
{
    public class SceneLoader : NetworkBehaviour
    {
        private Scene _currentGameScene;
        private NetworkManager _networkManager;

        private const string MainMenuSceneName = "MainMenu";
        private const string GameplaySceneName = "Gameplay";

        private bool _isSessionInitialized;

        public static SceneLoader Instance { get; private set; }

        public virtual void Awake()
        {
            InitSingleton();
            GetReferences();
        }

        private void Start()
        {
            InitStartUp();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _networkManager.OnServerStarted += InitNetworkSession;
            _networkManager.OnClientStarted += InitNetworkSession;
        }

        public override void OnNetworkDespawn()
        {
            _networkManager.OnServerStarted -= InitNetworkSession;
            _networkManager.OnServerStarted -= InitNetworkSession;
            
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

        private void GetReferences()
        {
            _networkManager = NetworkManager.Singleton;
        }

        private void InitStartUp()
        {
            LoadScene(MainMenuSceneName, false, LoadSceneMode.Single);
        }

        private void InitNetworkSession()
        {
            if (_isSessionInitialized) return;

            LoadScene(GameplaySceneName, true, LoadSceneMode.Single);
            _isSessionInitialized = true;
            Debug.Log(IsHost ? "HOST" : "CLIENT");
        }

        private void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode)
        {
            Debug.Log(sceneName + " load requested");
            if (useNetworkSceneManager && _networkManager.IsServer)
            {
                _networkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }
    }
}