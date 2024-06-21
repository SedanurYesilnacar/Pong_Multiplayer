using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class BallSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                enabled = false;
                return;
            }

            base.OnNetworkSpawn();

            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallbackHandler;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallbackHandler;

            base.OnNetworkDespawn();
        }

        private void OnClientConnectedCallbackHandler(ulong obj)
        {
            SpawnBall();
        }

        private void SpawnBall()
        {
            var instantiatedBall = Instantiate(ballPrefab);
            instantiatedBall.GetComponent<NetworkObject>().Spawn();
            instantiatedBall.GetComponent<BallController>().InitBall();
        }
    }
}