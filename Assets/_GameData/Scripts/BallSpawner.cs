using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class BallSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;

        private GameObject _instantiatedBall;
        private BallController _currentBallController;

        public void SpawnBall()
        {
            _instantiatedBall = Instantiate(ballPrefab);
            _instantiatedBall.GetComponent<NetworkObject>().Spawn();
            _currentBallController = _instantiatedBall.GetComponent<BallController>();
        }

        public void InitBall()
        {
            _currentBallController.InitBall();
        }
    }
}