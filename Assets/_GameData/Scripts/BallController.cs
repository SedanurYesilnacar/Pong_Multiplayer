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

        public void InitBall()
        {
            rb.isKinematic = false;
            
            var randomForceX = Random.Range(MinForce, MaxForce) * movementSpeed;
            var randomForceY = Random.Range(MinForce, MaxForce) * movementSpeed;

            if (randomForceX == 0 && randomForceY == 0) randomForceX = 1f * movementSpeed;

            rb.velocity = new Vector2(randomForceX, randomForceY);
        }

        private void FixedUpdate()
        {
            if (IsHost)
            {
                if (Mathf.Abs(rb.velocity.y) <= 0.05f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, Random.Range(MinForce, MaxForce) * movementSpeed);
                }
            }
        }
    }
}