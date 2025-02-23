using System;
using System.Collections;
using _GameData.Scripts.Core;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameData.Scripts
{
    public class BallController : NetworkBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleController hitParticle;
        [SerializeField] private float movementSpeed = 8f;

        private const float MinForce = -1f;
        private const float MaxForce = 1f;

        private float _initialHorizontalForceMultiplier;
        private float _initialVerticalForceMultiplier;
        private bool _isBallInitialized;

        private WaitForSeconds _ballTrainActivationDelay;

        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _ballTrainActivationDelay = new WaitForSeconds(0.1f);
        }

        public void InitBall()
        {
            rb.position = Vector3.zero;
            rb.isKinematic = false;
            SetTrailVisibilityClientRpc(true);

            _initialHorizontalForceMultiplier = Random.Range(0, 2) == 0 ? 1f : -1f;
            _initialVerticalForceMultiplier = Random.Range(0, 2) == 0 ? 1f : -1f;

            rb.velocity = new Vector2(_initialHorizontalForceMultiplier, _initialVerticalForceMultiplier) * movementSpeed;
            
            _isBallInitialized = true;
        }

        private void StopBall()
        {
            _isBallInitialized = false;
            
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            SetTrailVisibilityClientRpc(false);
        }

        [ClientRpc]
        private void SetTrailVisibilityClientRpc(bool isActivated)
        {
            if (isActivated) StartCoroutine(EnableTrailRoutine());
            else trail.enabled = false;
        }

        private IEnumerator EnableTrailRoutine()
        {
            yield return _ballTrainActivationDelay;
            trail.enabled = true;
        }

        [ClientRpc]
        private void HandleBallHitClientRpc()
        {
            AudioManager.Instance.PlaySfx(AudioManager.Instance.AudioLibraryData.ballHitAudio);
            hitParticle.Play(transform.position);
        }

        private void HandleFail(bool isHostFailed)
        {
            _gameManager.OnGameFailed.Invoke(isHostFailed);
            StopBall();
        }

        private void FixedUpdate()
        {
            if (!_isBallInitialized) return;
            if (!IsHost) return;
            
            if (Mathf.Abs(rb.velocity.y) <= 0.05f)
            {
                rb.velocity = new Vector2(rb.velocity.x, Random.Range(MinForce, MaxForce) * movementSpeed);
            }
            else if (Mathf.Abs(rb.velocity.x) <= 0.05f)
            {
                rb.velocity = new Vector2(Random.Range(MinForce, MaxForce) * movementSpeed, rb.velocity.y);
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            HandleBallHitClientRpc();
            
            if (!IsHost) return;
            
            if (col.gameObject.TryGetComponent(out EdgeController edge))
            {
                var isHostFailed = edge.IsHostSide;
                HandleFail(isHostFailed);
            }
        }
    }
}