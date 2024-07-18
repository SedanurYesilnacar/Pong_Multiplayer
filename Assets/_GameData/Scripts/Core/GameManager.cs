using System;
using _GameData.Scripts.UI;
using _GameData.Scripts.UI.InGameUI;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts.Core
{
    public class GameManager : NetworkBehaviour
    {
        private BallSpawner _ballSpawner;
        private CountdownCanvas _countdownCanvas;
        private ScoreCanvas _scoreCanvas;

        public Action<bool> OnGameFailed; // isHostFailed

        private void GetReferences()
        {
            _ballSpawner = FindObjectOfType<BallSpawner>();
            _countdownCanvas = FindObjectOfType<CountdownCanvas>();
            _scoreCanvas = FindObjectOfType<ScoreCanvas>();
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
            SubscribeEvents();
        }
        
        public override void OnNetworkDespawn()
        {
            UnsubscribeEvents();
            
            base.OnNetworkDespawn();
        }

        private void SubscribeEvents()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallbackHandler;
            _countdownCanvas.OnCountdownCompleted += OnCountdownCompletedHandler;
            OnGameFailed += OnGameFailedHandler;
        }

        private void UnsubscribeEvents()
        {
            if (NetworkManager) NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallbackHandler;
            if (_countdownCanvas) _countdownCanvas.OnCountdownCompleted -= OnCountdownCompletedHandler;
            OnGameFailed -= OnGameFailedHandler;
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

        private void OnDisable()
        {
            UnsubscribeEvents();
        }
    }
}