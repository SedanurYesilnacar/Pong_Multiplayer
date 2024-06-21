using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class BallSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;

        private GameObject _instantiatedBall;

        public void SpawnBall()
        {
            _instantiatedBall = Instantiate(ballPrefab);
            _instantiatedBall.GetComponent<NetworkObject>().Spawn();
        }

        public void InitBall()
        {
            _instantiatedBall.GetComponent<BallController>().InitBall();
        }
    }
}