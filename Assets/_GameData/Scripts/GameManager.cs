using System;
using _GameData.Scripts.UI;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        private BallSpawner _ballSpawner;
        private CountdownCanvas _countdownCanvas;
        private ScoreCanvas _scoreCanvas;

        public Action<bool> OnGameFailed;

        private void GetReferences()
        {
            _ballSpawner = FindObjectOfType<BallSpawner>();
            _countdownCanvas = FindObjectOfType<CountdownCanvas>();
            _scoreCanvas = FindObjectOfType<ScoreCanvas>();

            _countdownCanvas.OnCountdownCompleted += OnCountdownCompletedHandler;
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                enabled = false;
                return;
            }

            base.OnNetworkSpawn();

            GetReferences();
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallbackHandler;
            OnGameFailed += OnGameFailedHandler;
        }
        
        public override void OnNetworkDespawn()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallbackHandler;
            _countdownCanvas.OnCountdownCompleted -= OnCountdownCompletedHandler;
            OnGameFailed -= OnGameFailedHandler;
            
            base.OnNetworkDespawn();
        }
        
        private void OnClientConnectedCallbackHandler(ulong obj)
        {
            Debug.Log("OnClientConnectedCallbackHandler");
            _scoreCanvas.ResetScore();
            _ballSpawner.SpawnBall();
            _countdownCanvas.StartCountdown();
        }

        private void OnCountdownCompletedHandler()
        {
            _ballSpawner.InitBall();
        }

        private void OnGameFailedHandler(bool isHostFailed)
        {
            _scoreCanvas.UpdateScore(isHostFailed);
            _countdownCanvas.StartCountdown();
        }
    }
}