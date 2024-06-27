using System;
using _GameData.Scripts.Core;
using _GameData.Scripts.ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private PlayerVisualData playerVisualData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb;

        [SerializeField] private float movementSpeed = 8f;
        
        private const float EdgeOffset = 7.5f;

        private InputManager _inputManager;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            GetReferences();
            SetupPlayer();
        }

        private void GetReferences()
        {
            _inputManager = FindObjectOfType<InputManager>();
        }

        private void SetupPlayer()
        {
            float edgeMultiplier;
            if (IsHost)
            {
                if (IsOwner)
                {
                    rb.isKinematic = false;
                    spriteRenderer.sprite = playerVisualData.hostPlayerSprite;
                    edgeMultiplier = -1f;
                }
                else
                {
                    spriteRenderer.sprite = playerVisualData.clientPlayerSprite;
                    edgeMultiplier = 1f;
                }
            }
            else
            {
                if (IsOwner)
                {
                    rb.isKinematic = false;
                    spriteRenderer.sprite = playerVisualData.clientPlayerSprite;
                    edgeMultiplier = 1f;
                }
                else
                {
                    spriteRenderer.sprite = playerVisualData.hostPlayerSprite;
                    edgeMultiplier = -1f;
                }
            }
            
            transform.position = Vector3.right * EdgeOffset * edgeMultiplier;
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            
            rb.velocity = Vector2.up * (movementSpeed * _inputManager.VerticalValue);
        }
    }
}
