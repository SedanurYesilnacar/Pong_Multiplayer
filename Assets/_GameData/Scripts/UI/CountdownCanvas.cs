using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class CountdownCanvas : NetworkBehaviour
    {
        [SerializeField] private TMP_Text countdownText;
        
        private const int InitialCountdownTime = 3;
        private int _currentTimer;
        private WaitForSeconds _waitForSeconds;

        public event Action OnCountdownCompleted;

        private void Awake()
        {
            _waitForSeconds = new WaitForSeconds(1f);
        }

        public void StartCountdown() => StartCoroutine(StartCountdownRoutine());

        private IEnumerator StartCountdownRoutine()
        {
            _currentTimer = InitialCountdownTime;
            UpdateCountdownTextClientRpc(_currentTimer);
            SetCountdownVisibilityClientRpc(true);

            while (_currentTimer > 0)
            {
                yield return _waitForSeconds;
                _currentTimer--;
                UpdateCountdownTextClientRpc(_currentTimer);
            }
            
            SetCountdownVisibilityClientRpc(false);
            OnCountdownCompleted?.Invoke();
        }

        [ClientRpc]
        private void UpdateCountdownTextClientRpc(int currentTimer)
        {
            countdownText.text = currentTimer.ToString();
        }

        [ClientRpc]
        private void SetCountdownVisibilityClientRpc(bool isVisible)
        {
            countdownText.gameObject.SetActive(isVisible);
        }
    }
}