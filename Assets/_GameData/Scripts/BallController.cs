using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameData.Scripts
{
    public class BallController : NetworkBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float movementSpeed = 8f;

        private const float MinForce = -1f;
        private const float MaxForce = 1f;

        private float _initialHorizontalForceMultiplier;
        private float _initialVerticalForceMultiplier;
        private bool _isBallInitialized;

        public void InitBall()
        {
            rb.isKinematic = false;

            _initialHorizontalForceMultiplier = Random.Range(0, 2) == 0 ? 1f : -1f;
            _initialVerticalForceMultiplier = Random.Range(0, 2) == 0 ? 1f : -1f;

            rb.velocity = new Vector2(_initialHorizontalForceMultiplier, _initialVerticalForceMultiplier) * movementSpeed;
            
            _isBallInitialized = true;
        }

        private void FixedUpdate()
        {
            if (!_isBallInitialized) return;
            
            if (IsHost)
            {
                if (Mathf.Abs(rb.velocity.y) <= 0.05f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, Random.Range(MinForce, MaxForce) * movementSpeed);
                }
                else if (Mathf.Abs(rb.velocity.x) <= 0.05f)
                {
                    rb.velocity = new Vector2(Random.Range(MinForce, MaxForce) * movementSpeed, rb.velocity.y);
                }
            }
        }
    }
}