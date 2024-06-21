using System;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class GameManager : NetworkBehaviour
    {
        private BallSpawner _ballSpawner;
        private CountdownCanvas _countdownCanvas;

        private void GetReferences()
        {
            _ballSpawner = FindObjectOfType<BallSpawner>();
            _countdownCanvas = FindObjectOfType<CountdownCanvas>();

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
        }
        
        public override void OnNetworkDespawn()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallbackHandler;
            _countdownCanvas.OnCountdownCompleted -= OnCountdownCompletedHandler;
            
            base.OnNetworkDespawn();
        }

        private void OnClientConnectedCallbackHandler(ulong obj)
        {
            Debug.Log("OnClientConnectedCallbackHandler");
            _ballSpawner.SpawnBall();
            _countdownCanvas.StartCountdown();
        }

        private void OnCountdownCompletedHandler()
        {
            _ballSpawner.InitBall();
        }
    }
}