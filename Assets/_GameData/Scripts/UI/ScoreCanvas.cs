using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _GameData.Scripts.UI
{
    public class ScoreCanvas : NetworkBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        private const string ScoreSplitText = "    -    ";

        private int _hostScore;
        private int _clientScore;

        [ClientRpc]
        private void UpdateScoreTextClientRpc(int hostScore, int clientScore)
        {
            scoreText.text = hostScore + ScoreSplitText + clientScore;
        }

        public void ResetScore()
        {
            _hostScore = _clientScore = 0;
            UpdateScoreTextClientRpc(_hostScore, _clientScore);
        }

        public void UpdateScore(bool isHostFailed)
        {
            if (isHostFailed) _clientScore++;
            else _hostScore++;
            
            UpdateScoreTextClientRpc(_hostScore, _clientScore);
        }
    }
}